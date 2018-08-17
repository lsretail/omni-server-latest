using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Profile : Entity, IDisposable
    {
        public Profile(string id) : base(id)
        {
            Description = string.Empty;
            DataType = ProfileDataType.Boolean;
            DefaultValue = string.Empty;
            Mandatory = false;

            ContactValue = false;
        }

        public Profile() : this("0")
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
        public string Description { get; set; }
        [DataMember]
        public ProfileDataType DataType { get; set; }
        [DataMember]
        public string DefaultValue { get; set; }
        [DataMember]
        public bool Mandatory { get; set; }
        [DataMember]
        public bool ContactValue { get; set; }

        public Profile ShallowCopy()
        {
            return (Profile)MemberwiseClone();
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum ProfileDataType
    {
        [EnumMember]
        Text = 0,
        [EnumMember]
        Integer = 1,
        [EnumMember]
        Number = 2,
        [EnumMember]
        Date = 3,
        [EnumMember]
        Boolean = 4,
    }
}
