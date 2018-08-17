using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.SystemSettings
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class SystemSetting : Entity
    {
        [DataMember]
        public string Value { get; set; }

        public SystemSetting()
        {
        }

        public SystemSetting(string id)
            : base(id)
        {
        }
    }
}
