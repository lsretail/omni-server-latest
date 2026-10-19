using System;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.PreCommon;

namespace LSOmni.DataAccess.BOConnection.CentralPre
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
                LSCentralWSBase.TimeOutInSeconds = TimeoutSec;
            }
        }

        protected static LSLogger logger = new LSLogger();
        protected BOConfiguration config = null;

        public Version LSCVersion = null; //use this in code to check Nav version
        public PreCommonBase LSCentralWSBase = null;

        public NavBase(BOConfiguration configuration)
        {
            config = configuration;

            LSCentralWSBase = new PreCommonBase(configuration);
            LSCVersion = LSCentralWSBase.LSCVersion;
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
                LSCentralWSBase?.Dispose();
            disposed = true;
        }
    }
}
