using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    public enum BarcodeType
    {
        Unknown = 0,
        Barcode = 1,
        QrCode = 2,
        Nfc = 3,
    }

    public class BarcodeScan
    {
        public string Barcode { get; set; }
        public BarcodeType BarcodeType { get; set; }
    }
}
