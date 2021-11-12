using System;
using System.Collections.Generic;
using System.Globalization;
using LSOmni.Common.Util;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication
{
    public class SetupRepository : BaseRepository
    {
        public Currency GetCurrency(XMLTableData table, string culture)
        {
            if (table == null || table.NumberOfValues == 0)
                return null;

            Currency rec = new Currency();
            int curplace = 0;
            int roundoftype = 0;
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "Code": rec.Id = field.Values[0]; break;
                    case "Description": rec.Description = field.Values[0]; break;
                    case "Amount Rounding Precision": rec.RoundOffSales = ConvertTo.SafeDecimal(field.Values[0]); break;
                    case "Invoice Rounding Precision": rec.RoundOffAmount = ConvertTo.SafeDecimal(field.Values[0]); break;
                    case "POS Currency Symbol": rec.Symbol = field.Values[0]; break;
                    case "Placement Of Currency Symbol": curplace = ConvertTo.SafeInt(field.Values[0]); break;
                    case "Invoice Rounding Type": roundoftype = ConvertTo.SafeInt(field.Values[0]); break;
                }
            }
            rec.Culture = culture;
            rec.DecimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator;
            rec.ThousandSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyGroupSeparator;
            rec.DecimalPlaces = CurrencyGetHelper.GetNumberOfDecimals(rec.RoundOffSales);

            if (curplace == 0)
            {
                rec.Prefix = string.Empty;
                rec.Postfix = rec.Symbol;
            }
            else
            {
                rec.Prefix = rec.Symbol;
                rec.Postfix = string.Empty;
            }

            // to overwrite the culture on the server
            if (string.IsNullOrWhiteSpace(culture) == false)
            {
                CultureInfo ci = new CultureInfo(culture);//de-DE en-US
                rec.DecimalSeparator = ci.NumberFormat.CurrencyDecimalSeparator;
                rec.ThousandSeparator = ci.NumberFormat.CurrencyGroupSeparator;
            }

            switch (roundoftype)
            {
                case 0:
                    //  <!-- SaleRoundingMethod: RoundNearest, RoundDown,RoundUp -->
                    rec.SaleRoundingMethod = CurrencyRoundingMethod.RoundNearest;
                    break;
                case 1:
                    //  <!-- SaleRoundingMethod: RoundNearest, RoundDown,RoundUp -->
                    rec.SaleRoundingMethod = CurrencyRoundingMethod.RoundDown;
                    break;
                case 2:
                    //  <!-- SaleRoundingMethod: RoundNearest, RoundDown,RoundUp -->
                    rec.SaleRoundingMethod = CurrencyRoundingMethod.RoundUp;
                    break;
            }

            rec.AmountRoundingMethod = rec.SaleRoundingMethod; //nav doesn't have amount rounding..

            return rec;
        }

        public ImageView GetImage(XMLTableData table)
        {
            if (table == null || table.NumberOfValues == 0)
                return null;

            ImageView rec = new ImageView();
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "Code": rec.Id = field.Values[0]; break;
                    case "Image Location": rec.Location = field.Values[0]; break;
                    case "Type": rec.LocationType = (LocationType)ConvertTo.SafeInt(field.Values[0]); break;
                    case "Image Blob": rec.Image = field.Values[0]; break;
                }
                rec.ImgBytes = Convert.FromBase64String(rec.Image);
            }
            return rec;
        }

        public List<ImageView> GetImageLinks(XMLTableData table)
        {
            List<ImageView> list = new List<ImageView>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ImageView rec = new ImageView();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Image Id": rec.Id = field.Values[i]; break;
                        case "Display Order": rec.DisplayOrder = ConvertTo.SafeInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ProactiveDiscount> GetDiscount(XMLTableData table)
        {
            List<ProactiveDiscount> list = new List<ProactiveDiscount>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ProactiveDiscount rec = new ProactiveDiscount();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Offer No.": rec.Id = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Loyalty Scheme Code": rec.LoyaltySchemeCode = field.Values[i]; break;
                        case "Variant Code": rec.VariantId = field.Values[i]; break;
                        case "Unit of Measure Code": rec.UnitOfMeasureId = field.Values[i]; break;
                        case "Discount %": rec.Percentage = ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Minimum Quantity": rec.MinimumQuantity = ConvertTo.SafeDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public void SetDiscountInfo(XMLTableData table, ProactiveDiscount rec)
        {
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "Description": rec.Description = field.Values[0]; break;
                    case "Pop-up Line 1": rec.PopUpLine1 = field.Values[0]; break;
                    case "Pop-up Line 2": rec.PopUpLine2 = field.Values[0]; break;
                    case "Priority": rec.Priority = ConvertTo.SafeInt(field.Values[0]); break;
                    case "Type": rec.Type = (ProactiveDiscountType)(ConvertTo.SafeInt(field.Values[0]) + 1); break;
                    case "Discount % Value": rec.Percentage = (rec.Percentage == 0) ? ConvertTo.SafeDecimal(field.Values[0]) : rec.Percentage; break;
                }
            }
        }

        public decimal GetPointRate(XMLTableData table)
        {
            if (table == null || table.NumberOfValues == 0)
                return 0;
            
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "Relational Exch. Rate Amount": return ConvertTo.SafeDecimal(field.Values[0]);
                }
            }
            return 0;
        }

        public List<ShippingAgentService> GetShippingAgentServices(XMLTableData table)
        {
            List<ShippingAgentService> list = new List<ShippingAgentService>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ShippingAgentService rec = new ShippingAgentService();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Shipping Time": rec.ShippingTime = field.Values[i].ToString(); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public Terminal GetTerminalData(XMLTableData table)
        {
            if (table == null || table.NumberOfValues == 0)
                return null;

            Terminal rec = new Terminal();
            rec.Features = new FeatureFlags();
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "No.": rec.Id = field.Values[0]; break;
                    case "Description": rec.Description = field.Values[0]; break;
                    case "Store No.": rec.Store = new Store(field.Values[0]); break;
                    case "Terminal Type": rec.TerminalType = Convert.ToInt32(field.Values[0]); break;
                    case "Device License Key": rec.LicenseKey = field.Values[0]; break;
                    case "Device Unique ID": rec.UniqueId = field.Values[0]; break;
                    case "Inventory No. of Records": rec.NoOfRecords = ConvertTo.SafeInt(field.Values[0]); break;
                    case "Inventory Main Menu": rec.MainMenu = field.Values[0]; break;
                    case "Item Filtering Method": rec.ItemFilterMethod = ConvertTo.SafeInt(field.Values[0]); break;
                    case "Device Type": rec.DeviceType = ConvertTo.SafeInt(field.Values[0]); break;
                    case "Show Numberpad": rec.Features.AddFlag(FeatureFlagName.ShowNumberPad, field.Values[0]); break;
                }
            }
            return rec;
        }

        public List<Store> StoresGet(XMLTableData table)
        {
            List<Store> list = new List<Store>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                Store rec = new Store();
                rec.Address = new Address();
                rec.Address.Type = AddressType.Store;
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Name": rec.Description = field.Values[i]; break;
                        case "Address": rec.Address.Address1 = field.Values[i]; break;
                        case "Address 2": rec.Address.Address2 = field.Values[i]; break;
                        case "Post Code": rec.Address.PostCode = field.Values[i]; break;
                        case "City": rec.Address.City = field.Values[i]; break;
                        case "County": rec.Address.StateProvinceRegion = field.Values[i]; break;
                        case "Country Code": rec.Address.Country = field.Values[i]; break;
                        case "Latitude": rec.Latitude = (double)ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Longitude": rec.Longitude = (double)ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Phone No.": rec.Phone = field.Values[i]; break;
                        case "Click and Collect": rec.IsClickAndCollect = ConvertTo.SafeBoolean(field.Values[i]); break;
                        case "Currency": rec.Currency = new Currency(field.Values[i]); break;
                        case "Web Store": rec.IsWebStore = ConvertTo.SafeBoolean(field.Values[i]); break;
                        case "Loyalty": rec.IsLoyalty = ConvertTo.SafeBoolean(field.Values[i]); break;
                        case "Web Store POS Terminal": rec.WebOmniTerminal = field.Values[i]; break;
                        case "Web Store Staff ID": rec.WebOmniStaff = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<SalesEntry> SalesEntryList(XMLTableData table)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            for (int i = 0; i < table.NumberOfValues; i++)
            {
                SalesEntry entry = new SalesEntry();
                entry.IdType = DocumentIdType.Receipt;
                DateTime date = DateTime.MinValue;
                DateTime time = DateTime.MinValue;
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Transaction No.": entry.Id = field.Values[i]; break;
                        case "POS Terminal No.": entry.TerminalId = field.Values[i]; break;
                        case "Store No.": entry.StoreId = field.Values[i]; break;
                        case "Date": date = XMLHelper.GetWebDateTime(field.Values[i]); break;
                        case "Time": time = XMLHelper.GetWebDateTime(field.Values[i]); break;
                    }
                }
                entry.DocumentRegTime = ConvertTo.NavJoinDateAndTime(date, time);
                list.Add(entry);
            }
            return list;
        }
    }
}
