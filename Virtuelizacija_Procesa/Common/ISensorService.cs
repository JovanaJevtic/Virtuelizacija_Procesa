﻿using Common.Faults;
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
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        string StartSession(SessionMeta meta);

        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        string PushSample(SensorSample sample);

        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        string EndSession();
    }
}
