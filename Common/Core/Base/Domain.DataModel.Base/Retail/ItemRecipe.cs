using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ItemRecipe : Entity, IDisposable
    {
        public ItemRecipe()
        {
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
        public int LineNo { get; set; }
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
