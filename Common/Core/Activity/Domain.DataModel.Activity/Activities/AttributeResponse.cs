using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Activity.Activities
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public class AttributeResponse : IDisposable
    {
        public AttributeResponse()
        {
            LinkField = string.Empty;
            AttributeCode = string.Empty;
            AttributeValue = string.Empty;
            AttributeValueType = string.Empty;
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
        public string LinkField { set; get; }
        [DataMember]
        public string AttributeCode { set; get; }
        [DataMember]
        public string AttributeValue { set; get; }
        [DataMember]
        public int Sequence { set; get; }
        [DataMember]
        public string AttributeValueType { set; get; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Activity/2017")]
    public enum AttributeType
    {
        [EnumMember]
        Reservation = 0,
        [EnumMember]
        Activity = 1,
        [EnumMember]
        ActivityProduct = 2,
        [EnumMember]
        Resource = 3
    }
}
