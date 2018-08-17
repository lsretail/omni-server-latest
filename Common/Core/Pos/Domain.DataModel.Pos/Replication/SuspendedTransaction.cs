using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
	public class SuspendedTransaction : IDisposable
	{
		public SuspendedTransaction(string id)
		{
			Id = id;
			ReceiptNumber = string.Empty;
			StoreId = "";
			StoreName = "";
			Terminal = string.Empty;
			Staff = string.Empty;
			Amount = 0.0M;
            Date = new DateTime(1970, 1, 1);//min json date
			NetAmount = 0.0M;
			VatAmount = 0.0M;
			DiscountAmount = 0.0M;
			CurrencyCode = string.Empty;
			SearchKey = string.Empty;
			CustomerId = string.Empty;
		}

		public SuspendedTransaction()
			: this("")
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
		public string Id { get; set; }
		[DataMember]
		public string ReceiptNumber { get; set; }
		[DataMember]
		public string SearchKey { get; set; }
		[DataMember]
		public string StoreId { get; set; }
		[DataMember]
		public string StoreName { get; set; }
		[DataMember]
		public string Terminal { get; set; }
		[DataMember]
		public string Staff { get; set; }
		[DataMember]
		public decimal Amount { get; set; }
		[DataMember]
		public DateTime Date { get; set; }
		[DataMember]
		public decimal NetAmount { get; set; }
		[DataMember]
		public decimal VatAmount { get; set; }
		[DataMember]
		public decimal DiscountAmount { get; set; }
		[DataMember]
		public string CurrencyCode { get; set; }
		[DataMember]
		public string CustomerId { get; set; }
	}
}


