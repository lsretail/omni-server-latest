using System;
using System.ServiceModel;

using NLog;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.Service
{

    public class LoyService : LSOmniBase, ILoyService
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
                logger.Log(LogLevel.Error, lEx.Message, lEx);
                throw new FaultException(lEx.Message, new FaultCode(lEx.StatusCode.ToString()));
            }
            else
            {
                logger.Log(LogLevel.Error, ex, errMsg);
                //FaultException allows me to send a faultcode back to client
                throw new FaultException(errMsg + " - " + ex.Message, new FaultCode(Convert.ToInt32(StatusCode.Error).ToString()));
            }
            //System.Xml.XmlQualifiedName qname = new System.Xml.XmlQualifiedName("lsretail");
            //System.Web.Services.Protocols.SoapException sex = new System.Web.Services.Protocols.SoapException(ex.Message, qname, ex);
            //throw sex;

            //on client side 
            /*
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(System.Web.Services.Protocols.SoapHeaderException))
                    {
                        System.Web.Services.Protocols.SoapHeaderException lEx = (System.Web.Services.Protocols.SoapHeaderException)ex;
                        string faultCode = lEx.Code.Name;
                        string errorMsg = lEx.Message;
                    }
                    string x = ex.Message;
                }
             */
        }
        #endregion protected members 
    }

}
