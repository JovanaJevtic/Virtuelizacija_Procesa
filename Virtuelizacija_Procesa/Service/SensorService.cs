using Common;
using Common.Faults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SensorService : ISensorService
    {
        public string StartSession(SessionMeta meta)
        {
            if (meta == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("SessionMeta cannot be null."));

            if (string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException<ValidationFault>(
                    new ValidationFault("SessionId is required."));

            Console.WriteLine($"Session {meta.SessionId} started at {meta.StartTime}");
            return "ACK: Session started";
        }

        public string PushSample(SensorSample sample)
        {

            if (sample == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("SensorSample cannot be null."));

            if (sample.Pressure <= 0)
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Pressure must be greater than 0."));

            if (sample.Volume < 0)
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Volume cannot be negative."));

            if (sample.DateTime == default)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Sample DateTime is missing or invalid."));
            //fali provjera za T dht i T dht, ne znam koji interval da stavim
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
