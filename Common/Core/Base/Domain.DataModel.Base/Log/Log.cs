﻿using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Log
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum LogLevel
    {
        [EnumMember]
        Info = 0,
        [EnumMember]
        Warning = 1,
        [EnumMember]
        Error = 2,
        [EnumMember]
        Debug = 3,
        [EnumMember]
        Trace = 4
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum LogType
    {
        [EnumMember]
        All = -1,
        [EnumMember]
        App = 0,
        [EnumMember]
        ExternalAccessory = 1,
        [EnumMember]
        Pos = 2,
        [EnumMember]
        Console = 3,
        [EnumMember]
        WebService = 4,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Log
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public LogLevel LogLevel { get; set; }
        [DataMember]
        public LogType LogType { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Stacktrace { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeStamp { get; set; }

        public Log() : this(0)
        {
        }

        public Log(int id)
        {
            ID = id;
            LogLevel = LogLevel.Info;
            LogType = LogType.App;
            Message = string.Empty;
            Stacktrace = string.Empty;
            TimeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            var log = TimeStamp.ToString("G") + " - " + LogLevel + " - " + Message;

            if (!string.IsNullOrEmpty(Stacktrace))
            {
                log += Environment.NewLine + Stacktrace;
            }

            return log;
        }
    }
}
