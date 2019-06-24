using System;

using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    public class NavCustom : NavBase, ICustomBO
    {
        public NavCustom(BOConfiguration config) : base(config)
        {
        }

        public virtual string MyCustomFunction(string data)
        {
            // TODO: Here you put the code to access NAV or call NAV WS
            // For NAV WS v1, existing Data Mapping is done under XmlMapping folder
            // For NAV WS v2, existing Data Mapping is done under Mapping folder
            // For NAV Direct Database access, code is under Dal Folder
            return "My return data + Incoming data: " + data;
        }
    }
}
