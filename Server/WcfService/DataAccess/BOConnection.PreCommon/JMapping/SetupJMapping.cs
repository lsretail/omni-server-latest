using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class SetupJMapping : BaseJMapping
    {
        public SetupJMapping(bool json)
        {
            IsJson = json;
        }

        public List<ReplCurrency> GetReplCurrency(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCurrency> list = new List<ReplCurrency>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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

        public List<ReplStoreTenderTypeCurrency> GetReplTenderTypeCurrency(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplStoreTenderTypeCurrency> list = new List<ReplStoreTenderTypeCurrency>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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

        public List<ReplDiscountValidation> GetPeriod(string ret, string tenderMap, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplDiscountValidation> list = new List<ReplDiscountValidation>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
                        case "Node ID": line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplHierarchyHospDeal> GetReplHierarchyDeal(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplHierarchyHospDeal> list = new List<ReplHierarchyHospDeal>();
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
                    };
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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

        public List<ReplGS1BarcodeSetup> GetReplBarcodeGS1(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplGS1BarcodeSetup> list = new List<ReplGS1BarcodeSetup>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
            if (result == null)
                return list;

            // Insert update records
            foreach (ReplODataRecord rec in result.TableData.TableDataUpd.RecRefJson.Records)
            {
                ReplGS1BarcodeSetup line = new ReplGS1BarcodeSetup();
                foreach (ReplODataField data in rec.Fields)
                {
                    ReplODataRecordField fld = result.TableData.TableDataUpd.RecRefJson.RecordFields.Find(f => f.FieldIndex == data.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldNo)
                    {
                        case 1: line.Type = ConvertTo.SafeInt(data.FieldValue); break;
                        case 2: line.Identifier = data.FieldValue; break;
                        case 3: line.SectionType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 4: line.SectionSize = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.IdentifierSize = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.SectionMapping = ConvertTo.SafeInt(data.FieldValue); break;
                        case 7: line.MappingStartingChar = ConvertTo.SafeInt(data.FieldValue); break;
                        case 8: line.PreferredSequence = ConvertTo.SafeInt(data.FieldValue); break;
                        case 9: line.Decimals = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 10: line.ValueType = ConvertTo.SafeInt(data.FieldValue); break;
                        case 100: line.Value = data.FieldValue; break;
                        case 101: line.ValueDec = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 102: line.ValueDate = ConvertTo.SafeJsonDate(ConvertTo.SafeDateTime(data.FieldValue), IsJson); break;
                    }
                }
                list.Add(line);
            }

            if (result.TableData.TableDataDel == null || result.TableData.TableDataDel.RecRefJson == null)
                return list;

            // Deleted Action Records
            foreach (ReplODataRecord rec in result.TableData.TableDataDel.RecRefJson.Records)
            {
                ReplGS1BarcodeSetup line = new ReplGS1BarcodeSetup()
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
                        case 1: line.Type = ConvertTo.SafeInt(data.FieldValue); break;
                        case 2: line.Identifier = data.FieldValue; break;
                    }
                }
                list.Add(line);
            }
            return list;
        }

        public List<ReplCustomer> GetReplCustomer(string ret, ref string lastKey, ref int recordsRemaining)
        {
            List<ReplCustomer> list = new List<ReplCustomer>();
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplODataSet result = JsonToDataSet(ret, ref lastKey, ref recordsRemaining);
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
                        case "Send Receipt by E-mail": line.SendReceiptByEMail = (SendEmail)ConvertTo.SafeInt(data.FieldValue); break;
                    };
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
            ReplOTableData result = JsonToTableData(ret, ref lastKey, ref recordsRemaining);
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
