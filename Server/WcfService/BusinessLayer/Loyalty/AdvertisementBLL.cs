using System;
using System.Collections.Generic;

using NLog;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.BLL.Loyalty
{
    public class AdvertisementBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public AdvertisementBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
        }

        public AdvertisementBLL(int timeoutInSeconds)
            : this("", timeoutInSeconds)
        {
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id, string contactId)
        {
            //call local db or BO web service. 
            List<Advertisement> ads = new List<Advertisement>();
            

            try
            {
                //not exist in cache so get them from BO
                ads = BOLoyConnection.AdvertisementsGetById(id);
            }
            catch (LSOmniServiceException ex)
            {
                //ignore the error and return what is in the cache, if anything
                logger.Log(LogLevel.Error, "Something failed when calling BackOffice but continue...", ex);
                throw;
            }
            return ads;
        }
    }
}

 