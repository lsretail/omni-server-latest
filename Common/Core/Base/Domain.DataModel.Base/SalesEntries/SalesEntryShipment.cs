using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntryShipment : Entity, IDisposable
    {
        public SalesEntryShipment(string id) : base(id)
        {
            DocumentId = string.Empty;
            YourReference = string.Empty;
            ExternalId = string.Empty;
            TrackingID = string.Empty;
            Name = string.Empty;
            Contact = string.Empty;
            ShipmentMethodCode = string.Empty;
            AgentCode = string.Empty;
            AgentServiceCode = string.Empty;
            
            Address = new Address();
            Lines = new List<SalesEntryShipmentLine>();
        }

        public SalesEntryShipment() : this(string.Empty)
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public string DocumentId { get; set; }
        [DataMember]
        public string YourReference { get; set; }
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public string TrackingID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Contact { get; set; }
        [DataMember]
        public Address Address { get; set; }
        [DataMember]
        public DateTime ShipmentDate {  get; set; }
        [DataMember]
        public string ShipmentMethodCode { get; set; }
        [DataMember]
        public string AgentCode { get; set; }
        [DataMember]
        public string AgentServiceCode { get; set; }

        [DataMember]
        public List<SalesEntryShipmentLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntryShipmentLine : IDisposable
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
        public int LineNumber { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string UomId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }

        public string DocId { get; set; }
    }
}
