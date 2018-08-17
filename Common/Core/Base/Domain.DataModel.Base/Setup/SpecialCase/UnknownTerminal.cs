using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public sealed class UnknownTerminal : Terminal
    {
        public UnknownTerminal(string id) : base(id)
        {
            Unknown = true;
        }

        [DataMember]
        public bool Unknown { get; set; }
    }
}
