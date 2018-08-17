using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class SlipPayment : Entity, IDisposable
    {
        /// <summary>
        /// Base64 string representation of the signature image
        /// </summary>
        /// <value>The image.</value>
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public string EFTCardNumber { get; set; }
        [DataMember]
        public string EFTCardName { get; set; }
        [DataMember]
        public string EFTAuthCode { get; set; }
        [DataMember]
        public string EFTMessage { get; set; }
        [DataMember]
        public string Amount { get; set; }

        public SlipPayment()
        {
            Image = "";
            EFTCardNumber = "";
            EFTCardName = "";
            EFTAuthCode = "";
            EFTMessage = "";
            Amount = "";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
