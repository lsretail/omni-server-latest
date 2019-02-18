using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
	public class LoyTransaction : Entity, IDisposable
	{
		public LoyTransaction(string id) : base(id)
		{
			Store = null;
			Terminal = string.Empty;
			Staff = string.Empty;
			Amount = string.Empty;
			Date = null;
			SaleLines = new List<LoySaleLine>();
			TenderLines = new List<LoyTenderLine>();
			SaleLinesCount = 0;
			TenderLinesCount = 0;
			NetAmount = string.Empty;
			VatAmount = string.Empty;
			DiscountAmount = string.Empty;
            Platform = Platform.Unknown;
            Amt = 0.0M;
            NetAmt = 0.0M;
            VatAmt = 0.0M;
            DiscountAmt = 0.0M;
        }

		public LoyTransaction()
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
                if (SaleLines != null)
                    SaleLines.Clear();
                if (TenderLines != null)
                    TenderLines.Clear();
                if (Store != null)
                    Store.Dispose();
            }
        }

		public string DateToShortFormat
		{
			get
			{
				if (Date == null)
				{
					return string.Empty;
				}
				else
				{
					return Date.Value.ToString("d")/* + " - " + date.Value.ToShortTimeString()*/;
				}
			}

		}

		[DataMember]
		public Store Store { get; set; }
		[DataMember]
		public string Terminal { get; set; }
		[DataMember]
		public string Staff { get; set; }
		[DataMember]
		public DateTime? Date { get; set; }
        /// <summary>
        /// 0=Receipt Number, 1=Sales Invoice, 2=Unknown, 3=Credit Memo
        /// </summary>
        [DataMember]
        public EntryDocumentType DocumentType { get; set; }
        /// <summary>
        /// See Document Type for type of Document, if Receipt Number then number is also set in ReceiptNumber property
        /// </summary>
        [DataMember]
        public string DocumentNumber { get; set; }

        [DataMember]
		public int SaleLinesCount { get; set; }
        [DataMember]
        public int TenderLinesCount { get; set; }
        [DataMember]
		public List<LoySaleLine> SaleLines { get; set; }
		[DataMember]
		public List<LoyTenderLine> TenderLines { get; set; }
        [DataMember]
        public List<TaxLine> TaxLines { get; set; }
        [DataMember]
        public List<LoyTransactionHeader> TransactionHeaders { get; set; }
        [DataMember]
        public List<LoyTransactionFooter> TransactionFooters { get; set; }
        [DataMember]
        public Platform Platform { get; set; }

        [DataMember]
        public string Amount { get; set; }
        [DataMember]
		public string NetAmount { get; set; }
		[DataMember]
		public string VatAmount { get; set; }
	    [DataMember]
	    public string DiscountAmount { get; set; }
	    [DataMember]
	    public string ReceiptNumber { get; set; }

        [DataMember]
        public decimal Amt { get; set; }
        [DataMember]
        public decimal VatAmt { get; set; }
        [DataMember]
        public decimal NetAmt { get; set; }
        [DataMember]
        public decimal DiscountAmt { get; set; }
        [DataMember]
        public decimal TotalQty { get; set; }

        [DataMember]
        public string CardId { get; set; }
    }

    public enum EntryDocumentType
    {
        ReceiptNumber,
        SalesInvoice,
        Unknown,
        CreditMemo
    }
}
