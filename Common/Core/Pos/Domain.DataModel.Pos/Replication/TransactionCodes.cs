using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public enum SplitType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        SplitIn2 = 1,  //Split in 2 parts    /2
        [EnumMember]
        SplitIn3 = 2,
        [EnumMember]
        SplitIn4 = 3,
        [EnumMember]
        SplitEven = 4,     //split even  ALL
        [EnumMember]
        SplitRelative = 5,  //split Relative   REL
        [EnumMember]
        SplitByAmount = 6,   //split by Amount  AMT
    }
}
