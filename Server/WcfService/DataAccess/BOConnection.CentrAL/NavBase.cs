using System;
using System.Net;
using System.Net.Security;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.DataAccess.BOConnection.NavCommon;

namespace LSOmni.DataAccess.BOConnection.CentrAL
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
            }
        }

        protected static LSLogger logger = new LSLogger();
        protected static BOConfiguration config = null;

        public static Version NAVVersion = null; //use this in code to check Nav version
        public static NavCommonBase NavWSBase = null;

        public NavBase(BOConfiguration configuration)
        {
            config = configuration;

            NavWSBase = new NavCommonBase(configuration);
            NAVVersion = NavWSBase.NAVVersion;
        }
    }
}
