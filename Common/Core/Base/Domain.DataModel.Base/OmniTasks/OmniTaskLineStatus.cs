using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.OmniTasks
{
	[Flags]
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum OmniTaskLineStatus
	{
        [EnumMember]
		None = 0,
	    [EnumMember]
        Found = 1,
	    [EnumMember]
        NotFound = 2,
	    [EnumMember]
        Cancelled = 4,
	    [EnumMember]
        Deleted = 8
	}
}
