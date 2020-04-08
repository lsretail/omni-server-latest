using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ItemLocation : IDisposable
    {
        public ItemLocation()
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
        public string StoreId { get; set; }
        [DataMember]
        public string SectionCode { get; set; }
        [DataMember]
        public string SectionDescription { get; set; }
        [DataMember]
        public string ShelfCode { get; set; }
        [DataMember]
        public string ShelfDescription { get; set; }
    }
}
