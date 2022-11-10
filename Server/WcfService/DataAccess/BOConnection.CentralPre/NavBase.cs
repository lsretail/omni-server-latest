using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.PreCommon;

namespace LSOmni.DataAccess.BOConnection.CentralPre
{
    //Navision back office connection
    public class NavBase
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
        protected static BOConfiguration config = null;

        public static Version LSCVersion = null; //use this in code to check Nav version
        public static PreCommonBase LSCentralWSBase = null;

        public NavBase(BOConfiguration configuration)
        {
            config = configuration;

            LSCentralWSBase = new PreCommonBase(configuration);
            LSCVersion = LSCentralWSBase.LSCVersion;
        }
    }
}
