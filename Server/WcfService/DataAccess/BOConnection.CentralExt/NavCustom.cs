using System;
using LSOmni.DataAccess.BOConnection.CentralExt.Dal;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralExt
{
    public class NavCustom : NavBase, ICustomBO
    {
        public NavCustom(BOConfiguration config) : base(config)
        {
        }

        public virtual string MyCustomFunction(string data)
        {
            bool usedatabase = false;

            // using database lookup
            if (usedatabase)
            {
                MyCustomRepository rep = new MyCustomRepository(config, LSCVersion);
                return rep.GetMyData(data);
            }
            else
            {
                // using Web Service Lookup
                return LSCentralWSBase.MyCustomFunction(data);
            }
        }
    }
}
