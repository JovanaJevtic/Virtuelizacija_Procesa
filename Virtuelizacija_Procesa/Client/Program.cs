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
            using (ChannelFactory<ISensorService> factory = new ChannelFactory<ISensorService>("SensorService"))
            {
                IClientChannel proxy = factory.CreateChannel() as IClientChannel;

                if (proxy == null)
                {
                    Console.WriteLine("Greška pri kreiranju proxyja.");
                    return;
                }

                using (proxy)
                {
                    try
                    {
                        ISensorService service = proxy as ISensorService;

                        var meta = new SessionMeta
                        {
                            SessionId = Guid.NewGuid().ToString(),
                            StartTime = DateTime.Now
                        };

                        Console.WriteLine(service.StartSession(meta));

                        CsvLoader loader = new CsvLoader();
                        var samples = loader.LoadCsv(out List<string> invalidRows, 100);

                        foreach (var sample in samples)
                        {
                            try
                            {
                                service.PushSample(sample);
                            }
                            catch (FaultException<ValidationFault> ex)
                            {
                                Console.WriteLine($"Validation error: {ex.Detail.Message} for sample at {sample.DateTime}");
                            }
                        }

                        Console.WriteLine(service.EndSession());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška prilikom rada sa servisom: {ex.Message}");
                    }
                } // proxy.Dispose() automatski poziva Close()
            } // factory.Dispose()
        }
    }
}
