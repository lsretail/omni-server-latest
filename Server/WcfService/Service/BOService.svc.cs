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
