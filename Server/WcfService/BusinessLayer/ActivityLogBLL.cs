using System;

using NLog;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.BLL.Loyalty
{
    public class ActivityLogBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string ipAddress;

        public ActivityLogBLL(string deviceId, string ipAddress, int timeoutInSeconds)
            : base("", deviceId, timeoutInSeconds)
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
                //
                logger.Log(LogLevel.Debug, base.DeviceId + " " + activityLog.ContactId + " " +  activityLog.Value);
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

 