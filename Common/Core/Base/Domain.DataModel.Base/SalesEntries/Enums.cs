using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum LineType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        Payment = 1,
        [EnumMember]
        PerDiscount = 2,
        [EnumMember]
        TotalDiscount = 3,
        [EnumMember]
        IncomeExpense = 4,
        [EnumMember]
        FreeText = 5,
        [EnumMember]
        Coupon = 6,
        [EnumMember]
        Shipping = 7,
        [EnumMember]
        Unknown = 99,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum DocumentIdType
    {
        [EnumMember]
        Order = 0,
        [EnumMember]
        External = 1,
        [EnumMember]
        Receipt = 2,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum SalesEntryStatus
    {
        [EnumMember]
        Created = 0,
        [EnumMember]
        Pending = 1,
        [EnumMember]
        Processing = 2,
        [EnumMember]
        Complete = 3,
        [EnumMember]
        Canceled = 4,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum ShippingStatus
    {
        [EnumMember]
        ShippigNotRequired = 10,
        [EnumMember]
        NotYetShipped = 20,
        [EnumMember]
        PartiallyShipped = 25,
        [EnumMember]
        Shipped = 30,
        [EnumMember]
        Delivered = 40,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum PaymentStatus
    {
        [EnumMember]
        PreApproved = 10,
        [EnumMember]
        Approved = 20,
        [EnumMember]
        Posted = 25
    }
}
