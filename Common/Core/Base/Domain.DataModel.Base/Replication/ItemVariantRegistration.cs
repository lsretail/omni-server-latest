using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemVariantRegistrationResponse : IDisposable
    {
        public ReplItemVariantRegistrationResponse(string id)
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            ItemVariantRegistrations = new List<ReplItemVariantRegistration>();
        }

        public ReplItemVariantRegistrationResponse()
            : this(string.Empty)
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
                if (ItemVariantRegistrations != null)
                    ItemVariantRegistrations.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemVariantRegistration> ItemVariantRegistrations { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemVariantRegistration : IDisposable
    {
        public ReplItemVariantRegistration()
        {
            ItemId = string.Empty;
            VariantId = string.Empty;
            FrameworkCode = string.Empty;
            VariantDimension1 = string.Empty;
            VariantDimension2 = string.Empty;
            VariantDimension3 = string.Empty;
            VariantDimension4 = string.Empty;
            VariantDimension5 = string.Empty;
            VariantDimension6 = string.Empty;
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
        public bool IsDeleted { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string FrameworkCode { get; set; }
        [DataMember]
        public string VariantDimension1 { get; set; }
        [DataMember]
        public string VariantDimension2 { get; set; }
        [DataMember]
        public string VariantDimension3 { get; set; }
        [DataMember]
        public string VariantDimension4 { get; set; }
        [DataMember]
        public string VariantDimension5 { get; set; }
        [DataMember]
        public string VariantDimension6 { get; set; }
        [DataMember]
        public int BlockedOnPos { get; set; }
        [DataMember]
        public int BlockedOnECom { get; set; }
    }
}
