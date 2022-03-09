using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using Newtonsoft.Json;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntry : Entity, IDisposable
    {
        public SalesEntry(string id) : base(id)
        {
            IdType = DocumentIdType.Order;
            Status = SalesEntryStatus.Pending;
            ShippingStatus = ShippingStatus.ShippigNotRequired;

            Lines = new List<SalesEntryLine>();
            DiscountLines = new List<SalesEntryDiscountLine>();
            Payments = new List<SalesEntryPayment>();
        }

        public SalesEntry() : this(string.Empty)
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
                if (Lines != null)
                    Lines.Clear();
                if (DiscountLines != null)
                    DiscountLines.Clear();
                if (Payments != null)
                    Payments.Clear();
            }
        }

        /// <summary>
        /// External Id for Sale
        /// </summary>
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public string CustomerOrderNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime DocumentRegTime { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string StoreName { get; set; }
        [DataMember]
        public string TerminalId { get; set; }

        /// <summary>
        /// Member Contact Card Id
        /// </summary>
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public string CustomerId { get; set; }

        [DataMember]
        public string ContactName { get; set; }
        [DataMember]
        public Address ContactAddress { get; set; }
        [DataMember]
        public string ContactEmail { get; set; }
        [DataMember]
        public string ContactDayTimePhoneNo { get; set; }

        // shipping info
        [DataMember]
        public string ShipToName { get; set; }
        [DataMember]
        public Address ShipToAddress { get; set; }
        [DataMember]
        public string ShipToEmail { get; set; }
        [DataMember]
        public DateTime RequestedDeliveryDate { get; set; }

        [DataMember]
        public DocumentIdType IdType { get; set; }
        [DataMember]
        public SalesEntryStatus Status { get; set; }
        [DataMember]
        public ShippingStatus ShippingStatus { get; set; }

        [DataMember]
        public string ShippingAgentCode { get; set; }
        [DataMember]
        public string ShippingAgentServiceCode { get; set; }

        [DataMember]
        public decimal TotalNetAmount { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
        [DataMember]
        public decimal TotalDiscount { get; set; }
        [JsonIgnore]
        public decimal VatAmount => TotalAmount - TotalNetAmount;

        [DataMember]
        public int LineItemCount { get; set; }

        /// <summary>
        /// True if SalesEntry has already been Posted in Accounting
        /// </summary>
        [DataMember]
        public bool Posted { get; set; }
        /// <summary>
        /// Sales Order or Click and Collect Order
        /// Click And Collect order are processed in the store at POS Terminal
        /// </summary>
        [DataMember]
        public bool ClickAndCollectOrder { get; set; }
        /// <summary>
        /// AnonymousOrder
        /// </summary>
        [DataMember]
        public bool AnonymousOrder { get; set; }
        /// <summary>
        /// Transaction is return sale
        /// </summary>
        [DataMember]
        public bool ReturnSale { get; set; }
        /// <summary>
        /// Transaction has return sale transaction
        /// </summary>
        [DataMember]
        public bool HasReturnSale { get; set; }

        [DataMember]
        public List<SalesEntryLine> Lines { get; set; }
        [DataMember]
        public List<SalesEntryDiscountLine> DiscountLines { get; set; }
        [DataMember]
        public List<SalesEntryPayment> Payments { get; set; }

        /// <summary>
        /// Points used as payment in order
        /// </summary>
        [DataMember]
        public decimal PointsUsedInOrder { get; set; }
        /// <summary>
        /// Points Rewarded for this SalesEntry
        /// </summary>
        [DataMember]
        public decimal PointsRewarded { get; set; }

        public override string ToString()
        {
            string req = string.Empty, osldreq = string.Empty;
            try
            {
                foreach (SalesEntryLine line in Lines)
                    req += "[" + line.ToString() + "] ";
                foreach (SalesEntryDiscountLine line in DiscountLines)
                    osldreq += "[" + line.ToString() + "] ";
            }
            catch
            {
            }

            string s = string.Format("Id: {0} StoreId: {1} CardId: {2} SourceType: {3}  SalesEntryLineCreateRequests: {4}  SalesEntryDiscountLineCreateRequests: {5}",
                Id, StoreId, CardId, IdType.ToString(), req, osldreq);
            return s;
        }
    }
}

