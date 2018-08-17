using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Printer;

namespace LSRetail.Omni.Domain.DataModel.Pos.Payments
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReceiptInfo : IDisposable
    {
        public const string CustomerReceiptInfo = "CustomerReceiptInfo";
        public const string MerchantReceiptInfo = "MerchantReceiptInfo";

        public ReceiptInfo(string key, List<PrintLine> value)
        {
            Key = key;
            Value = value;
            Type = "Unknown"; // Type of receipt so Nav can key off it and print.. Handpoint etc
        }

        public ReceiptInfo()
            : this("", new List<PrintLine>())
        {
        }

        public ReceiptInfo(string key, string value) : this(key, new List<PrintLine>(){new PrintLine(){LineType = PrintLineType.PrintLine, Text = value}})
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
        public string Key { get; set; }

        [DataMember]
        public List<PrintLine> Value { get; set; }
        [DataMember]
        public string Type { get; set; }

        public bool IsLargeValue { get; set; }
        public string ValueAsText { get; set; }
    }
}
