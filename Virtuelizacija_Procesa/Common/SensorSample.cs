﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class SensorSample
    {
        [DataMember] public double Volume { get; set; }
        [DataMember] public double T_DHT { get; set; }
        [DataMember] public double T_BMP { get; set; }
        [DataMember] public double Pressure { get; set; }
        [DataMember] public DateTime DateTime { get; set; }
    }
}

