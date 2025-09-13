using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ISensorService
    {
        [OperationContract]
        string StartSession(SessionMeta meta);

        [OperationContract]
        string PushSample(SensorSample sample);

        [OperationContract]
        string EndSession();
    }
}
