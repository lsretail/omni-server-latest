using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.DiscountEngine.DataModels
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/DiscountEngine/2017")]
    public class ProactiveDiscount
    {
        [DataMember]
        public ProactiveDiscountType Type { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public decimal Percentage { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal PriceWithDiscount { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public List<string> ItemIds { get; set; }
        [DataMember]
        public string LoyaltySchemeCode { get; set; }
        [DataMember]
        public decimal MinimumQuantity { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string PopUpLine1 { get; set; }
        [DataMember]
        public string PopUpLine2 { get; set; }
    }
}
