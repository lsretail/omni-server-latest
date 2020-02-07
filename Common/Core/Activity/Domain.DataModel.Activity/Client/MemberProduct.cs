using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Activity.Client
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class MemberProduct : Entity, IDisposable
    {
        public MemberProduct(string id) : base(id)
        {
            Description = string.Empty;
            ChargeType = string.Empty;
            AccessType = string.Empty;
            Status = string.Empty;
            ExpirationCalculation = string.Empty;
            CommitmentPeriod = string.Empty;
            RetailItem = string.Empty;
            RequiresMemberShip = string.Empty;
            SellingOption = string.Empty;
            EntryType = string.Empty;
            SubscriptionType = string.Empty;
            MemberClub = string.Empty;
            PricingUpdate = string.Empty;
            PriceCommitmentPeriod = string.Empty;
        }

        public MemberProduct() : this(string.Empty)
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
        public string Description { get; set; }
        [DataMember]
        public string ChargeType { get; set; }
        [DataMember]
        public string AccessType { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string ExpirationCalculation { get; set; }
        [DataMember]
        public string CommitmentPeriod { get; set; }
        [DataMember]
        public int NoOfTimes { get; set; }
        [DataMember]
        public string RetailItem { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public int MinAge { get; set; }
        [DataMember]
        public int MaxAge { get; set; }
        [DataMember]
        public DateTime FixedIssueDate { get; set; }
        [DataMember]
        public DateTime FixedEndDate { get; set; }
        [DataMember]
        public string RequiresMemberShip { get; set; }
        [DataMember]
        public string SellingOption { get; set; }
        [DataMember]
        public string EntryType { get; set; }
        [DataMember]
        public string SubscriptionType { get; set; }
        [DataMember]
        public string MemberClub { get; set; }
        [DataMember]
        public string PricingUpdate { get; set; }
        [DataMember]
        public string PriceCommitmentPeriod { get; set; }
    }
}
