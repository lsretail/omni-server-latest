using System;
using System.Collections.Generic;
using System.Text;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
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
                        case 6: line.ChangePassword = ConvertTo.SafeInt(data.FieldValue); break;
                        case 7: line.StoreID = data.FieldValue; break;
                        case 35: line.FirstName = data.FieldValue; break;
                        case 36: line.LastName = data.FieldValue; break;
                        case 90: line.NameOnReceipt = data.FieldValue; break;
                        case 111: line.Blocked = ConvertTo.SafeInt(data.FieldValue); break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Name = data.FieldValue; break;
                        case 3: line.Street = data.FieldValue; break;
                        case 5: line.ZipCode = data.FieldValue; break;
                        case 6: line.City = data.FieldValue; break;
                        case 7: line.County = data.FieldValue; break;
                        case 8: line.Country = data.FieldValue; break;
                        case 9: line.Latitute = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 10: line.Longitude = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 11: line.Phone = data.FieldValue; break;
                        case 12: line.Currency = data.FieldValue; break;
                        case 13: line.FunctionalityProfile = data.FieldValue; break;
                        case 14: line.TaxGroup = data.FieldValue; break;
                        case 15: line.ClickAndCollect = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 21: lcy = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 5: line.Id = data.FieldValue; break;
                    }
                }
                list.Add(line);
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.Id = data.FieldValue; break;
                        case 2: line.Description = data.FieldValue; break;
                        case 3: line.Type = (HierarchyType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 4: line.StartDate = ConvertTo.SafeDateTime(data.FieldValue); break;
                        case 5: line.Priority = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.SalesType = data.FieldValue; break;
                        case 7: line.ValidationScheduleId = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.Id = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.HierarchyCode = data.FieldValue; break;
                        case 2: line.Id = data.FieldValue; break;
                        case 3: line.ParentNode = data.FieldValue; break;
                        case 4: line.Description = data.FieldValue; break;
                        case 5: line.ChildrenOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.Indentation = ConvertTo.SafeInt(data.FieldValue); break;
                        case 7: line.PresentationOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case 8: line.ImageId = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.HierarchyCode = data.FieldValue; break;
                        case 2: line.Id = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.HierarchyCode = data.FieldValue; break;
                        case 2: line.NodeId = data.FieldValue; break;
                        case 3: line.Type = (HierarchyLeafType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 4: line.Id = data.FieldValue; break;
                        case 5: line.Description = data.FieldValue; break;
                        case 6: line.ItemUOM = data.FieldValue; break;
                        case 7: line.SortOrder = ConvertTo.SafeInt(data.FieldValue); break;
                        case 8: line.ImageId = data.FieldValue; break;
                        case 9: line.IsMemberClub = ConvertTo.SafeBoolean(data.FieldValue); break;
                        case 10: line.MemberValue = data.FieldValue; break;
                        case 11: line.DealPrice = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 12: line.ValidationPeriod = data.FieldValue; break;
                        case 13: line.IsActive = ConvertTo.SafeBoolean(data.FieldValue); break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.HierarchyCode = data.FieldValue; break;
                        case 2: line.Id = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.HierarchyCode = data.FieldValue; break;
                        case 2: line.ParentNode = data.FieldValue; break;
                        case 3: line.DealNo = data.FieldValue; break;
                        case 4: line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.Type = (HierarchyDealType)ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.No = data.FieldValue; break;
                        case 7: line.Description = data.FieldValue; break;
                        case 8: line.VariantCode = data.FieldValue; break;
                        case 9: line.UnitOfMeasure = data.FieldValue; break;
                        case 10: line.MinSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 11: line.MaxSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 12: line.AddedAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 13: line.DealModSizeGroupIndex = ConvertTo.SafeInt(data.FieldValue); break;
                        case 14: line.ImageId = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 3: line.DealNo = data.FieldValue; break;
                        case 4: line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
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

                    switch (fld.FieldIndex)
                    {
                        case 1: line.HierarchyCode = data.FieldValue; break;
                        case 2: line.ParentNode = data.FieldValue; break;
                        case 3: line.DealNo = data.FieldValue; break;
                        case 4: line.DealLineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 6: line.DealLineCode = data.FieldValue; break;
                        case 7: line.ItemNo = data.FieldValue; break;
                        case 8: line.Description = data.FieldValue; break;
                        case 9: line.VariantCode = data.FieldValue; break;
                        case 10: line.UnitOfMeasure = data.FieldValue; break;
                        case 11: line.MinSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 12: line.MaxSelection = ConvertTo.SafeInt(data.FieldValue); break;
                        case 13: line.AddedAmount = ConvertTo.SafeDecimal(data.FieldValue); break;
                        case 14: line.ImageId = data.FieldValue; break;
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

                    switch (fld.FieldIndex)
                    {
                        case 3: line.DealNo = data.FieldValue; break;
                        case 4: line.DealLineNo = ConvertTo.SafeInt(data.FieldValue); break;
                        case 5: line.LineNo = ConvertTo.SafeInt(data.FieldValue); break;
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
                        case 22: line.Currency = data.FieldValue; break;
                        case 35: line.Country = data.FieldValue; break;
                        case 39: line.Blocked = ConvertTo.SafeInt(data.FieldValue); break;
                        case 82: line.IncludeTax = ConvertTo.SafeInt(data.FieldValue); break;
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
    }
}
