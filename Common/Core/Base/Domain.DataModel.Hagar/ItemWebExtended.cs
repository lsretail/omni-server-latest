using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Hagar
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Hagar/2021")]
    public class ReplHagarItemWebExtResponse : IDisposable
    {
        public ReplHagarItemWebExtResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            WebExtension = new List<ReplItemWebExtended>();
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
                if (WebExtension != null)
                    WebExtension.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemWebExtended> WebExtension { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Hagar/2021")]
    public class ReplItemWebExtended
    {
        public ReplItemWebExtended()
        {
            ItemId = string.Empty;
            VariantId = string.Empty;
            Information = string.Empty;
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public WebExtDataType DataType { get; set; }
        [DataMember]
        public string Information { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Hagar/2021")]
    public enum WebExtDataType
    {
        [EnumMember]
        Empty,
        [EnumMember]
        Ingredients,
        [EnumMember]
        HowToUse,
        [EnumMember]
        WebShortDescription,
        [EnumMember]
        WebDetailedDescription,
        [EnumMember]
        SearchWords,
        [EnumMember]
        WebDescription
    }
}
