using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    //How many tendertypes will Loyalty support. Card, points, coupons ?
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum TenderType
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

        //[EnumMember]
        //Voucher = 3,
        //[EnumMember]
        //GiftCard = 4,

        //// In LS Nav demo company 
        //[EnumMember]
        //Cash = 1,
        //[EnumMember]
        //Check = 2,
        //[EnumMember]
        //Cards = 3,
        //[EnumMember]
        //CustomerAccount = 4,
        //[EnumMember]
        //Currency = 6,
        //[EnumMember]
        //Voucher = 7,
        //[EnumMember]
        //GiftCard = 8,
        //[EnumMember]
        //TenderRemoveFloat = 9,
        //[EnumMember]
        //Coupons = 10,
        //[EnumMember]
        //MemberCardPayment = 11,
        //[EnumMember]
        //SpecialOrderDeposit = 15,
        //[EnumMember]
        //CurrencyRemoveFloat = 19,
    }
}
 