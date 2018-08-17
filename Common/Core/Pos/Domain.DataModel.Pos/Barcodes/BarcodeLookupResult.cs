using LSRetail.Omni.Domain.DataModel.Pos.Items;

namespace LSRetail.Omni.Domain.DataModel.Pos.Barcodes
{
    public class BarcodeLookupResult
    {
        public RetailItem RetailItem { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}
