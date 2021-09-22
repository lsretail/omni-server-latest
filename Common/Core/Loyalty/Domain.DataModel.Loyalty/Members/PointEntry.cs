using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class PointEntry : Entity, IDisposable
    {
        public PointEntry(string id) : base(id)
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
        public MemberPointSourceType SourceType { get; set; }
        [DataMember]
        public string DocumentNo { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public MemberPointEntryType EntryType { get; set; }
        [DataMember]
        public MemberPointType PointType { get; set; }
        [DataMember]
        public decimal Points { get; set; }
        [DataMember]
        public decimal RemainingPoints { get; set; }
        [DataMember]
        public DateTime ExpirationDate { get; set; }
        [DataMember]
        public string StoreNo { get; set; }
        [DataMember]
        public string StoreName { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public enum MemberPointEntryType
    {
        [EnumMember]
        Sales,
        [EnumMember]
        Redemption,
        [EnumMember]
        Expire,
        [EnumMember]
        PositiveAdjustment,
        [EnumMember]
        NegativeAdjustment,
        [EnumMember]
        TransferFrom,
        [EnumMember]
        TransferTo
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public enum MemberPointType
    {
        [EnumMember]
        AwardPoints,
        [EnumMember]
        OtherPoints
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public enum MemberPointSourceType
    {
        [EnumMember]
        POSTransaction,
        [EnumMember]
        SalesInvoice,
        [EnumMember]
        Journal,
        [EnumMember]
        CreditMemo,
        [EnumMember]
        CustomerOrder
    }
}
