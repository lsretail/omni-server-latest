using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class PrintBufferResponse : IDisposable
    {
        public PrintBufferResponse()
        {
            TransactionNo = 0;
            BufferIndex = 0;
            StationNo = 0;
            PageNo = 0;
            PrintedLineNo = 0;
            LineType = 0;
            HostId = "";
            ProfileId = "";
            TransactionType = 0;
            Text = "";
            Width = 0;
            Height = 0;
            BCType = "";
            BCPos = 0;
            SetBackPrinting = 0;
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
        public int SetBackPrinting { get; set; }

        public override string ToString()
        {
            return string.Format("StationNo: {0} TransactionNo: {1}", StationNo, TransactionNo);
        }
    }
}