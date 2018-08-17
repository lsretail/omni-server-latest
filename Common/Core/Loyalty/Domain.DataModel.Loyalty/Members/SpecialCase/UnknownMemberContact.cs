using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class UnknownMemberContact : MemberContact
    {
        public UnknownMemberContact(string id) : base(id)
        {
        }
    }
}
