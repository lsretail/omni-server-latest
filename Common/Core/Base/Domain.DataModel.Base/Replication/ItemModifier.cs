using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemModifierResponse : IDisposable
    {
        public ReplItemModifierResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Modifiers = new List<ReplItemModifier>();
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
                if (Modifiers != null)
                    Modifiers.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemModifier> Modifiers { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemModifier : IDisposable
    {
        public ReplItemModifier()
        {
            Id = string.Empty;
            Code = string.Empty;
            SubCode = string.Empty;
            TriggerCode = string.Empty;
            Description = string.Empty;
            UnitOfMeasure = string.Empty;
            VariantCode = string.Empty;
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
        public bool IsDeleted { get; set; }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string SubCode { get; set; }
        [DataMember]
        public string TriggerCode { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ExplanatoryHeaderText { get; set; }
        [DataMember]
        public string Prompt { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public string VariantCode { get; set; }

        [DataMember]
        public ItemModifierType Type { get; set; }
        [DataMember]
        public ItemUsageCategory UsageCategory { get; set; }
        [DataMember]
        public ItemTriggerFunction TriggerFunction { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        [DataMember]
        public ItemModifierPriceType PriceType { get; set; }
        [DataMember]
        public ItemModifierPriceHandling AlwaysCharge { get; set; }
        [DataMember]
        public decimal AmountPercent { get; set; }
        [DataMember]
        public decimal TimeModifierMinutes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2020")]
    public enum ItemModifierType
    {
        [EnumMember]
        Item,
        [EnumMember]
        Time,
        [EnumMember]
        Text
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2020")]
    public enum ItemUsageCategory
    {
        [EnumMember]
        Default,
        [EnumMember]
        CrossSelling,
        [EnumMember]
        ItemModifier
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2020")]
    public enum ItemTriggerFunction
    {
        [EnumMember]
        Default,
        [EnumMember]
        Item,
        [EnumMember]
        DiscountGroup,
        [EnumMember]
        RunObject,
        [EnumMember]
        VATBusPostingGroup,
        [EnumMember]
        Infocode,
        [EnumMember]
        TimeModifier,
        [EnumMember]
        TextModifier,
        [EnumMember]
        TaxAreaCode
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2020")]
    public enum ItemModifierPriceType
    {
        [EnumMember]
        None,
        [EnumMember]
        FromItem,
        [EnumMember]
        Amount,
        [EnumMember]
        Percent
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2020")]
    public enum ItemModifierPriceHandling
    {
        [EnumMember]
        None,
        [EnumMember]
        AlwaysCharge,
        [EnumMember]
        NoCharge
    }
}
