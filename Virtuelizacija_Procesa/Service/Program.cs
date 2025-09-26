using System;
using System.Configuration;
using System.ServiceModel;

namespace Service
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(SensorService));
            host.Open();
            Console.WriteLine("SensorService je pokrenut...");
            Console.WriteLine("Pritisnite bilo koji taster za zaustavljanje.");
            Console.ReadKey();
            Console.WriteLine("SensorService je zaustavljen.");
            host.Close();
        }
    }
}
