using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Currencies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class StoreTenderTypeCurrency : Entity, IAggregateRoot
	{
        //Foreign currencies allow to use as tender type
	    [DataMember]
        public string StoreId { get; set; }
	    [DataMember]
        public string TenderTypeId { get; set; }
	    [DataMember]
        public string Description { get; set; }

        public StoreTenderTypeCurrency ()
		{
			this.StoreId = string.Empty;
			this.TenderTypeId = string.Empty;
			this.Description = string.Empty;
		}

		public StoreTenderTypeCurrency (string currencyCode) : base(currencyCode)
		{
			this.StoreId = string.Empty;
			this.TenderTypeId = string.Empty;
			this.Description = string.Empty;
		}
	}
}

