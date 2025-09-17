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
            Console.WriteLine("SensorService is running...");
            Console.WriteLine("Press any key to stop.");
            Console.ReadKey();
            host.Close();
            Console.WriteLine("SensorService stopped.");
        }
    }
}
