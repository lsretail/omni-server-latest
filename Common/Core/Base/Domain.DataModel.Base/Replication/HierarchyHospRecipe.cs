using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospRecipeResponse : IDisposable
    {
        public ReplHierarchyHospRecipeResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Items = new List<ReplHierarchyHospRecipe>();
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
        public List<ReplHierarchyHospRecipe> Items { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospRecipe : IDisposable
    {
        public ReplHierarchyHospRecipe()
        {
            HierarchyCode = string.Empty;
            ParentNode = string.Empty;
            RecipeNo = string.Empty;
            ItemNo = string.Empty;
            Description = string.Empty;
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
        public string RecipeNo { get; set; }

        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ItemNo { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public string ImageId { get; set; }

        [DataMember]
        public decimal QuantityPer { get; set; }
        [DataMember]
        public bool Exclusion { get; set; }
        [DataMember]
        public decimal ExclusionPrice { get; set; }
    }
}
