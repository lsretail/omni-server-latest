using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplicateTransactionDiscountLine : Entity, IDisposable
    {
        public ReplicateTransactionDiscountLine(string id) : base(id)
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
        public int LineNo { get; set; }
        [DataMember]
        public int No { get; set; }
        [DataMember]
        public int PeriodicDiscType { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int DiscountType { get; set; }
        [DataMember]
        public string PeriodicDiscGroup { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
        [DataMember]
        public decimal DiscountAmonut { get; set; }
    }
}
