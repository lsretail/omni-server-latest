using System.Collections.Generic;
using Newtonsoft.Json;

namespace LSRetail.Omni.Domain.DataModel.Base.Printer
{
    public enum NavLineType
    {
        StartTransaction = 0,
        EndTransaction = 1,
        PrintLine,
        PrintBarcode,
        PrintBitmap,
        InsertPage,
        RemovePage,
        Rotation,
        InitPrinter,
        PrintBitmapFile
    }

    public class NavPrintJob
    {
        [JsonProperty("PrintBuffer")]
        public List<NavPrintBuffer> PrintBuffer { get; set; }

        [JsonProperty("PrinterSettings")]
        public NavPrinterSettings PrinterSettings { get; set; }
    }

    public class NavPrinterSettings
    {
        [JsonProperty("Logo")]
        public string Logo { get; set; }
    }

    public class NavPrintBuffer
    {
        public string timestamp { get; set; }

        [JsonProperty("Store No.")]
        public string StoreNo { get; set; }

        [JsonProperty("Terminal No.")]
        public string TerminalNo { get; set; }

        [JsonProperty("Transaction No.")]
        public int TransactionNo { get; set; }

        [JsonProperty("Entry No.")]
        public int EntryNo { get; set; }

        [JsonProperty("Buffer Index")]
        public int BufferIndex { get; set; }

        [JsonProperty("Station No.")]
        public int StationNo { get; set; }

        [JsonProperty("Page No.")]
        public int PageNo { get; set; }

        [JsonProperty("Line No.")]
        public int LineNo { get; set; }

        public int LineType { get; set; }
        public int FiscalLineType { get; set; }
        public string HostID { get; set; }
        public string ProfileID { get; set; }
        public int TransactionType { get; set; }
        public string Text { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string BCType { get; set; }
        public int BCPos { get; set; }
        public bool SetBackPrinting { get; set; }
        public int FontType { get; set; }
        public bool Training { get; set; }
        public string Xml { get; set; }
        public string DesignText { get; set; }
    }
}