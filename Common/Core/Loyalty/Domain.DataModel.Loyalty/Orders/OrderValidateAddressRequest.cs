using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderValidateAddressRequest : IDisposable
    {
        public OrderValidateAddressRequest()
        {
            ShipToAddress = string.Empty;
            ShipToAddress2 = string.Empty;
            ShipToCity = string.Empty;
            ShipToPostCode = string.Empty;
            ShipToCounty = string.Empty;
            ShipToCountryRegionCode = string.Empty;
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
        public string ShipToAddress { get; set; }
        [DataMember]
        public string ShipToAddress2 { get; set; }
        [DataMember]
        public string ShipToCity { get; set; }
        [DataMember]
        public string ShipToPostCode { get; set; }
        [DataMember]
        public string ShipToCounty { get; set; }
        [DataMember]
        public string ShipToCountryRegionCode { get; set; }

        public override string ToString()
        {
            string s = string.Format("ShipToAddress: {0} ShipToAddress2: {1} ShipToCity: {2} ShipToPostCode: {3} ",
                ShipToAddress, ShipToAddress2, ShipToCity, ShipToPostCode);
            return s;
        }
    }
}
