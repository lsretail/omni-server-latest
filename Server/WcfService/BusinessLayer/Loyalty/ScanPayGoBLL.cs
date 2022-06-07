using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class ScanPayGoBLL : BaseLoyBLL
    {
        public ScanPayGoBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public virtual ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo)
        {
            return BOLoyConnection.ScanPayGoProfileGet(profileId, storeNo);
        }

        public virtual bool SecurityCheckProfile(string orderNo, string storeNo)
        {
            return BOLoyConnection.SecurityCheckProfile(orderNo, storeNo);
        }

        public virtual string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering)
        {
            return BOLoyConnection.OpenGate(qrCode, storeNo, devLocation, memberAccount, exitWithoutShopping, isEntering);
        }

        public virtual OrderCheck ScanPayGoOrderCheck(string documentId)
        {
            return BOLoyConnection.ScanPayGoOrderCheck(documentId);
        }
    }
}
