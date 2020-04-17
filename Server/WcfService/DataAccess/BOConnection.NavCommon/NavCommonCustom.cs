using System;

namespace LSOmni.DataAccess.BOConnection.NavCommon
{
    public partial class NavCommonBase
    {
        public virtual string MyCustomFunction(string data)
        {
            // TODO: Here you put the code to access BC or NAV WS
            // For NAV WS v1, existing Data Mapping is done under XmlMapping folder
            // For NAV WS v2, existing Data Mapping is done under Mapping folder
            return "My return data + Incoming data: " + data;
        }
    }
}
