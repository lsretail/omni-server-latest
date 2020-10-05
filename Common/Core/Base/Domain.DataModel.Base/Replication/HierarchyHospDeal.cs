using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospDealResponse : IDisposable
    {
        public ReplHierarchyHospDealResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Items = new List<ReplHierarchyHospDeal>();
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
        public List<ReplHierarchyHospDeal> Items { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospDeal : IDisposable
    {
        public ReplHierarchyHospDeal()
        {
            HierarchyCode = string.Empty;
            ParentNode = string.Empty;
            DealNo = string.Empty;
            Description = string.Empty;
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
        public string Description { get; set; }
        [DataMember]
        public int LineNo { get; set; }

        [DataMember]
        public string ModifierCode { get; set; }
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
        /// <summary>
        /// Amount to add to a deal price
        /// </summary>
        [DataMember]
        public decimal AddedAmount { get; set; }
        /// <summary>
        /// Used to group deals together, f.ex. if Medium Fries goes with Medium Soda.
        /// </summary>
        [DataMember]
        public int DealModSizeGroupIndex { get; set; }
    }
}
