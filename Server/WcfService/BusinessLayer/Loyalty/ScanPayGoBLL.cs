using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class ScanPayGoBLL : BaseLoyBLL
    {
        public ScanPayGoBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual ClientToken PaymentClientTokenGet(string customerId)
        {
            return new ClientToken()
            {
                CustomerId = customerId,
                Token = String.Empty
            };
        }

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo, Statistics stat)
        {
            return BOLoyConnection.ScanPayGoProfileGet(profileId, storeNo, stat);
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo, Statistics stat)
        {
            return BOLoyConnection.SecurityCheckProfile(orderNo, storeNo, stat);
        }

        public virtual bool SecurityCheckLogResponse(string orderNo, string validationError, bool validationSuccessful, Statistics stat)
        {
            return BOLoyConnection.SecurityCheckLogResponse(orderNo, validationError, validationSuccessful, stat);
        }

        public virtual ScanPayGoSecurityLog SecurityCheckLog(string orderNo, Statistics stat)
        {
            return BOLoyConnection.SecurityCheckLog(orderNo, stat);
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering, Statistics stat)
        {
            return BOLoyConnection.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping, isEntering, stat);
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId, Statistics stat)
        {
            return BOLoyConnection.ScanPayGoOrderCheck(documentId, stat);
        }

        public virtual bool TokenEntrySet(ClientToken token, bool deleteToken, Statistics stat)
        {
            return BOLoyConnection.TokenEntrySet(token, deleteToken, stat);
        }

        public virtual List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken, Statistics stat)
        {
            return BOLoyConnection.TokenEntryGet(accountNo, hotelToken, stat);
        }

        public virtual async Task<bool> GetAuthPaymentCodeAsync(string authorizationCode, Statistics stat)
        {
            //use empty string for store to get defaul profile
            ScanPayGoProfile profile = ScanPayGoProfileGet(string.Empty, string.Empty, stat);

            logger.Debug(config.LSKey.Key, $"Check payment Auth:[{authorizationCode}], PayMeth:{profile.Flags.GetFlagInt(FeatureFlagName.CardPaymentMethod, 0)} LSPayService:{profile.Flags.GetFlagString(FeatureFlagName.LsPayServiceIpAddress, string.Empty)} Port:{profile.Flags.GetFlagString(FeatureFlagName.LsPayServicePort, string.Empty)}");

            if ((ScanPayGoCardPaymentMethod)profile.Flags.GetFlagInt(FeatureFlagName.CardPaymentMethod, 0) == ScanPayGoCardPaymentMethod.LsPay)
            {
                try
                {
                    /*
                    LSPayClient client = new LSPayClient(profile.Flags.GetFlagString(FeatureFlagName.LsPayServiceIpAddress, string.Empty), profile.Flags.GetFlagString(FeatureFlagName.LsPayServicePort, string.Empty));
                    await client.SelectAsync(profile.Flags.GetFlagString(FeatureFlagName.LsPayPluginId, string.Empty));

                    LSPay.Domain.EFT.TransactionIdentification eft = new LSPay.Domain.EFT.TransactionIdentification()
                    {
                        TransactionId = authorizationCode
                    };

                    var payment = new LSPay.Domain.ECOM.Payment()
                    {
                        TransactionID = authorizationCode
                    };

                    var checkoutResponse = await client.ECOM.GetTransactionAsync(payment);
                    if (checkoutResponse != null && checkoutResponse.Status == Status.Success)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    */
                }
                //catch any exception so the correct status code goes to the app
                catch (Exception)
                {
                    return false;
                }
            }

            //if flag isnt set to LS Pay, approve
            return true;
        }
    }
}
