using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum ContactSearchType
    {
        [EnumMember]
        CardId = 0,
        [EnumMember]
        ContactNumber = 1,
        [EnumMember]
        PhoneNumber = 2,
        [EnumMember]
        Email = 3,
        [EnumMember]
        Name = 4,
        [EnumMember]
        UserName = 5,
    }
}
