using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospModifierResponse : IDisposable
    {
        public ReplHierarchyHospModifierResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Modifiers = new List<ReplHierarchyHospModifier>();
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
                if (Modifiers != null)
                    Modifiers.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplHierarchyHospModifier> Modifiers { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplHierarchyHospModifier : IDisposable
    {
        public ReplHierarchyHospModifier()
        {
            HierarchyCode = string.Empty;
            ParentNode = string.Empty;
            ParentItem = string.Empty;
            ItemNo = string.Empty;
            SubCode = string.Empty;
            Description = string.Empty;
            UnitOfMeasure = string.Empty;
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
        public string ParentItem { get; set; }
        [DataMember]
        public ModifierType Type { get; set; }

        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string SubCode { get; set; }
        [DataMember]
        public string ItemNo { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }

        [DataMember]
        public int MinSelection { get; set; }
        [DataMember]
        public int MaxSelection { get; set; }
        [DataMember]
        public ModifierPriceType PriceType { get; set; }
        [DataMember]
        public bool AlwaysCharge { get; set; }
        [DataMember]
        public decimal AmountPercent { get; set; }
    }

    public enum ModifierType
    {
        Item,
        Time,
        Text
    }

    public enum ModifierPriceType
    {
        None,
        FromItem,
        Amount,
        Percent
    }
}
