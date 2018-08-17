using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum AddressType
    {
        [EnumMember]
        Residential = 0,
        [EnumMember]
        Commercial = 1,
        [EnumMember]
        Store = 2,
        [EnumMember]
        Shipping = 3,
        [EnumMember]
        Billing = 4,
        [EnumMember]
        Work = 5,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Address : IDisposable
    {
        public Address(string id)
        {
            Id = id;
            Type = AddressType.Residential;
            Address1 = string.Empty;
            Address2 = string.Empty;
            City = string.Empty;
            PostCode = string.Empty;
            StateProvinceRegion = string.Empty;
            Country = string.Empty;
        }

        public Address() : this(string.Empty)
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
        public AddressType Type { get; set; }
        [DataMember]
        public string Address1 { get; set; }  //Address line 1, street address, po box, company name, c/o
        [DataMember]
        public string Address2 { get; set; }  //Address line 2, Appartment, suite, unit, floor etc
        [DataMember]
        public string HouseNo { get; set; }  
        [DataMember]
        public string City { get; set; }
        [DataMember]
        //public string County { get; set; }
        public string PostCode { get; set; }
        [DataMember]
        public string StateProvinceRegion { get; set; }  //State/Province/Region
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string CellPhoneNumber { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }

        public string FormatAddress
        {
            get
            {
                string address = Address1;

                if (string.IsNullOrEmpty(Address2) == false)
                    address += Environment.NewLine + Address2;

                address += Environment.NewLine + City;

                if (string.IsNullOrEmpty(StateProvinceRegion) == false)
                    address += ", " + StateProvinceRegion;

                if (string.IsNullOrEmpty(PostCode) == false)
                    address += ", " + PostCode; 
                
                if (string.IsNullOrEmpty(Country) == false)
                    address += Environment.NewLine + Country;

                return address;
            }
        }

        public override string ToString()
        {
            string s = string.Format("Id: {0} Type: {1} Address1: {2} Address2: {3} City: {4}  PostCode: {5}  StateProvinceRegion: {6}  Country: {7} ",
                Id, Type.ToString(), Address1, Address2, City, PostCode, StateProvinceRegion, Country);
            return s;
        }
    }
}

/* UK
 * Your table would have the following fields
ID .........Region Type .......Region Name  
1 ..........Country ...........UK ......... 
2 ..........Region ............England ... 
3 ..........County ............Berkshire .. 
4 ..........Town ..............Slough ... 
5 ..........Postal Area .......SL1 ....... 
*/
