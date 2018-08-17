using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LSRetail.Omni.Domain.DataModel.Pos.Items.SpecialCase
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class UnknownRetailItem : RetailItem
    {
        public UnknownRetailItem(string id) : base(id)
        {
        }
    }
}
