using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    //0,1,2  standard, mobile, eCommerce like in post sale order
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum Platform
    {
        /// <summary>
        /// Standard, in store sale
        /// </summary>
        [EnumMember]
        Standard = 0,
        /// <summary>
        /// Mobile sale (Loyalty)
        /// </summary>
        [EnumMember]
        Mobile = 1,
        /// <summary>
        /// eCommerce sale
        /// </summary>
        [EnumMember]
        eCommerce = 1,
        /// <summary>
        /// Unknown
        /// </summary>
        [EnumMember]
        Unknown = 10,      
    }
}
 