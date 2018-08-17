using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class TenderPaymentLine : Entity, IDisposable
    {
        public TenderPaymentLine(string id) : base(id)
        {
            LineNumber = 1;
            Type = TenderType.Cash;
            CurrencyCode = string.Empty;
            Amount = 0.0M;
            CardType = string.Empty;
            CardOrCustNumber = string.Empty;
            EFTCardNumber = string.Empty;
            EFTCardName = string.Empty;
            EFTAuthCode = string.Empty;
            EFTCaptureCode = string.Empty;
            EFTToken = string.Empty;
            EFTMessage = string.Empty;
            Points = 0.0M;    //for tendertype points only
#if WCFSERVER
            //this is the tender type id sent to NAV, I need to map the TenderType coming from devices to this int
            //reading from AppSetting table the mappings between Nav and LSOmni!
            TenderTypeId = Convert.ToInt32(Type); 
#endif
        }

        public TenderPaymentLine()
            : this(string.Empty)
        {
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

        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string CardOrCustNumber { get; set; }
        [DataMember]
        public TenderType Type { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public decimal Points { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public string CardType { get; set; }
        [DataMember]
        public string EFTCardNumber { get; set; }
        [DataMember]
        public string EFTCardName { get; set; }
        [DataMember]
        public string EFTAuthCode { get; set; }
        [DataMember]
        public string EFTCaptureCode { get; set; }
        [DataMember]
        public string EFTToken { get; set; }
        [DataMember]
        public string EFTMessage { get; set; }
#if WCFSERVER
        /// <summary>
        /// Tender Type Mapping
        /// </summary>
        /// <remarks>
        /// <p>Tender Type Mapping from Omni TenderType Enum to NAV Tender IDs</p>
        /// Mapping setup is stored in AppSettings Table in Omni Database<br/>
        /// Key = "TenderType_Mapping"<br/>
        /// Default Mapping Value "0=1,1=3,2=10,3=11"
        /// </remarks>
        public int TenderTypeId { get; set; }
#endif
        
    }
}
 