using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class SetupJMapping : BaseJMapping
    {
        public SetupJMapping(bool json)
        {
            IsJson = json;
        }

        public bool GetTestConnection(string ret, out string appVersion, out string retailVersion, out string appBuild, out string copyRight, out bool licenseActive, out bool ecomLicense, out string errorText, out string errorCode)
        {
            appVersion = string.Empty;
            retailVersion = string.Empty;
            appBuild = string.Empty;
            copyRight = string.Empty;
            errorText = string.Empty;
            errorCode = string.Empty;

            try
            {
                JObject parsed = JObject.Parse(ret);
                var token = parsed.SelectToken("value");
                parsed = JObject.Parse(token.ToString());
                errorCode = parsed.SelectToken("ResponseCode").Value<string>();
                errorText = parsed.SelectToken("ErrorText").Value<string>();
                token = parsed.SelectToken("TestConnectionResponse");
                parsed = JObject.Parse(token.ToString());
                retailVersion = parsed.SelectToken("LSRetailVersion").Value<string>();
                appVersion = parsed.SelectToken("ApplicationVersion").Value<string>();
                appBuild = parsed.SelectToken("ApplicationBuild").Value<string>();
                copyRight = parsed.SelectToken("LSRetailCopyright").Value<string>();
                licenseActive = parsed.SelectToken("LSRetailLicenseKeyActive").Value<bool>();
                ecomLicense = parsed.SelectToken("LSRetailLicenseUnitEcom").Value<bool>();
            }
            catch (Exception)
            {
                throw new Exception("OData4 Error:" + ret);
            }
            return true;
        }

        public List<ReplCurrency> GetReplCurrency(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCurrency> list = new List<ReplCurrency>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplCurrency line = new ReplCurrency();
                line.RoundOfSales = 0.01M;
                line.RoundOfAmount = 0.01M;
                int cursymplacement = 0;
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.CurrencyCode = data.FieldValue; break;
                        case 15: line.Description = data.FieldValue; break;
                        case 10: line.RoundOfAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 12: line.RoundOfTypeAmount = ConvertTo.SafeInt(data.FieldValue); break;
                        case 13: line.RoundOfSales = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 99008900: line.Symbol = data.FieldValue; break;
                        case 99008901: cursymplacement = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }

                if (cursymplacement == 0)
                {
                    line.CurrencyPrefix = string.Empty;
                    line.CurrencySuffix = line.Symbol;
                }
                else
                {
                    line.CurrencyPrefix = line.Symbol;
                    line.CurrencySuffix = string.Empty;
                }

                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplCurrency line = new ReplCurrency()
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
                        case 1: line.CurrencyCode = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplCurrencyExchRate> GetReplCurrencyExchRate(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCurrencyExchRate> list = new List<ReplCurrencyExchRate>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplCurrencyExchRate line = new ReplCurrencyExchRate();
                decimal exchRateAmt = 0M;
                decimal exchPosRateAmt = 0M;
                decimal relExchRateAmt = 0M;
                decimal relExchPosRateAmt = 0M;
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.CurrencyCode = data.FieldValue; break;
                        case 2: line.StartingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 3: exchRateAmt = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 5: line.RelationalCurrencyCode = data.FieldValue; break;
                        case 6: relExchRateAmt = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 99001450: exchPosRateAmt = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 99001451: relExchPosRateAmt = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }

                if (exchPosRateAmt != 0)
                {
                    line.CurrencyFactor = ((1 / exchPosRateAmt) * relExchPosRateAmt);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(line.RelationalCurrencyCode))
                    {
                        if (exchRateAmt != 0)
                        {
                            line.CurrencyFactor = ((1 / exchRateAmt) * relExchRateAmt);
                        }
                    }
                    else
                    {
                        //using (ReplCurrencyExchRate relcur = CurrencyExchRateGetById(code))
                        //{
                        //    exchrate = relcur.CurrencyFactor;
                        //}   
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplCurrencyExchRate line = new ReplCurrencyExchRate()
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
                        case 1: line.CurrencyCode = data.FieldValue; break;
                        case 2: line.StartingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplTaxSetup> GetReplTaxSetup(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplTaxSetup> list = new List<ReplTaxSetup>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplTaxSetup line = new ReplTaxSetup();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.BusinessTaxGroup = data.FieldValue; break;
                        case 2: line.ProductTaxGroup = data.FieldValue; break;
                        case 4: line.TaxPercent = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplTaxSetup line = new ReplTaxSetup()
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
                        case 1: line.BusinessTaxGroup = data.FieldValue; break;
                        case 2: line.ProductTaxGroup = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplStoreTenderType> GetReplTenderType(string ret, string tenderMap, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStoreTenderType> list = new List<ReplStoreTenderType>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplStoreTenderType line = new ReplStoreTenderType();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.TenderTypeId = data.FieldValue; break;
                        case 5: line.Name = data.FieldValue; break;
                        case 10: line.TenderFunction = ConvertTo.SafeInt(data.FieldValue); break;
                        case 11: line.ValidOnMobilePOS = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 25: line.ChangeTenderId = data.FieldValue; break;
                        case 26: line.AboveMinimumTenderId = data.FieldValue; break;
                        case 30: line.MinimumChangeAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 35: line.RoundingMethode = ConvertTo.SafeInt(data.FieldValue); break;
                        case 40: line.Rounding = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 110: line.AllowOverTender = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 111: line.MaximumOverTenderAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 115: line.AllowUnderTender = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 120: line.ReturnAllowed = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 125: line.OpenDrawer = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 225: line.ForeignCurrency = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 305: line.CountingRequired = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 390: line.StoreID = data.FieldValue; break;
                    }
                }

                line.OmniTenderTypeId = ConfigSetting.TenderTypeMapping(tenderMap, line.TenderTypeId, true);
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplStoreTenderType line = new ReplStoreTenderType()
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
                        case 1: line.TenderTypeId = data.FieldValue; break;
                        case 390: line.StoreID = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplStoreTenderType> GetReplTenderTypeEx(string ret, string tenderMap, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStoreTenderType> list = new List<ReplStoreTenderType>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplStoreTenderType line = new ReplStoreTenderType();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": line.TenderTypeId = data.FieldValue; break;
                        case "Store No.": line.StoreID = data.FieldValue; break;
                        case "Description": line.Name = data.FieldValue; break;
                        case "Function": line.TenderFunction = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Valid on Mobile POS": line.ValidOnMobilePOS = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Change Tend. Code": line.ChangeTenderId = data.FieldValue; break;
                        case "Above Min. Change Tender Type": line.AboveMinimumTenderId = data.FieldValue; break;
                        case "Min. Change": line.MinimumChangeAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Rounding": line.RoundingMethode = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Rounding To": line.Rounding = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Return/Minus Allowed": line.ReturnAllowed = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Foreign Currency": line.ForeignCurrency = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Undertender Allowed": line.AllowUnderTender = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Overtender Allowed": line.AllowOverTender = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Overtender Max. Amt.": line.MaximumOverTenderAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Counting Required": line.CountingRequired = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "Drawer Opens": line.OpenDrawer = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "DateEntryCodes": line.DataEntryCodes = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplStoreTenderType line = new ReplStoreTenderType()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": line.TenderTypeId = data.FieldValue; break;
                        case "Store No.": line.StoreID = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplStoreTenderTypeCurrency> GetReplTenderTypeCurrency(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStoreTenderTypeCurrency> list = new List<ReplStoreTenderTypeCurrency>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplStoreTenderTypeCurrency line = new ReplStoreTenderTypeCurrency();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.CurrencyCode = data.FieldValue; break;
                        case 2: line.Description = data.FieldValue; break;
                        case 3: line.StoreID = data.FieldValue; break;
                        case 4: line.TenderTypeId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplStoreTenderTypeCurrency line = new ReplStoreTenderTypeCurrency()
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
                        case 1: line.CurrencyCode = data.FieldValue; break;
                        case 3: line.StoreID = data.FieldValue; break;
                        case 4: line.TenderTypeId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplDataTranslationLangCode> GetReplDataTransLanguages(string ret, out string resCode, out string resErr)
        {
            List<ReplDataTranslationLangCode> list = new List<ReplDataTranslationLangCode>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject item in items)
            {
                list.Add(new ReplDataTranslationLangCode()
                {
                    Code = item.Value<string>("languageCode")
                });
            }
            return list;
        }

        public List<Currency> GetCurrency(string ret, string culture, out string resCode, out string resErr)
        {
            List<Currency> list = new List<Currency>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            if (items.Count == 0)
                return list;

            JObject item = (JObject)items[0];
            Currency rec = new Currency(item.Value<string>("code"))
            {
                Description = item.Value<string>("description"),
                RoundOffSales = item.Value<decimal>("amountRoundingPrecision"),
                RoundOffAmount = item.Value<decimal>("invoiceRoundingPrecision"),
                Symbol = item.Value<string>("posCurrencySymbol")
            };

            rec.Culture = culture;
            rec.DecimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator;
            rec.ThousandSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyGroupSeparator;
            rec.DecimalPlaces = CurrencyGetHelper.GetNumberOfDecimals(rec.RoundOffSales);

            if (ParsePlacement(item.SelectToken("placementOfCurrencySymbol")))
            {
                rec.Prefix = rec.Symbol;
                rec.Postfix = string.Empty;
            }
            else
            {
                rec.Prefix = string.Empty;
                rec.Postfix = rec.Symbol;
            }

            if (string.IsNullOrWhiteSpace(culture) == false)
            {
                CultureInfo ci = new CultureInfo(culture);//de-DE en-US
                rec.DecimalSeparator = ci.NumberFormat.CurrencyDecimalSeparator;
                rec.ThousandSeparator = ci.NumberFormat.CurrencyGroupSeparator;
            }

            rec.SaleRoundingMethod = ParseRoundingType(item.Value<string>("invoiceRoundingType"));
            rec.AmountRoundingMethod = rec.SaleRoundingMethod; //nav doesn't have amount rounding..

            list.Add(rec);
            return list;
        }

        // curplace: symbol placed as prefix (true) or postfix (false) - defensive parse since the field's exact AL shape (Boolean vs Option) hasn't been verified against the real page yet
        private static bool ParsePlacement(JToken token)
        {
            if (token == null)
                return false;
            if (token.Type == JTokenType.Boolean)
                return token.Value<bool>();
            return ConvertTo.SafeInt(token.ToString()) != 0;
        }

        // AL Option assumed "Nearest,Up,Down" (per Microsoft's standard BC Currency."Invoice Rounding Type" field) - matched by name, not ordinal
        private static CurrencyRoundingMethod ParseRoundingType(string value)
        {
            switch (value)
            {
                case "Down": return CurrencyRoundingMethod.RoundDown;
                case "Up": return CurrencyRoundingMethod.RoundUp;
                default: return CurrencyRoundingMethod.RoundNearest;
            }
        }

        public List<ReplShippingAgent> GetShippingAgents(string ret, out string resCode, out string resErr)
        {
            List<ReplShippingAgent> list = new List<ReplShippingAgent>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                ReplShippingAgent agent = new ReplShippingAgent(rec.Value<string>("code"))
                {
                    Name = rec.Value<string>("name"),
                    InternetAddress = rec.Value<string>("internetAddress"),
                    AccountNo = rec.Value<string>("accountNo")
                };

                var token = rec.SelectToken("shippingAgentServices");
                if (token != null)
                {
                    foreach (JObject svc in JArray.Parse(token.ToString()))
                    {
                        agent.Services.Add(new ShippingAgentService(svc.Value<string>("code"))
                        {
                            Description = svc.Value<string>("description"),
                            ShippingTime = svc.Value<string>("shippingTime")
                        });
                    }
                }
                list.Add(agent);
            }
            return list;
        }

        public List<ShippingAgentService> GetShippingAgentServices(string ret, out string resCode, out string resErr)
        {
            List<ShippingAgentService> list = new List<ShippingAgentService>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                list.Add(new ShippingAgentService(rec.Value<string>("code"))
                {
                    Description = rec.Value<string>("description"),
                    ShippingTime = rec.Value<string>("shippingTime")
                });
            }
            return list;
        }

        public decimal GetCurRate(string ret, out string resCode, out string resErr)
        {
            decimal rate = 0;
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return rate;

            JArray items = JArray.Parse(result);
            if (items.Count == 0)
                return rate;

            decimal posextamt = items[0].Value<decimal>("lscPosExchangeRateAmount");
            decimal posrelamt = items[0].Value<decimal>("lscPosRelExchRateAmount");
            decimal extamt = items[0].Value<decimal>("exchangeRateAmount");
            decimal relamt = items[0].Value<decimal>("relationalExchRateAmount");

            if (posextamt != 0)
            {
                rate = ((1 / posextamt) * posrelamt);
            }
            else
            {
                if (extamt != 0)
                {
                    rate = ((1 / extamt) * relamt);
                }
            }
            return rate;
        }

        public List<Terminal> GetTerminal(string ret, out string resCode, out string resErr)
        {
            List<Terminal> list = new List<Terminal>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                Terminal ter = new Terminal(rec.Value<string>("no"))
                {
                    Description = rec.Value<string>("description"),
                    Store = new Store(rec.Value<string>("storeNo")),
                    TerminalType = ParseTerminalType(rec.Value<string>("terminalType")),
                    NoOfRecords = rec.Value<int>("inventoryNoOfRecords"),
                    InventoryMainMenuId = rec.Value<string>("inventoryMainMenu"),
                    ItemFilterMethod = ParseItemFilteringMethod(rec.Value<string>("itemFilteringMethod")),
                    DeviceType = ParseDeviceType(rec.Value<string>("deviceType")),
                    AsnQuantityMethod = ParseAsnQuantityMethod(rec.Value<string>("asnQuantityMethod")),
                    StaffBarcodeLogon = rec.Value<bool>("staffBarcodeLogon")
                };
                // Device License Key / Device Unique ID are not on the posTerminals API page
                ter.Features.AddFlag(FeatureFlagName.ExitAfterEachTransaction, rec.Value<bool>("exitAfterEachTrans") ? 1 : 0);
                ter.Features.AddFlag(FeatureFlagName.ShowNumberPad, rec.Value<bool>("showNumberpad") ? 1 : 0);
                // idleTimeout replaces the legacy "AutoLogoff After (Min.)" field but is in seconds, not minutes -
                // convert so the FeatureFlags value stays in the unit legacy callers expect
                ter.Features.AddFlag(FeatureFlagName.AutoLogOffAfterMin, rec.Value<int>("idleTimeout") / 60);
                list.Add(ter);
            }
            return list;
        }

        // "Terminal Type" caption -> ordinal. Legacy generic-table reads returned the raw AL option ordinal (int);
        // the API page returns caption text instead. The exact caption list isn't visible from this repo, but
        // TerminalTypeCheck (NavPOS.cs) gates mobile-POS operations on TerminalType == 1 || == 3, and live sample
        // data confirms "Mobile POS" is used for an active MPOS terminal - so it must resolve to 1 or 3.
        // Inferred mapping (verify against the real AL enum): blank -> 0, "*Hospitality*" -> 3, "*Mobile*" -> 1, else -> 2.
        private static int ParseTerminalType(string caption)
        {
            string c = DecodeEdmEnumCaption(caption);
            if (string.IsNullOrEmpty(c))
                return 0;
            if (c.IndexOf("Hospitality", StringComparison.OrdinalIgnoreCase) >= 0)
                return (int)TerminalTypes.Hospitality;
            if (c.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0)
                return (int)TerminalTypes.Retail;
            return 2;
        }

        // "Item Filtering Method" caption -> ordinal. NavInventory.ReplicateInventoryItems treats ItemFilterMethod == 0
        // as "no filtering, replicate all items" - live sample data confirms caption "All" for that case.
        private static int ParseItemFilteringMethod(string caption)
        {
            string c = DecodeEdmEnumCaption(caption);
            return (string.IsNullOrEmpty(c) || c.Equals("All", StringComparison.OrdinalIgnoreCase)) ? 0 : 1;
        }

        // "Device Type" caption -> ordinal. NavPOS throws DeviceIsBlocked when DeviceType == 0 ("inactive"); any
        // populated caption (e.g. "Mobile") represents an active device type.
        private static int ParseDeviceType(string caption)
        {
            return string.IsNullOrEmpty(DecodeEdmEnumCaption(caption)) ? 0 : 1;
        }

        private static AsnQuantityMethod ParseAsnQuantityMethod(string caption)
        {
            string c = DecodeEdmEnumCaption(caption).Replace(" ", string.Empty);
            if (c.Equals("QuickScan", StringComparison.OrdinalIgnoreCase))
                return AsnQuantityMethod.QuickScan;
            if (c.Equals("Ask", StringComparison.OrdinalIgnoreCase))
                return AsnQuantityMethod.Ask;
            if (c.Equals("SuggestiveQuickScan", StringComparison.OrdinalIgnoreCase))
                return AsnQuantityMethod.SuggestiveQuickScan;
            return AsnQuantityMethod.None;
        }

        public void AddFeatureFlags(string ret, FeatureFlags flags, out string resCode, out string resErr)
        {
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return;

            JArray items = JArray.Parse(result);
            // posTerminalFeatureFlags does not resolve terminal/store/global precedence server-side (OR-blank filter
            // returns all tiers) - apply lowest to highest specificity so a terminal-specific row overwrites a
            // store-level or global row for the same flag code
            foreach (JObject rec in items.Cast<JObject>().OrderBy(FeatureFlagSpecificity))
            {
                string flagName = rec.Value<string>("featureFlag");
                if (string.IsNullOrEmpty(flagName))
                    continue;

                flags.AddFlag(flagName, rec.Value<string>("value"));
            }
        }

        private static int FeatureFlagSpecificity(JObject rec)
        {
            bool hasTerminal = string.IsNullOrEmpty(rec.Value<string>("posTerminal")) == false;
            bool hasStore = string.IsNullOrEmpty(rec.Value<string>("storeNo")) == false;
            return (hasTerminal ? 2 : 0) + (hasStore ? 1 : 0);
        }

        public List<(string HierarchyId, string HierarchyCode, string ValidationScheduleId, int Priority, string SalesTypeFilter)> GetHierarchyDates(string ret, out string resCode, out string resErr)
        {
            List<(string, string, string, int, string)> list = new List<(string, string, string, int, string)>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                list.Add((rec.Value<string>("hierarchyId"), rec.Value<string>("hierarchyCode"), rec.Value<string>("validationScheduleId"),
                          rec.Value<int>("priority"), rec.Value<string>("salesTypeFilter")));
            }
            return list;
        }

        public (Hierarchy Header, List<HierarchyNode> Nodes) GetHierarchyTree(string ret, out string resCode, out string resErr)
        {
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null)
                return (null, new List<HierarchyNode>());

            JObject root = JObject.Parse(result);
            JObject hdr = (JObject)root["hierarchy"];
            Hierarchy hierarchy = new Hierarchy(hdr.Value<string>("code"))
            {
                Description = hdr.Value<string>("description"),
                Type = ParseHierarchyType(hdr.Value<string>("type"))
            };

            JArray nodeImages = (JArray)root["nodeImages"];
            List<HierarchyNode> nodes = new List<HierarchyNode>();
            foreach (JObject n in (JArray)root["nodes"])
            {
                string code = n.Value<string>("code");
                string nodeId = n.Value<string>("nodeId");
                HierarchyNode node = new HierarchyNode(nodeId)
                {
                    ParentNode = n.Value<string>("parentNodeId"),
                    Description = n.Value<string>("description"),
                    Indentation = n.Value<int>("indentation"),
                    PresentationOrder = n.Value<int>("presentationOrder"),
                    HierarchyCode = code
                };

                JObject img = nodeImages?.Cast<JObject>().FirstOrDefault(i => i.Value<string>("keyValue") == code + "," + nodeId);
                if (img != null)
                    node.ImageId = img.Value<string>("imageId");

                nodes.Add(node);
            }
            return (hierarchy, nodes);
        }

        public List<HierarchyLeaf> GetHierarchyLeaves(string ret, string hierarchyCode, out string resCode, out string resErr)
        {
            List<HierarchyLeaf> list = new List<HierarchyLeaf>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                list.Add(new HierarchyLeaf(rec.Value<string>("no"))
                {
                    ParentNode = rec.Value<string>("nodeId"),
                    HierarchyCode = hierarchyCode,
                    Description = rec.Value<string>("description"),
                    ItemUOM = rec.Value<string>("itemUnitOfMeasure"),
                    SortOrder = rec.Value<int>("sortOrder"),
                    Type = ParseHierarchyLeafType(rec.Value<string>("type")),
                    Price = rec.Value<decimal?>("itemPriceExclTax") ?? 0M,
                    PriceWithTax = rec.Value<decimal?>("itemPriceInclTax") ?? 0M
                });
            }
            return list;
        }

        // hierarchyV2's own "type" field is a plain, already-formatted Edm.String (not an enum reference) - captions observed live
        private static HierarchyType ParseHierarchyType(string value)
        {
            switch (value)
            {
                case "Vendor": return HierarchyType.Vendor;
                case "Store/Warehouse Loc./Customer": return HierarchyType.StoreWarehouseCustomer;
                default: return HierarchyType.ItemDeal;
            }
        }

        // AL Enum "lscHierarchyNodeType" on LSC Hierarchy(Leaf)."Type" - EDM-encoded, e.g. "Store Group" -> "Store_x0020_Group"
        private static HierarchyLeafType ParseHierarchyLeafType(string value)
        {
            switch (DecodeEdmEnumCaption(value))
            {
                case "Deal": return HierarchyLeafType.Deal;
                case "Store": return HierarchyLeafType.Store;
                case "Store Group": return HierarchyLeafType.StoreGroup;
                case "Warehouse Location": return HierarchyLeafType.WarehouseLocation;
                case "Customer": return HierarchyLeafType.Customer;
                case "Customer Group": return HierarchyLeafType.CustomerGroup;
                case "Vendor": return HierarchyLeafType.Vendor;
                case "Item Category": return HierarchyLeafType.ItemCategory;
                default: return HierarchyLeafType.Item;
            }
        }

        public List<string> GetHierarchyDateValidationScheduleIds(string ret, out string resCode, out string resErr)
        {
            List<string> list = new List<string>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                string id = rec.Value<string>("validationScheduleId");
                if (string.IsNullOrEmpty(id) == false)
                    list.Add(id);
            }
            return list;
        }

        public List<ReplValidationSchedule> GetValidationSchedule(string ret, out string resCode, out string resErr)
        {
            List<ReplValidationSchedule> list = new List<ReplValidationSchedule>();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject rec in items)
            {
                ReplValidationSchedule sch = new ReplValidationSchedule()
                {
                    Id = rec.Value<string>("code"),
                    Description = rec.Value<string>("description"),
                    Lines = new List<ValidationScheduleLine>()
                };

                JToken linesToken = rec.SelectToken("validationScheduleLines");
                if (linesToken != null)
                {
                    foreach (JObject lrec in JArray.Parse(linesToken.ToString()))
                    {
                        sch.Lines.Add(new ValidationScheduleLine()
                        {
                            LineNo = lrec.Value<int>("lineNo"),
                            Description = lrec.Value<string>("description"),
                            Comment = lrec.Value<string>("comment"),
                            Priority = lrec.Value<int>("priority"),
                            DateSchedule = GetDateSchedule(lrec.SelectToken("dateSchedules")),
                            TimeSchedule = GetTimeSchedule(lrec.SelectToken("timeSchedules"))
                        });
                    }
                }

                list.Add(sch);
            }
            return list;
        }

        private VSDateSchedule GetDateSchedule(JToken token)
        {
            if (token == null)
                return null;

            JArray arr = JArray.Parse(token.ToString());
            if (arr.Count == 0)
                return null;

            JObject rec = (JObject)arr[0];
            VSDateSchedule sch = new VSDateSchedule()
            {
                Id = rec.Value<string>("code"),
                Description = rec.Value<string>("description"),
                Mondays = rec.Value<bool>("mondays"),
                Tuesdays = rec.Value<bool>("tuesdays"),
                Wednesdays = rec.Value<bool>("wednesdays"),
                Thursdays = rec.Value<bool>("thursdays"),
                Fridays = rec.Value<bool>("fridays"),
                Saturdays = rec.Value<bool>("saturdays"),
                Sundays = rec.Value<bool>("sundays"),
                ValidAllWeekdays = rec.Value<bool>("validAllWeekdays"),
                Lines = new List<VSDateScheduleLine>()
            };

            JToken linesToken = rec.SelectToken("dateScheduleLines");
            if (linesToken != null)
            {
                foreach (JObject lrec in JArray.Parse(linesToken.ToString()))
                {
                    sch.Lines.Add(new VSDateScheduleLine()
                    {
                        LineNo = lrec.Value<int>("lineNo"),
                        StartingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(lrec.Value<string>("startingDate")), IsJson),
                        EndingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(lrec.Value<string>("endingDate")), IsJson),
                        Exclude = lrec.Value<bool>("exclude")
                    });
                }
            }
            return sch;
        }

        private VSTimeSchedule GetTimeSchedule(JToken token)
        {
            if (token == null)
                return null;

            JArray arr = JArray.Parse(token.ToString());
            if (arr.Count == 0)
                return null;

            JObject rec = (JObject)arr[0];
            VSTimeSchedule sch = new VSTimeSchedule()
            {
                Id = rec.Value<string>("code"),
                Description = rec.Value<string>("description"),
                Type = ParseScheduleType(rec.Value<string>("scheduleType")),
                Lines = new List<VSTimeScheduleLine>()
            };

            JToken linesToken = rec.SelectToken("timeScheduleLines");
            if (linesToken != null)
            {
                foreach (JObject lrec in JArray.Parse(linesToken.ToString()))
                {
                    sch.Lines.Add(new VSTimeScheduleLine(IsJson)
                    {
                        Period = lrec.Value<string>("period"),
                        TimeFrom = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(lrec.Value<string>("timeFrom")), IsJson),
                        TimeTo = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(lrec.Value<string>("timeTo")), IsJson),
                        TimeToIsPastMidnight = lrec.Value<bool>("timeToIsPastMidnight"),
                        DiningDurationCode = lrec.Value<string>("diningDurationCode"),
                        SelectedByDefault = lrec.Value<bool>("selectedByDefault"),
                        ReservationInterval = lrec.Value<int>("reservationIntervalMinutes")
                    });
                }
            }
            return sch;
        }

        // "Schedule Type" caption -> VSTimeScheduleType (AL enum: NotSpecified, Dining Hours, Validation Schedule)
        private static VSTimeScheduleType ParseScheduleType(string caption)
        {
            string c = DecodeEdmEnumCaption(caption);
            if (c.Equals("Dining Hours", StringComparison.OrdinalIgnoreCase))
                return VSTimeScheduleType.DiningHours;
            if (c.Equals("Validation Schedule", StringComparison.OrdinalIgnoreCase))
                return VSTimeScheduleType.ValidationSchedule;
            return VSTimeScheduleType.NotSpecified;
        }

        public string GetMobileLicense(string ret, out string resCode, out string resErr)
        {
            string licence = string.Empty;
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return licence;

            JArray items = JArray.Parse(result);
            if (items.Count == 0)
                return licence;

            return ((JObject)items[0]).Value<string>("deviceLicenseKey");
        }

        public ScanPayGoSecurityLog GetSecurityLog(string ret, out string resCode, out string resErr)
        {
            ScanPayGoSecurityLog log = new ScanPayGoSecurityLog();
            string result = GetJsonPageValue(ret, out resCode, out resErr, out bool isArray);
            if (result == null || isArray == false)
                return log;

            JArray items = JArray.Parse(result);
            if (items.Count == 0)
                return log;

            JObject rec = (JObject)items[0];
            log.Source = rec.Value<bool>("source");
            log.Check = rec.Value<bool>("check");
            return log;
        }

        public List<ReplDiscountValidation> GetPeriod(string ret, string tenderMap, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDiscountValidation> list = new List<ReplDiscountValidation>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplDiscountValidation line = new ReplDiscountValidation(IsJson);
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Description = data.FieldValue; break;
                        case 10: line.StartDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 11: line.EndDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 12: line.StartTime = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 13: line.EndTime = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 14: line.TimeWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;

                        case 15: line.MondayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 16: line.MondayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 17: line.MondayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 18: line.TuesdayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 19: line.TuesdayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 20: line.TuesdayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 21: line.WednesdayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 22: line.WednesdayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 23: line.WednesdayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 24: line.ThursdayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 25: line.ThursdayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 26: line.ThursdayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 27: line.FridayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 28: line.FridayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 29: line.FridayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 30: line.SaturdayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 31: line.SaturdayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 32: line.SaturdayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 33: line.SundayStart = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 34: line.SundayEnd = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 35: line.SundayWithinBounds = ConvertTo.SafeBoolean(data.FieldValue); break;

                        case 40: line.EndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 41: line.MondayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 42: line.TuesdayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 43: line.WednesdayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 44: line.ThursdayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 45: line.FridayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 46: line.SaturdayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 47: line.SundayEndAfterMidnight = ConvertTo.SafeBoolean(data.FieldValue); break;

                        case 82: line.OfferStartTime = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                        case 83: line.OfferEndTime = ConvertTo.SafeJsonTime(ConvertTo.SafeTime(data.FieldValue), IsJson); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplDiscountValidation line = new ReplDiscountValidation(IsJson)
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

        public List<ReplStaff> GetReplStaff(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStaff> list = new List<ReplStaff>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplStaff line = new ReplStaff();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 5: line.Password = data.FieldValue; break;
                        case 6: line.ChangePassword = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 7: line.StoreID = data.FieldValue; break;
                        case 35: line.FirstName = data.FieldValue; break;
                        case 36: line.LastName = data.FieldValue; break;
                        case 90: line.NameOnReceipt = data.FieldValue; break;
                        case 111: line.Blocked = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 112: line.BlockingDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 1100: line.InventoryActive = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 1101: line.InventoryMainMenu = data.FieldValue; break;
                    }
                }
                line.Name = line.FirstName + " " + line.LastName;
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplStaff line = new ReplStaff()
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

        public List<ReplStaffStoreLink> GetReplStaffStore(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStaffStoreLink> list = new List<ReplStaffStoreLink>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplStaffStoreLink line = new ReplStaffStoreLink();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.StaffId = data.FieldValue; break;
                        case 10: line.StoreId = data.FieldValue; break;
                        case 25: line.DefaultHospType = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplStaffStoreLink line = new ReplStaffStoreLink()
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
                        case 1: line.StaffId = data.FieldValue; break;
                        case 10: line.StoreId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplStore> GetReplStore(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStore> list = new List<ReplStore>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplStore line = new ReplStore();
                string lcy = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "No.": line.Id = data.FieldValue; break;
                        case "Name": line.Name = data.FieldValue; break;
                        case "Address": line.Street = data.FieldValue; break;
                        case "Post Code": line.ZipCode = data.FieldValue; break;
                        case "City": line.City = data.FieldValue; break;
                        case "County": line.County = data.FieldValue; break;
                        case "Country Code": line.Country = data.FieldValue; break;
                        case "Latitude": line.Latitute = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Longitude": line.Longitude = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Phone No.": line.Phone = data.FieldValue; break;
                        case "Currency Code": line.Currency = data.FieldValue; break;
                        case "Functionality Profile": line.FunctionalityProfile = data.FieldValue; break;
                        case "Store VAT Bus. Post. Gr.": line.TaxGroup = data.FieldValue; break;
                        case "Click and Collect": line.ClickAndCollect = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Store Group Codes": line.StoreGroupCodes = data.FieldValue; break;
                        case "Price Group Codes": line.PriceGroupCodes = data.FieldValue; break;
                        case "Sales Type Filter": line.HospSalesTypes = data.FieldValue; break;
                        case "LCY Code": lcy = data.FieldValue; break;
                    }
                }

                if (string.IsNullOrWhiteSpace(line.Currency))
                    line.Currency = lcy;

                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplStore line = new ReplStore()
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
                        case "No.": line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplStaffPermission> GetReplStaffPermission(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStaffPermission> list = new List<ReplStaffPermission>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            recordsRemaining = 0;
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplStaffPermission line = null;
                string staff = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldIndex)
                    {
                        case 1: staff = data.FieldValue; break;
                        case 2:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.ManagerPrivileges,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 3:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.VoidTransaction,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 4:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.XZYReport,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 5:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.TenderDeclaration,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 6:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.FloatingDeclaration,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 7:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.PriceOverRide,
                                StaffId = staff,
                                Type = PermissionType.List,
                                Value = data.FieldValue
                            };
                            break;
                        case 8:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.SuspendTransaction,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 9:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.ReturnInTransaction,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 10:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.VoidLine,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 11:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.AddPayment,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 12:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.CreateCustomer,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 13:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.ViewSalesHistory,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 14:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.CustomerComments,
                                StaffId = staff,
                                Type = PermissionType.List,
                                Value = data.FieldValue
                            };
                            break;
                        case 15:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.UpdateCustomer,
                                StaffId = staff,
                                Type = PermissionType.Boolean,
                                Value = data.FieldValue
                            };
                            break;
                        case 16:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.MaxDiscountToGiveAmount,
                                StaffId = staff,
                                Type = PermissionType.Decimal,
                                Value = data.FieldValue
                            };
                            break;
                        case 17:
                            line = new ReplStaffPermission()
                            {
                                Id = PermissionEntry.MaxTotalDiscountPercent,
                                StaffId = staff,
                                Type = PermissionType.Decimal,
                                Value = data.FieldValue
                            };
                            break;
                    }
                    if (fld.FieldIndex > 1)
                        list.Add(line);
                }
            }
            return list;
        }

        public List<ReplTerminal> GetReplTerminal(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplTerminal> list = new List<ReplTerminal>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplTerminal line = new ReplTerminal();
                line.Features.AddFlag(FeatureFlagName.ExitAfterEachTransaction, 0);
                line.Features.AddFlag(FeatureFlagName.AutoLogOffAfterMin, 0);
                line.Features.AddFlag(FeatureFlagName.ShowNumberPad, 0);

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.StoreId = data.FieldValue; break;
                        case 5: line.Id = data.FieldValue; break;
                        case 7: line.TerminalType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 10: line.Name = data.FieldValue; break;
                        case 182: line.EFTStoreId = data.FieldValue; break;
                        case 183: line.SFTTerminalId = data.FieldValue; break;
                        case 150: line.Features.AddFlag(FeatureFlagName.ExitAfterEachTransaction, data.FieldValue); break;
                        case 155: line.Features.AddFlag(FeatureFlagName.AutoLogOffAfterMin, data.FieldValue); break;
                        case 601: line.DefaultHospType = data.FieldValue; break;
                        case 1005: line.HardwareProfile = data.FieldValue; break;
                        case 1007: line.VisualProfile = data.FieldValue; break;
                        case 1008: line.HardwareProfile = data.FieldValue; break;
                        case 2102: line.HospTypeFilter = data.FieldValue; break;
                        case 3003: line.ASNQuantityMethod = (AsnQuantityMethod)ConvertTo.SafeInt(data.FieldValue); break;
                        case 10012704: line.Features.AddFlag(FeatureFlagName.ShowNumberPad, data.FieldValue); break;
                        case 10012705: line.DeviceType = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                //GetFeatureFlags(term.Features, term.StoreId, term.Id);
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplTerminal line = new ReplTerminal()
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
                        case 5: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplCountryCode> GetReplCountry(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCountryCode> list = new List<ReplCountryCode>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplCountryCode line = new ReplCountryCode();
                string lcy = string.Empty;

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": line.Code = data.FieldValue; break;
                        case "Name": line.Name = data.FieldValue; break;
                        case "LSC Web Store Customer No.": line.CustomerNo = data.FieldValue; break;
                        case "TaxPostGroup": line.TaxPostGroup = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplCountryCode line = new ReplCountryCode()
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
                        case "Code": line.Code = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplHierarchy> GetReplHierarchy(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplHierarchy> list = new List<ReplHierarchy>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplHierarchy line = new ReplHierarchy();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.Id = data.FieldValue; break;
                        case "Description": line.Description = data.FieldValue; break;
                        case "Type": line.Type = (HierarchyType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "Start Date": line.StartDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case "Priority": line.Priority = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Sales Type Filter": line.SalesType = data.FieldValue; break;
                        case "Validation Schedule ID": line.ValidationScheduleId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplHierarchy line = new ReplHierarchy()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplHierarchyNode> GetReplHierarchyNode(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplHierarchyNode> list = new List<ReplHierarchyNode>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplHierarchyNode line = new ReplHierarchyNode();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.HierarchyCode = data.FieldValue; break;
                        case "Node ID": line.Id = data.FieldValue; break;
                        case "Parent Node ID": line.ParentNode = data.FieldValue; break;
                        case "Description": line.Description = data.FieldValue; break;
                        case "Children Order": line.ChildrenOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Indentation": line.Indentation = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Presentation Order": line.PresentationOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Retail Image ID": line.ImageId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplHierarchyNode line = new ReplHierarchyNode()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.HierarchyCode = data.FieldValue; break;
                        case "Node ID": line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplHierarchyLeaf> GetReplHierarchyLeaf(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplHierarchyLeaf> list = new List<ReplHierarchyLeaf>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplHierarchyLeaf line = new ReplHierarchyLeaf();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.HierarchyCode = data.FieldValue; break;
                        case "Node ID": line.NodeId = data.FieldValue; break;
                        case "Type": line.Type = (HierarchyLeafType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "No.": line.Id = data.FieldValue; break;
                        case "Description": line.Description = data.FieldValue; break;
                        case "Item Unit of Measure": line.ItemUOM = data.FieldValue; break;
                        case "Sort Order": line.SortOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Retail Image ID": line.ImageId = data.FieldValue; break;
                        case "Member Type": line.IsMemberClub = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case "Member Value": line.MemberValue = data.FieldValue; break;
                        case "Deal Price": line.DealPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Validation Period ID": line.ValidationPeriod = data.FieldValue; break;
                        case "Status": line.IsActive = ConvertTo.SafeBoolean(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplHierarchyLeaf line = new ReplHierarchyLeaf()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.HierarchyCode = data.FieldValue; break;
                        case "Node ID": line.NodeId = data.FieldValue; break;
                        case "Type": line.Type = (HierarchyLeafType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "No.": line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplHierarchyHospDeal> GetReplHierarchyDeal(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplHierarchyHospDeal> list = new List<ReplHierarchyHospDeal>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplHierarchyHospDeal line = new ReplHierarchyHospDeal();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.HierarchyCode = data.FieldValue; break;
                        case "Node ID": line.ParentNode = data.FieldValue; break;
                        case "No.": line.No = data.FieldValue; break;
                        case "Description": line.Description = data.FieldValue; break;
                        case "Offer No.": line.DealNo = data.FieldValue; break;
                        case "Line No.": line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Type": line.Type = (HierarchyDealType)ConvertTo.SafeInt(data.FieldValue); break;
                        case "Variant Code": line.VariantCode = data.FieldValue; break;
                        case "Unit of Measure": line.UnitOfMeasure = data.FieldValue; break;
                        case "Min. Selection": line.MinSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Max. Selection": line.MaxSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Modifier Added Amount": line.AddedAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Deal Mod. Size Gr. Index": line.DealModSizeGroupIndex = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Retail Image ID": line.ImageId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplHierarchyHospDeal line = new ReplHierarchyHospDeal()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Offer No.": line.DealNo = data.FieldValue; break;
                        case "Line No.": line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplHierarchyHospDealLine> GetReplHierarchyDealLine(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplHierarchyHospDealLine> list = new List<ReplHierarchyHospDealLine>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplHierarchyHospDealLine line = new ReplHierarchyHospDealLine();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Hierarchy Code": line.HierarchyCode = data.FieldValue; break;
                        case "Node ID": line.ParentNode = data.FieldValue; break;
                        case "Offer No.": line.DealNo = data.FieldValue; break;
                        case "Offer Line No.": line.DealLineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Deal Modifier Line No.": line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Deal Modifier Code": line.DealLineCode = data.FieldValue; break;
                        case "Item No.": line.ItemNo = data.FieldValue; break;
                        case "Description": line.Description = data.FieldValue; break;
                        case "Variant Code": line.VariantCode = data.FieldValue; break;
                        case "Unit of Measure": line.UnitOfMeasure = data.FieldValue; break;
                        case "Min. Selection": line.MinSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Max. Item Selection": line.MaxSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Added Amount": line.AddedAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case "Retail Image ID": line.ImageId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplHierarchyHospDealLine line = new ReplHierarchyHospDealLine()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Offer No.": line.DealNo = data.FieldValue; break;
                        case "Offer Line No.": line.DealLineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case "Deal Modifier Line No.": line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplPlu> GetReplPlu(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplPlu> list = new List<ReplPlu>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplPlu line = new ReplPlu();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.StoreId = data.FieldValue; break;
                        case 2: line.PageId = ConvertTo.SafeInt(data.FieldValue); break;
                        case 3: line.PageIndex = ConvertTo.SafeInt(data.FieldValue); break;
                        case 10: line.ItemId = data.FieldValue; break;
                        case 11: line.Descritpion = data.FieldValue; break;
                        case 12: line.ImageId = data.FieldValue; break;
                        case 13: line.ItemImage = Convert.FromBase64String(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplDataTranslation> GetReplTranslation(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDataTranslation> list = new List<ReplDataTranslation>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplDataTranslation line = new ReplDataTranslation();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.TranslationId = data.FieldValue; break;
                        case 2: line.Key = data.FieldValue; break;
                        case 3: line.LanguageCode = data.FieldValue; break;
                        case 10: line.Text = data.FieldValue; break;
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
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.TranslationId = data.FieldValue; break;
                        case 2: line.Key = data.FieldValue; break;
                        case 3: line.LanguageCode = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplBarcodeMask> GetReplBarcodeMask(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplBarcodeMask> list = new List<ReplBarcodeMask>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplBarcodeMask line = new ReplBarcodeMask();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.Mask = data.FieldValue; break;
                        case 10: line.Description = data.FieldValue; break;
                        case 15: line.MaskType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 25: line.Prefix = data.FieldValue; break;
                        case 30: line.Symbology = ConvertTo.SafeInt(data.FieldValue); break;
                        case 35: line.NumberSeries = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplBarcodeMask line = new ReplBarcodeMask()
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
                        case 1: line.Id = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplBarcodeMaskSegment> GetReplBarcodeSegment(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplBarcodeMaskSegment> list = new List<ReplBarcodeMaskSegment>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            int rowid = 1;
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplBarcodeMaskSegment line = new ReplBarcodeMaskSegment();
                line.Id = rowid++;
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 10: line.MaskId = ConvertTo.SafeInt(data.FieldValue); break;
                        case 20: line.Number = ConvertTo.SafeInt(data.FieldValue); break;
                        case 25: line.Length = ConvertTo.SafeInt(data.FieldValue); break;
                        case 30: line.SegmentType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 35: line.Decimals = ConvertTo.SafeInt(data.FieldValue); break;
                        case 40: line.Char = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplBarcodeMaskSegment line = new ReplBarcodeMaskSegment()
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
                        case 10: line.MaskId = ConvertTo.SafeInt(data.FieldValue); break;
                        case 20: line.Number = ConvertTo.SafeInt(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplCustomer> GetReplCustomer(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCustomer> list = new List<ReplCustomer>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplCustomer line = new ReplCustomer();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.AccountNumber = data.FieldValue; break;
                        case 2: line.Name = data.FieldValue; break;
                        case 5: line.Street = data.FieldValue; break;
                        case 7: line.City = data.FieldValue; break;
                        case 9: line.PhoneLocal = data.FieldValue; break;
                        case 15: line.State = data.FieldValue; break;
                        case 22: line.Currency = data.FieldValue; break;
                        case 23: line.PriceGroup = data.FieldValue; break;
                        case 27: line.PaymentTerms = data.FieldValue; break;
                        case 34: line.DiscountGroup = data.FieldValue; break;
                        case 35: line.Country = data.FieldValue; break;
                        case 39: line.Blocked = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 82: line.IncludeTax = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 83: line.ShippingLocation = data.FieldValue; break;
                        case 91: line.ZipCode = data.FieldValue; break;
                        case 92: line.County = data.FieldValue; break;
                        case 102: line.Email = data.FieldValue; break;
                        case 103: line.URL = data.FieldValue; break;
                        case 110: line.TaxGroup = data.FieldValue; break;
                        case 10012701: line.CellularPhone = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplCustomer line = new ReplCustomer()
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
                        case 1: line.AccountNumber = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplCustomer> GetReplMember(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCustomer> list = new List<ReplCustomer>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplCustomer line = new ReplCustomer();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.AccountNumber = data.FieldValue; break;
                        case 2: line.ClubCode = data.FieldValue; break;
                        case 3: line.SchemeCode = data.FieldValue; break;
                        case 5: line.Id = data.FieldValue; break;
                        case 10: line.Name = data.FieldValue; break;
                        case 13: line.Street = data.FieldValue; break;
                        case 15: line.City = data.FieldValue; break;
                        case 17: line.ZipCode = data.FieldValue; break;
                        case 18: line.Email = data.FieldValue; break;
                        case 19: line.URL = data.FieldValue; break;
                        case 20: line.PhoneLocal = data.FieldValue; break;
                        case 21: line.CellularPhone = data.FieldValue; break;
                        case 23: line.State = data.FieldValue; break;
                        case 26: line.County = data.FieldValue; break;
                        case 27: line.Country = data.FieldValue; break;
                        case 31: line.BirthDay = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                        case 100: line.Blocked = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case 5054: line.FirstName = data.FieldValue; break;
                        case 5055: line.MiddleName = data.FieldValue; break;
                        case 5056: line.LastName = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplCustomer line = new ReplCustomer()
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
                        case 1: line.AccountNumber = data.FieldValue; break;
                        case 5: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplCustomer> GetReplMember2(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCustomer> list = new List<ReplCustomer>();
            ReplODataSet result = null;

            result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.DataSet.DataSetUpd.DynDataSet.DataSetRows)
            {
                ReplCustomer line = new ReplCustomer();
                line.Cards = new List<Card>();

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Account No.": line.AccountNumber = data.FieldValue; break;
                        case "Club Code": line.ClubCode = data.FieldValue; break;
                        case "Scheme Code": line.SchemeCode = data.FieldValue; break;
                        case "Contact No.": line.Id = data.FieldValue; break;
                        case "Name": line.Name = data.FieldValue; break;
                        case "Address": line.Street = data.FieldValue; break;
                        case "City": line.City = data.FieldValue; break;
                        case "Post Code": line.ZipCode = data.FieldValue; break;
                        case "E-Mail": line.Email = data.FieldValue; break;
                        case "Home Page": line.URL = data.FieldValue; break;
                        case "Phone No.": line.PhoneLocal = data.FieldValue; break;
                        case "Mobile Phone No.": line.CellularPhone = data.FieldValue; break;
                        case "Territory Code": line.State = data.FieldValue; break;
                        case "County": line.County = data.FieldValue; break;
                        case "Country/Region Code": line.Country = data.FieldValue; break;
                        case "Blocked": line.Blocked = XMLHelper.GetWebBoolInt(data.FieldValue); break;
                        case "First Name": line.FirstName = data.FieldValue; break;
                        case "Middle Name": line.MiddleName = data.FieldValue; break;
                        case "Surname": line.LastName = data.FieldValue; break;
                        case "CardNo": line.Cards.Add(new Card(data.FieldValue)); break;
                        case "LoginId": line.UserName = data.FieldValue; break;
                        case "CustomerId": line.CustomerId = data.FieldValue; break;
                        case "Send Receipt by E-mail": line.SendReceiptByEMail = (SendEmail)ConvertTo.SafeInt(data.FieldValue); break;
                        case "Date of Birth": line.BirthDay = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                    }
                }
                list.Add(line);
            }

            if (result.DataSet.DataSetDel == null || result.DataSet.DataSetDel.DynDataSet == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.DataSet.DataSetDel.DynDataSet.DataSetRows)
            {
                ReplCustomer line = new ReplCustomer()
                {
                    IsDeleted = true
                };

                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataSetField fld = result.DataSet.DataSetUpd.DynDataSet.DataSetFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Account No.": line.AccountNumber = data.FieldValue; break;
                        case "Contact No.": line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplVendor> GetReplVendor(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplVendor> list = new List<ReplVendor>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplVendor line = new ReplVendor();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Name = data.FieldValue; break;
                        case 39: line.Blocked = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 54: line.UpdatedOnUtc = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                    }
                }

                line.AllowCustomersToSelectPageSize = false;
                line.DisplayOrder = 1;
                line.ManufacturerTemplateId = 1;
                line.PageSize = 4;
                line.PictureId = 0;
                line.Published = true;

                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplVendor line = new ReplVendor()
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

        public List<ReplCollection> GetReplCollection(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCollection> list = new List<ReplCollection>();
            ReplOTableData result = null;

            result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplCollection line = new ReplCollection();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.UnitOfMeasureId = data.FieldValue; break;
                        case 2: line.ItemId = data.FieldValue; break;
                        case 3: line.VariantId = data.FieldValue; break;
                        case 4: line.Quantity = ConvertTo.SafeDecimal(data.FieldValue); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplCollection line = new ReplCollection()
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
                        case 1: line.UnitOfMeasureId = data.FieldValue; break;
                        case 2: line.ItemId = data.FieldValue; break;
                        case 3: line.VariantId = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }
    }
}
