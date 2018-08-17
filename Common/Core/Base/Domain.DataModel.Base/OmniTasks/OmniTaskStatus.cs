using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.OmniTasks
{
	[Flags]
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum OmniTaskStatus
	{
        [EnumMember]
		None = 0,
	    [EnumMember]
        UnAssigned = 1,
	    [EnumMember]
        Assigned = 2,
	    [EnumMember]
        InProgress = 4,
	    [EnumMember]
        Done = 8,
	    [EnumMember]
        Cancelled = 16,
	    [EnumMember]
        Rejected = 32,
	    [EnumMember]
        Deleted = 64
	}
}
