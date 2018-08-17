using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;
using NLog;

namespace LSOmni.BLL
{
    public class CustomBLL : BaseBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ICustomBO iBOConnection = null;

        protected ICustomBO BOCustomConnection
        {
            get
            {
                if (iBOConnection == null)
                    iBOConnection = GetBORepository<ICustomBO>();
                return iBOConnection;
            }
        }

        public virtual string MyCustomFunction(string data)
        {
            logger.Debug("Debug data: " + data);
            return BOCustomConnection.MyCustomFunction(data);
        }
    }
}
