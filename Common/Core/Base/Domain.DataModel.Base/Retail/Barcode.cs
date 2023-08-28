using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Barcode : Entity
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public string VariantId { get; set; }
        public string UnitOfMeasureId { get; set; }
        public bool Blocked { get; set; }

        public Barcode() : this(null)
        {
        }

        public Barcode(string code) : base(code)
        {
            ItemId = string.Empty;
            Description = string.Empty;
            VariantId = string.Empty;
            UnitOfMeasureId = string.Empty;
            Blocked = false;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2023")]
    public enum BarcodeTypes
    {
        Unknown = 0,
        Barcode = 1,
        QrCode = 2,
    }
}
