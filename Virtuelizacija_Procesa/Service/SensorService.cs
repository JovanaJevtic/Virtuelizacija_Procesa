using Common;
using Common.Faults;
using System;
using System.Configuration;
using System.ServiceModel;

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

        public void PushSample(SensorSample sample)
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
            if (sample.T_DHT < 0 || sample.T_DHT > 50)
                throw new FaultException<ValidationFault>(
                    new ValidationFault("T_DHT out of expected range"));

            if (sample.T_BMP < 0 || sample.T_BMP > 50)
                throw new FaultException<ValidationFault>(
                    new ValidationFault("T_BMP out of expected range"));

          //  Console.WriteLine($"Sample received: V={sample.Volume}, T_DHT={sample.T_DHT}, T_BMP={sample.T_BMP}, P={sample.Pressure}, Time={sample.DateTime}");
           // return "ACK: Sample received";
        }

        public string EndSession()
        {
            Console.WriteLine("Session ended.");
            return "COMPLETED";
        }
    }
}
