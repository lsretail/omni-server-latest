using System.Runtime.Serialization;

namespace LSRetail.Omni.DiscountEngine.DataModels
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/DiscountEngine/2017")]
    /// <summary>
    /// NAV "Periodic Discount" type mapping: 0 - Multibuy, 1 - Mix and Match, 2 - Disc. Offer.
    /// This enum maps perfectly to PeriodicDiscType
    /// </summary>
    public enum ProactiveDiscountType
    {
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
