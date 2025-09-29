using Common;
using Common.Faults;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("=====================================");
                Console.WriteLine("   Kancelarijski senzorski sistem");
                Console.WriteLine("=====================================");
                Console.WriteLine("1. Pokreni prenos podataka");
                Console.WriteLine("2. Testiraj Dispose pattern (simuliraj prekid)");
                Console.WriteLine("0. Izlaz");
                Console.Write("Izbor: ");
                string izbor = Console.ReadLine();

                switch (izbor)
                {
                    case "1":
                        PokreniPrenos();
                        break;
                    case "2":
                        TestirajDispose();
                        break;
                    case "0":
                        Console.WriteLine("Izlaz iz aplikacije...");
                        return;
                    default:
                        Console.WriteLine("Nepoznata opcija, pokušaj ponovo.");
                        break;
                }

                Console.WriteLine("\nPritisni Enter za povratak u meni...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        // Opcija 1 – standardan prenos CSV podataka
        private static void PokreniPrenos()
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
                                Thread.Sleep(100);
                            }
                            catch (FaultException<ValidationFault> ex)
                            {
                                Console.WriteLine($"Validation error: {ex.Detail.Message} for sample at {sample.DateTime}");
                           
                            }
                            catch (FaultException<DataFormatFault> ex)
                            {
                                Console.WriteLine($"[DataFormat] {ex.Detail.Message} for sample at {sample.DateTime}");
                              
                            }
                        }

                        Console.WriteLine(service.EndSession());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška prilikom rada sa servisom: {ex.Message}");
                    }
                }
            }
        }

        // Opcija 2 – testiranje Dispose kroz simulaciju prekida prenosa
        private static void TestirajDispose()
        {
            Console.WriteLine("Testiranje Dispose pattern-a (simulacija prekida veze)...");

            try
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
                        ISensorService service = proxy as ISensorService;

                        var meta = new SessionMeta
                        {
                            SessionId = Guid.NewGuid().ToString(),
                            StartTime = DateTime.Now
                        };

                        Console.WriteLine(service.StartSession(meta));

                        CsvLoader loader = new CsvLoader();
                        var samples = loader.LoadCsv(out List<string> invalidRows, 100);

                        int counter = 0;
                        foreach (var sample in samples)
                        {
                            if (counter == 10)
                            {
                                Console.WriteLine("Simulacija: gubitak konekcije/usred prenosa...");
                                throw new Exception("Simulirani prekid veze");
                            }

                            service.PushSample(sample);
                            Thread.Sleep(100);
                            counter++;
                        }

                        Console.WriteLine(service.EndSession());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Izuzetak uhvaćen: {ex.Message}");
                Console.WriteLine("Provjera: resursi su automatski zatvoreni zahvaljujući Dispose implementaciji.");
            }
        }
    }
}