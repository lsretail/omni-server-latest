using System;
using System.ServiceModel.Web;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.Service
{
    // LoyaltyJson returns data in Json format
    public class BOJson : LSOmniBase, IBOJson
    {
        private static LSLogger logger = new LSLogger();
        private System.Net.HttpStatusCode exStatusCode = System.Net.HttpStatusCode.RequestedRangeNotSatisfiable;//.RequestedRangeNotSatisfiable; //code=416

        public string PingGet()
        {
            return base.Ping();
        }
        public string VersionGet()
        {
            return base.Version();
        }

        #region protected members
        protected override void HandleExceptions(Exception ex, string errMsg)
        {
            //handle all errors in once place
            //if LoyaltyException is thrown then dont mess with it, let it flow to client

            if (ex.GetType() == typeof(LSOmniServiceException))
            {
                //Authentication failed for statuses etc..

                LSOmniServiceException lEx = (LSOmniServiceException)ex;
                logger.Error(config.LSKey.Key, lEx, lEx.Message);
                throw new WebFaultException<LSOmniException>(new LSOmniException(lEx.StatusCode, lEx.Message), exStatusCode);
            }
            else
            {
                logger.Error(config.LSKey.Key, ex, errMsg);
                throw new WebFaultException<LSOmniException>(new LSOmniException(StatusCode.Error, errMsg + " - " + ex.Message), exStatusCode);
            }
            //throw new System.ServiceModel.Web.WebFaultException<string>("My error description.", System.Net.HttpStatusCode.Conflict); 
            //WebOperationContext.Current.OutgoingResponse.Headers.Add("Origin", "*"); 
            //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
        }
        #endregion protected members
    }
}
