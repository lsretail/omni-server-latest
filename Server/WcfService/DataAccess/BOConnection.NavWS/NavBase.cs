using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.NavCommon;
using LSOmni.DataAccess.BOConnection.PreCommon;

namespace LSOmni.DataAccess.BOConnection.NavWS
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
                NavWSBase.TimeOutInSeconds = TimeoutSec;
                LSCWSBase.TimeOutInSeconds = TimeoutSec;
            }
        }

        protected static LSLogger logger = new LSLogger();
        protected static BOConfiguration config = null;

        public static Version NAVVersion = null; //use this in code to check Nav version
        public static NavCommonBase NavWSBase = null;
        public static PreCommonBase LSCWSBase = null;

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
    }
}
