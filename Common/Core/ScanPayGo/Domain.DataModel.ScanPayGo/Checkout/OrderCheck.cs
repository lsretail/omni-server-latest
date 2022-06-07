using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class OrderCheck : IDisposable
    {
        public OrderCheck()
        {
            Lines = new List<OrderCheckLines>();
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public bool OrderPayed { get; set; }
        [DataMember]
        public bool DoCheck { get; set; }
        [DataMember]
        public int NumberOfItemsToCheck { get; set; }
        [DataMember]
        public List<OrderCheckLines> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class OrderCheckLines : IDisposable
    {
        public OrderCheckLines()
        {
            DocumentID = string.Empty;
            ItemId = string.Empty;
            ItemDescription = string.Empty;
            VariantCode = string.Empty;
            VariantDescription = string.Empty;
            UnitofMeasureCode = string.Empty;   
            UOMDescription = string.Empty;
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
        public string DocumentID;
        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantCode { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public string UnitofMeasureCode { get; set; }
        [DataMember]
        public string UOMDescription { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
    }
}