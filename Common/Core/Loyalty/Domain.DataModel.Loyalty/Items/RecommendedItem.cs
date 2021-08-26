using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
	public class RecommendedItem
	{
		public RecommendedItem()
		{
			itemNo = string.Empty;
		}

		[DataMember]
		public string itemNo { get; set; }
		[DataMember]
		public decimal lift { get; set; }
	}
}
