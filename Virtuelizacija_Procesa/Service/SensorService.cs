using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SensorService : ISensorService
    {
        public string StartSession(SessionMeta meta)
        {
            Console.WriteLine($"Session {meta.SessionId} started at {meta.StartTime}");
            return "ACK: Session started";
        }

        public string PushSample(SensorSample sample)
        {
            Console.WriteLine($"Sample received: V={sample.Volume}, T_DHT={sample.T_DHT}, T_BMP={sample.T_BMP}, P={sample.Pressure}, Time={sample.DateTime}");
            return "ACK: Sample received";
        }

        public string EndSession()
        {
            Console.WriteLine("Session ended.");
            return "COMPLETED";
        }
    }
}
