using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Exceptions
{
	public sealed class UnsupportedTransactionResponseException : Exception
	{
		public enum CauseType
		{
			General,
			TipsOrServiceCharge,
			SplitBillItemSplit,
            UnsupportedCurrency
		}

		public CauseType Cause;

		public UnsupportedTransactionResponseException(CauseType cause = CauseType.General)
			: base()
		{
			this.Cause = cause;
		}
	}
}