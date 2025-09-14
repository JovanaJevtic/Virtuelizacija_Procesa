using Common;
using Common.Faults;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<ISensorService> factory =
               new ChannelFactory<ISensorService>("SensorService");

            ISensorService proxy = factory.CreateChannel();


            var meta = new SessionMeta
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now
            };
            Console.WriteLine(proxy.StartSession(meta));

            // učitavanje CSV fajla
            CsvLoader loader = new CsvLoader();
            var samples = loader.LoadCsv(out List<string> invalidRows, 100);

            foreach (var sample in samples)
            {
                try
                {
                    proxy.PushSample(sample);
                }
                catch (FaultException<ValidationFault> ex)
                {
                    Console.WriteLine($"Validation error: {ex.Detail.Message} for sample at {sample.DateTime}");
                }
            }

            Console.ReadKey();
        }
    }
}
