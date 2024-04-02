using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum AddressType
    {
        [EnumMember]
        Residential = 0,
        [EnumMember]
        Commercial = 1,
        [EnumMember]
        Store = 2,
        [EnumMember]
        Shipping = 3,
        [EnumMember]
        Billing = 4,
        [EnumMember]
        Work = 5,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/POS/2014/10")]
    public enum SendEmail
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        Yes = 1,
        [EnumMember]
        No = 2
    }
}
