using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntryList : IDisposable
    {
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
        public string OrderId { get; set; }
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public List<SalesEntry> SalesEntries { get; set; }
        [DataMember]
        public List<SalesEntryShipment> Shipments { get; set; }
    }

    public class SalesEntryId
    {
        public string ReceiptId { get; set; }
        public string OrderId { get; set; }
    }
}
