using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class ItemJMapping : BaseJMapping
    {
        public ItemJMapping(bool json, Version version)
        {
            IsJson = json;
            LSCVersion = version;
        }

        public List<ReplItem> GetReplItems(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItem> list = new List<ReplItem>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItem line = new ReplItem();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 3: line.Description = data.FieldValue; break;
                        case 8: line.BaseUnitOfMeasure = data.FieldValue; break;
                        case 10: line.Type = (ItemType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 18: line.UnitPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 31: line.VendorId = data.FieldValue; break;
                        case 32: line.VendorItemId = data.FieldValue; break;
                        case 41: line.GrossWeight = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 43: line.UnitsPerParcel = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 44: line.UnitVolume = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 47: line.TariffNo = data.FieldValue; break;
                        case 54: line.Blocked = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 95: line.CountryOfOrigin = data.FieldValue; break;
                        case 99: line.TaxItemGroupId = data.FieldValue; break;
                        case 5425: line.SalseUnitOfMeasure = data.FieldValue; break;
                        case 5426: line.PurchUnitOfMeasure = data.FieldValue; break;
                        case 5702: line.ItemCategoryCode = data.FieldValue; break;
                        case 6500: line.ItemTrackingCode = data.FieldValue; break;
                        case 10000703: line.ProductGroupId = data.FieldValue; break;
                        case 10000704: line.SpecialGroups = data.FieldValue; break;
                        case 10001401: line.SeasonCode = data.FieldValue; break;
                        case 99001463: line.ItemFamilyCode = data.FieldValue; break;
                        case 99001480: line.ZeroPriceValId = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 99001484: line.NoDiscountAllowed = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 99001487: line.KeyingInPrice = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 99001490: line.ScaleItem = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 99001491: line.KeyingInQty = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012860: line.Details = ConvertTo.Base64Decode(data.FieldValue); break;
                        case 10012861: line.BlockedOnPos = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012862: line.BlockPurchaseReturn = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012863: line.BlockedOnECom = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012864: line.BlockDiscount = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012865: line.BlockManualPriceChange = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012866: line.BlockNegativeAdjustment = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 10012867: line.BlockPositiveAdjustment = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItem line = new ReplItem()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplPrice> GetReplPrice(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplPrice> list = new List<ReplPrice>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplPrice line = new ReplPrice();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.StoreId = data.FieldValue; break;
                        case 2: line.ItemId = data.FieldValue; break;
                        case 3: line.VariantId = data.FieldValue; break;
                        case 10: line.CustomerDiscountGroup = data.FieldValue; break;
                        case 11: line.LoyaltySchemeCode = data.FieldValue; break;
                        case 12: line.CurrencyCode = data.FieldValue; break;
                        case 19: line.UnitPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 20: line.UnitPriceInclVat = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 21: line.UnitOfMeasure = data.FieldValue; break;
                        case 30: line.ModifyDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 40: line.QtyPerUnitOfMeasure = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplPrice line = new ReplPrice()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplPrice> GetReplBasePrice(string ret, string storeid, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplPrice> list = new List<ReplPrice>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplPrice line = new ReplPrice();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.SaleCode = data.FieldValue; break;
                        case 3: line.CurrencyCode = data.FieldValue; break;
                        case 4: line.StartingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 5: line.UnitPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 6: line.PriceInclVat = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 11: line.VATPostGroup = data.FieldValue; break;
                        case 13: line.SaleType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 14: line.MinimumQuantity = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 15: line.EndingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 5400: line.UnitOfMeasure = data.FieldValue; break;
                        case 5700: line.VariantId = data.FieldValue; break;
                        case 99001450: line.UnitPriceInclVat = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                line.StoreId = storeid;
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplPrice line = new ReplPrice()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.SaleCode = data.FieldValue; break;
                        case 3: line.CurrencyCode = data.FieldValue; break;
                        case 4: line.StartingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 13: line.SaleType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 14: line.MinimumQuantity = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 5400: line.UnitOfMeasure = data.FieldValue; break;
                        case 5700: line.VariantId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplDiscount> GetReplDiscount(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDiscount> list = new List<ReplDiscount>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplDiscount line = new ReplDiscount(IsJson);
                line.Type = ReplDiscountType.Multibuy;
                string p1 = string.Empty;
                string p2 = string.Empty;
                string p3 = string.Empty;
                decimal amt = 0;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.StoreId = data.FieldValue; break;
                        case 2: line.PriorityNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 3: line.ItemId = data.FieldValue; break;
                        case 4: line.VariantId = data.FieldValue; break;
                        case 10: line.CustomerDiscountGroup = data.FieldValue; break;
                        case 11: line.LoyaltySchemeCode = data.FieldValue; break;
                        case 12: line.FromDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 13: line.ToDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 14: line.MinimumQuantity = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 20: line.DiscountValue = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 21: line.UnitOfMeasureId = data.FieldValue; break;
                        case 23: line.OfferNo = data.FieldValue; break;
                        case 30: line.ModifyDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 40: line.Type = (ReplDiscountType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 41: line.Description = data.FieldValue; break;
                        case 42: line.DiscountValueType = (DiscountValueType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 43: p1 = data.FieldValue; break;
                        case 44: p2 = data.FieldValue; break;
                        case 45: p3 = data.FieldValue; break;
                        case 46: line.ValidationPeriodId = ConvertTo.SafeInt(data.FieldValue); break;
                        case 47: amt = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }

                line.Details = p1;
                if (string.IsNullOrEmpty(p2) == false)
                    line.Details += "\r\n" + p2;
                if (string.IsNullOrEmpty(p3) == false)
                    line.Details += "\r\n" + p3;

                if (amt > 0 && line.Type == ReplDiscountType.DiscOffer)
                {
                    line.DiscountValueType = DiscountValueType.Amount;
                    line.DiscountValue = amt;
                }

                if (string.IsNullOrEmpty(line.OfferNo) == false)
                    list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplDiscount line = new ReplDiscount(IsJson)
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 23: line.OfferNo = data.FieldValue; break;
                    }
                }
                if (string.IsNullOrEmpty(line.OfferNo) == false)
                    list.Add(line);
            }
            return list;
        }

        public List<ReplDiscount> GetReplMixMatch(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDiscount> list = new List<ReplDiscount>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplDiscount line = new ReplDiscount(IsJson);
                string p1 = string.Empty;
                string p2 = string.Empty;
                string p3 = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.StoreId = data.FieldValue; break;
                        case 2: line.ItemId = data.FieldValue; break;
                        case 3: line.VariantId = data.FieldValue; break;
                        case 10: line.CustomerDiscountGroup = data.FieldValue; break;
                        case 11: line.LoyaltySchemeCode = data.FieldValue; break;
                        case 12: line.FromDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 13: line.ToDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 20: line.OfferNo = data.FieldValue; break;
                        case 30: line.ModifyDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 40: line.Type = (ReplDiscountType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 41: line.Description = data.FieldValue; break;
                        case 42: line.PriorityNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 43: p1 = data.FieldValue; break;
                        case 44: p2 = data.FieldValue; break;
                        case 45: p3 = data.FieldValue; break;
                        case 46: line.ValidationPeriodId = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }

                line.Details = p1;
                if (string.IsNullOrEmpty(p2) == false)
                    line.Details += "\r\n" + p2;
                if (string.IsNullOrEmpty(p3) == false)
                    line.Details += "\r\n" + p3;

                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplDiscount line = new ReplDiscount(IsJson)
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplDiscountSetup> GetReplDiscountSetup(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDiscountSetup> list = new List<ReplDiscountSetup>();
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplDiscountSetup line = new ReplDiscountSetup(IsJson);
                string pop1 = string.Empty;
                string pop2 = string.Empty;
                string pop3 = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Offer No.": line.OfferNo = data.FieldValue; break;
                        case "Line No.": line.LineNumber = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Type": line.Type = (ReplDiscountType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "No.": line.Number = data.FieldValue; break;
                        case "Variant Code": line.VariantId = data.FieldValue; break;
                        case "Description": line.Description = data.FieldValue; break;
                        case "Standard Price Including VAT": line.StandardPriceInclVAT = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Standard Price": line.StandardPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Split Deal Price/Disc. %": line.SplitDealPriceDiscount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Deal Price/Disc. %": line.DealPriceDiscount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Price Group": line.LinePriceGroup = data.FieldValue; break;
                        case "Currency Code": line.CurrencyCode = data.FieldValue; break;
                        case "Unit of Measure": line.UnitOfMeasureId = data.FieldValue; break;
                        case "Prod. Group Category": line.ProductItemCategory = data.FieldValue; break;
                        case "Valid From Before Exp. Date": line.ValidFromBeforeExpDate = data.FieldValue; break;
                        case "Valid To Before Exp. Date": line.ValidToBeforeExpDate = data.FieldValue; break;
                        case "Line Group": line.LineGroup = data.FieldValue; break;
                        case "No. of Items Needed": line.NumberOfItemNeeded = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Disc. Type": line.IsPercentage = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Discount Amount": line.LineDiscountAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Offer Price": line.OfferPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Offer Price Including VAT": line.OfferPriceInclVAT = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Discount Amount Including VAT": line.LineDiscountAmountInclVAT = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Trigger Pop-up on POS": line.TriggerPopUp = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Exclude": line.Exclude = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Status": line.Enabled = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Priority": line.PriorityNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Validation Period ID": line.ValidationPeriodId = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Discount Type": line.DiscountValueType = (DiscountValueType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "Deal Price Value": line.DealPriceValue = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Discount % Value": line.DiscountValue = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Discount Amount Value": line.DiscountAmountValue = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Customer Disc. Group": line.CustomerDiscountGroup = data.FieldValue; break;
                        case "Amount to Trigger": line.AmountToTrigger = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Member Value": line.LoyaltySchemeCode = data.FieldValue; break;
                        case "Pop-up Line 1": pop1 = data.FieldValue; break;
                        case "Pop-up Line 2": pop2 = data.FieldValue; break;
                        case "Pop-up Line 3": pop3 = data.FieldValue; break;
                        case "Coupon Code": line.CouponCode = data.FieldValue; break;
                        case "Coupon Qty Needed": line.CouponQtyNeeded = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Member Type": line.MemberType = (ReplDiscMemberType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "Member Attribute": line.MemberAttribute = data.FieldValue; break;
                        case "Maximum Discount Amount": line.MaxDiscountAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Tender Type Code": line.TenderTypeCode = data.FieldValue; break;
                        case "Tender Type Value": line.TenderTypeValue = data.FieldValue; break;
                        case "Prompt for Action": line.PromptForAction = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Tender Offer %": line.TenderOffer = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Tender Offer Amount": line.TenderOfferAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Member Points": line.MemberPoints = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }

                line.Details = pop1;
                if (string.IsNullOrEmpty(pop2) == false)
                    line.Details += "\r\n" + pop2;
                if (string.IsNullOrEmpty(pop3) == false)
                    line.Details += "\r\n" + pop3;

                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplDiscountSetup line = new ReplDiscountSetup(IsJson)
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetDel.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Offer No.": line.OfferNo = data.FieldValue; break;
                        case "Line No.": line.LineNumber = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplImage> GetReplImage(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplImage> list = new List<ReplImage>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplImage line = new ReplImage();
                int w = 0;
                int h = 0;
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Description = data.FieldValue; break;
                        case 10: line.MediaId = data.FieldValue.Replace("{", "").Replace("}", ""); break;
                        case 11: line.Image64 = data.FieldValue; break;
                        case 12: h = ConvertTo.SafeInt(data.FieldValue); break;
                        case 13: w = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                line.Size = new ImageSize(w, h);
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplImage line = new ReplImage()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplImageLink> GetReplImageLink(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplImageLink> list = new List<ReplImageLink>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplImageLink line = new ReplImageLink();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 10: line.ImageId = data.FieldValue; break;
                        case 11: line.DisplayOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case 20: line.TableName = data.FieldValue; break;
                        case 21: line.KeyValue = data.FieldValue; break;
                        case 22: line.Description = data.FieldValue; break;
                        case 31: line.ImageDescription = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplImageLink line = new ReplImageLink()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1:
                            string[] recid = data.FieldValue.Split(':');
                            line.TableName = recid[0].Trim();
                            line.KeyValue = recid[1].Trim();
                            break;
                        case 10: line.ImageId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemModifier> GetReplModifier(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemModifier> list = new List<ReplItemModifier>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemModifier line = new ReplItemModifier();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Code = data.FieldValue; break;
                        case 3: line.SubCode = data.FieldValue; break;
                        case 10: line.ExplanatoryHeaderText = data.FieldValue; break;
                        case 11: line.Prompt = data.FieldValue; break;
                        case 12: line.GroupMinSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 13: line.GroupMaxSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        //case 30: line.ItemNo = data.FieldValue; break;
                        case 31: line.VariantCode = data.FieldValue; break;
                        case 32: line.Description = data.FieldValue; break;
                        case 33: line.MinSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 34: line.MaxSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 35: line.AlwaysCharge = (ItemModifierPriceHandling)ConvertTo.SafeInt(data.FieldValue); break;
                        case 36: line.PriceType = (ItemModifierPriceType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 37: line.AmountPercent = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 38: line.UnitOfMeasure = data.FieldValue; break;
                        //case 39: line.QtrPerUOM = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 40: line.TimeModifierMinutes = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemModifier line = new ReplItemModifier()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Code = data.FieldValue; break;
                        case 3: line.SubCode = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemRecipe> GetReplRecipe(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemRecipe> list = new List<ReplItemRecipe>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemRecipe line = new ReplItemRecipe();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.RecipeNo = data.FieldValue; break;
                        case 2: line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.Description = data.FieldValue; break;
                        case 7: line.UnitOfMeasure = data.FieldValue; break;
                        case 8: line.QuantityPer = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 10012113: line.ItemNo = data.FieldValue; break;
                        case 10012115: line.Exclusion = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 10012116: line.ExclusionPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 10012860: line.ImageId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemRecipe line = new ReplItemRecipe()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.RecipeNo = data.FieldValue; break;
                        case 2: line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplBarcode> GetReplBarcode(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplBarcode> list = new List<ReplBarcode>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplBarcode line = new ReplBarcode();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 10: line.Id = data.FieldValue; break;
                        case 25: line.Description = data.FieldValue; break;
                        case 110: line.VariantId = data.FieldValue; break;
                        case 200: line.UnitOfMeasure = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplBarcode line = new ReplBarcode()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 10: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemVariantRegistration> GetReplItemVariantReg(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemVariantRegistration> list = new List<ReplItemVariantRegistration>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemVariantRegistration line = new ReplItemVariantRegistration();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.FrameworkCode = data.FieldValue; break;
                        case 10: line.VariantDimension1 = data.FieldValue; break;
                        case 11: line.VariantDimension2 = data.FieldValue; break;
                        case 12: line.VariantDimension3 = data.FieldValue; break;
                        case 13: line.VariantDimension4 = data.FieldValue; break;
                        case 14: line.VariantDimension5 = data.FieldValue; break;
                        case 15: line.VariantDimension6 = data.FieldValue; break;
                        case 20: line.VariantId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemVariantRegistration line = new ReplItemVariantRegistration()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 10: line.VariantDimension1 = data.FieldValue; break;
                        case 11: line.VariantDimension2 = data.FieldValue; break;
                        case 12: line.VariantDimension3 = data.FieldValue; break;
                        case 13: line.VariantDimension4 = data.FieldValue; break;
                        case 14: line.VariantDimension5 = data.FieldValue; break;
                        case 15: line.VariantDimension6 = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemVariantRegistration> GetReplItemVariantWithStatus(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemVariantRegistration> list = new List<ReplItemVariantRegistration>();
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplItemVariantRegistration line = new ReplItemVariantRegistration();
                string lcy = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": line.ItemId = data.FieldValue; break;
                        case "Framework Code": line.FrameworkCode = data.FieldValue; break;
                        case "Variant Dimension 1": line.VariantDimension1 = data.FieldValue; break;
                        case "Variant Dimension 2": line.VariantDimension2 = data.FieldValue; break;
                        case "Variant Dimension 3": line.VariantDimension3 = data.FieldValue; break;
                        case "Variant Dimension 4": line.VariantDimension4 = data.FieldValue; break;
                        case "Variant Dimension 5": line.VariantDimension5 = data.FieldValue; break;
                        case "Variant Dimension 6": line.VariantDimension6 = data.FieldValue; break;
                        case "Variant": line.VariantId = data.FieldValue; break;
                        case "Block Sale on POS": line.BlockedOnPos = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Blocked on eCommerce": line.BlockedOnECom = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplItemVariantRegistration line = new ReplItemVariantRegistration()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetDel.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": line.ItemId = data.FieldValue; break;
                        case "Variant Dimension 1": line.VariantDimension1 = data.FieldValue; break;
                        case "Variant Dimension 2": line.VariantDimension2 = data.FieldValue; break;
                        case "Variant Dimension 3": line.VariantDimension3 = data.FieldValue; break;
                        case "Variant Dimension 4": line.VariantDimension4 = data.FieldValue; break;
                        case "Variant Dimension 5": line.VariantDimension5 = data.FieldValue; break;
                        case "Variant Dimension 6": line.VariantDimension6 = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemVariant> GetReplItemVariant(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemVariant> list = new List<ReplItemVariant>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemVariant line = new ReplItemVariant();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.VariantId = data.FieldValue; break;
                        case 2: line.ItemId = data.FieldValue; break;
                        case 3: line.Description = data.FieldValue; break;
                        case 4: line.Description2 = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemVariant line = new ReplItemVariant()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.VariantId = data.FieldValue; break;
                        case 2: line.ItemId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplLoyVendorItemMapping> GetReplVendorItem(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplLoyVendorItemMapping> list = new List<ReplLoyVendorItemMapping>();
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplLoyVendorItemMapping line = new ReplLoyVendorItemMapping();
                line.IsFeaturedProduct = true;
                line.DisplayOrder = 1;
                string lcy = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "ItemNo": line.NavProductId = data.FieldValue; break;
                        case "VendorNo": line.NavManufacturerId = data.FieldValue; break;
                        case "VendorItemNo": line.NavManufacturerItemId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplLoyVendorItemMapping line = new ReplLoyVendorItemMapping()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetDel.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "ItemNo": line.NavProductId = data.FieldValue; break;
                        case "VendorNo": line.NavManufacturerId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }


        public List<ReplUnitOfMeasure> GetReplUOM(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplUnitOfMeasure> list = new List<ReplUnitOfMeasure>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplUnitOfMeasure line = new ReplUnitOfMeasure();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Description = data.FieldValue; break;
                    }
                }
                line.ShortDescription = line.Description;
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplUnitOfMeasure line = new ReplUnitOfMeasure()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplExtendedVariantValue> GetReplExtVariants(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplExtendedVariantValue> list = new List<ReplExtendedVariantValue>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplExtendedVariantValue line = new ReplExtendedVariantValue();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 5: line.FrameworkCode = data.FieldValue; break;
                        case 9: line.Code = data.FieldValue; break;
                        case 10: line.Dimensions = data.FieldValue; break;
                        case 11: line.Value = data.FieldValue; break;
                        case 16: line.LogicalOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case 23: line.ValueDescription = data.FieldValue; break;
                        case 40: line.DimensionLogicalOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case 41: line.CodeDescription = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplExtendedVariantValue line = new ReplExtendedVariantValue()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 5: line.FrameworkCode = data.FieldValue; break;
                        case 9: line.Code = data.FieldValue; break;
                        case 11: line.Value = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemUnitOfMeasure> GetReplItemUOM(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemUnitOfMeasure> list = new List<ReplItemUnitOfMeasure>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemUnitOfMeasure line = new ReplItemUnitOfMeasure();
                line.QtyPrUOM = 1;
                line.Order = 1;
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.Code = data.FieldValue; break;
                        case 3: line.QtyPrUOM = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 99001455: line.CountAsOne = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 99001456: line.Selection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 99001457: line.Order = ConvertTo.SafeInt(data.FieldValue); break;
                        //case 0: line.Description = data.FieldValue; break;
                    }
                }
                line.ShortDescription = line.Code;
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemUnitOfMeasure line = new ReplItemUnitOfMeasure()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.Code = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemUnitOfMeasure> GetReplItemUOM2(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemUnitOfMeasure> list = new List<ReplItemUnitOfMeasure>();
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplItemUnitOfMeasure line = new ReplItemUnitOfMeasure();
                string lcy = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": line.ItemId = data.FieldValue; break;
                        case "Code": line.Code = data.FieldValue; break;
                        case "Qty. per Unit of Measure": line.QtyPrUOM = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "LSC Count as 1 on Receipt": line.CountAsOne = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "LSC POS Selection": line.Selection = ConvertTo.SafeInt(data.FieldValue); break;
                        case "LSC Order": line.Order = ConvertTo.SafeInt(data.FieldValue); break;
                        case "LSC Ecom Selection": line.EComSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Description": line.Description = data.FieldValue; break;
                    }
                }
                line.ShortDescription = line.Code;
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplItemUnitOfMeasure line = new ReplItemUnitOfMeasure()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetDel.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": line.ItemId = data.FieldValue; break;
                        case "Code": line.Code = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplProductGroup> GetReplProductGroup(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplProductGroup> list = new List<ReplProductGroup>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplProductGroup line = new ReplProductGroup();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemCategoryID = data.FieldValue; break;
                        case 2: line.Id = data.FieldValue; break;
                        case 3: line.Description = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplProductGroup line = new ReplProductGroup()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemCategoryID = data.FieldValue; break;
                        case 2: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemCategory> GetReplItemCatagory(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemCategory> list = new List<ReplItemCategory>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemCategory line = new ReplItemCategory();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 3: line.Description = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemCategory line = new ReplItemCategory()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplItemLocation> GetReplItemLocation(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplItemLocation> list = new List<ReplItemLocation>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplItemLocation line = new ReplItemLocation();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.StoreId = data.FieldValue; break;
                        case 5: line.SectionCode = data.FieldValue; break;
                        case 6: line.SectionDescription = data.FieldValue; break;
                        case 10: line.ShelfCode = data.FieldValue; break;
                        case 11: line.ShelfDescription = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplItemLocation line = new ReplItemLocation()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.ItemId = data.FieldValue; break;
                        case 2: line.StoreId = data.FieldValue; break;
                        case 5: line.SectionCode = data.FieldValue; break;
                        case 10: line.ShelfCode = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplAttribute> GetReplAttribute(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplAttribute> list = new List<ReplAttribute>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplAttribute line = new ReplAttribute();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Code = data.FieldValue; break;
                        case 5: line.Description = data.FieldValue; break;
                        case 20: line.ValueType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 21: line.DefaultValue = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplAttribute line = new ReplAttribute()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Code = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplAttributeValue> GetReplAttributeValue(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplAttributeValue> list = new List<ReplAttributeValue>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplAttributeValue line = new ReplAttributeValue();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.LinkField1 = data.FieldValue; break;
                        case 2: line.Code = data.FieldValue; break;
                        case 3: line.Value = data.FieldValue; break;
                        case 4: line.Sequence = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.LinkType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.LinkField2 = data.FieldValue; break;
                        case 7: line.LinkField3 = data.FieldValue; break;
                        case 8: line.NumbericValue = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplAttributeValue line = new ReplAttributeValue()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.LinkField1 = data.FieldValue; break;
                        case 2: line.Code = data.FieldValue; break;
                        case 4: line.Sequence = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.LinkType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.LinkField2 = data.FieldValue; break;
                        case 7: line.LinkField3 = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplAttributeOptionValue> GetReplAttributeOptionValue(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplAttributeOptionValue> list = new List<ReplAttributeOptionValue>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplAttributeOptionValue line = new ReplAttributeOptionValue();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Code = data.FieldValue; break;
                        case 2: line.Sequence = ConvertTo.SafeInt(data.FieldValue); break;
                        case 3: line.Value = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplAttributeOptionValue line = new ReplAttributeOptionValue()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Code = data.FieldValue; break;
                        case 2: line.Sequence = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplDataTranslation> GetReplHtml(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDataTranslation> list = new List<ReplDataTranslation>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplDataTranslation line = new ReplDataTranslation();
                line.TranslationId = "T0010001410-F0000000020";

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Key = data.FieldValue; break;
                        case 20: line.Text = ConvertTo.Base64Decode(data.FieldValue); break;
                        case 30: line.LanguageCode = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplDataTranslation line = new ReplDataTranslation()
                {
                    IsDeleted = true,
                    TranslationId = "T0010001410-F0000000020"
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Key = data.FieldValue; break;
                        case 30: line.LanguageCode = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplInvStatus> GetReplInvStatus(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplInvStatus> list = new List<ReplInvStatus>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplInvStatus line = new ReplInvStatus();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 10: line.ItemId = data.FieldValue; break;
                        case 15: line.VariantId = data.FieldValue; break;
                        case 25: line.StoreId = data.FieldValue; break;
                        case 120: line.Quantity += ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 123: line.Quantity += ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplInvStatus line = new ReplInvStatus()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 10: line.ItemId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }
    }
}
