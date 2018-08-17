using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class LoySaleLine : Entity
	{
		public LoySaleLine(string storeId, string terminalId, string transactionId, string id) : base(id)
		{
			TransactionId = transactionId;
			TerminalId = terminalId;
			StoreId = storeId;
			Item = null;
			Quantity = 0.0M;
			Amount = string.Empty;
			DiscountAmount = string.Empty;
		    VariantReg = null;
			Uom = null;
			NetAmount = string.Empty;
			VatAmount = string.Empty;
            Amt = 0.0M;
            NetAmt = 0.0M;
			VatAmt = 0.0M;
			DiscountAmt = 0.0M;

			ExtraInfoLines = new List<string>();
		}

        public LoySaleLine(string id)
			: this(string.Empty, string.Empty, string.Empty, id)
        {
        }

        public LoySaleLine()
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
                if (ExtraInfoLines != null)
                    ExtraInfoLines.Clear();
            }
        }

		[DataMember]
		public string TerminalId { get; set; }
		[DataMember]
		public string StoreId { get; set; }
		[DataMember]
		public string TransactionId { get; set; }
        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
		public LoyItem Item { get; set; }

        [DataMember]
		public decimal Quantity { get; set; }
        [DataMember]
        public decimal ReturnQuantity { get; set; }
        [DataMember]
		public string Amount { get; set; }
		[DataMember]
		public string DiscountAmount { get; set; }
        [DataMember]
        public string NetAmount { get; set; }
        [DataMember]
        public string VatAmount { get; set; }

        [DataMember]
        public decimal Amt { get; set; }
        [DataMember]
        public decimal NetAmt { get; set; }
        [DataMember]
        public decimal VatAmt { get; set; }
        [DataMember]
        public decimal DiscountAmt { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal NetPrice { get; set; }

        [DataMember]
        public VariantRegistration VariantReg { get; set; }
        [DataMember]
        public UnitOfMeasure Uom { get; set; }
        [DataMember]
        public List<string> ExtraInfoLines { get; set; }

        public string FormatQuantity(decimal qty)
        {
            string returnString = string.Empty;
            if (Uom == null)
                returnString += qty.ToString("N0");
            else
                returnString += qty.ToString("N" + Uom.Decimals) + " " + Uom.Description;
            return returnString;
        }
    }
}
