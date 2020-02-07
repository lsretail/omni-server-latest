using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class Membership : Entity, IDisposable
    {
        public Membership(string id) : base(id)
        {
            ContactNo = string.Empty;
            MemberName = string.Empty;
            MembershipType = string.Empty;
            MembershipDescription = string.Empty;
            LastProcessBatch = string.Empty;
            Status = string.Empty;
            DiscountReasonCode = string.Empty;
            AccessID = string.Empty;
            SalesPersonCode = string.Empty;
            EntryType = string.Empty;
            ChargeTo = string.Empty;
            ChargeToName = string.Empty;
            StatusCode = string.Empty;
            PaymentMethodCode = string.Empty;
        }

        public Membership() : this(string.Empty)
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
        public string ContactNo { get; set; }
        [DataMember]
        public string MemberName { get; set; }
        [DataMember]
        public string MembershipType { get; set; }
        [DataMember]
        public string MembershipDescription { get; set; }
        [DataMember]
        public DateTime DateIssued { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public DateTime DateExpires { get; set; }
        [DataMember]
        public DateTime DateLastProcessed { get; set; }
        [DataMember]
        public string LastProcessBatch { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal Discount { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public DateTime CommitmentDate { get; set; }
        [DataMember]
        public string DiscountReasonCode { get; set; }
        [DataMember]
        public string AccessID { get; set; }
        [DataMember]
        public string SalesPersonCode { get; set; }
        [DataMember]
        public string EntryType { get; set; }
        [DataMember]
        public int NoOfVisits { get; set; }
        [DataMember]
        public DateTime OnHoldUntil { get; set; }
        [DataMember]
        public DateTime StatusDate { get; set; }
        [DataMember]
        public string ChargeTo { get; set; }
        [DataMember]
        public string ChargeToName { get; set; }
        [DataMember]
        public DateTime AccessFrom { get; set; }
        [DataMember]
        public DateTime AccessUntil { get; set; }
        [DataMember]
        public string StatusCode { get; set; }
        [DataMember]
        public DateTime PriceCommitmentExpires { get; set; }
        [DataMember]
        public string PaymentMethodCode { get; set; }
    }
}
