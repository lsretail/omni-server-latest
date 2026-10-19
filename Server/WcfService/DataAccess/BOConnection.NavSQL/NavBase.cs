using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.NavCommon;

namespace LSOmni.DataAccess.BOConnection.NavSQL
{
    //Navision back office connection
    public class NavBase : IDisposable
    {
        protected int TimeOutInSeconds { get; set; }

        protected static LSLogger logger = new LSLogger();
        protected BOConfiguration config = null;

        public Version NAVVersion = null; //use this in code to check Nav version
        public NavCommonBase NavWSBase = null;

        public NavBase(BOConfiguration configuration)
        {
            config = configuration;

            NavWSBase = new NavCommonBase(configuration);
            NAVVersion = NavWSBase.NAVVersion;
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
                NavWSBase?.Dispose();
            disposed = true;
        }
    }
}
