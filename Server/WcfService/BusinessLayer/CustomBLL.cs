using System;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.BLL
{
    public class CustomBLL : BaseBLL
    {
        private static LSLogger logger = new LSLogger();
        private ICustomBO iBOCustom = null;

        protected ICustomBO BOCustom
        {
            get
            {
                if (iBOCustom == null)
                    iBOCustom = GetBORepository<ICustomBO>(config.LSKey.Key);
                return iBOCustom;
            }
        }

        public CustomBLL(BOConfiguration config) : base(config)
        {
        }

        public virtual string MyCustomFunction(string data)
        {
            logger.Debug(config.LSKey.Key, "Debug data: " + data);
            return BOCustom.MyCustomFunction(data);
        }
    }
}
