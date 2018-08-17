using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class TextModifier : Modifier, IDisposable
    {
        public TextModifier(string id) : base(id)
        {
        }

        public TextModifier() : this(string.Empty)
        {
        }

        public new TextModifier Clone()
        {
            return base.Clone() as TextModifier;
        }
    }
}
