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

            List<SalesEntryLine> lines = new List<SalesEntryLine>();
            foreach (SalesEntryLine line in entry.Lines)
            {
                if (line.LineNumber == line.ParentLine)
                    line.ParentLine = 0;

                List<SalesEntryLine> sublines = lines.FindAll(s => s.ParentLine == line.LineNumber);
                SalesEntryLine linefound = lines.Find(l => l.ItemId == line.ItemId && l.VariantId == line.VariantId && l.UomId == line.UomId && l.LineType == line.LineType && ((line.ParentLine > 0 && l.ParentLine == line.ParentLine) || (line.ParentLine == 0 && sublines.Count == 0)));
                if (linefound == null)
                {
                    lines.Add(line);
                    continue;
                }

                linefound.Quantity += line.Quantity;
                linefound.Amount += line.Amount;
                linefound.NetAmount += line.NetAmount;
                linefound.TaxAmount += line.TaxAmount;
                linefound.DiscountAmount += line.DiscountAmount;
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

            List<SalesEntry> result = new List<SalesEntry>();
            List<SalesEntryId> list = BOLoyConnection.SalesEntryGetSalesByOrderId(orderId, stat);
            foreach (SalesEntryId line in list)
            {
                result.Add(SalesEntryGet(line.ReceiptId, DocumentIdType.Receipt, stat));
            }
            return result;
        }
    }
}

 