using System.Collections.Generic;
using System.Runtime.Serialization;
using Infrastructure.Devices.Printer.Base.Utils;

namespace LSRetail.Omni.Domain.DataModel.Base.Printer
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2018")]
    public enum PrintLineType
    {
        PrintLine,
        PrintBarcode,
        PrintBitmap,
        LineFeed
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

        /// <summary>
        /// Converts the nav receipt to base receipt.
        /// </summary>
        /// <returns>List of receipts (where each receipt is a list of lines to print).</returns>
        /// <param name="printBuffer">Print buffer.</param>
        /// <param name="logo">Logo to be added to the first PrintBitmap line</param>
        public static List<List<PrintLine>> ConvertNavToBaseReceipt(List<NavPrintBuffer> printBuffer, string logo = "")
        {
            List<List<PrintLine>> receipts = new List<List<PrintLine>>();
            List<PrintLine> currReceipt = new List<PrintLine>();
            for (int i = 0; i < printBuffer.Count; i++)
            {
                var buffer = printBuffer[i];
                PrintLine currPrintLine = new PrintLine
                {
                    Text = buffer.Text,
                    Width = buffer.Width,
                    Height = buffer.Height,
                    BarcodePosition = buffer.BCPos
                };

                switch (buffer.LineType)
                {
                    case (int)NavLineType.PrintLine:
                        currPrintLine.LineType = PrintLineType.PrintLine;
                        currPrintLine.FontType = ConvertFontTypeFromNavToBase(buffer.FontType);
                        currPrintLine.FontSize = ConvertNavFontWidthToBaseFontSize(buffer.Width);
                        currReceipt.Add(currPrintLine);
                        break;
                    case (int)NavLineType.PrintBarcode:
                        currPrintLine.LineType = PrintLineType.PrintBarcode;
                        currPrintLine.BarcodeType = ConvertBarcodeSystemFromNavToBase(buffer.BCType);
                        currReceipt.Add(currPrintLine);
                        break;
                    case (int)NavLineType.PrintBitmap:
                        currPrintLine.LineType = PrintLineType.PrintBitmap;
                        currPrintLine.Base64Image = buffer.Text;
                        currReceipt.Add(currPrintLine);
                        break;
                    case (int)NavLineType.EndTransaction:
                        currPrintLine.LineType = PrintLineType.LineFeed;
                        currReceipt.Add(currPrintLine);
                        receipts.Add(currReceipt);
                        currReceipt = null;
                        currReceipt = new List<PrintLine>();
                        break;
                }
            }

            return receipts;
        }

        private static FontType ConvertFontTypeFromNavToBase(int navFontType)
        {
            switch (navFontType)
            {
                case (int)NavFontType.Normal: return FontType.Normal;
                case (int)NavFontType.Bold: return FontType.Bold;
                case (int)NavFontType.Wide: return FontType.Large;
                case (int)NavFontType.High: return FontType.Large;
                case (int)NavFontType.WideHigh: return FontType.Large;
                case (int)NavFontType.Italic: break;
                case (int)NavFontType.CustomESC: break;
                case (int)NavFontType.Image: break;
                case (int)NavFontType.Barcode: break;
            }
            return FontType.Normal;
        }

        private static FontSize ConvertNavFontWidthToBaseFontSize(int navFontWidth)
        {
            switch (navFontWidth)
            {
                case 0: return FontSize.Normal;
                case 1: return FontSize.Normal;
                case 2: return FontSize.Large;
                case 3: return FontSize.Large;
                case 4: return FontSize.Large;
                case 5: return FontSize.Large;
                case 6: return FontSize.Large;
                default: return FontSize.Normal;
            }
        }

        private static BarcodeSystem ConvertBarcodeSystemFromNavToBase(string barcodeSystemFromNav)
        {
            switch (barcodeSystemFromNav)
            {
                case "EAN13":
                    return BarcodeSystem.EAN13;

                case "EAN8":
                    return BarcodeSystem.EAN8;

                case "UPCA":
                    return BarcodeSystem.UPC_A;

                case "UPCE":
                    return BarcodeSystem.UPC_E;

                case "CODE39":
                    return BarcodeSystem.CODE39;

                case "CODE128": 
                case "CODE128_B":
                    return BarcodeSystem.CODE128;

                case "PDF417":
                    return BarcodeSystem.PDF417;

                case "MAXICODE":
                    return BarcodeSystem.Unsupported;

                default:
                    return BarcodeSystem.Unsupported;
            }
        }
    }
}
