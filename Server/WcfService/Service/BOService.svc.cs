using System;
using System.ServiceModel;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.Service
{
    public class BOService : LSOmniBase, IBOService
    {
        private static LSLogger logger = new LSLogger();

        #region protected members

        protected override void HandleExceptions(Exception ex, string errMsg)
        {
            //handle all errors in once place
            //if InventoryException is thrown then dont mess with it, let it flow to client

            if (ex.GetType() == typeof(LSOmniServiceException))
            {
                //Authentication failed for statuses etc..
                LSOmniServiceException lEx = (LSOmniServiceException)ex;
                logger.Error(config.LSKey.Key, lEx, lEx.Message);
                throw new FaultException(lEx.Message, new FaultCode(lEx.StatusCode.ToString()));
            }
            else
            {
                logger.Error(config.LSKey.Key, ex, errMsg);
                //FaultException allows me to send a faultcode back to client
                throw new FaultException(errMsg + " - " + ex.Message, new FaultCode(Convert.ToInt32(StatusCode.Error).ToString()));
            }
        }

        #endregion protected members
    }
}
