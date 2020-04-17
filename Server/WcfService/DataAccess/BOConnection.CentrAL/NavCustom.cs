using System;
using LSOmni.DataAccess.BOConnection.CentrAL.Dal;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentrAL
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
                MyCustomRepository rep = new MyCustomRepository(config, NAVVersion);
                return rep.GetMyData(data);
            }
            else
            {
                // using Web Service Lookup
                return NavWSBase.MyCustomFunction(data);
            }
        }
    }
}
