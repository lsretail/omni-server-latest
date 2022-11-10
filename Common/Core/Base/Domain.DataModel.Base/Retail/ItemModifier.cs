using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemModifier : IDisposable
    {
        public ItemModifier()
        {
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
        public int GroupMinSelection { get; set; }
        [DataMember]
        public int GroupMaxSelection { get; set; }
        [DataMember]
        public ItemModifierPriceType PriceType { get; set; }
        [DataMember]
        public ItemModifierPriceHandling AlwaysCharge { get; set; }
        [DataMember]
        public decimal AmountPercent { get; set; }
        [DataMember]
        public decimal TimeModifierMinutes { get; set; }
    }
}
