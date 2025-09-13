using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
            //izmjeniti
            var sample = new SensorSample
            {
                Volume = 42.5,
                T_DHT = 24.3,
                T_BMP = 23.9,
                Pressure = 1013.25,
                DateTime = DateTime.Now
            };
            Console.WriteLine(proxy.PushSample(sample));

            Console.WriteLine(proxy.EndSession());

            Console.ReadKey();
        }
    }
}
