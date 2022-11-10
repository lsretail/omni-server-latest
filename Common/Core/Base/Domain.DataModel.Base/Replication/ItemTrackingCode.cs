using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemTrackingCodeResponse : IDisposable
    {
        public ReplItemTrackingCodeResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Codes = new List<ReplItemTrackingCode>();
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
                if (Codes != null)
                    Codes.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplItemTrackingCode> Codes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplItemTrackingCode : IDisposable
    {
        public ReplItemTrackingCode()
        {
            Code = string.Empty;
            Description = string.Empty;
            WarrantyDateFormula = string.Empty;
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
        public bool IsDeleted { get; set; }

        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string WarrantyDateFormula { get; set; }
        [DataMember]
        public bool ManWarrantyDateEntryReqired { get; set; }
        [DataMember]
        public bool ManExpirationDateEntryReqired { get; set; }
        [DataMember]
        public bool StrictExpirationPosting { get; set; }
        [DataMember]
        public bool UseExpirationDates { get; set; }
        [DataMember]
        public bool CreateSNInfoOnPosting { get; set; }
        [DataMember]
        public bool SNSpecificTracking { get; set; }
        [DataMember]
        public bool SNInfoInboundMustExist { get; set; }
        [DataMember]
        public bool SNInfoOutboundMustExist { get; set; }
        [DataMember]
        public bool SNWarehouseTracking { get; set; }
        [DataMember]
        public bool SNPurchaseInboundTracking { get; set; }
        [DataMember]
        public bool SNPurchaseOutboundTracking { get; set; }
        [DataMember]
        public bool SNSalesInboundTracking { get; set; }
        [DataMember]
        public bool SNSalesOutboundTracking { get; set; }
        [DataMember]
        public bool SNPosAdjmtInboundTracking { get; set; }
        [DataMember]
        public bool SNPosAdjmtOutboundTracking { get; set; }
        [DataMember]
        public bool SNNegAdjmtInboundTracking { get; set; }
        [DataMember]
        public bool SNNegAdjmtOutboundTracking { get; set; }
        [DataMember]
        public bool SNTransferTracking { get; set; }
        [DataMember]
        public bool SNManufInboundTracking { get; set; }
        [DataMember]
        public bool SNManufOutboundTracking { get; set; }
        [DataMember]
        public bool SNAssemblyInboundTracking { get; set; }
        [DataMember]
        public bool SNAssemblyOutboundTracking { get; set; }
        [DataMember]
        public bool LotSpecificTracking { get; set; }
        [DataMember]
        public bool LotInfoInboundMustExist { get; set; }
        [DataMember]
        public bool LotInfoOutboundMustExist { get; set; }
        [DataMember]
        public bool LotWarehouseTracking { get; set; }
        [DataMember]
        public bool LotPurchaseInboundTracking { get; set; }
        [DataMember]
        public bool LotPurchaseOutboundTracking { get; set; }
        [DataMember]
        public bool LotSalesInboundTracking { get; set; }
        [DataMember]
        public bool LotSalesOutboundTracking { get; set; }
        [DataMember]
        public bool LotPosAdjmtInbboundTracking { get; set; }
        [DataMember]
        public bool LotPosAdjmtOutboundTracking { get; set; }
        [DataMember]
        public bool LotNegAdjmtInboundTracking { get; set; }
        [DataMember]
        public bool LotNegAdjmtOutboundTracking { get; set; }
        [DataMember]
        public bool LotTransferTracking { get; set; }
        [DataMember]
        public bool LotManufacturingInboundTracking { get; set; }
        [DataMember]
        public bool LotManufacturingOutboundTracking { get; set; }
        [DataMember]
        public bool LotAssemblyInboundTracking { get; set; }
        [DataMember]
        public bool LotAssemblyOutboundTracking { get; set; }
    }
}
