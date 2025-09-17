using System;
using System.IO;

namespace Common
{

    public class SessionFiles : IDisposable
    {
        public StreamWriter MeasurementsWriter { get; private set; }
        public StreamWriter RejectsWriter { get; private set; }

        public string MeasurementsFilePath { get; private set; }
        public string RejectsFilePath { get; private set; }

        public SessionFiles(string measurementsFilePath, string rejectsFilePath)
        {
            MeasurementsFilePath = measurementsFilePath;
            RejectsFilePath = rejectsFilePath;

            // Kreiranje fajlova i otvaranje StreamWriter-a
            MeasurementsWriter = new StreamWriter(File.Open(MeasurementsFilePath, FileMode.Create, FileAccess.Write));
            RejectsWriter = new StreamWriter(File.Open(RejectsFilePath, FileMode.Create, FileAccess.Write));

            // Dodaj zaglavlja
            MeasurementsWriter.WriteLine("DateTime,Volume,T_DHT,T_BMP,Pressure");
            MeasurementsWriter.Flush();

            RejectsWriter.WriteLine("DateTime,Volume,T_DHT,T_BMP,Pressure,Reason");
            RejectsWriter.Flush();
        }
        public void Dispose()
        {
            MeasurementsWriter?.Dispose();
            MeasurementsWriter = null;

            RejectsWriter?.Dispose();
            RejectsWriter = null;
        }
    }
}
