using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum CurrencyRoundingMethod
    {
        [EnumMember]
        RoundNearest = 0,
        [EnumMember]
        RoundDown = 1,
        [EnumMember]
        RoundUp = 2
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownCurrency))]
    public class Currency : Entity
    {
        public Currency(string id) : base(id)
        {
            Prefix = string.Empty;
            Postfix = string.Empty;
            Symbol = string.Empty;
            Description = string.Empty;
            RoundOffSales = 0.01M;
            DecimalSeparator = string.Empty;
            ThousandSeparator = string.Empty;
            DecimalPlaces = 2;
            SaleRoundingMethod = CurrencyRoundingMethod.RoundNearest;
            RoundOffAmount = 0.01M;
            AmountRoundingMethod = CurrencyRoundingMethod.RoundNearest;
            Culture = string.Empty;
        }

        public Currency()
            : this(string.Empty)
        {
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

        /// <summary>
        /// currency symbol as prefix for amont
        /// </summary>
        [DataMember]
        public string Prefix { get; set; }
        /// <summary>
        /// currency symbol as postfix for amount
        /// </summary>
        [DataMember]
        public string Postfix { get; set; }
        /// <summary>
        /// $  kr. currency symbol
        /// </summary>
        [DataMember]
        public string Symbol { get; set; }
        /// <summary>
        /// 0.01 = 2 decimal places
        /// </summary>
        [DataMember]
        public decimal RoundOffSales { get; set; }
        /// <summary>
        /// 0.01 = 2 decimal places
        /// </summary>
        [DataMember]
        public decimal RoundOffAmount { get; set; }

        /// <summary>
        /// .  from region on server
        /// </summary>
        [DataMember]
        public string DecimalSeparator { get; set; }
        /// <summary>
        ///  ,  from region on server
        /// </summary>
        [DataMember]
        public string ThousandSeparator { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int DecimalPlaces { get; set; }
        [DataMember]
        public CurrencyRoundingMethod SaleRoundingMethod { get; set; }
        [DataMember]
        public CurrencyRoundingMethod AmountRoundingMethod { get; set; }
        [DataMember]
        public string Culture { get; set; } //en-US

        public string FormatDecimal(decimal dec)
        {
            return Prefix + dec.ToString("N" + DecimalPlaces) + Postfix;
        }
    }
}
 
