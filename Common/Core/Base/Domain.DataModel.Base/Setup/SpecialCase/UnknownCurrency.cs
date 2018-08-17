using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class UnknownCurrency : Currency
    {
        public UnknownCurrency() : base()
        {
        }

        public UnknownCurrency(string id) : base(id)
        {
        }
    }
}
