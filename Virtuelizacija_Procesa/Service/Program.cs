using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
