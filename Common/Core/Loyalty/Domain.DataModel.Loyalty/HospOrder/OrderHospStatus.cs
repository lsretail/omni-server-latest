﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2020")]
    public class OrderHospStatus : IDisposable
    {
        public OrderHospStatus()
        {
            ReceiptNo = string.Empty;
            KotNo = string.Empty;
            QueueCounter = string.Empty;
            Lines = new List<OrderHospStatusLine>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public string ReceiptNo { get; set; }
        [DataMember]
        public string KotNo { get; set; }
        [DataMember]
        public string QueueCounter { get;set; }
        [DataMember]
        public int EstimatedTime { get; set; }
        [DataMember]
        public KOTStatus Status { get; set; }
        [DataMember]
        public bool Confirmed { get; set; }
        [DataMember]
        public decimal ProductionTime { get; set; }
        [DataMember]
        public List<OrderHospStatusLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2020")]
    public class OrderHospStatusLine : IDisposable
    {
        public OrderHospStatusLine()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2020")]
    public enum KOTStatus
    {
        [EnumMember]
        NotSent,
        [EnumMember]
        NASError,
        [EnumMember]
        KDSError,
        [EnumMember]
        Sent,
        [EnumMember]
        Started,
        [EnumMember]
        Finished,
        [EnumMember]
        Served,
        [EnumMember]
        None,
        [EnumMember]
        Posted,
        [EnumMember]
        Voided
    }
}
