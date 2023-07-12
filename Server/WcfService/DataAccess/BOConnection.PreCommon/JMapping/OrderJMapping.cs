using System;
using System.Collections.Generic;
using System.Linq;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class OrderJMapping : BaseJMapping
    {
        public OrderJMapping(bool json)
        {
            IsJson = json;
        }

        public List<SalesEntry> GetSalesEntryHistory(string ret)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            WSODataCollection result = JsonToWSOData(ret, "MemberContactSalesHistoryDaCo");
            if (result == null)
                return list;

            // get data
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000818);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntry rec = new SalesEntry();
                rec.ContactAddress = new Address();
                rec.ShipToAddress = new Address();

                foreach (ReplODataField col in row.Fields)
                {
                    switch (col.FieldIndex)
                    {
                        case 2: rec.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                        case 3: rec.Id = col.FieldValue; break;
                        case 4: rec.StoreId = col.FieldValue; break;
                        case 5: rec.DocumentRegTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case 6: rec.ReturnSale = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case 7: rec.CustomerOrderNo = col.FieldValue; break;
                        case 8: rec.TerminalId = col.FieldValue; break;
                        case 9: rec.LineItemCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 10: rec.LineCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 11: rec.TotalNetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 12: rec.TotalAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 13: rec.TotalDiscount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 14: rec.CardId = col.FieldValue; break;
                        case 15: rec.CustomerId = col.FieldValue; break;
                        case 16: rec.ExternalId = col.FieldValue; break;
                        case 17: rec.HasReturnSale = col.FieldValue != string.Empty; break;
                        case 18: rec.StoreName = col.FieldValue; break;
                        case 19: rec.StoreCurrency = col.FieldValue; break;
                        case 20: rec.ContactName = col.FieldValue; break;
                        case 21: rec.ContactAddress.Address1 = col.FieldValue; break;
                        case 22: rec.ContactAddress.Address2 = col.FieldValue; break;
                        case 23: rec.ContactAddress.City = col.FieldValue; break;
                        case 24: rec.ContactAddress.StateProvinceRegion = col.FieldValue; break;
                        case 25: rec.ContactAddress.PostCode = col.FieldValue; break;
                        case 26: rec.ContactAddress.Country = col.FieldValue; break;
                        case 27: rec.ContactAddress.PhoneNumber = col.FieldValue; break;
                        case 28: rec.ContactEmail = col.FieldValue; break;
                        case 29: rec.ContactAddress.HouseNo = col.FieldValue; break;
                        case 30: rec.ContactAddress.CellPhoneNumber = col.FieldValue; break;
                        case 31: rec.ContactDayTimePhoneNo = col.FieldValue; break;
                        case 32: rec.ShipToName = col.FieldValue; break;
                        case 33: rec.ShipToAddress.Address1 = col.FieldValue; break;
                        case 34: rec.ShipToAddress.Address2 = col.FieldValue; break;
                        case 35: rec.ShipToAddress.City = col.FieldValue; break;
                        case 36: rec.ShipToAddress.StateProvinceRegion = col.FieldValue; break;
                        case 37: rec.ShipToAddress.PostCode = col.FieldValue; break;
                        case 38: rec.ShipToAddress.Country = col.FieldValue; break;
                        case 39: rec.ShipToAddress.PhoneNumber = col.FieldValue; break;
                        case 40: rec.ShipToEmail = col.FieldValue; break;
                        case 41: rec.ShipToAddress.HouseNo = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public SalesEntry GetSalesEntry(string ret)
        {
            SalesEntry entry = null;
            WSODataCollection result = JsonToWSOData(ret, "SelectedSalesDocDaCo");
            if (result == null)
                return entry;

            // get header data
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000818);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return entry;

            ReplODataRecord row = dynDataSet.DataSetRows.FirstOrDefault();

            entry = new SalesEntry()
            {
                ContactAddress = new Address(),
                ShipToAddress = new Address(),
                Lines = new List<SalesEntryLine>(),
                DiscountLines = new List<SalesEntryDiscountLine>(),
                Payments = new List<SalesEntryPayment>()
            };

            foreach (ReplODataField col in row.Fields)
            {
                switch (col.FieldIndex)
                {
                    case 2: entry.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                    case 3: entry.Id = col.FieldValue; break;
                    case 4: entry.StoreId = col.FieldValue; break;
                    case 5: entry.DocumentRegTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                    case 6: entry.ReturnSale = ConvertTo.SafeBoolean(col.FieldValue); break;
                    case 7: entry.CustomerOrderNo = col.FieldValue; break;
                    case 8: entry.TerminalId = col.FieldValue; break;
                    case 9: entry.LineItemCount = ConvertTo.SafeInt(col.FieldValue); break;
                    case 10: entry.LineCount = ConvertTo.SafeInt(col.FieldValue); break;
                    case 11: entry.TotalNetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case 12: entry.TotalAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case 13: entry.TotalDiscount = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case 14: entry.CardId = col.FieldValue; break;
                    case 15: entry.CustomerId = col.FieldValue; break;
                    case 16: entry.ExternalId = col.FieldValue; break;
                    case 17: entry.HasReturnSale = col.FieldValue != string.Empty; break;
                    case 18: entry.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case 19: entry.StoreName = col.FieldValue; break;
                    case 20: entry.StoreCurrency = col.FieldValue; break;
                    case 21: entry.CreateTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                    case 22: entry.PointsRewarded = ConvertTo.SafeDecimal(col.FieldValue);break;
                    case 23: entry.PointsUsedInOrder = ConvertTo.SafeDecimal(col.FieldValue); break;

                    case 26: entry.ContactName = col.FieldValue; break;
                    case 27: entry.ContactAddress.Address1 = col.FieldValue; break;
                    case 28: entry.ContactAddress.Address2 = col.FieldValue; break;
                    case 29: entry.ContactAddress.City = col.FieldValue; break;
                    case 30: entry.ContactAddress.County = col.FieldValue; break;
                    case 31: entry.ContactAddress.PostCode = col.FieldValue; break;
                    case 32: entry.ContactAddress.Country = col.FieldValue; break;
                    case 33: entry.ContactAddress.PhoneNumber = col.FieldValue; break;
                    case 34: entry.ContactEmail = col.FieldValue; break;
                    case 35: entry.ContactAddress.HouseNo = col.FieldValue; break;
                    case 36: entry.ContactAddress.CellPhoneNumber = col.FieldValue; break;
                    case 37: entry.ContactDayTimePhoneNo = col.FieldValue; break;
                    case 38: entry.ContactAddress.StateProvinceRegion = col.FieldValue; break;

                    case 39: entry.ShipToName = col.FieldValue; break;
                    case 40: entry.ShipToAddress.Address1 = col.FieldValue; break;
                    case 41: entry.ShipToAddress.Address2 = col.FieldValue; break;
                    case 42: entry.ShipToAddress.City = col.FieldValue; break;
                    case 43: entry.ShipToAddress.County = col.FieldValue; break;
                    case 44: entry.ShipToAddress.PostCode = col.FieldValue; break;
                    case 45: entry.ShipToAddress.Country = col.FieldValue; break;
                    case 46: entry.ShipToAddress.PhoneNumber = col.FieldValue; break;
                    case 47: entry.ShipToEmail = col.FieldValue; break;
                    case 48: entry.ShipToAddress.HouseNo = col.FieldValue; break;
                    case 49: entry.ShippingAgentCode = col.FieldValue; break;
                    case 50: entry.ShippingAgentServiceCode = col.FieldValue; break;
                    case 51: entry.CreateAtStoreId = col.FieldValue; break;
                }
            }

            // get line data
            dynDataSet = result.GetDataSet(10000835);
            if (dynDataSet != null && dynDataSet.DataSetRows.Count > 0)
            {
                foreach (ReplODataRecord line in dynDataSet.DataSetRows)
                {
                    SalesEntryLine rec = new SalesEntryLine();
                    string cardType = string.Empty;
                    string cardNo = string.Empty;
                    string curCode = string.Empty;
                    decimal curFact = 0;
                    foreach (ReplODataField col in line.Fields)
                    {
                        switch (col.FieldIndex)
                        {
                            case 2: rec.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                            case 3: rec.LineType = (LineType)ConvertTo.SafeInt(col.FieldValue); break;
                            case 4: rec.ItemId = col.FieldValue; break;
                            case 5: rec.ItemDescription = col.FieldValue; break;
                            case 6: curCode = col.FieldValue; break;
                            case 7: curFact = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 8: rec.VariantId = col.FieldValue; break;
                            case 9: rec.VariantDescription = col.FieldValue; break;
                            case 10: rec.UomId = col.FieldValue; break;
                            case 11: rec.NetPrice = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 12: rec.Price = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 13: rec.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 14: rec.DiscountPercent = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 15: rec.DiscountAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 16: rec.NetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 17: rec.TaxAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 18: rec.Amount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case 20: rec.ParentLine = ConvertTo.SafeInt(col.FieldValue); break;
                            case 21: cardType = col.FieldValue; break;
                            case 22: cardNo = col.FieldValue; break;
                            case 23: rec.StoreId = col.FieldValue; break;
                            case 24: rec.StoreName = col.FieldValue; break;
                            case 27: rec.ExternalId = col.FieldValue; break;
                            case 28: rec.ClickAndCollectLine = ConvertTo.SafeBoolean(col.FieldValue); break;
                            case 29: rec.ItemImageId = col.FieldValue; break;
                        }
                    }

                    if (rec.LineType == LineType.Payment)
                    {
                        entry.Payments.Add(new SalesEntryPayment()
                        {
                            Amount = rec.Amount,
                            LineNumber = rec.LineNumber,
                            TenderType = rec.ItemId,
                            CardNo = cardNo,
                            CurrencyCode = curCode,
                            CurrencyFactor = curFact,
                            Type = PaymentType.Payment
                        });
                    }
                    else
                    {
                        entry.Lines.Add(rec);
                    }
                }

                foreach (SalesEntryLine line in entry.Lines)
                {
                    if (line.ClickAndCollectLine && entry.ClickAndCollectOrder == false)
                        entry.ClickAndCollectOrder = true;
                }
            }

            // get discount lines
            dynDataSet = result.GetDataSet(10000836);
            if (dynDataSet != null && dynDataSet.DataSetRows.Count > 0)
            {
                foreach (ReplODataRecord line in dynDataSet.DataSetRows)
                {
                    SalesEntryDiscountLine rec = new SalesEntryDiscountLine();
                    rec.DiscountType = DiscountType.PeriodicDisc;

                    foreach (ReplODataField col in line.Fields)
                    {
                        switch (col.FieldIndex)
                        {
                            case 1: rec.Id = col.FieldValue; break;
                            case 2: rec.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                            case 4: rec.SetDiscType((OfferDiscountType)ConvertTo.SafeInt(col.FieldValue)); break;
                            case 5: rec.OfferNumber = col.FieldValue; break;
                            case 6: rec.Description = col.FieldValue; break;
                            case 7: rec.DiscountAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        }
                    }
                    rec.PeriodicDiscGroup = rec.OfferNumber;
                    entry.DiscountLines.Add(rec);
                }
            }

            entry.Posted = false;
            switch (entry.IdType)
            {
                case DocumentIdType.Receipt:
                    entry.Posted = true;
                    entry.Status = SalesEntryStatus.Complete;
                    entry.ShippingStatus = ShippingStatus.Shipped;
                    entry.LineItemCount = (int)entry.Lines.Sum(q => q.Quantity);
                    entry.Quantity = entry.LineItemCount;
                    entry.LineCount = entry.Lines.Count;
                    entry.ClickAndCollectOrder = string.IsNullOrEmpty(entry.CustomerOrderNo) == false;
                    if (string.IsNullOrEmpty(entry.ShipToName))
                    {
                        entry.ShipToName = entry.ContactName;
                        entry.ShipToEmail = entry.ContactEmail;
                    }
                    break;
                case DocumentIdType.Order:
                    entry.ClickAndCollectOrder = (entry.Lines.Find(l => l.ClickAndCollectLine == true) != null);
                    entry.Status = SalesEntryStatus.Created;
                    entry.ShippingStatus = ShippingStatus.NotYetShipped;
                    entry.CreateAtStoreId = entry.StoreId;
                    if (entry.Lines != null && entry.Lines.Count > 0)
                    {
                        entry.StoreId = entry.Lines[0].StoreId;
                        entry.StoreName = entry.Lines[0].StoreName;
                    }
                    break;
                case DocumentIdType.HospOrder:
                    entry.CreateTime = entry.DocumentRegTime;
                    entry.CreateAtStoreId = entry.StoreId;
                    entry.Status = SalesEntryStatus.Processing;
                    entry.ShippingStatus = ShippingStatus.ShippigNotRequired;
                    if (string.IsNullOrEmpty(entry.ShipToName))
                    {
                        entry.ShipToName = entry.ContactName;
                        entry.ShipToEmail = entry.ContactEmail;
                    }
                    break;
            }
            return entry;
        }

        public List<SalesEntry> GetSalesEntryHistory2(string ret)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            WSODataCollection result = JsonToWSOData(ret, "MemberContactSalesHistoryDaCo");
            if (result == null)
                return list;

            // get data
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000818);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntry rec = new SalesEntry();
                rec.ContactAddress = new Address();
                rec.ShipToAddress = new Address();

                foreach (ReplODataField col in row.Fields)
                {
                    switch (col.FieldIndex)
                    {
                        case 2: rec.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                        case 3: rec.Id = col.FieldValue; break;
                        case 4: rec.StoreId = col.FieldValue; break;
                        case 5: rec.DocumentRegTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case 6: rec.ReturnSale = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case 7: rec.CustomerOrderNo = col.FieldValue; break;
                        case 8: rec.TerminalId = col.FieldValue; break;
                        case 9: rec.LineItemCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 10: rec.LineCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 11: rec.TotalNetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 12: rec.TotalAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 13: rec.TotalDiscount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 14: rec.CardId = col.FieldValue; break;
                        case 15: rec.CustomerId = col.FieldValue; break;
                        case 16: rec.ExternalId = col.FieldValue; break;
                        case 17: rec.HasReturnSale = col.FieldValue != string.Empty; break;
                        case 18: rec.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 19: rec.StoreName = col.FieldValue; break;
                        case 20: rec.StoreCurrency = col.FieldValue; break;
                        case 21: rec.ContactName = col.FieldValue; break;
                        case 22: rec.ContactAddress.Address1 = col.FieldValue; break;
                        case 23: rec.ContactAddress.Address2 = col.FieldValue; break;
                        case 24: rec.ContactAddress.City = col.FieldValue; break;
                        case 25: rec.ContactAddress.StateProvinceRegion = col.FieldValue; break;
                        case 26: rec.ContactAddress.PostCode = col.FieldValue; break;
                        case 27: rec.ContactAddress.Country = col.FieldValue; break;
                        case 28: rec.ContactAddress.PhoneNumber = col.FieldValue; break;
                        case 29: rec.ContactEmail = col.FieldValue; break;
                        case 30: rec.ContactAddress.HouseNo = col.FieldValue; break;
                        case 31: rec.ContactAddress.CellPhoneNumber = col.FieldValue; break;
                        case 32: rec.ContactDayTimePhoneNo = col.FieldValue; break;
                        case 33: rec.ShipToName = col.FieldValue; break;
                        case 34: rec.ShipToAddress.Address1 = col.FieldValue; break;
                        case 35: rec.ShipToAddress.Address2 = col.FieldValue; break;
                        case 36: rec.ShipToAddress.City = col.FieldValue; break;
                        case 37: rec.ShipToAddress.StateProvinceRegion = col.FieldValue; break;
                        case 38: rec.ShipToAddress.PostCode = col.FieldValue; break;
                        case 39: rec.ShipToAddress.Country = col.FieldValue; break;
                        case 40: rec.ShipToAddress.PhoneNumber = col.FieldValue; break;
                        case 41: rec.ShipToEmail = col.FieldValue; break;
                        case 42: rec.ShipToAddress.HouseNo = col.FieldValue; break;
                        case 43: rec.CreateAtStoreId = col.FieldValue; break;
                        case 44: rec.CreateTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case 45: rec.PointsRewarded = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 46: rec.PointsUsedInOrder = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }

                rec.Posted = false;
                switch (rec.IdType)
                {
                    case DocumentIdType.Receipt:
                        rec.Posted = true;
                        rec.Status = SalesEntryStatus.Complete;
                        rec.ShippingStatus = ShippingStatus.Shipped;
                        rec.ClickAndCollectOrder = string.IsNullOrEmpty(rec.CustomerOrderNo) == false;
                        break;
                    case DocumentIdType.Order:
                        rec.ClickAndCollectOrder = (rec.Lines.Find(l => l.ClickAndCollectLine == true) != null);
                        rec.Status = SalesEntryStatus.Created;
                        rec.ShippingStatus = ShippingStatus.NotYetShipped;
                        break;
                    case DocumentIdType.HospOrder:
                        rec.CreateTime = rec.DocumentRegTime;
                        rec.CreateAtStoreId = rec.StoreId;
                        rec.Status = SalesEntryStatus.Processing;
                        rec.ShippingStatus = ShippingStatus.ShippigNotRequired;
                        break;
                }
                list.Add(rec);
            }
            return list;
        }

        private DocumentIdType GetDocIdType(int type)
        {
            switch (type)
            {
                case 1:
                    return DocumentIdType.Order;
                case 2:
                    return DocumentIdType.HospOrder;
            }
            return DocumentIdType.Receipt;
        }
    }
}
