using System;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
    {
        public virtual string MyCustomFunction(string data)
        {
            // TODO: Here you put the code to access BC or NAV WS
            // Data Mapping is done under Mapping folder
            return "My return data + Incoming data: " + data;
        }
    }
}
