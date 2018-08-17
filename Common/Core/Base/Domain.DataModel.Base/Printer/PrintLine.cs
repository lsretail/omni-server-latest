using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Printer
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2018")]
    public enum PrintLineType
    {
        PrintLine,
        PrintBarcode,
        PrintBitmap,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2018")]
    public class PrintLine
    {
        [DataMember]
        public PrintLineType LineType { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public BarcodeSystem BarcodeType { get; set; }
        [DataMember]
        public FontType FontType { get; set; }
        [DataMember]
        public FontSize FontSize { get; set; }
        [DataMember]
        public int BarcodePosition { get; set; }
        [DataMember]
        public string Base64Image { get; set; }

        public PrintLine()
        {
            LineType = PrintLineType.PrintLine;
            Text = "";
            BarcodeType = BarcodeSystem.CODE128;
            FontType = FontType.Normal;
            FontSize = FontSize.Normal;
            Base64Image = "";
        }
    }
}
