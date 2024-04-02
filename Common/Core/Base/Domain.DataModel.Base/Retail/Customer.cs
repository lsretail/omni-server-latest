using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Customer : Entity, IAggregateRoot
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Currency Currency { get; set; }
        [DataMember]
        public bool IsBlocked { get; set; }
        [DataMember]
        public Address Address { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int InclTax { get; set; }
        [DataMember]
        public string TaxGroup { get; set; }
        [DataMember]
        public string PriceGroup { get; set; }
        [DataMember]
        public string DiscountGroup { get; set; }
        [DataMember]
        public string PaymentTerms { get; set; }
        [DataMember]
        public string ShippingLocation { get; set; }
        [DataMember]
        public int ReceiptOption { get; set; }
        [DataMember]
        public string ReceiptEmail { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string NamePrefix { get; set; }
        [DataMember]
        public string NameSuffix { get; set; }

        public Customer()
            : this(null)
        {
        }

        public Customer(string id)
            : base(id)
        {
            Name = string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if ((obj is Customer) == false)
                return false;

            if ((obj as Customer).Id != Id)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum CustomerSearchType
    {
        [EnumMember]
        CustomerId = 0,
        [EnumMember]
        PhoneNumber = 1,
        [EnumMember]
        Email = 2,
        [EnumMember]
        Name = 3,
    }
}
