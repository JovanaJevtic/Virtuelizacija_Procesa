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

        private readonly double T_dht_threshold = double.Parse(ConfigurationManager.AppSettings["T_dht_threshold"] ?? "50");
        private readonly double T_bmp_threshold = double.Parse(ConfigurationManager.AppSettings["T_bmp_threshold"] ?? "50");
        private readonly double V_threshold = double.Parse(ConfigurationManager.AppSettings["V_threshold"] ?? "100");

        private readonly SensorEvents events = new SensorEvents();

        //test
        private bool testExceptionTriggered = false;

        private double lastVolume = 0;



        public SensorService()
        {
            // Pretplate na događaje (logovanje na konzolu)
            events.OnTransferStarted += () => Console.WriteLine("Prenos započet...");
            events.OnSampleReceived += (sample) => Console.WriteLine($"Sample primljen: {sample.DateTime}, V={sample.Volume}, T_DHT={sample.T_DHT}, T_BMP={sample.T_BMP}");
            events.OnTransferCompleted += () => Console.WriteLine("Prenos završen!");
            events.OnWarningRaised += (msg, s) => Console.WriteLine($"Upozorenje za sample {s.DateTime}: {msg}");
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
            return "ACK: Session started";
        }

        public void PushSample(SensorSample sample)
        {
            // provera validnosti podataka
            if (sample == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Uzorak senzora (SensorSample) ne sme biti prazan."));

            if (sample.DateTime == default(DateTime))
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Datum i vreme (DateTime) su obavezni."));

            if (double.IsNaN(sample.Volume) || double.IsInfinity(sample.Volume))
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za Volume mora biti ispravan broj."));

            if (double.IsNaN(sample.T_DHT) || double.IsInfinity(sample.T_DHT))
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za T_DHT mora biti ispravan broj."));

            if (double.IsNaN(sample.T_BMP) || double.IsInfinity(sample.T_BMP))
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za T_BMP mora biti ispravan broj."));

            if (double.IsNaN(sample.Pressure) || double.IsInfinity(sample.Pressure))
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Vrednost za Pressure mora biti ispravan broj."));

            if (sample.Pressure <= 0)
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Pritisak (Pressure) mora biti veći od nule."));
            /////

            string line = $"{sample.DateTime},{sample.Volume},{sample.T_DHT},{sample.T_BMP},{sample.Pressure}";

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

                // Detekcija naglog skoka/opažanja u odnosu na prethodni sample
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


                // Ažuriranje proseka
                sampleCount++;
                avgVolume = (avgVolume * (sampleCount - 1) + sample.Volume) / sampleCount;
                avgT_DHT = (avgT_DHT * (sampleCount - 1) + sample.T_DHT) / sampleCount;
                avgT_BMP = (avgT_BMP * (sampleCount - 1) + sample.T_BMP) / sampleCount;

                lastVolume = sample.Volume;

                // Upis u CSV
                sessionFiles.MeasurementsWriter.WriteLine(line);
                sessionFiles.MeasurementsWriter.Flush();

                // Podizanje događaja
                events.RaiseSampleReceived(sample); 
                foreach (var w in warnings)
                    events.RaiseWarning(w, sample);


                // Simulacija izuzetka za testiranje Dispose-a
                //if (sampleCount == 3 && !testExceptionTriggered)
                //{
                //    testExceptionTriggered = true;
                //    throw new Exception("Simulirani prekid prenosa! (TEST)");
                //}


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
            return "COMPLETED";
        }
    }
}
