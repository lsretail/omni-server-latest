using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    //How many tendertypes will Loyalty support. Card, points, coupons ?
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum LoyTenderType
    {
        [EnumMember]
        Cash = 0,
        [EnumMember]
        Card = 1,
        [EnumMember]
        Coupon = 2,
        [EnumMember]
        Points = 3,
        [EnumMember]
        WebPay = 4,
    }
}
 