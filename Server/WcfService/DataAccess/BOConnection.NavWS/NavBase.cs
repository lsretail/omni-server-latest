using System;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.NavCommon;
using LSOmni.DataAccess.BOConnection.PreCommon;

namespace LSOmni.DataAccess.BOConnection.NavWS
{
    //Navision back office connection
    public class NavBase : IDisposable
    {
        private int TimeoutSec = 0;
        protected int TimeOutInSeconds
        {
            get
            {
                return TimeoutSec;
            }
            set
            {
                TimeoutSec = value;
                NavWSBase.TimeOutInSeconds = TimeoutSec;
                LSCWSBase.TimeOutInSeconds = TimeoutSec;
            }
        }

        protected static LSLogger logger = new LSLogger();
        protected BOConfiguration config = null;

        public Version NAVVersion = null; //use this in code to check Nav version
        public NavCommonBase NavWSBase = null;
        public PreCommonBase LSCWSBase = null;

        public NavBase(BOConfiguration configuration, bool ping = false)
        {
            if (configuration == null && !ping)
            {
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "SecurityToken invalid");
            }
            config = configuration;

            NavWSBase = new NavCommonBase(configuration);
            LSCWSBase = new PreCommonBase(configuration);
            NAVVersion = LSCWSBase.LSCVersion;
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                NavWSBase?.Dispose();
                LSCWSBase?.Dispose();
            }
            disposed = true;
        }
    }
}
