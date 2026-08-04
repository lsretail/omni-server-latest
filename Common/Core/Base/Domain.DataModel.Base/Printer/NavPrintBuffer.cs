using System.Collections.Generic;

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
        public List<NavPrintBuffer> PrintBuffer { get; set; }

        public NavPrinterSettings PrinterSettings { get; set; }
    }

    public class NavPrinterSettings
    {
        public string Logo { get; set; }
    }

    public class NavPrintBuffer
    {
        public string timestamp { get; set; }
        public string StoreNo { get; set; }
        public string TerminalNo { get; set; }
        public int TransactionNo { get; set; }
        public int EntryNo { get; set; }
        public int BufferIndex { get; set; }
        public int StationNo { get; set; }
        public int PageNo { get; set; }
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