using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum OfferType
    {
        [EnumMember]
        General = 0,  // not a part of member mgt system
        [EnumMember]
        SpecialMember = 1, //member attribute based, specifically the member contact signed up for this (golf etc)
        [EnumMember]
        PointOffer = 2, //Points and Coupons, item Points and Coupons,  something you can pay with
        [EnumMember]
        Club = 3,      //Club and scheme, offer is part of member mgt, got offer since you were in club,scheme,account or contact
        [EnumMember]
        Unknown = 100,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum OfferDiscountType
    {
        [EnumMember]
        Promotion = 0,
        [EnumMember]
        Deal = 1,
        [EnumMember]
        Multibuy = 2,
        [EnumMember]
        MixAndMatch = 3,
        [EnumMember]
        DiscountOffer = 4,
        [EnumMember]
        TotalDiscount = 5,
        [EnumMember]
        TenderType = 6,
        [EnumMember]
        ItemPoint = 7,
        [EnumMember]
        LineDiscount = 8,
        [EnumMember]
        Coupon = 9,
        [EnumMember]
        Unknown = 100,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum OfferDiscountLineType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        ProductGroup = 1,
        [EnumMember]
        ItemCategory = 2,
        [EnumMember]
        All = 3,
        [EnumMember]
        SpecialGroup = 4,
        [EnumMember]
        PLUMenu,
        [EnumMember]
        DealModifier,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum OfferLineVariantType
    {
        [EnumMember]
        None,
        [EnumMember]
        Variant,
        [EnumMember]
        Dimension
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum DiscountType
    {
        //Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon  in NAV
        [EnumMember]
        PeriodicDisc = 0,
        [EnumMember]
        Customer = 1,
        [EnumMember]
        InfoCode = 2,
        [EnumMember]
        Total = 3,
        [EnumMember]
        Line = 4,
        [EnumMember]
        Promotion = 5,
        [EnumMember]
        Deal = 6,
        [EnumMember]
        TotalDiscount = 7,
        [EnumMember]
        TenderType = 8,
        [EnumMember]
        ItemPoint = 9,
        [EnumMember]
        LineDiscount = 10,
        [EnumMember]
        MemberPoint = 11,
        [EnumMember]
        Coupon = 12,
        [EnumMember]
        Unknown = 99,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum PeriodicDiscType
    {
        // ,Multibuy,Mix&Match,Disc. Offer,Item Point  in Nav
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Multibuy = 1,
        [EnumMember]
        MixMatch = 2,
        [EnumMember]
        DiscOffer = 3,
        [EnumMember]
        ItemPoint = 4,
        [EnumMember]
        LineDiscount = 5,
    }
}

