using System;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
	public class TransactionJournalEntry
	{
		public TransactionJournalEntry()
		{
			TransactionId = string.Empty;
			TransactionNumber = string.Empty;
			StoreId = string.Empty;
			TerminalId = string.Empty;
			ReceiptNumber = string.Empty;
			MemberCardNumber = string.Empty;
			TransactionDateTime = DateTime.MinValue;
			Amount = new Money(0M, string.Empty);
		}

		public string TransactionId { get; set; }
		public string TransactionNumber { get; set; }
		public string StoreId { get; set; }
		public string TerminalId { get; set; }
		public string ReceiptNumber { get; set; }
		public string MemberCardNumber { get; set; }
		public DateTime? TransactionDateTime { get; set; }
		public Money Amount { get; set; }
	}
}
