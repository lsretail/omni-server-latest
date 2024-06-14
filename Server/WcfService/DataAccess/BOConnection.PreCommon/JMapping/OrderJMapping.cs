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
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000818);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return rec;

            ReplODataRecord row = dynDataSet.DataSetRows.FirstOrDefault();

            rec = new SalesEntry()
            {
                ContactAddress = new Address(),
                ShipToAddress = new Address(),
                Lines = new List<SalesEntryLine>(),
                DiscountLines = new List<SalesEntryDiscountLine>(),
                Payments = new List<SalesEntryPayment>()
            };

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

            // get line data
            dynDataSet = result.GetDataSet(10000835);
            if (dynDataSet != null && dynDataSet.DataSetRows.Count > 0)
            {
                foreach (ReplODataRecord line in dynDataSet.DataSetRows)
                {
                    SalesEntryLine recLine = new SalesEntryLine();
                    string cardType = string.Empty;
                    string cardNo = string.Empty;
                    string curCode = string.Empty;
                    decimal curFact = 0;
                    foreach (ReplODataField col in line.Fields)
                    {
                        ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                        if (fld == null)
                            continue;

                        switch (fld.FieldName)
                        {
                            case "Line No.": recLine.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                            case "Entry Type": recLine.LineType = (LineType)ConvertTo.SafeInt(col.FieldValue); break;
                            case "Number": recLine.ItemId = col.FieldValue; break;
                            case "Description": recLine.ItemDescription = col.FieldValue; break;
                            case "Currency Code": curCode = col.FieldValue; break;
                            case "Currency Factor": curFact = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Variant Code": recLine.VariantId = col.FieldValue; break;
                            case "Variant Description": recLine.VariantDescription = col.FieldValue; break;
                            case "Unit of Measure": recLine.UomId = col.FieldValue; break;
                            case "Net Price": recLine.NetPrice = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Price": recLine.Price = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Quantity": recLine.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Discount %": recLine.DiscountPercent = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Discount Amount": recLine.DiscountAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Net Amount": recLine.NetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "VAT Amount": recLine.TaxAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Amount": recLine.Amount = ConvertTo.SafeDecimal(col.FieldValue); break;
                            case "Parent Line": recLine.ParentLine = ConvertTo.SafeInt(col.FieldValue); break;
                            case "Card Type": cardType = col.FieldValue; break;
                            case "Card or Account": cardNo = col.FieldValue; break;
                            case "Store No.": recLine.StoreId = col.FieldValue; break;
                            case "Store Name": recLine.StoreName = col.FieldValue; break;
                            case "External ID": recLine.ExternalId = col.FieldValue; break;
                            case "Click and Collect Line": recLine.ClickAndCollectLine = ConvertTo.SafeBoolean(col.FieldValue); break;
                            case "Image Id": recLine.ItemImageId = col.FieldValue; break;
                        }
                    }

                    if (recLine.LineType == LineType.Payment)
                    {
                        rec.Payments.Add(new SalesEntryPayment()
                        {
                            Amount = recLine.Amount,
                            AmountLCY = recLine.Amount,
                            LineNumber = recLine.LineNumber,
                            TenderType = recLine.ItemId,
                            CardNo = cardNo,
                            CardType = cardType,
                            CurrencyCode = curCode,
                            CurrencyFactor = curFact,
                            Type = PaymentType.Payment
                        });
                    }
                    else
                    {
                        rec.Lines.Add(recLine);
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

                        SalesEntryLine recLine = rec.Lines.Find(l => l.LineNumber == lineNo);
                        if (recLine != null)
                            recLine.ExtraInformation = $"Code:{entryNo} Type:{etype} Pin:{pinNo}";
                    }
                }
                 
                foreach (SalesEntryLine line in rec.Lines)
                {
                    if (line.ClickAndCollectLine && rec.ClickAndCollectOrder == false)
                        rec.ClickAndCollectOrder = true;
                }
            }

            // get discount lines
            dynDataSet = result.GetDataSet(10000836);
            if (dynDataSet != null && dynDataSet.DataSetRows.Count > 0)
            {
                foreach (ReplODataRecord line in dynDataSet.DataSetRows)
                {
                    SalesEntryDiscountLine recDisc = new SalesEntryDiscountLine();
                    recDisc.DiscountType = DiscountType.PeriodicDisc;

                    foreach (ReplODataField col in line.Fields)
                    {
                        ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                        if (fld == null)
                            continue;

                        switch (fld.FieldName)
                        {
                            case "Document ID": recDisc.Id = col.FieldValue; break;
                            case "Document Line No.": recDisc.LineNumber = ConvertTo.SafeInt(col.FieldValue); break;
                            case "Offer Type": recDisc.SetDiscType((OfferDiscountType)ConvertTo.SafeInt(col.FieldValue)); break;
                            case "Offer No.": recDisc.OfferNumber = col.FieldValue; break;
                            case "Description": recDisc.Description = col.FieldValue; break;
                            case "Discount Amount": recDisc.DiscountAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        }
                    }
                    recDisc.PeriodicDiscGroup = recDisc.OfferNumber;
                    rec.DiscountLines.Add(recDisc);
                }
            }

            rec.Posted = false;
            switch (rec.IdType)
            {
                case DocumentIdType.Receipt:
                    rec.Posted = true;
                    rec.Status = SalesEntryStatus.Complete;
                    rec.ShippingStatus = ShippingStatus.Shipped;
                    rec.LineItemCount = (int)rec.Lines.Sum(q => q.Quantity);
                    rec.Quantity = rec.LineItemCount;
                    rec.LineCount = rec.Lines.Count;
                    rec.ClickAndCollectOrder = string.IsNullOrEmpty(rec.CustomerOrderNo) == false;
                    if (string.IsNullOrEmpty(rec.ShipToName))
                    {
                        rec.ShipToName = rec.ContactName;
                        rec.ShipToEmail = rec.ContactEmail;
                    }
                    break;
                case DocumentIdType.Order:
                    rec.ClickAndCollectOrder = (rec.Lines.Find(l => l.ClickAndCollectLine == true) != null);
                    rec.Status = SalesEntryStatus.Created;
                    rec.ShippingStatus = ShippingStatus.NotYetShipped;
                    rec.CreateAtStoreId = rec.StoreId;
                    if (rec.Lines != null && rec.Lines.Count > 0)
                    {
                        rec.StoreId = rec.Lines[0].StoreId;
                        rec.StoreName = rec.Lines[0].StoreName;
                    }
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
            return rec;
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
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Document Source Type": rec.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                        case "Document ID": rec.Id = col.FieldValue; break;
                        case "Store No.": rec.StoreId = col.FieldValue; break;
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
                        case "Name": rec.ContactName = col.FieldValue; break;
                        case "Address": rec.ContactAddress.Address1 = col.FieldValue; break;
                        case "Address 2": rec.ContactAddress.Address2 = col.FieldValue; break;
                        case "City": rec.ContactAddress.City = col.FieldValue; break;
                        case "County": rec.ContactAddress.County = col.FieldValue; break;
                        case "Post Code": rec.ContactAddress.PostCode = col.FieldValue; break;
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
                        case "Created at Store": rec.CreateAtStoreId = col.FieldValue; break;
                        case "Create DateTime": rec.CreateTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Points Rewarded": rec.PointsRewarded = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Points Used In Order": rec.PointsUsedInOrder = ConvertTo.SafeDecimal(col.FieldValue); break;
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
