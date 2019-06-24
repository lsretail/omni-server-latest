using System;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class ActivityLogBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private string ipAddress;

        public ActivityLogBLL(string deviceId, string ipAddress, int timeoutInSeconds)
            : base(null, deviceId, timeoutInSeconds)
        {
            this.ipAddress = ipAddress;
        }

        public virtual bool Save(ActivityLog activityLog)
        {
            //if no ad id is given, get one
            if (activityLog == null)
                throw new ApplicationException("activityLog cannot be null");

            try
            {
                activityLog.DeviceId = base.DeviceId;
                activityLog.IPAddress = this.ipAddress;
                logger.Debug(config.LSKey.Key, base.DeviceId + " " + activityLog.ContactId + " " + activityLog.Value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        #region private


        #endregion private
    }
}
