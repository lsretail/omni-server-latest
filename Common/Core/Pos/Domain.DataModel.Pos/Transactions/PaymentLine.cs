using LSRetail.Omni.Domain.DataModel.Pos.Payments;

using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
	public class PaymentLine : BaseLine
	{
		[DataMember]
		public Payment Payment { get; private set; }

		public PaymentLine() : this(0)
		{
			Payment = new Payment();
		}

		public PaymentLine(int lineNumber) : base(lineNumber)
		{
			Payment = new Payment();
		}

		public PaymentLine(int lineNumber, Payment payment) : base(lineNumber)
		{
			Payment = payment;
		}

		public PaymentLine(int lineNumber, Payment payment, bool voided) : base(lineNumber)
		{
			Payment = payment;
			Voided = voided;
		}
	}
}
