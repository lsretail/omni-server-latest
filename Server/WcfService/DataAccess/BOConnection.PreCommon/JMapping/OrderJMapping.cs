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

        public SalesEntry GetSalesEntry(string ret)
        {
            SalesEntry rec = null;
            WSODataCollection result = JsonToWSOData(ret, "SelectedSalesDocDaCo");
            if (result == null)
                return rec;

            // get header data
            List<SalesEntry> list = LoadSaleEntries(result.GetDataSet(10000818));
            if (list.Count == 0)
                return rec;

            rec = list.FirstOrDefault();
            LoadSaleEntryLines(result, rec);
            rec.DiscountLines = LoadSaleDiscounts(result.GetDataSet(10000836));

            switch (rec.IdType)
            {
                case DocumentIdType.Receipt:
                    rec.LineItemCount = (int)rec.Lines.Sum(q => q.Quantity);
                    rec.Quantity = rec.LineItemCount;
                    rec.LineCount = rec.Lines.Count;
                    break;
                case DocumentIdType.Order:
                    if (rec.Lines != null && rec.Lines.Count > 0)
                    {
                        rec.StoreId = rec.Lines[0].StoreId;
                        rec.StoreName = rec.Lines[0].StoreName;
                    }
                    break;
            }
            return rec;
        }

        public List<SalesEntry> GetSalesEntryHistory(string ret)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            WSODataCollection result = JsonToWSOData(ret, "MemberContactSalesHistoryDaCo");
            if (result == null)
                return list;

            // get data
            list = LoadSaleEntries(result.GetDataSet(10000818));
            return list;
        }

        public SalesEntryList GetSalesEntrySales(string ret, bool getCardId, out string cardId)
        {
            SalesEntryList entry = new SalesEntryList();
            cardId = string.Empty;

            WSODataCollection result = JsonToWSOData(ret, "SalesInfoDaCo");
            if (result == null)
                return entry;

            if (getCardId)
                cardId = JsonToWSODataValue(ret, "CardNo");

            List<SalesEntry> entries = LoadSaleEntries(result.GetDataSet(10000818));
            foreach (SalesEntry rec in entries)
            {
                LoadSaleEntryLines(result, rec);
                rec.DiscountLines = LoadSaleDiscounts(result.GetDataSet(10000836));
            }
            entry.Order = entries.Find(e => e.IdType == DocumentIdType.Order);
            entry.SalesEntries = entries.FindAll(e => e.IdType == DocumentIdType.Receipt);
            entry.Shipments = LoadShipment(result.GetDataSet(110));
            List<SalesEntryShipmentLine> shipLines = LoadShipmentLine(result.GetDataSet(111));
            foreach (SalesEntryShipment rec in entry.Shipments)
            {
                rec.Lines = shipLines.FindAll(l => l.DocId == rec.Id);
            }
            return entry;
        }

        private List<SalesEntry> LoadSaleEntries(ReplODataSetRecRef dynDataSet)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntry rec = new SalesEntry();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Document Source Type": rec.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                        case "Document ID": rec.Id = col.FieldValue; break;
                        case "Store No.": rec.StoreId = col.FieldValue; break;
                        case "Created at Store": rec.CreateAtStoreId = col.FieldValue; break;
                        case "Date Time": rec.DocumentRegTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Sale Is Return Sale": rec.ReturnSale = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Customer Document ID": rec.CustomerOrderNo = col.FieldValue; break;
                        case "POS Terminal No.": rec.TerminalId = col.FieldValue; break;
                        case "Number of Items": rec.LineItemCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Number of Lines": rec.LineCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Net Amount": rec.TotalNetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Gross Amount": rec.TotalAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Discount Amount": rec.TotalDiscount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Member Card No.": rec.CardId = col.FieldValue; break;
                        case "Customer No.": rec.CustomerId = col.FieldValue; break;
                        case "External ID": rec.ExternalId = col.FieldValue; break;
                        case "Refund Receipt No.": rec.HasReturnSale = col.FieldValue != string.Empty; break;
                        case "Quantity": rec.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Store Name": rec.StoreName = col.FieldValue; break;
                        case "Store Currency Code": rec.StoreCurrency = col.FieldValue; break;
                        case "Create DateTime": rec.CreateTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Points Rewarded": rec.PointsRewarded = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Points Used In Order": rec.PointsUsedInOrder = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Posted": rec.Posted = ConvertTo.SafeBoolean(col.FieldValue); break;

                        case "Name": rec.ContactName = col.FieldValue; break;
                        case "Address": rec.ContactAddress.Address1 = col.FieldValue; break;
                        case "Address 2": rec.ContactAddress.Address2 = col.FieldValue; break;
                        case "City": rec.ContactAddress.City = col.FieldValue; break;
                        case "County": rec.ContactAddress.County = col.FieldValue; break;
                        case "Post Code": rec.ContactAddress.PostCode = col.FieldValue; break;
                        case "Territory Code": rec.ContactAddress.StateProvinceRegion = col.FieldValue; break;
                        case "Country/Region Code": rec.ContactAddress.Country = col.FieldValue; break;
                        case "Phone No.": rec.ContactAddress.PhoneNumber = col.FieldValue; break;
                        case "Email": rec.ContactEmail = col.FieldValue; break;
                        case "House/Apartment No.": rec.ContactAddress.HouseNo = col.FieldValue; break;
                        case "Mobile Phone No.": rec.ContactAddress.CellPhoneNumber = col.FieldValue; break;
                        case "Daytime Phone No.": rec.ContactDayTimePhoneNo = col.FieldValue; break;

                        case "Ship-to Name": rec.ShipToName = col.FieldValue; break;
                        case "Ship-to Address": rec.ShipToAddress.Address1 = col.FieldValue; break;
                        case "Ship-to Address 2": rec.ShipToAddress.Address2 = col.FieldValue; break;
                        case "Ship-to City": rec.ShipToAddress.City = col.FieldValue; break;
                        case "Ship-to County": rec.ShipToAddress.County = col.FieldValue; break;
                        case "Ship-to Post Code": rec.ShipToAddress.PostCode = col.FieldValue; break;
                        case "Ship-to Country/Region Code": rec.ShipToAddress.Country = col.FieldValue; break;
                        case "Ship-to Phone No.": rec.ShipToAddress.PhoneNumber = col.FieldValue; break;
                        case "Ship-to Email": rec.ShipToEmail = col.FieldValue; break;
                        case "Ship-to House/Apartment No.": rec.ShipToAddress.HouseNo = col.FieldValue; break;
                        case "Shipping Agent Code": rec.ShippingAgentCode = col.FieldValue; break;
                        case "Shipping Agent Service Code": rec.ShippingAgentServiceCode = col.FieldValue; break;
                    }
                }

                switch (rec.IdType)
                {
                    case DocumentIdType.Receipt:
                        rec.Status = SalesEntryStatus.Complete;
                        rec.ShippingStatus = ShippingStatus.Shipped;
                        rec.ClickAndCollectOrder = string.IsNullOrEmpty(rec.CustomerOrderNo) == false;
                        if (string.IsNullOrEmpty(rec.ShipToName))
                        {
                            rec.ShipToName = rec.ContactName;
                            rec.ShipToEmail = rec.ContactEmail;
                        }
                        break;
                    case DocumentIdType.Order:
                        rec.ClickAndCollectOrder = (rec.Lines.Find(l => l.ClickAndCollectLine == true) != null);
                        rec.Status = (rec.ReturnSale) ? SalesEntryStatus.Canceled : SalesEntryStatus.Created;
                        rec.ShippingStatus = ShippingStatus.NotYetShipped;
                        rec.CreateAtStoreId = rec.StoreId;
                        break;
                    case DocumentIdType.HospOrder:
                        rec.CreateTime = rec.DocumentRegTime;
                        rec.CreateAtStoreId = rec.StoreId;
                        rec.Status = SalesEntryStatus.Processing;
                        rec.ShippingStatus = ShippingStatus.ShippigNotRequired;
                        if (string.IsNullOrEmpty(rec.ShipToName))
                        {
                            rec.ShipToName = rec.ContactName;
                            rec.ShipToEmail = rec.ContactEmail;
                        }
                        break;
                }
                list.Add(rec);
            }
            return list;
        }

        private void LoadSaleEntryLines(WSODataCollection result, SalesEntry entry)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000835);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntryLine rec = new SalesEntryLine();
                string docId = string.Empty;
                string cardType = string.Empty;
                string cardNo = string.Empty;
                string curCode = string.Empty;
                decimal curFact = 0;

                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Line No.": rec.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Entry Type": rec.LineType = (LineType)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Number": rec.ItemId = col.FieldValue; break;
                        case "Description": rec.ItemDescription = col.FieldValue; break;
                        case "Variant Code": rec.VariantId = col.FieldValue; break;
                        case "Variant Description": rec.VariantDescription = col.FieldValue; break;
                        case "Unit of Measure": rec.UomId = col.FieldValue; break;
                        case "Net Price": rec.NetPrice = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Price": rec.Price = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Quantity": rec.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Discount %": rec.DiscountPercent = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Discount Amount": rec.DiscountAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Net Amount": rec.NetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "VAT Amount": rec.TaxAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Amount": rec.Amount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Parent Line": rec.ParentLine = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Store No.": rec.StoreId = col.FieldValue; break;
                        case "Store Name": rec.StoreName = col.FieldValue; break;
                        case "External ID": rec.ExternalId = col.FieldValue; break;
                        case "Click and Collect Line": rec.ClickAndCollectLine = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Image Id": rec.ItemImageId = col.FieldValue; break;

                        case "Document ID": docId = col.FieldValue; break;
                        case "Currency Code": curCode = col.FieldValue; break;
                        case "Currency Factor": curFact = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Card Type": cardType = col.FieldValue; break;
                        case "Card or Account": cardNo = col.FieldValue; break;
                    }
                }

                if (docId != entry.Id)
                    continue;

                if (rec.LineType == LineType.Payment)
                {
                    entry.Payments.Add(new SalesEntryPayment()
                    {
                        Amount = rec.Amount,
                        AmountLCY = rec.Amount,
                        LineNumber = rec.LineNumber,
                        TenderType = rec.ItemId,
                        CardNo = cardNo,
                        CardType = cardType,
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

            dynDataSet = result.GetDataSet(10000885);
            if (dynDataSet != null && dynDataSet.DataSetRows.Count > 0)
            {
                string entryNo = string.Empty;
                string etype = string.Empty;
                string pinNo = string.Empty;
                int lineNo = 0;

                foreach (ReplODataRecord line in dynDataSet.DataSetRows)
                {
                    foreach (ReplODataField col in line.Fields)
                    {
                        ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                        if (fld == null)
                            continue;

                        switch (fld.FieldName)
                        {
                            case "Entry Type": etype = col.FieldValue; break;
                            case "Entry Code": entryNo = col.FieldValue; break;
                            case "Created by Line No.": lineNo = ConvertTo.SafeInt(col.FieldValue); break;
                            case "PIN": pinNo = col.FieldValue; break;
                        }
                    }

                    SalesEntryLine recLine = entry.Lines.Find(l => l.LineNumber == lineNo);
                    if (recLine != null)
                        recLine.ExtraInformation = $"Code:{entryNo} Type:{etype} Pin:{pinNo}";
                }
            }

            foreach (SalesEntryLine line in entry.Lines)
            {
                if (line.ClickAndCollectLine && entry.ClickAndCollectOrder == false)
                    entry.ClickAndCollectOrder = true;
            }
        }

        private List<SalesEntryDiscountLine> LoadSaleDiscounts(ReplODataSetRecRef dynDataSet)
        {
            List<SalesEntryDiscountLine> list = new List<SalesEntryDiscountLine>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntryDiscountLine rec = new SalesEntryDiscountLine();
                rec.DiscountType = DiscountType.PeriodicDisc;

                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Document ID": rec.Id = col.FieldValue; break;
                        case "Document Line No.": rec.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Offer Type": rec.SetDiscType((OfferDiscountType)ConvertTo.SafeInt(col.FieldValue)); break;
                        case "Offer No.": rec.OfferNumber = col.FieldValue; break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Discount Amount": rec.DiscountAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }

                rec.PeriodicDiscGroup = rec.OfferNumber;
                list.Add(rec);
            }
            return list;
        }

        private List<SalesEntryShipment> LoadShipment(ReplODataSetRecRef dynDataSet)
        {
            List<SalesEntryShipment> list = new List<SalesEntryShipment>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntryShipment rec = new SalesEntryShipment();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "No.": rec.Id = col.FieldValue; break;
                        case "Shipment Date": rec.ShipmentDate = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Shipment Method Code": rec.ShipmentMethodCode = col.FieldValue; break;
                        case "Order No.": rec.DocumentId = col.FieldValue; break;
                        case "External Document No.": rec.ExternalId = col.FieldValue; break;
                        case "Shipping Agent Code": rec.AgentCode = col.FieldValue; break;
                        case "Shipping Agent Service Code": rec.AgentServiceCode = col.FieldValue; break;
                        case "Package Tracking No.": rec.TrackingID = col.FieldValue; break;
                        case "Ship-to Name": rec.Name = col.FieldValue; break;
                        case "Ship-to Contact": rec.Contact = col.FieldValue; break;

                        case "Ship-to Address": rec.Address.Address1 = col.FieldValue; break;
                        case "Ship-to Address 2": rec.Address.Address2 = col.FieldValue; break;
                        case "Ship-to City": rec.Address.City = col.FieldValue; break;
                        case "Ship-to Post Code": rec.Address.PostCode = col.FieldValue; break;
                        case "Ship-to County": rec.Address.County = col.FieldValue; break;
                        case "Ship-to Country/Region Code": rec.Address.Country = col.FieldValue; break;
                    }
                }

                list.Add(rec);
            }
            return list;
        }

        private List<SalesEntryShipmentLine> LoadShipmentLine(ReplODataSetRecRef dynDataSet)
        {
            List<SalesEntryShipmentLine> list = new List<SalesEntryShipmentLine>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntryShipmentLine rec = new SalesEntryShipmentLine();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Document No.": rec.DocId = col.FieldValue; break;
                        case "Line No.": rec.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                        case "No.": rec.ItemId = col.FieldValue; break;
                        case "Description": rec.ItemDescription = col.FieldValue; break;
                        case "Variant Code": rec.VariantId = col.FieldValue; break;
                        case "Unit of Measure Code": rec.UomId = col.FieldValue; break;
                        case "Quantity": rec.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
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
