using Common;
using Common.Faults;
using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Collections.Generic;

namespace Service
{
    public class SensorService : ISensorService
    {
        private string sessionDir;
        private SessionFiles sessionFiles;
        private bool transferStarted = false;
        private double avgVolume = 0;
        private double avgT_DHT = 0;
        private double avgT_BMP = 0;
        private int sampleCount = 0;

        private readonly double T_dht_threshold = double.Parse(ConfigurationManager.AppSettings["T_dht_threshold"] ?? "25");
        private readonly double T_bmp_threshold = double.Parse(ConfigurationManager.AppSettings["T_bmp_threshold"] ?? "25");
        private readonly double V_threshold = double.Parse(ConfigurationManager.AppSettings["V_threshold"] ?? "100");

        private readonly SensorEvents events = new SensorEvents();

        // poslednje vrednosti za Δ izračunavanje
        private double lastVolume = 0;
        private double lastT_DHT = 0;
        private double lastT_BMP = 0;

        public SensorService()
        {
            // Pretplate na događaje (logovanje na konzolu)
            events.OnTransferStarted += () => Console.WriteLine("Prenos započet...");
            events.OnSampleReceived += (sample) => Console.WriteLine($"Sample primljen: {sample.DateTime}, V={sample.Volume}, T_DHT={sample.T_DHT}, T_BMP={sample.T_BMP}");
            events.OnTransferCompleted += () => Console.WriteLine("Prenos završen!");
            events.OnWarningRaised += (msg, s) => Console.WriteLine($"Upozorenje za sample {s.DateTime}: {msg}");
            events.OnVolumeSpike += (msg, s, dV) => Console.WriteLine($"[ΔV Spike] {msg} za sample {s.DateTime}");
            events.OnOutOfBandWarning += (msg, s, avg) => Console.WriteLine($"[OutOfBand] {msg} (avg={avg:F2}) za sample {s.DateTime}");
            events.OnTemperatureSpikeDHT += (msg, s, dT) => Console.WriteLine($"[ΔTdht Spike] {msg} za sample {s.DateTime}");
            events.OnTemperatureSpikeBMP += (msg, s, dT) => Console.WriteLine($"[ΔTbmp Spike] {msg} za sample {s.DateTime}");
        }

