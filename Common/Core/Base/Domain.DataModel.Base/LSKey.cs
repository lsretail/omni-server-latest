using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class LSKey
    {
        public LSKey() : this(string.Empty)
        {
        }
        public LSKey(string lskey)
        {
            Key = lskey;
            Description = string.Empty;
            Active = true;
        }

        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool Active { get; set; }
    }
}
