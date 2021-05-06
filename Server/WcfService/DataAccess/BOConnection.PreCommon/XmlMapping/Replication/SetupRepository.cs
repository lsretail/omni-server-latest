using System;
using System.Collections.Generic;
using System.Globalization;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Replication
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
                    case "Amount Rounding Precision": rec.RoundOffSales = GetWebDecimal(field.Values[0]); break;
                    case "Invoice Rounding Precision": rec.RoundOffAmount = GetWebDecimal(field.Values[0]); break;
                    case "POS Currency Symbol": rec.Symbol = field.Values[0]; break;
                    case "Placement Of Currency Symbol": curplace = GetWebInt(field.Values[0]); break;
                    case "Invoice Rounding Type": roundoftype = GetWebInt(field.Values[0]); break;
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
                    case "Type": rec.LocationType = (LocationType)GetWebInt(field.Values[0]); break;
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
                        case "Display Order": rec.DisplayOrder = GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public decimal GetPointRate(XMLTableData table)
        {
            if (table == null || table.NumberOfValues == 0)
                return 0;
            
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "Relational Exch. Rate Amount": return GetWebDecimal(field.Values[0]);
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
                    case "Inventory No. of Records": rec.NoOfRecords = GetWebInt(field.Values[0]); break;
                    case "Inventory Main Menu": rec.MainMenu = field.Values[0]; break;
                    case "Item Filtering Method": rec.ItemFilterMethod = GetWebInt(field.Values[0]); break;
                    case "Device Type": rec.DeviceType = GetWebInt(field.Values[0]); break;
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
                        case "Latitude": rec.Latitude = (double)GetWebDecimal(field.Values[i]); break;
                        case "Longitude": rec.Longitude = (double)GetWebDecimal(field.Values[i]); break;
                        case "Phone No.": rec.Phone = field.Values[i]; break;
                        case "Click and Collect": rec.IsClickAndCollect = GetWebBool(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
