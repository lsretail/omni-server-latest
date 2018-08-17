using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class LoyTenderLine : Entity, IDisposable
    {
        public LoyTenderLine(string storeId, string terminalId, string transactionId, string id) : base(id)
        {
            TransactionId = transactionId;
            TerminalId = terminalId;
            StoreId = storeId;
            Type = string.Empty;
            Description = string.Empty;
            Amount = string.Empty;
            Amt = 0.0M;
        }

        public LoyTenderLine(string id)
            : this(string.Empty, string.Empty, string.Empty, id)
        {
        }

        public LoyTenderLine()
            : this(string.Empty, string.Empty, string.Empty, string.Empty)
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

        [DataMember]
        public string TerminalId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string TransactionId { get; set; }
        [DataMember]
        public string LineNo { get; set; }
        /// <summary>
        /// Omni TenderType.<p/>
        /// Default mapping to NAV: 0=Cash, 1=Card, 2=Coupon, 3=WebPay<p/>
        /// Tendertype Mapping can be modified in LSOmni Database - Appsettings table - Key:TenderType_Mapping
        /// </summary>
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Amount { get; set; }
        [DataMember]
        public decimal Amt { get; set; }
    }
}
 