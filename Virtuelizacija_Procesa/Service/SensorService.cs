using Common;
using Common.Faults;
using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;

namespace Service
{
    public class SensorService : ISensorService
    {
        private string sessionDir;
        // private string measurementsFile;
        // private string rejectsFile;
        private SessionFiles sessionFiles;
        public string StartSession(SessionMeta meta)
        {
            if (meta == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("SessionMeta cannot be null."));

            if (string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException<ValidationFault>(
                    new ValidationFault("SessionId is required."));

            string basePath = ConfigurationManager.AppSettings["SessionsPath"];
            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory; // fallback
            }

            sessionDir = Path.Combine(basePath, meta.SessionId);
            Directory.CreateDirectory(sessionDir);

            // Kreiramo CSV fajlove koristeći SessionFiles
            string measurementsPath = Path.Combine(sessionDir, "measurements_session.csv");
            string rejectsPath = Path.Combine(sessionDir, "rejects.csv");

            sessionFiles = new SessionFiles(measurementsPath, rejectsPath);

            Console.WriteLine($"Session {meta.SessionId} started at {meta.StartTime}");
            return "ACK: Session started";
        }

        public void PushSample(SensorSample sample)
        {
            if (sample == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("SensorSample cannot be null."));

            string line = $"{sample.DateTime},{sample.Volume},{sample.T_DHT},{sample.T_BMP},{sample.Pressure}";

            try
            {
                // Validacija
                if (sample.Pressure <= 0)
                    throw new FaultException<ValidationFault>(new ValidationFault("Pressure must be greater than 0."));

                if (sample.Volume < 0)
                    throw new FaultException<ValidationFault>(new ValidationFault("Volume cannot be negative."));

                if (sample.DateTime == default)
                    throw new FaultException<DataFormatFault>(new DataFormatFault("Sample DateTime is missing or invalid."));

                if (sample.T_DHT < 0 || sample.T_DHT > 50)
                    throw new FaultException<ValidationFault>(new ValidationFault("T_DHT out of expected range"));

                if (sample.T_BMP < 0 || sample.T_BMP > 50)
                    throw new FaultException<ValidationFault>(new ValidationFault("T_BMP out of expected range"));

                // zapis u measurements CSV
                sessionFiles.MeasurementsWriter.WriteLine(line);
                sessionFiles.MeasurementsWriter.Flush();
            }
            catch (FaultException<ValidationFault> ex)
            {
                // zapis u rejects CSV
                sessionFiles.RejectsWriter.WriteLine(line + $",{ex.Detail.Message}");
                sessionFiles.RejectsWriter.Flush();
                throw;
            }
        }

        public string EndSession()
        {
            sessionFiles?.Dispose();
            Console.WriteLine("Session ended.");
            return "COMPLETED";
        }
    }
}
