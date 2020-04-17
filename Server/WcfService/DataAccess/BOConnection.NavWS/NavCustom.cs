using System;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavWS
{
    public class NavCustom : NavBase, ICustomBO
    {
        public NavCustom(BOConfiguration config) : base(config)
        {
        }

        public virtual string MyCustomFunction(string data)
        {
            // using Web Service Lookup
            return NavWSBase.MyCustomFunction(data);
        }
    }
}
