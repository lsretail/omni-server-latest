using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Pos.Currencies;
using LSRetail.Omni.Domain.DataModel.Pos.Items;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
	public class IncomeExpenseLine : BaseLine
	{
		/// <summary>
		/// Initializes a new <see cref="IncomeExpenseLine"/>.
		/// </summary>
		/// <remarks>A parameterless constructor is required for xml serialization.</remarks>
		public IncomeExpenseLine()
		{ }

		// in case the eft payment is reverted, so we can void this line as well

		public IncomeExpenseLine(int lineNumber, string accountNumber, decimal amount, string tenderLineReference, Currency currency) : base(lineNumber)
		{
			this.AccountNumber = accountNumber;
			this.Amount = new Money(amount, currency);
			this.TenderLineReference = tenderLineReference;

			Voided = false;
		}

		[DataMember]
		public string AccountNumber { get; set; }
		[DataMember]
		public Money Amount { get; set; }
		[DataMember]
		public string TenderLineReference { get; set; }

		public void Void()
		{
			Voided = true;
		}

		public static IncomeExpenseLine FromRepository(
			int lineNumber,
			string accountNumber,
			decimal amount,
			string tenderLineReference,
			Currency currency,
			bool voided)
		{
			IncomeExpenseLine incomeExpenseLine = new IncomeExpenseLine(lineNumber, accountNumber, amount, tenderLineReference, currency);
			incomeExpenseLine.Voided = voided;

			return incomeExpenseLine;
		}

	}
}
