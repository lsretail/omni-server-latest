using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Pos.Currencies;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Pos.TenderTypes
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum TenderTypeFunction
    {
        [EnumMember]
        CashOrCurrency = 0,
        [EnumMember]
        Cards = 1,
        [EnumMember]
        Points = 7
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum TenderTypeName
    {
        [EnumMember]
        Cash = 0,
        [EnumMember]
        Cards = 1,
        [EnumMember]
        Currency = 2,
        [EnumMember]
        Points = 3
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017"), KnownType(typeof(UnknownTenderType))]
    public class TenderType : Entity, IAggregateRoot
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public TenderTypeFunction Function { get; set; }
        [DataMember]
        public TenderType ChangeTenderType { get; set; }
        [DataMember]
        public decimal MinimumChangeAmount { get; set; }
        [DataMember]
        public TenderType AboveMinimumChangeTenderType { get; set; }
        [DataMember]
        public bool AllowVoiding { get; set; }
        [DataMember]
        public bool AllowOverTendering { get; set; }
        [DataMember]
        public decimal MaximumOverTenderAmount { get; set; }
        [DataMember]
        public bool AllowUnderTendering { get; set; }
        [DataMember]
        public bool ReturnAllowed { get; set; }
        [DataMember]
        public bool ValidOnMobilePOS { get; set; }
        [DataMember]
        public bool ForeignCurrency { get; set; }
        [DataMember]
        public decimal RoundOff { get; set; }
        [DataMember]
        public CurrencyRoundingMethod RoundOffType { get; set; }

        public TenderType()
        {
        }

        public TenderType(string id) : base(id)
        {
        }
    }
}
