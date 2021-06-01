using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class EodReportRequest : IDisposable
    {
        public EodReportRequest()
        {
            StoreNo = string.Empty;
            TerminalNo = string.Empty;
            StaffId = string.Empty;
            IsXReport = false;
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
        public string StoreNo { get; set; }
        [DataMember]
        public string TerminalNo { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public bool IsXReport { get; set; }

        public override string ToString()
        {
            return string.Format(" StoreNo:{0} TerminalNo:{1} TransactionNo:{2} IsXReport:{3}",
                              StoreNo, TerminalNo, StaffId, IsXReport);
        }
    }
    
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class EodReportRepsonse : IDisposable
    {
        public EodReportRepsonse()
        {
            TransactionNo = 0;
            BufferIndex = 0;
            StationNo = 0;
            PageNo = 0;
            PrintedLineNo = 0;
            LineType = 0;
            HostId = string.Empty;
            ProfileId = string.Empty;
            TransactionType = 0;
            Text = string.Empty;
            Width = 0;
            Height = 0;
            BCType = string.Empty;
            BCPos = 0;
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
        public int TransactionNo { get; set; }
        [DataMember]
        public int BufferIndex { get; set; }
        [DataMember]
        public int StationNo { get; set; }
        [DataMember]
        public int PageNo { get; set; }
        [DataMember]
        public int PrintedLineNo { get; set; }
        [DataMember]
        public int LineType { get; set; }
        [DataMember]
        public string HostId { get; set; }
        [DataMember]
        public string ProfileId { get; set; }
        [DataMember]
        public int TransactionType { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public string BCType { get; set; }
        [DataMember]
        public int BCPos { get; set; }
        [DataMember]
        public bool SetBackPrinting { get; set; } 

        public override string ToString()
        {
            return string.Empty;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ShiftRequest : IDisposable
    {
        public ShiftRequest()
        {
            StoreNo = string.Empty;
            TerminalNo = string.Empty;
            StaffId = string.Empty;
            Amount = 0.0M;
            TenderTypeId = TenderTypeName.Cash;
            Value = string.Empty; //use for something running out of TIME!
            ShiftAction = ShiftAction.Unknown;
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
        public string StoreNo { get; set; }
        [DataMember]
        public string TerminalNo { get; set; }
        [DataMember]
        public string StaffId { get; set; }
        [DataMember]
        public ShiftAction ShiftAction { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public TenderTypeName TenderTypeId { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    [Flags]
    public enum ShiftAction
    {
        [EnumMember]
        FloatEntry = 0, // declare cash in register
        [EnumMember]
        TenderRemoval = 1, //remove cash from register
        [EnumMember]
        Unknown = 999,
    }
}


