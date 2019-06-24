using System;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.BLL
{
    public class CustomBLL : BaseBLL
    {
        private static LSLogger logger = new LSLogger();
        private ICustomBO iBOConnection = null;

        public CustomBLL(BOConfiguration config) : base(config)
        {
            iBOConnection = GetDbRepository<ICustomBO>(config);
        }

        public virtual string MyCustomFunction(string data)
        {
            logger.Debug(config.LSKey.Key, "Debug data: " + data);
            return iBOConnection.MyCustomFunction(data);
        }
    }
}
