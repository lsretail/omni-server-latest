using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Beacon
{
    public class BeaconData : Entity
    {
        public enum BeaconDataType
        {
            Store = 0,
            Offer = 1
        }

        public BeaconDataType DataType { get; set; }
        
        public string Data { get; set; }

        public string Uuid { get; set; }
        
        public int Major { get; set; }
        
        public int Minor { get; set; }
    }
}
