using System;
using System.ServiceModel;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UC" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select UC.svc or UC.svc.cs at the Solution Explorer and start debugging.
    public class PortalService : LSOmniBase, IPortalService
    {
        private static LSLogger logger = new LSLogger();

        #region protected members

        protected override void HandleExceptions(Exception ex, string errMsg)
        {
            logger.Error(config.LSKey.Key, ex, errMsg);
            string errtext = errMsg + " > " + ex.Message;
            if (ex.InnerException != null)
                errtext += " > " + ex.InnerException.Message;

            if (ex.GetType() == typeof(LSOmniServiceException))
            {
                LSOmniServiceException lEx = (LSOmniServiceException)ex;
                throw new FaultException(errtext, new FaultCode(lEx.StatusCode.ToString()));
            }
            if (ex.GetType() == typeof(LSOmniException))
            {
                LSOmniException lEx = (LSOmniException)ex;
                throw new FaultException(errtext, new FaultCode(lEx.StatusCode.ToString()));
            }
            throw new FaultException(errtext, new FaultCode("Error"));
        }

        #endregion protected members 
    }
}
