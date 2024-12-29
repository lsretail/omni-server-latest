using System;
using System.Linq;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
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

        public List<ReplPrice> GetReplBasePrice(string ret, string storeId, ref string lastKey, ref int recordsRemaining)
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
                line.StoreId = storeId;
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
                        case 46: line.ValidationPeriodId = data.FieldValue; break;
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
                        case 46: line.ValidationPeriodId = data.FieldValue; break;
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
                bool has1 = false;
                bool has2 = false;
                bool has3 = false;
                bool has4 = false;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "No.":
                            if (has1 == false)
                                line.Number = string.IsNullOrEmpty(line.Number) ? data.FieldValue : line.Number; 
                            has1 = true;
                            break;
                        case "Description":
                            if (has2)
                                line.Description = data.FieldValue;
                            has2 = true;
                            break;
                        case "Type":
                            if (has3)
                                line.Type = (ReplDiscountType)ConvertTo.SafeInt(data.FieldValue);
                            else
                                line.LineType = (ReplDiscountLineType)ConvertTo.SafeInt(data.FieldValue);
                            has3 = true;
                            break;
                        case "Member Points":
                            if (has4 == false)
                                line.MemberPoints = ConvertTo.SafeDecimal(data.FieldValue);
                            has4 = true;
                            break;

                        case "Offer No.": line.OfferNo = data.FieldValue; break;
                        case "Line No.": line.LineNumber = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Variant Code": line.VariantId = data.FieldValue; break;
                        case "Variant Type": line.VariantType = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Standard Price Including VAT": line.StandardPriceInclVAT = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Standard Price": line.StandardPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Split Deal Price/Disc. %": line.SplitDealPriceDiscount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Deal Price/Disc. %": line.DealPriceDiscount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Price Group": line.LinePriceGroup = data.FieldValue; break;
                        case "Head Price Group": line.PriceGroup = data.FieldValue; break;
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
                        case "Validation Period ID": line.ValidationPeriodId = data.FieldValue; break;
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
                        case "Store Group Codes": line.StoreGroupCodes = data.FieldValue; break;
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
                            string[] recId = data.FieldValue.Split(':');
                            line.TableName = recId[0].Trim();
                            line.KeyValue = recId[1].Trim();
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

        public List<ReplItemCategory> GetReplItemCategory(string ret, ref string lastKey, ref int recordsRemaining)
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
                        case 123: 
                            if (LSCVersion < new Version("24.0"))
                                line.Quantity += ConvertTo.SafeDecimal(data.FieldValue); 
                            break;
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

        public LoyItem GetItem(string ret, bool inclDetail)
        {
            WSODataCollection result = JsonToWSOData(ret, "ItemInfo");
            if (result == null)
                return null;

            // get data
            LoyItem item = LoadItem(result.GetDataSet(27));
            if (item == null)
                return item;

            item.Details = LoadItemHtml(result.GetDataSet(10001410));
            item.SpecialGroups = LoadItemSpecialGroup(result.GetDataSet(10000736));
            item.Images = LoadImage(result.GetDataSet(99009064), item.Id);
            xItemStatus stat = LoadStatus(result.GetDataSet(10001404), item.Id, string.Empty);
            if (stat != null)
            {
                item.BlockDiscount = stat.blockDiscount;
                item.BlockManualPriceChange = stat.blockManPrice;
                item.AllowedToSell = stat.blockPOS;
            }

            if (inclDetail)
            {
                item.Prices = LoadItemPrice(result.GetDataSet(10012861));
                item.Locations = LoadItemLocation(result);
                item.UnitOfMeasures = LoadItemUOM(result);
                item.VariantsRegistration = LoadItemVarReg(result);
                item.VariantsExt = LoadItemExtVar(result.GetDataSet(10001413));
                item.ItemAttributes = LoadItemAttribute(result);
                item.Recipes = LoadItemRecipe(result);
                item.Modifiers = LoadItemModify(result);
            }
            return item;
        }

        private LoyItem LoadItem(ReplODataSetRecRef dynDataSet)
        {
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return null;

            ReplODataRecord row = dynDataSet.DataSetRows[0];
            LoyItem rec = new LoyItem();

            foreach (ReplODataField col in row.Fields)
            {
                ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                if (fld == null)
                    continue;

                switch (fld.FieldName)
                {
                    case "No.": rec.Id = col.FieldValue; break;
                    case "Description": rec.Description = col.FieldValue; break;
                    case "Sales Unit of Measure": rec.SalesUomId = col.FieldValue; break;
                    case "Gross Weight": rec.GrossWeight = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case "Item Tracking Code": rec.ItemTrackingCode = col.FieldValue; break;
                    case "Item Category Code": rec.ItemCategoryCode = col.FieldValue; break;
                    case "Units per Parcel": rec.UnitsPerParcel = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case "Unit Volume": rec.UnitVolume = ConvertTo.SafeDecimal(col.FieldValue); break;
                    case "LSC Scale Item": rec.ScaleItem = ConvertTo.SafeBoolean(col.FieldValue); break;
                    case "LSC Retail Product Code": rec.ProductGroupId = col.FieldValue; break;
                    case "LSC Season Code": rec.SeasonCode = col.FieldValue; break;
                    case "LSC Item Family Code": rec.ItemFamilyCode = col.FieldValue; break;
                    case "Tariff No.": rec.TariffNo = col.FieldValue; break;
                    case "Blocked": rec.Blocked = ConvertTo.SafeBoolean(col.FieldValue); break;
                }
            }
            return rec;
        }

        private List<Price> LoadItemPrice(ReplODataSetRecRef dynDataSet)
        {
            List<Price> list = new List<Price>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Price rec = new Price();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Store No.": rec.StoreId = col.FieldValue; break;
                        case "Item No.": rec.ItemId = col.FieldValue; break;
                        case "Variant Code": rec.VariantId = col.FieldValue; break;
                        case "Unit of Measure Code": rec.UomId = col.FieldValue; break;
                        case "Net Unit Price": rec.NetAmt = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Unit Price":
                            rec.Amount = col.FieldValue;
                            rec.Amt = ConvertTo.SafeDecimal(col.FieldValue); 
                            break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<VariantRegistration> LoadItemVarReg(WSODataCollection result)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10001414);
            List<VariantRegistration> list = new List<VariantRegistration>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                VariantRegistration rec = new VariantRegistration();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": rec.ItemId = col.FieldValue; break;
                        case "Variant Dimension 1": rec.Dimension1 = col.FieldValue; break;
                        case "Variant Dimension 2": rec.Dimension2 = col.FieldValue; break;
                        case "Variant Dimension 3": rec.Dimension3 = col.FieldValue; break;
                        case "Variant Dimension 4": rec.Dimension4 = col.FieldValue; break;
                        case "Variant Dimension 5": rec.Dimension5 = col.FieldValue; break;
                        case "Variant Dimension 6": rec.Dimension6 = col.FieldValue; break;
                        case "Framework Code": rec.FrameworkCode = col.FieldValue; break;
                        case "Variant": rec.Id = col.FieldValue; break;
                    }
                }

                rec.Images = LoadImage(result.GetDataSet(99009064), $"{rec.ItemId},{rec.Id}");
                list.Add(rec);
            }
            return list;
        }

        private List<VariantExt> LoadItemExtVar(ReplODataSetRecRef dynDataSet)
        {
            List<VariantExt> tmpList = new List<VariantExt>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return tmpList;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                string frameCode = string.Empty;
                string value = string.Empty;
                VariantExt rec = new VariantExt();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": rec.ItemId = col.FieldValue; break;
                        case "Code": rec.Code = col.FieldValue; break;
                        case "Value": rec.tmpValue = col.FieldValue; break;
                        case "Dimension": rec.Dimension = col.FieldValue; break;
                        case "Logical Order": rec.DisplayOrder = ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }
                tmpList.Add(rec);
            }

            List<VariantExt> list = new List<VariantExt>();
            foreach (VariantExt var in tmpList.FindAll(x => string.IsNullOrEmpty(x.tmpValue)))
            {
                VariantExt rec = new VariantExt()
                {
                    ItemId = var.ItemId,
                    Code = var.Code,
                    Dimension = var.Dimension
                };
                
                foreach (VariantExt val in tmpList.FindAll(x => (string.IsNullOrEmpty(x.tmpValue) == false) && x.Dimension == rec.Dimension))
                {
                    rec.Values.Add(new DimValue()
                    {
                        Value = val.tmpValue,
                        DisplayOrder = val.DisplayOrder
                    });
                }
                list.Add(rec);
            }
            return list;
        }

        private List<RetailAttribute> LoadItemAttribute(WSODataCollection result)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000786);
            List<RetailAttribute> list = new List<RetailAttribute>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                RetailAttribute rec = new RetailAttribute();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Link Type": rec.LinkType = (AttributeLinkType)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Link Field 1": rec.LinkField1 = col.FieldValue; break;
                        case "Link Field 2": rec.LinkField2 = col.FieldValue; break;
                        case "Link Field 3": rec.LinkField3 = col.FieldValue; break;
                        case "Attribute Code": rec.Code = col.FieldValue; break;
                        case "Sequence": rec.Sequence = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Attribute Value": rec.Value = col.FieldValue; break;
                        case "Numeric Value": rec.NumericValue = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }

                LoadAttribute(result.GetDataSet(10000784), ref rec);
                list.Add(rec);
            }
            return list;
        }

        private void LoadAttribute(ReplODataSetRecRef dynDataSet, ref RetailAttribute attribute)
        {
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return;

            string desc = string.Empty;
            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                RetailAttribute rec = new RetailAttribute();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rec.Code = col.FieldValue; break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Value Type": rec.ValueType = (AttributeValueType)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Default Value": rec.DefaultValue = col.FieldValue; break;
                    }
                }
                if (attribute.Code == rec.Code)
                {
                    attribute.Description = rec.Description;
                    attribute.ValueType = rec.ValueType;
                    attribute.DefaultValue = rec.DefaultValue;
                    return;
                }
            }
        }

        private List<ItemLocation> LoadItemLocation(WSODataCollection result)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(99001533);
            List<ItemLocation> list = new List<ItemLocation>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                ItemLocation rec = new ItemLocation();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Store No.": rec.StoreId = col.FieldValue; break;
                        case "Section Code": rec.SectionCode = col.FieldValue; break;
                        case "Shelf Code": rec.ShelfCode = col.FieldValue; break;
                    }
                }

                rec.SectionDescription = LoadDescription(result.GetDataSet(99001530), 2, rec.StoreId, rec.SectionCode, string.Empty);
                rec.ShelfDescription = LoadDescription(result.GetDataSet(99001531), 3, rec.StoreId, rec.ShelfCode, rec.SectionCode);
                list.Add(rec);
            }
            return list;
        }

        private List<UnitOfMeasure> LoadItemUOM(WSODataCollection result)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(5404);
            List<UnitOfMeasure> list = new List<UnitOfMeasure>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                UnitOfMeasure rec = new UnitOfMeasure();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": rec.ItemId = col.FieldValue; break;
                        case "Code": rec.Id = col.FieldValue; break;
                        case "Qty. per Unit of Measure": rec.QtyPerUom = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }

                rec.Description = LoadDescription(result.GetDataSet(204), 1, string.Empty, rec.Id, string.Empty);
                list.Add(rec);
            }
            return list;
        }

        private string LoadDescription(ReplODataSetRecRef dynDataSet, int mode, string storeNo, string code, string sectCode)
        {
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return null;

            string desc = string.Empty;
            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                string rowStore = string.Empty;
                string rowCode = string.Empty;
                string rowSect = string.Empty;
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Store No.": rowStore = col.FieldValue; break;
                        case "Section Code": rowSect = col.FieldValue; break;
                        case "Code": rowCode = col.FieldValue; break;
                        case "Description": desc = col.FieldValue; break;
                    }
                }

                switch (mode)
                {
                    case 1:
                        if (code == rowCode)
                            return desc;
                        break;
                    case 2:
                        if (storeNo == rowStore && code == rowCode)
                            return desc;
                        break;
                    case 3:
                        if (storeNo == rowStore && code == rowCode && sectCode == rowSect)
                            return desc;
                        break;
                }
            }
            return desc;
        }

        private List<ImageView> LoadImage(ReplODataSetRecRef dynDataSet, string keyValue)
        {
            List<ImageView> list = new List<ImageView>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                ImageView rec = new ImageView();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Image Id": rec.Id = col.FieldValue; break;
                        case "KeyValue": rec.Location = col.FieldValue; break;
                        case "Display Order": rec.DisplayOrder = ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list.FindAll(x => x.Location == keyValue);
        }

        private class xItemStatus
        {
            public string itemNo;
            public string statusCode;
            public string variantDim1;
            public string variantCode;
            public string storeGrCode;
            public DateTime startDate;
            public bool blockPOS;
            public bool blockDiscount;
            public bool blockManPrice;
            public bool blockNegAdj;
            public bool blockPosAdj;
            public bool blockPerDisc;
            public bool blockPurchaseRet;
            public bool blockSaleRet;
            public bool blockECom;
        }

        private xItemStatus LoadStatus(ReplODataSetRecRef dynDataSet, string itemNo, string variantNo)
        {
            List<xItemStatus> list = new List<xItemStatus>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return null;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                xItemStatus rec = new xItemStatus();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Item No.": rec.itemNo = col.FieldValue; break;
                        case "Status Code": rec.statusCode = col.FieldValue; break;
                        case "Variant Dimension 1 Code": rec.variantDim1 = col.FieldValue; break;
                        case "Variant Code": rec.variantCode = col.FieldValue; break;
                        case "Store Group Code": rec.storeGrCode = col.FieldValue; break;
                        case "Starting Date": rec.startDate = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Block Sale on POS": rec.blockPOS = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Discount": rec.blockDiscount = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Manual Price Change": rec.blockManPrice = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Negative Adjustment": rec.blockNegAdj = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Positive Adjustment": rec.blockPosAdj = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Periodic Discount": rec.blockPerDisc = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Purchase Return": rec.blockPurchaseRet = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Blocked on eCommerce": rec.blockECom = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Block Sales Return": rec.blockSaleRet = ConvertTo.SafeBoolean(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list.Find(x => x.itemNo == itemNo && x.variantCode == variantNo);
        }

        private string LoadItemHtml(ReplODataSetRecRef dynDataSet)
        {
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return null;

            bool found = false;
            string html = null;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Language": found = string.IsNullOrEmpty(col.FieldValue); break;
                        case "Html": html = ConvertTo.Base64Decode(col.FieldValue); break;
                    }
                }
                if (found)
                    return html;
            }
            return null;
        }

        private string LoadItemSpecialGroup(ReplODataSetRecRef dynDataSet)
        {
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return null;

            string spGroup = string.Empty;
            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Special Group Code": spGroup += col.FieldValue + ";"; break;
                    }
                }
            }
            return spGroup;
        }

        private List<ItemRecipe> LoadItemRecipe(WSODataCollection result)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(90);
            List<ItemRecipe> list = new List<ItemRecipe>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                ItemRecipe rec = new ItemRecipe();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "LSC Item No.": rec.Id = col.FieldValue; break;
                        case "Line No.": rec.LineNo = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Unit of Measure Code": rec.UnitOfMeasure = col.FieldValue; break;
                        case "Quantity per": rec.QuantityPer = ConvertTo.SafeInt(col.FieldValue); break;
                        case "LSC Exclusion": rec.Exclusion = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "LSC Price on Exclusion": rec.ExclusionPrice = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }

                List<ImageView> images = LoadImage(result.GetDataSet(99009064), rec.Id);
                if (images != null && images.Count > 0)
                    rec.ImageId = images.FirstOrDefault().Id;
                list.Add(rec);
            }
            return list;
        }

        private List<ItemModifier> LoadItemModify(WSODataCollection result)
        {
            ReplODataSetRecRef dynDataSet = result.GetDataSet(99001479);
            List<ItemModifier> list = new List<ItemModifier>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                string value = string.Empty;
                ItemModifier recMain = new ItemModifier();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Value": value = col.FieldValue; break;
                        case "Infocode Code": recMain.Code = col.FieldValue; break;
                        case "Usage Category": recMain.UsageCategory = (ItemUsageCategory)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Usage Sub-Category": recMain.Type = (ItemModifierType)ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }

                List<ItemModifier> tmpList = LoadInfoSubCode(result.GetDataSet(99001483), recMain.Code);
                foreach (ItemModifier mod in tmpList)
                {
                    mod.UsageCategory = recMain.UsageCategory;
                    mod.Type = recMain.Type;
                    mod.MinSelection = ConvertTo.SafeInt(LoadInfoSelection(result.GetDataSet(99001482), 1, mod.TriggerCode));
                    mod.MaxSelection =  ConvertTo.SafeInt(LoadInfoSelection(result.GetDataSet(99001482), 2, mod.TriggerCode));
                    mod.GroupMinSelection = ConvertTo.SafeInt(LoadInfoSelection(result.GetDataSet(99001482), 1, mod.Code));
                    mod.GroupMaxSelection = ConvertTo.SafeInt(LoadInfoSelection(result.GetDataSet(99001482), 2, mod.Code));
                    mod.ExplanatoryHeaderText = LoadInfoSelection(result.GetDataSet(99001482), 3, mod.Code);
                    mod.Prompt = LoadInfoSelection(result.GetDataSet(99001482), 4, mod.Code);

                    if (mod.TriggerFunction == ItemTriggerFunction.Infocode)
                    {
                        list.AddRange(LoadInfoSubCode(result.GetDataSet(99001483), mod.TriggerCode));
                    }
                }
                list.AddRange(tmpList);
            }
            return list;
        }

        private List<ItemModifier> LoadInfoSubCode(ReplODataSetRecRef dynDataSet, string code)
        {
            List<ItemModifier> list = new List<ItemModifier>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return null;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                ItemModifier rec = new ItemModifier();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rec.Code = col.FieldValue; break;
                        case "Subcode": rec.SubCode = col.FieldValue; break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Variant Code": rec.VariantCode = col.FieldValue; break;
                        case "Unit of Measure": rec.UnitOfMeasure = col.FieldValue; break;
                        case "Trigger Function": rec.TriggerFunction = (ItemTriggerFunction)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Trigger Code": rec.TriggerCode = col.FieldValue; break;
                        case "Price Type": rec.PriceType = (ItemModifierPriceType)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Price Handling": rec.AlwaysCharge = (ItemModifierPriceHandling)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Amount /Percent": rec.AmountPercent = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Time Modifier Minutes": rec.TimeModifierMinutes = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }

                if (rec.Code == code)
                {
                    list.Add(rec);
                }
            }
            return list;
        }

        private string LoadInfoSelection(ReplODataSetRecRef dynDataSet, int getParam, string code)
        {
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return string.Empty;

            string desc = string.Empty;
            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                string rowCode = string.Empty;
                string rowText = string.Empty;
                string rowPrompt = string.Empty;
                string rowMin = string.Empty;
                string rowMax = string.Empty;
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rowCode = col.FieldValue; break;
                        case "Prompt": rowPrompt = col.FieldValue; break;
                        case "Explanatory Header Text": rowText = col.FieldValue; break;
                        case "Min. Selection": rowMin = col.FieldValue; break;
                        case "Max. Selection": rowMax = col.FieldValue; break;
                    }
                }

                if (rowCode != code)
                    continue;

                switch (getParam)
                {
                    case 1:
                        return rowMin;
                    case 2: 
                        return rowMax;
                    case 3: 
                        return rowText;
                    case 4: 
                        return rowPrompt;
                }
            }
            return string.Empty;
        }
    }
}
