using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSOmni.BLL.Loyalty
{
    public class TransactionBLL : BaseLoyBLL
    {
        public TransactionBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries, Statistics stat)
        {
            return BOLoyConnection.SalesEntriesGetByCardId(cardId, storeId, date, dateGreaterThan , maxNumberOfEntries, stat);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type, Statistics stat)
        {
            if (string.IsNullOrEmpty(entryId))
                throw new LSOmniException(StatusCode.TransacitionIdMissing, "Id can not be empty");

            SalesEntry entry = BOLoyConnection.SalesEntryGet(entryId, type, stat);
            if (entry == null)
                return null;

            bool compress = BOLoyConnection.CompressCOActive(stat);

            List<SalesEntryLine> lines = new List<SalesEntryLine>();
            foreach (SalesEntryLine line in entry.Lines)
            {
                if (line.LineNumber == line.ParentLine)
                    line.ParentLine = 0;

                List<SalesEntryLine> sublines = lines.FindAll(s => s.ParentLine == line.LineNumber);
                SalesEntryLine lineFound = lines.Find(l => l.ItemId == line.ItemId && l.VariantId == line.VariantId && l.UomId == line.UomId && l.LineType == line.LineType && ((line.ParentLine > 0 && l.ParentLine == line.ParentLine) || (line.ParentLine == 0 && sublines.Count == 0)));
                if (lineFound == null || string.IsNullOrEmpty(line.ExtraInformation) == false || compress)
                {
                    lines.Add(line);
                    continue;
                }

                SalesEntryDiscountLine dLine = entry.DiscountLines.Find(l => l.LineNumber >= line.LineNumber && l.LineNumber < line.LineNumber + 10000);
                if (dLine != null)
                {
                    // update discount line number to match existing record, as we will sum up the orderlines
                    dLine.LineNumber = lineFound.LineNumber + dLine.LineNumber / 100;
                }

                lineFound.Quantity += line.Quantity;
                lineFound.Amount += line.Amount;
                lineFound.NetAmount += line.NetAmount;
                lineFound.TaxAmount += line.TaxAmount;
                lineFound.DiscountAmount += line.DiscountAmount;
            }
            entry.Lines = lines;
            return entry;
        }

        public virtual List<SalesEntry> SalesEntryGetReturnSales(string receiptNo, Statistics stat)
        {
            if (string.IsNullOrEmpty(receiptNo))
                throw new LSOmniException(StatusCode.TransacitionIdMissing, "receiptNo can not be empty");

            List<SalesEntry> result = new List<SalesEntry>();
            List<SalesEntryId> list = BOLoyConnection.SalesEntryGetReturnSales(receiptNo, stat);
            foreach (SalesEntryId line in list)
            {
                SalesEntry en = SalesEntryGet(line.ReceiptId, DocumentIdType.Receipt, stat);
                en.CustomerOrderNo = line.OrderId;
                result.Add(en);
            }
            return result;
        }

        public virtual List<SalesEntry> SalesEntryGetSalesByOrderId(string orderId, Statistics stat)
        {
            if (string.IsNullOrEmpty(orderId))
                throw new LSOmniException(StatusCode.TransacitionIdMissing, "orderId can not be empty");

            SalesEntryList data = BOLoyConnection.SalesEntryGetSalesByOrderId(orderId, stat);
            return data.SalesEntries;
        }

        public virtual SalesEntryList SalesEntryGetSalesExtByOrderId(string orderId, Statistics stat)
        {
            if (string.IsNullOrEmpty(orderId))
                throw new LSOmniException(StatusCode.TransacitionIdMissing, "orderId can not be empty");

            return BOLoyConnection.SalesEntryGetSalesByOrderId(orderId, stat);
        }
    }
}

 