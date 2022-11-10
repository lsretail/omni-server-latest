using System;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication
{
    public abstract class BaseRepository
    {
        protected static LSLogger logger = new LSLogger();
        protected static BOConfiguration config = null;

        public BaseRepository(BOConfiguration configuration)
        {
            config = configuration;
        }
    }
}