        public string StartSession(SessionMeta meta)
        {
            if (meta == null)
                throw new FaultException<DataFormatFault>(new DataFormatFault("SessionMeta cannot be null."));
            if (string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException<ValidationFault>(new ValidationFault("SessionId is required."));

            string basePath = ConfigurationManager.AppSettings["SessionsPath"] ?? AppDomain.CurrentDomain.BaseDirectory;
            sessionDir = Path.Combine(basePath, meta.SessionId);
            Directory.CreateDirectory(sessionDir);

            sessionFiles = new SessionFiles(
                Path.Combine(sessionDir, "measurements_session.csv"),
                Path.Combine(sessionDir, "rejects.csv"));

            transferStarted = false;
            avgVolume = 0;
            avgT_DHT = 0;
            avgT_BMP = 0;
            sampleCount = 0;

            events.RaiseTransferStarted();
            return "Sesija zapoceta!";
        }

        public void PushSample(SensorSample sample)
        {
            string line = $"{sample.DateTime},{sample.Volume},{sample.T_DHT},{sample.T_BMP},{sample.Pressure}";
            // provera validnosti podataka
            if (sample == null)
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",SensorSample je null");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                        new DataFormatFault("Uzorak senzora (SensorSample) ne sme biti prazan."));
            }
            if (sample.DateTime == default(DateTime))
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",Datum i vreme (DateTime) su obavezni");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                        new ValidationFault("Datum i vreme (DateTime) su obavezni."));
            }
            if (double.IsNaN(sample.Volume) || double.IsInfinity(sample.Volume))
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",Volume nije ispravan broj");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                        new DataFormatFault("Vrednost za Volume mora biti ispravan broj."));
            }
            if (sample.Volume < 0)
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",Volume je negativan");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Volume ne može biti negativan."));
            }

            if (double.IsNaN(sample.T_DHT) || double.IsInfinity(sample.T_DHT))
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",T_DHT nije ispravan broj");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za T_DHT mora biti ispravan broj."));
            }
            if (double.IsNaN(sample.T_BMP) || double.IsInfinity(sample.T_BMP))
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",T_BMP nije ispravan broj");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za T_BMP mora biti ispravan broj."));
            }

            if (double.IsNaN(sample.Pressure) || double.IsInfinity(sample.Pressure))
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",Pressure nije validan ili <=0");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za Pressure mora biti ispravan broj."));
            }

            if (sample.Pressure <= 0)
            {
                sessionFiles.RejectsWriter.WriteLine(line + ",Pressure <= 0");
                sessionFiles.RejectsWriter.Flush();
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Pritisak (Pressure) mora biti veći od nule."));
            }
         
            try
            {
                if (!transferStarted)
                    transferStarted = true;

                List<string> warnings = new List<string>();

                // Pragovi
                if (sample.Volume > V_threshold)
                    warnings.Add($"Volume iznad praga: {sample.Volume:F2}");
                if (sample.T_DHT > T_dht_threshold)
                    warnings.Add($"T_DHT iznad praga: {sample.T_DHT:F2}");
                if (sample.T_BMP > T_bmp_threshold)
                    warnings.Add($"T_BMP iznad praga: {sample.T_BMP:F2}");

                // ±25% od proseka (ako postoji prethodni sample)
                if (sampleCount > 0)
                {
                    if (sample.Volume < avgVolume * 0.75 || sample.Volume > avgVolume * 1.25)
                        warnings.Add($"Volume odstupa ±25% od proseka: {sample.Volume:F2} (avg={avgVolume:F2})");

                    if (sample.T_DHT < avgT_DHT * 0.75 || sample.T_DHT > avgT_DHT * 1.25)
                        warnings.Add($"T_DHT odstupa ±25% od proseka: {sample.T_DHT:F2} (avg={avgT_DHT:F2})");

                    if (sample.T_BMP < avgT_BMP * 0.75 || sample.T_BMP > avgT_BMP * 1.25)
                        warnings.Add($"T_BMP odstupa ±25% od proseka: {sample.T_BMP:F2} (avg={avgT_BMP:F2})");
                }

                // Detekcija naglog skoka u Volume
                if (sampleCount > 0)
                {
                    double deltaV = sample.Volume - lastVolume;
                    if (Math.Abs(deltaV) > V_threshold)
                    {
                        string direction = deltaV > 0 ? "iznad očekivanog" : "ispod očekivanog";
                        events.RaiseVolumeSpike($"ΔV={deltaV:F2} {direction}", sample, deltaV);
                    }
                }

                // Provera OutOfBand za Volume
                if (sampleCount > 0)
                {
                    if (sample.Volume < avgVolume * 0.75)
                        events.RaiseOutOfBandWarning("Volume ispod očekivane vrednosti", sample, avgVolume);
                    else if (sample.Volume > avgVolume * 1.25)
                        events.RaiseOutOfBandWarning("Volume iznad očekivane vrednosti", sample, avgVolume);
                }

                // Detekcija nagle promene temperature DHT
                if (sampleCount > 0)
                {
                    double deltaTdht = sample.T_DHT - lastT_DHT;
                    if (Math.Abs(deltaTdht) > T_dht_threshold)
                    {
                        string direction = deltaTdht > 0 ? "iznad očekivanog" : "ispod očekivanog";
                        events.RaiseTemperatureSpikeDHT($"ΔTdht={deltaTdht:F2} {direction}", sample, deltaTdht);
                    }
                }

                // Detekcija nagle promene temperature BMP
                if (sampleCount > 0)
                {
                    double deltaTbmp = sample.T_BMP - lastT_BMP;
                    if (Math.Abs(deltaTbmp) > T_bmp_threshold)
                    {
                        string direction = deltaTbmp > 0 ? "iznad očekivanog" : "ispod očekivanog";
                        events.RaiseTemperatureSpikeBMP($"ΔTbmp={deltaTbmp:F2} {direction}", sample, deltaTbmp);
                    }
                }

                // Ažuriranje proseka
                sampleCount++;
                avgVolume = (avgVolume * (sampleCount - 1) + sample.Volume) / sampleCount;
                avgT_DHT = (avgT_DHT * (sampleCount - 1) + sample.T_DHT) / sampleCount;
                avgT_BMP = (avgT_BMP * (sampleCount - 1) + sample.T_BMP) / sampleCount;

                // čuvanje last vrednosti za Δ
                lastVolume = sample.Volume;
                lastT_DHT = sample.T_DHT;
                lastT_BMP = sample.T_BMP;

                // Upis u CSV
                sessionFiles.MeasurementsWriter.WriteLine(line);
                sessionFiles.MeasurementsWriter.Flush();

                // Podizanje događaja
                events.RaiseSampleReceived(sample);
                foreach (var w in warnings)
                    events.RaiseWarning(w, sample);
                Console.WriteLine("-------------------------------------------------");

            }
            catch (Exception ex)
            {
                sessionFiles.RejectsWriter.WriteLine(line + $",{ex.Message}");
                sessionFiles.RejectsWriter.Flush();
                throw;
            }
        }

        public string EndSession()
        {
            if (transferStarted)
                events.RaiseTransferCompleted();

            sessionFiles?.Dispose();
            return "Sesija zavrsena!";
        }
    }
}
