using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStoreTenderTypeResponse : IDisposable
    {
        public ReplStoreTenderTypeResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            StoreTenderTypes = new List<ReplStoreTenderType>();
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
                if (StoreTenderTypes != null)
                    StoreTenderTypes.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplStoreTenderType> StoreTenderTypes { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStoreTenderType : IDisposable
    {
        public ReplStoreTenderType()
        {
            IsDeleted = false;
            StoreID = "";
            TenderTypeId = "";
            Name = "";
            TenderFunction = 0;
            ChangeTenderId = "";
            AboveMinimumTenderId = "";
            MinimumChangeAmount = 0M;
            RoundingMethode = 0;
            Rounding = 0M;
            MaximumOverTenderAmount = 0M;
            CountingRequired = 0;
            AllowVoiding = 0;
            AllowOverTender = 0;
            AllowUnderTender = 0;
            OpenDrawer = 0;
            ReturnAllowed = 0;

            ValidOnMobilePOS = 1;
            ForeignCurrency = 0;
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
        public string StoreID { get; set; }
        [DataMember]
        public string TenderTypeId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int TenderFunction { get; set; }
        [DataMember]
        public string ChangeTenderId { get; set; }
        [DataMember]
        public string AboveMinimumTenderId { get; set; }
        [DataMember]
        public decimal MinimumChangeAmount { get; set; }
        [DataMember]
        public int RoundingMethode { get; set; }
        [DataMember]
        public decimal Rounding { get; set; }
        [DataMember]
        public decimal MaximumOverTenderAmount { get; set; }
        [DataMember]
        public int CountingRequired { get; set; }
        [DataMember]
        public int AllowVoiding { get; set; }
        [DataMember]
        public int AllowOverTender { get; set; }
        [DataMember]
        public int AllowUnderTender { get; set; }
        [DataMember]
        public int OpenDrawer { get; set; }
        [DataMember]
        public int ReturnAllowed { get; set; }
        [DataMember]
        public int ValidOnMobilePOS { get; set; }
        [DataMember]
        public int ForeignCurrency { get; set; }
    }
}
