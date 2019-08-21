using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class TenantSetting
    {
        public TenantSetting(string key, string value, string comment, string dataType, bool advanced, bool isDefault)
        {
            Key = key;
            Value = value;
            Comment = comment;
            DataType = dataType;
            Advanced = advanced;
            IsDefault = isDefault;
        }

        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string DataType { get; set; }
        [DataMember]
        public bool Advanced { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
    }
}
