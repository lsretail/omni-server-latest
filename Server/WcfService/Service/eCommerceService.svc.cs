using System;
using System.ServiceModel;

using NLog;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "eCommerce" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select eCommerce.svc or eCommerce.svc.cs at the Solution Explorer and start debugging.
    public class eCommerceService : LSOmniBase, IeCommerceService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region protected members

        protected override void HandleExceptions(Exception ex, string errMsg)
        {
            //handle all errors in once place
            //if InventoryException is thrown then dont mess with it, let it flow to client

            if (ex.GetType() == typeof(LSOmniServiceException))
            {
                //Authentication failed for statuses etc..
                LSOmniServiceException lEx = (LSOmniServiceException)ex;
                logger.Log(LogLevel.Error, lEx, lEx.Message);
                throw new FaultException(lEx.Message, new FaultCode(lEx.StatusCode.ToString()));
            }
            else
            {
                logger.Log(LogLevel.Error, ex, errMsg);
                //FaultException allows me to send a faultcode back to client
                throw new FaultException(errMsg + " - " + ex.Message, new FaultCode(Convert.ToInt32(StatusCode.Error).ToString()));
            }
        }

        #endregion protected members 
    }
}
