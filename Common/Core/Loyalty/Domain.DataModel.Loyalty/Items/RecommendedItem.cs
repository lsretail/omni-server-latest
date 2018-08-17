using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
	public class RecommendedItem : LoyItem
	{
		public RecommendedItem()
		{
			Reasoning = string.Empty;
			Rating = 0.0M;
		}

		[DataMember]
		public string Reasoning { get; set; }
		[DataMember]
		public decimal Rating { get; set; }
	}
}
