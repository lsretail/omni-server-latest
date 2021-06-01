using System;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Utils
{
    public enum BarcodeType
    {
        Unknown,
        Barcode,
        QRCode,
    }
    
    public class BarcodeScannedEventArgs : EventArgs
    {
        public string Barcode { get; set; }
        public BarcodeType Type { get; set; }
    }
    
    public interface INotificationReceiver
    {
        event EventHandler<BarcodeScannedEventArgs> BarcodeScanned;
    }
}
