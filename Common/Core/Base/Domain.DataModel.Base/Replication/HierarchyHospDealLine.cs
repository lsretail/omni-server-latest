using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospDealLineResponse : IDisposable
    {
        public ReplHierarchyHospDealLineResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Items = new List<ReplHierarchyHospDealLine>();
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
                if (Items != null)
                    Items.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplHierarchyHospDealLine> Items { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospDealLine : IDisposable
    {
        public ReplHierarchyHospDealLine()
        {
            HierarchyCode = string.Empty;
            ParentNode = string.Empty;
            DealNo = string.Empty;
            DealLineCode = string.Empty;
            Description = string.Empty;
            ItemNo = string.Empty;
            VariantCode = string.Empty;
            UnitOfMeasure = string.Empty;
            ImageId = string.Empty;
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
        public string HierarchyCode { get; set; }
        [DataMember]
        public string ParentNode { get; set; }
        [DataMember]
        public string DealNo { get; set; }
        [DataMember]
        public int DealLineNo { get; set; }
        [DataMember]
        public string DealLineCode { get; set; }

        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ItemNo { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string VariantCode { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public string ImageId { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        [DataMember]
        public decimal AddedAmount { get; set; }
    }
}
