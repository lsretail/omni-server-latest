namespace LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase
{
	public class UnknownCustomer : Customer
	{
		public UnknownCustomer (string customerId) : base(customerId)
		{
		}

        public UnknownCustomer() : this(string.Empty)
        {
        }
    }
}

