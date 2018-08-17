using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class LoyEnvironment
    {
        public LoyEnvironment()
        {
            Currency = new Currency();
            Version = string.Empty;
            PasswordPolicy = string.Empty;
        }

        [DataMember]
        public Currency Currency { get; set; }

        [DataMember]
        public string PasswordPolicy { get; set; }

        [DataMember]
        public string Version { get; set; }
    }
}