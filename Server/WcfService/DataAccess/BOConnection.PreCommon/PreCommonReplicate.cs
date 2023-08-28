using System;
using System.Collections.Generic;
using System.Linq;
using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.JMapping;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Replication;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
    {
        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIItemBuffer(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplItems(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(27, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplItem> list = rep.ReplicateItems(table);

            if (list.Count == 0)
                return list;

            // fields to get 
            List<string> fields = new List<string>()
            {
                "Item No.", "Html"
            };

            int index = 0;
            while (true)
            {
                List<string> values = new List<string>();
                int end;
                if (list.Count <= 10)
                {
                    end = list.Count;
                }
                else
                {
                    end = index + 10;
                    if (end > list.Count)
                        end = list.Count;
                }

                for (int i = index; i < end; i++)
                {
                    values.Add(list[i].Id);
                }

                // filter
                List<XMLFieldData> filter = new List<XMLFieldData>();
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Item No.",
                    Values = values
                });

                // get HTML detail
                try
                {
                    NAVWebXml xml = new NAVWebXml();
                    string xmlRequest = xml.GetBatchWebRequestXML("LSC Item HTML", fields, filter, string.Empty);
                    string xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);

                    rep.ReplicateItemsHtml(table, list);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to get HTML details");
                }

                index += 10;
                if (index >= list.Count - 1)
                    break;
            }
            return list;
        }

        public List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetBarcode(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplBarcode(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001451, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateBarcodes(table);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetBarcodeMaskSegment(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplBarcodeSegment(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001480, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateBarcodeMaskSegments(table);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetBarcodeMask(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplBarcodeMask(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001459, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateBarcodeMasks(table);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetExtdVariantValues(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplExtVariants(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10001413, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplExtendedVariantValue> list = rep.ReplicateExtendedVariantValues(table);

            // fields to get 
            List<string> fields = new List<string>()
            {
                "Logical Order"
            };

            List<XMLFieldData> filter = new List<XMLFieldData>();
            NAVWebXml xml = new NAVWebXml();
            foreach (ReplExtendedVariantValue extvar in list)
            {
                // filter
                filter.Clear();
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Framework Code",
                    Values = new List<string>() { extvar.FrameworkCode }
                });
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Dimension No.",
                    Values = new List<string>() { extvar.Dimensions }
                });
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Item",
                    Values = new List<string>() { "=''" }
                });

                try
                {
                    // get Qty for UOM
                    string xmlRequest = xml.GetBatchWebRequestXML("LSC Extd. Variant Dimensions", fields, filter, string.Empty);
                    string xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);
                    if (table != null && table.NumberOfValues > 0)
                    {
                        extvar.DimensionLogicalOrder = (string.IsNullOrEmpty(table.FieldList[0].Values[0]) ? 0 : Convert.ToInt32(table.FieldList[0].Values[0]));
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to get Logical order value");
                }
            }
            return list;
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret;
                if (LSCVersion >= new Version("20.3"))
                {
                    ret = odataWS.GetItemUnitOfMeasure(storeId, batchSize, fullRepl, lastKey, lastEntry);
                    logger.Trace(config.LSKey.Key, ret);
                    return map.GetReplItemUOM2(ret, ref lastKey, ref recordsRemaining);
                }
                else
                {
                    ret = odataWS.GetItemUnitOfMeasure(storeId, batchSize, fullRepl, lastKey, lastEntry);
                    logger.Trace(config.LSKey.Key, ret);
                    return map.GetReplItemUOM(ret, ref lastKey, ref recordsRemaining);
                }
            }

            XMLTableData table = DoReplication(5404, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplItemUnitOfMeasure> list = rep.ReplicateItemUnitOfMeasure(table);

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest;
            string xmlResponse;

            foreach (ReplItemUnitOfMeasure item in list)
            {
                xmlRequest = xml.GetGeneralWebRequestXML("Unit of Measure", "Code", item.Code);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table != null && table.NumberOfValues > 0)
                {
                    item.Description = table.FieldList[1].Values[0];
                }
            }

            return list;
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret;
                if (LSCVersion >= new Version("20.1"))
                {
                    ret = odataWS.GetVariantRegWithStatus(storeId, batchSize, fullRepl, lastKey, lastEntry);
                    logger.Trace(config.LSKey.Key, ret);
                    return map.GetReplItemVariantWithStatus(ret, ref lastKey, ref recordsRemaining);
                }
                else
                {
                    ret = odataWS.GetVariantReg(storeId, batchSize, fullRepl, lastKey, lastEntry);
                    logger.Trace(config.LSKey.Key, ret);
                    return map.GetReplItemVariantReg(ret, ref lastKey, ref recordsRemaining);
                }
            }

            XMLTableData table = DoReplication(10001414, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateItemVariantRegistration(table);
        }

        public virtual List<ReplItemVariant> ReplicateItemVariant(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("21.0"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetItemVariant(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplItemVariant(ret, ref lastKey, ref recordsRemaining);
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Item Variant");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateItemVariant(table);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.5"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetVendor(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplVendor(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(23, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateVendor(table);
        }

        public virtual List<ReplLoyVendorItemMapping> ReplicateVendorItems(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("21.0"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetVendorItem(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplVendorItem(ret, ref lastKey, ref recordsRemaining);
            }

            List<ReplLoyVendorItemMapping> list = new List<ReplLoyVendorItemMapping>();
            List<ReplItem> items = ReplicateItems(appId, string.Empty, storeId, batchSize, fullRepl, ref lastKey, ref recordsRemaining);
            foreach (ReplItem item in items)
            {
                if (string.IsNullOrEmpty(item.VendorId))
                    continue;

                list.Add(new ReplLoyVendorItemMapping()
                {
                    NavManufacturerId = item.VendorId,
                    NavManufacturerItemId = item.VendorItemId,
                    NavProductId = item.Id,
                    Deleted = false,
                    DisplayOrder = 1,
                    IsFeaturedProduct = true
                });
            }
            return list;
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetCurrency(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplCurrency(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(4, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateCurrency(table);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetCurrencyExchRate(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplCurrencyExchRate(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(330, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateCurrencyExchRate(table);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetItemCategory(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplItemCatagory(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(5722, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateItemCategory(table);
        }

        public virtual List<ReplItemLocation> ReplicateItemLocation(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIItemLocationBuffer(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplItemLocation(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001533, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateItemLocation(table);
        }

        public virtual List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                // TODO update
                if (fullRepl == false)
                {
                    if (lastKey.Contains(';') == false)
                        lastKey = "0";
                    fullRepl = true;
                }

                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIPrice(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplPrice(ret, ref lastKey, ref recordsRemaining);
            }

            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("LSC WI Price", null, filter, lastKey, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplPrice> list = rep.ReplicatePrice(table, ref lastKey);

            // fields to get 
            List<string> fields = new List<string>()
            {
                "Item No.", "Code", "Qty. per Unit of Measure"
            };

            foreach (ReplPrice price in list)
            {
                filter.Clear();
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Item No.",
                    Values = new List<string>() { price.ItemId }
                });
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Code",
                    Values = new List<string>() { price.UnitOfMeasure }
                });

                try
                {
                    // get Qty for UOM
                    xmlRequest = xml.GetBatchWebRequestXML("Item Unit of Measure", fields, filter, string.Empty);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);
                    if (table == null || table.NumberOfValues == 0)
                        continue;

                    string item = string.Empty;
                    string code = string.Empty;
                    foreach (XMLFieldData field in table.FieldList)
                    {
                        switch (field.FieldName)
                        {
                            case "Code": code = field.Values[0]; break;
                            case "Item No.": item = field.Values[0]; break;
                            case "Qty. per Unit of Measure":
                                ReplPrice it = list.Find(f => f.ItemId == item && f.UnitOfMeasure == code);
                                if (it != null)
                                    it.QtyPerUnitOfMeasure = (string.IsNullOrEmpty(field.Values[0]) ? 0 : ConvertTo.SafeDecimal(field.Values[0]));
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to get detailed values");
                }
            }
            return list;
        }

        public virtual List<ReplPrice> ReplicateBasePrice(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("20.4"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetSalesPrice(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplBasePrice(ret, storeId, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetProductGroup(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplProductGroup(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10000705, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateProductGroups(table);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            NAVWebXml xml = null;
            if (LSCVersion < new Version("20.4"))
            {
                if (string.IsNullOrEmpty(appId))
                {
                    appId = ecomAppId;
                }
                if (string.IsNullOrEmpty(appType))
                {
                    appType = ecomAppType;
                }

                xml = new NAVWebXml(storeId, appId, appType);
                if (ResetReplication(fullRepl, lastKey))
                {
                    int i = 0;
                    StartWebReplication(xml, 0, ref i);
                }
            }

            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetStoreBuffer(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplStore(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001470, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplStore> list = rep.ReplicateStores(table);
            string curcode = string.Empty;
            try
            {
                string xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table != null && table.NumberOfValues > 0)
                {
                    XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                    curcode = field.Values[0];
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to get Logical local currency");
            }

            foreach (ReplStore store in list)
            {
                if (string.IsNullOrWhiteSpace(store.Currency) == false)
                    continue;

                store.Currency = curcode;
            }
            return list;
        }

        public virtual List<ReplStore> ReplicateInvStores(string appId, string appType, string storeId, bool fullReplication, string terminalId)
        {
            string lastKey = string.Empty;
            string maxKey = string.Empty;
            int recordsRemaining = 0;

            if (string.IsNullOrEmpty(appId))
            {
                appId = ecomAppId;
            }
            if (string.IsNullOrEmpty(appType))
            {
                appType = ecomAppType;
            }

            //Get all storeIds linked to this terminal
            NAVWebXml xml = new NAVWebXml(storeId, appId, appType);
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Inventory Terminal-Store");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplStore> list = rep.ReplicateInvStores(table, terminalId);

            //Get all stores
            List<ReplStore> allStores = ReplicateStores(appId, appType, storeId, 0, fullReplication, ref lastKey, ref recordsRemaining);

            //Only return the stores connected to this terminal
            List<ReplStore> InvStores = new List<ReplStore>();
            if (list.Count == 0)
            {
                ReplStore temp = allStores.FirstOrDefault(x => x.Id == storeId);
                if (temp != null)
                    InvStores.Add(temp);
            }
            else
            {
                foreach (ReplStore store in list)
                {
                    ReplStore temp = allStores.FirstOrDefault(x => x.Id == store.Id);
                    if (temp != null)
                        InvStores.Add(temp);
                }
            }
            return InvStores;
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetUnitOfMeasure(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplUOM(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(204, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateUnitOfMeasure(table);
        }

        public virtual List<ReplCollection> ReplicateCollection(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("20.4"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetCollection(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplCollection(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10001430, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateCollection(table);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                // TODO update
                if (fullRepl == false)
                {
                    if (lastKey.Contains(';') == false)
                        lastKey = "0";
                    fullRepl = true;
                }

                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIDiscounts(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplDiscount(ret, ref lastKey, ref recordsRemaining);
            }

            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("LSC WI Discounts", null, filter, lastKey, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplDiscount> list = rep.ReplicateDiscounts(table, ref lastKey);
            List<ReplDiscount> extlist = new List<ReplDiscount>();

            foreach (ReplDiscount disc in list)
            {
                ReplDiscount it = extlist.Find(f => f.OfferNo == disc.OfferNo);
                if (it != null)
                    continue;

                filter.Clear();
                filter.Add(new XMLFieldData()
                {
                    FieldName = "No.",
                    Values = new List<string>() { disc.OfferNo }
                });

                // get Qty for UOM
                xmlRequest = xml.GetBatchWebRequestXML("LSC Periodic Discount", null, filter, string.Empty);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    continue;

                bool hasdata = false;
                decimal amt = 0;
                it = new ReplDiscount(config.IsJson);
                extlist.Add(it);
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.":
                            it.OfferNo = field.Values[0];
                            break;
                        case "Type":
                            it.Type = (ReplDiscountType)ConvertTo.SafeInt(field.Values[0]);
                            break;
                        case "Discount Type":
                            it.DiscountValueType = (DiscountValueType)ConvertTo.SafeInt(field.Values[0]);
                            break;
                        case "Description":
                            it.Description = field.Values[0];
                            break;
                        case "Pop-up Line 1":
                        case "Pop-up Line 2":
                        case "Pop-up Line 3":
                            if (string.IsNullOrEmpty(field.Values[0]) == false)
                                it.Details += (string.IsNullOrEmpty(it.Details) ? string.Empty : "\r\n") + field.Values[0];
                            break;
                        case "Validation Period ID":
                            it.ValidationPeriodId = ConvertTo.SafeInt(field.Values[0]);
                            break;
                        case "Discount Amount Value":
                            amt = ConvertTo.SafeDecimal(field.Values[0]);
                            break;
                    }

                    if (hasdata)
                        break;
                }

                if (amt > 0 && disc.Type == ReplDiscountType.DiscOffer)
                {
                    it.DiscountValueType = DiscountValueType.Amount;
                    it.DiscountValue = amt;
                }
                else
                {
                    it.DiscountValue = disc.DiscountValue;
                }
            }

            foreach (ReplDiscount disc in extlist)
            {
                List<ReplDiscount> batch = list.FindAll(d => d.OfferNo == disc.OfferNo);
                foreach (ReplDiscount d in batch)
                {
                    d.Type = disc.Type;
                    d.DiscountValueType = disc.DiscountValueType;
                    d.Description = disc.Description;
                    d.Details = disc.Details;
                    d.ValidationPeriodId = disc.ValidationPeriodId;
                    d.DiscountValue = disc.DiscountValue;
                }
            }
            return list;
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                // TODO update
                if (fullRepl == false)
                {
                    if (lastKey.Contains(';') == false)
                        lastKey = "0";
                    fullRepl = true;
                }

                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIMixMatch(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplMixMatch(ret, ref lastKey, ref recordsRemaining);
            }

            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("LSC WI Mix & Match Offer", null, filter, lastKey, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplDiscount> list = rep.ReplicateDiscounts(table, ref lastKey);

            foreach (ReplDiscount disc in list)
            {
                filter.Clear();
                filter.Add(new XMLFieldData()
                {
                    FieldName = "No.",
                    Values = new List<string>() { disc.OfferNo }
                });

                // get Qty for UOM
                xmlRequest = xml.GetBatchWebRequestXML("LSC Periodic Discount", null, filter, string.Empty);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    continue;

                decimal amt = 0;
                string code = string.Empty;
                ReplDiscount it;
                foreach (XMLFieldData field in table.FieldList)
                {
                    it = null;
                    switch (field.FieldName)
                    {
                        case "No.":
                            code = field.Values[0];
                            it = list.Find(f => f.OfferNo == code);
                            break;
                        case "Type":
                            if (it != null)
                                it.Type = (ReplDiscountType)ConvertTo.SafeInt(field.Values[0]);
                            break;
                        case "Discount Type":
                            if (it != null)
                                it.DiscountValueType = (DiscountValueType)ConvertTo.SafeInt(field.Values[0]);
                            break;
                        case "Description":
                            if (it != null)
                                it.Description = field.Values[0];
                            break;
                        case "Pop-up Line 1":
                        case "Pop-up Line 2":
                        case "Pop-up Line 3":
                            if (it != null)
                                it.Details += field.Values[0] + "\r\n";
                            break;
                        case "Validation Period ID":
                            if (it != null)
                                it.ValidationPeriodId = ConvertTo.SafeInt(field.Values[0]);
                            break;
                        case "Discount Amount Value":
                            if (it != null)
                                amt = ConvertTo.SafeDecimal(field.Values[0]);
                            break;
                    }
                }

                if (amt > 0 && disc.Type == ReplDiscountType.DiscOffer)
                {
                    disc.DiscountValueType = DiscountValueType.Amount;
                    disc.DiscountValue = amt;
                }
            }
            return list;
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("20.4"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetValidationPeriod(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);

                string tenderMap = config.SettingsGetByKey(ConfigKey.TenderType_Mapping);
                return map.GetPeriod(ret, tenderMap, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001481, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateDiscountValidations(table);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetTenderType(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);

                string tenderMap = config.SettingsGetByKey(ConfigKey.TenderType_Mapping);
                return map.GetReplTenderType(ret, tenderMap, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001462, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplStoreTenderType> list = rep.ReplicateStoreTenderType(table, config.SettingsGetByKey(ConfigKey.TenderType_Mapping));
            if (string.IsNullOrEmpty(storeId))
                return list;

            return list.FindAll(t => t.StoreID == storeId).ToList();
        }

        public virtual List<ReplStoreTenderTypeCurrency> ReplicateStoreTenderTypeCurrency(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetTenderTypeCurrency(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplTenderTypeCurrency(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001636, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateStoreTenderTypeCurrency(table);
        }

        public virtual List<ReplItemModifier> ReplicateItemModifier(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIItemModifier(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplModifier(ret, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public virtual List<ReplItemRecipe> ReplicateItemRecipe(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIItemRecipeBuffer(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplRecipe(ret, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetHierarchy(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHierarchy(ret, ref lastKey, ref recordsRemaining);
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Hierar. Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table == null || table.NumberOfValues == 0)
                return null;

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplHierarchy> list = new List<ReplHierarchy>();

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                string hircode = string.Empty;
                DateTime startdate = DateTime.MinValue;

                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Hierarchy Code": hircode = field.Values[i]; break;
                        case "Start Date": startdate = ConvertTo.SafeJsonDate(XMLHelper.GetWebDateTime(field.Values[i]), config.IsJson); break;
                    }
                }

                xmlRequest = xml.GetGeneralWebRequestXML("LSC Hierarchy", "Hierarchy Code", hircode);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table2 = xml.GetGeneralWebResponseXML(xmlResponse);
                list.AddRange(rep.ReplicateHierarchy(table2, startdate));
            }
            return list;
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetHierarchyNodes(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHierarchyNode(ret, ref lastKey, ref recordsRemaining);
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Hierar. Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Hierarchy Code"));

            List<ReplHierarchyNode> list = new List<ReplHierarchyNode>();
            ReplicateRepository rep = new ReplicateRepository(config);

            table = DoReplication(10000921, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);
            List<ReplHierarchyNode> data = rep.ReplicateHierarchyNode(table);

            foreach (string hircode in field.Values)
            {
                list.AddRange(data.FindAll(d => d.HierarchyCode.Equals(hircode)));
            }
            return list;
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetHierarchyLeaf(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHierarchyLeaf(ret, ref lastKey, ref recordsRemaining);
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Hierar. Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData hirfield = table.FieldList.Find(f => f.FieldName.Equals("Hierarchy Code"));

            List<ReplHierarchyLeaf> list = new List<ReplHierarchyLeaf>();
            ReplicateRepository rep = new ReplicateRepository(config);

            table = DoReplication(10000922, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);
            List<ReplHierarchyLeaf> data = rep.ReplicateHierarchyLeaf(table);

            foreach (string hircode in hirfield.Values)
            {
                list.AddRange(data.FindAll(d => d.HierarchyCode.Equals(hircode)));
            }

            // get image ids
            List<XMLFieldData> filter = new List<XMLFieldData>();
            foreach (ReplHierarchyLeaf leaf in list)
            {
                filter.Clear();
                filter.Add(new XMLFieldData()
                {
                    FieldName = "KeyValue",
                    Values = new List<string>() { leaf.Id }
                });
                filter.Add(new XMLFieldData()
                {
                    FieldName = "Display Order",
                    Values = new List<string>() { "0" }
                });
                filter.Add(new XMLFieldData()
                {
                    FieldName = "TableName",
                    Values = new List<string>() { (leaf.Type == HierarchyLeafType.Item) ? "Item" : "Offer" }
                });

                try
                {
                    xmlRequest = xml.GetBatchWebRequestXML("LSC Retail Image Link", null, filter, string.Empty);
                    xmlResponse = RunOperation(xmlRequest, true);
                    HandleResponseCode(ref xmlResponse);
                    table = xml.GetGeneralWebResponseXML(xmlResponse);
                    if (table != null && table.NumberOfValues > 0)
                    {
                        XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Image Id"));
                        leaf.ImageId = field.Values[0];
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to get Image Id");
                }
            }
            return list;
        }

        public virtual List<ReplHierarchyHospDeal> ReplicateHierarchyDeal(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetHierarchyDeal(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHierarchyDeal(ret, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public virtual List<ReplHierarchyHospDealLine> ReplicateHierarchyDealLine(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetHierarchyDealLine(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHierarchyDealLine(ret, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public virtual List<ReplImageLink> ReplicateImageLinks(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetImageLink(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplImageLink(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99009064, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateImageLink(table);
        }

        public virtual List<ReplImage> ReplicateImages(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetWIImageBuffer(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplImage(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99009063, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateImage(table);
        }

        public virtual List<ReplAttribute> ReplicateAttribute(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetAttribute(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplAttribute(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10000784, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateAttribute(table);
        }

        public virtual List<ReplAttributeValue> ReplicateAttributeValue(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetAttributeValues(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplAttributeValue(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10000786, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateAttributeValue(table);
        }

        public virtual List<ReplAttributeOptionValue> ReplicateAttributeOptionValue(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetAttributeOptionValues(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplAttributeOptionValue(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10000785, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateAttributeOptionValue(table);
        }

        public virtual List<ReplDataTranslation> ReplicateDataTranslation(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetDataTranslation(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplTranslation(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(10000971, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateDataTranslation(table);
        }

        public virtual List<ReplDataTranslation> ReplicateItemHtmlTranslation(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("20.4"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetItemHTML(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHtml(ret, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public virtual List<ReplDataTranslation> ReplicateDealHtmlTranslation(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("20.4"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetOfferHTML(string.Empty, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplHtml(ret, ref lastKey, ref recordsRemaining);
            }

            throw new NotImplementedException();
        }

        public virtual List<ReplDataTranslationLangCode> ReplicateDataTranslationLangCode(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("LSC Data Translation Language");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateDataTranslationLangCode(table);
        }

        public virtual List<ReplShippingAgent> ReplicateShippingAgent(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Shipping Agent");
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplShippingAgent> list = rep.ReplicateShippingAgent(table);

            foreach (ReplShippingAgent sa in list)
            {
                sa.Services = GetShippingAgentService(sa.Id, new Statistics());
            }
            return list;
        }

        public virtual List<ReplCustomer> ReplicateMember(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.5"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                if (LSCVersion >= new Version("22.3"))
                {
                    string ret = odataWS.GetContact2(batchSize, fullRepl, lastKey, lastEntry);
                    logger.Trace(config.LSKey.Key, ret);
                    return map.GetReplMember2(ret, ref lastKey, ref recordsRemaining);
                }
                else
                {
                    string ret = odataWS.GetContact(batchSize, fullRepl, lastKey, lastEntry);
                    logger.Trace(config.LSKey.Key, ret);
                    return map.GetReplMember(ret, ref lastKey, ref recordsRemaining);
                }
            }

            XMLTableData table = DoReplication(99009002, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateMembers(table);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetCustomer(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplCustomer(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(18, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateCustomer(table);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetStaff(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplStaff(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001461, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateStaff(table);
        }

        public virtual List<ReplStaffStoreLink> ReplicateStaffStoreLink(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetStoreStaff(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplStaffStore(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001633, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateStaffStoreLink(table);
        }

        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                string ret = odataWS.GetVATPostingSetup(batchSize, true, string.Empty, 0);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplTaxSetup(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(325, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateTaxSetup(table);
        }

        public virtual List<ReplTerminal> ReplicateTerminals(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetPOSTerminal(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplTerminal(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99001471, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateTerminals(table);
        }

        public virtual List<ReplCountryCode> ReplicateCountryCode(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("20.4"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetCountryCode(batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                List<ReplCountryCode> list = map.GetReplCountry(ret, ref lastKey, ref recordsRemaining);

                List<XMLFieldData> filter = new List<XMLFieldData>();
                NAVWebXml xml = new NAVWebXml();
                foreach (ReplCountryCode country in list)
                {
                    country.TaxItemGroups = new List<TaxItemGroup>();
                    if (string.IsNullOrEmpty(country.TaxPostGroup))
                        continue;

                    filter.Clear();
                    filter.Add(new XMLFieldData()
                    {
                        FieldName = "VAT Bus. Posting Group",
                        Values = new List<string>() { country.TaxPostGroup }
                    });

                    try
                    {
                        string xmlRequest = xml.GetBatchWebRequestXML("VAT Posting Setup", null, filter, string.Empty);
                        string xmlResponse = RunOperation(xmlRequest, true);
                        HandleResponseCode(ref xmlResponse);
                        XMLTableData tab = xml.GetGeneralWebResponseXML(xmlResponse);
                        if (tab != null && tab.NumberOfValues > 0)
                        {
                            for (int i = 0; i < tab.NumberOfValues; i++)
                            {
                                TaxItemGroup tax = new TaxItemGroup();
                                XMLFieldData field = tab.FieldList.Find(f => f.FieldName.Equals("VAT Prod. Posting Group"));
                                tax.Code = field.Values[0];
                                field = tab.FieldList.Find(f => f.FieldName.Equals("VAT %"));
                                tax.TaxPercent = ConvertTo.SafeDecimal(field.Values[0]);
                                country.TaxItemGroups.Add(tax);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "Failed to get Tax Item Groups");
                    }
                }
                return list;
            }

            XMLTableData table = DoReplication(9, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            return rep.ReplicateCountryCode(table);
        }

        public virtual List<ReplPlu> ReplicatePlu(string appId, string appType, string storeId, int batchSize, bool fullRepl, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                SetupJMapping map = new SetupJMapping(config.IsJson);

                string ret = odataWS.GetPluBuffer(storeId, 0, true, string.Empty, 0);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplPlu(ret, ref lastKey, ref recordsRemaining);
            }

            XMLTableData table = DoReplication(99009274, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplPlu> list = rep.ReplicatePlu(table);

            foreach (ReplPlu plu in list)
            {
                List<ImageView> imgs = ImagesGetByLink("Item", plu.ItemId, string.Empty, string.Empty, true, new Statistics());
                if (imgs.Count > 0)
                {
                    plu.ImageId = imgs[0].Id;
                    plu.ItemImage = imgs[0].ImgBytes;
                    plu.ItemImageId = imgs[0].Id;
                }
            }
            return list;
        }

        public virtual List<ReplInvStatus> ReplicateInventoryStatus(string appId, string appType, string storeId, bool fullRepl, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            if (LSCVersion >= new Version("19.3"))
            {
                ItemJMapping map = new ItemJMapping(config.IsJson, LSCVersion);

                map.SetKeys(fullRepl, ref lastKey, out int lastEntry);
                string ret = odataWS.GetInventoryStatus(storeId, batchSize, fullRepl, lastKey, lastEntry);
                logger.Trace(config.LSKey.Key, ret);
                return map.GetReplInvStatus(ret, ref lastKey, ref recordsRemaining);
            }


            // fields to get 
            List<string> fields = new List<string>()
            {
                "Item No.", "Variant Code", "Store No.", "Serial No.", "Lot No.", "Net Inventory", "Replication Counter"
            };

            if (LSCVersion >= new Version("18.3"))
            {
                fields.Add("Sourcing Location Inventory");
            }

            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            filter.Add(new XMLFieldData()
            {
                FieldName = "Replication Counter",
                Values = new List<string>() { ">" + lastKey }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("LSC Inventory Lookup Table", fields, filter, (fullRepl) ? lastKey : string.Empty, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            string maxKey = string.Empty;
            ReplicateRepository rep = new ReplicateRepository(config);
            List<ReplInvStatus> list = rep.ReplicateInvStatus(table, ref lastKey, ref maxKey);

            if (table.NumberOfValues < batchSize || fullRepl == false)
                lastKey = maxKey;

            return list;
        }
    }
}
