using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class AdvertisementBLL : BaseLoyBLL
    {
        public AdvertisementBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id, string contactId, Statistics stat)
        {
            //call local db or BO web service. 
            List<Advertisement> ads = new List<Advertisement>();

            try
            {
                //not exist in cache so get them from BO
                ads = BOLoyConnection.AdvertisementsGetById(id, stat);
            }
            catch (LSOmniServiceException ex)
            {
                //ignore the error and return what is in the cache, if anything
                logger.Error(config.LSKey.Key, ex, "Something failed when calling BackOffice but continue...");
                throw;
            }
            return ads;
        }
    }
}
