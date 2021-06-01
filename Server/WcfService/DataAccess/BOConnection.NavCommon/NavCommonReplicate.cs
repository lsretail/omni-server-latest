using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.NavCommon
{
    public partial class NavCommonBase
    {
        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(27, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
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
                NAVWebXml xml = new NAVWebXml();
                string xmlRequest = xml.GetBatchWebRequestXML("Item HTML", fields, filter, string.Empty);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                rep.ReplicateItemsHtml(table, list);

                index += 10;
                if (index >= list.Count - 1)
                    break;
            }

            return list;
        }

        public List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001451, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateBarcodes(table);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001480, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateBarcodeMaskSegments(table);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001459, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateBarcodeMasks(table);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10001413, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            List<ReplExtendedVariantValue> list = rep.ReplicateExtendedVariantValues(table);

            // fields to get 
            List<string> fields = new List<string>()
            {
                "Framework Code", "Dimension No.", "Extension", "Item", "Logical Order"
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

                // get Qty for UOM
                string xmlRequest = xml.GetBatchWebRequestXML("Extended Variant Dimensions", fields, filter, string.Empty);
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    continue;

                string dim = string.Empty;
                string code = string.Empty;
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Framework Code": code = field.Values[0]; break;
                        case "Dimension No.": dim = field.Values[0]; break;
                        case "Logical Order":
                            ReplExtendedVariantValue it = list.Find(f => f.FrameworkCode == code && f.Dimensions == dim);
                            if (it != null)
                                it.DimensionLogicalOrder = (string.IsNullOrEmpty(field.Values[0]) ? 0 : Convert.ToInt32(field.Values[0]));
                            break;
                    }
                }
            }
            return list;
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(5404, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemUnitOfMeasure(table);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10001414, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemVariantRegistration(table);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(23, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateVendor(table);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(4, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCurrency(table);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(330, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCurrencyExchRate(table);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(5722, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemCategory(table);
        }

        public virtual List<ReplItemLocation> ReplicateItemLocation(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001533, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemLocation(table);
        }

        public virtual List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("WI Price", null, filter, lastKey, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            ReplicateRepository rep = new ReplicateRepository();
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
                                it.QtyPerUnitOfMeasure = (string.IsNullOrEmpty(field.Values[0]) ? 0 : Convert.ToDecimal(field.Values[0]));
                            break;
                    }
                }
            }
            return list;
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication((NAVVersion > new Version("14.2")) ? 10000705 : 5723, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateProductGroups(table);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            ResetReplication(fullReplication, lastKey);
            XMLTableData table = DoReplication(99001470, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            List<ReplStore> list = rep.ReplicateStores(table);

            NAVWebXml xml = new NAVWebXml();
            foreach (ReplStore store in list)
            {
                if (string.IsNullOrWhiteSpace(store.Currency) == false)
                    continue;

                string xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table == null || table.NumberOfValues == 0)
                    break;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                store.Currency = field.Values[0];
            }
            return list;
        }

        public virtual List<ReplStore> ReplicateInvStores(string appId, string appType, string storeId, bool fullReplication, string terminalId)
        {
            string lastKey = string.Empty;
            string maxKey = string.Empty;
            ResetReplication(fullReplication, lastKey);
            XMLTableData table = DoReplication(10012806, storeId, appId, appType, 0, ref lastKey, out int recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            //Get all storeIds linked to this terminal
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

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(204, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateUnitOfMeasure(table);
        }

        public virtual List<ReplCollection> ReplicateCollection(string appId, string appType, string storeId, int batchSize, bool fullReplication, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10001430, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCollection(table);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("WI Discounts", null, filter, lastKey, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            ReplicateRepository rep = new ReplicateRepository();
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
                xmlRequest = xml.GetBatchWebRequestXML("Periodic Discount", null, filter, string.Empty);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                if (table == null || table.NumberOfValues == 0)
                    continue;

                bool hasdata = false;
                decimal amt = 0;
                it = new ReplDiscount();
                extlist.Add(it);
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.":
                            it.OfferNo = field.Values[0];
                            break;
                        case "Type":
                            it.Type = (ReplDiscountType)Convert.ToInt32(field.Values[0]);
                            break;
                        case "Discount Type":
                            it.DiscountValueType = (DiscountValueType)Convert.ToInt32(field.Values[0]);
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
                            it.ValidationPeriodId = Convert.ToInt32(field.Values[0]);
                            break;
                        case "Discount Amount Value":
                            amt = Convert.ToDecimal(field.Values[0]);
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

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("WI Mix & Match Offer", null, filter, lastKey, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            ReplicateRepository rep = new ReplicateRepository();
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
                xmlRequest = xml.GetBatchWebRequestXML("Periodic Discount", null, filter, string.Empty);
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
                                it.Type = (ReplDiscountType)Convert.ToInt32(field.Values[0]);
                            break;
                        case "Discount Type":
                            if (it != null)
                                it.DiscountValueType = (DiscountValueType)Convert.ToInt32(field.Values[0]);
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
                                it.ValidationPeriodId = Convert.ToInt32(field.Values[0]);
                            break;
                        case "Discount Amount Value":
                            if (it != null)
                                amt = Convert.ToDecimal(field.Values[0]);
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

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001481, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDiscountValidations(table);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001462, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStoreTenderType(table);
        }

        public virtual List<ReplValidationSchedule> ReplicateValidationSchedule(string appId, string appType, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            // TODO
            return new List<ReplValidationSchedule>();
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Hierarchy Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            ReplicateRepository rep = new ReplicateRepository();
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
                        case "Start Date": startdate = DateTime.Parse(field.Values[i]); break;
                    }
                }

                xmlRequest = xml.GetGeneralWebRequestXML("Hierarchy", "Hierarchy Code", hircode);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);
                list.AddRange(rep.ReplicateHierarchy(table, startdate));
            }
            return list;
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Hierarchy Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Hierarchy Code"));

            List<ReplHierarchyNode> list = new List<ReplHierarchyNode>();
            ReplicateRepository rep = new ReplicateRepository();

            table = DoReplication(10000921, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);
            List<ReplHierarchyNode> data = rep.ReplicateHierarchyNode(table);

            foreach (string hircode in field.Values)
            {
                list.AddRange(data.FindAll(d => d.HierarchyCode.Equals(hircode)));
            }
            return list;
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Hierarchy Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData hirfield = table.FieldList.Find(f => f.FieldName.Equals("Hierarchy Code"));

            List<ReplHierarchyLeaf> list = new List<ReplHierarchyLeaf>();
            ReplicateRepository rep = new ReplicateRepository();

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

                xmlRequest = xml.GetBatchWebRequestXML("Retail Image Link", null, filter, string.Empty);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table == null || table.NumberOfValues == 0)
                    return null;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Image Id"));
                leaf.ImageId = field.Values[0];
            }
            return list;
        }

        public virtual List<ReplImageLink> ReplEcommImageLinks(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99009064, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateImageLink(table);
        }

        public virtual List<ReplImage> ReplEcommImages(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99009063, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateImage(table);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10000784, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateAttribute(table);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10000786, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateAttributeValue(table);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10000785, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateAttributeOptionValue(table);
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10000971, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDataTranslation(table);
        }

        public virtual List<ReplDataTranslationLangCode> ReplicateEcommDataTranslationLangCode(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(10000972, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDataTranslationLangCode(table);
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(291, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            List<ReplShippingAgent> list = rep.ReplicateShippingAgent(table);

            foreach (ReplShippingAgent sa in list)
            {
               sa.Services = GetShippingAgentService(sa.Id);
            }
            return list;
        }

        public virtual List<ReplCustomer> ReplEcommMember(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99009002, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateMembers(table);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(18, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCustomer(table);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001461, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStaff(table);
        }

        public virtual List<ReplStaffStoreLink> ReplicateStaffStoreLink(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001633, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStaffStoreLink(table);
        }

        public virtual List<ReplTenderType> ReplicateTenderTypes(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001466, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateTenderTypes(table);
        }

        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(325, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateTaxSetup(table);
        }

        public virtual List<ReplStoreTenderTypeCurrency> ReplicateStoreTenderTypeCurrency(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001636, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStoreTenderTypeCurrency(table);
        }

        public virtual List<ReplTerminal> ReplicateTerminals(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99001471, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateTerminals(table);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(9, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCountryCode(table);
        }

        public virtual List<ReplPlu> ReplicatePlu(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            XMLTableData table = DoReplication(99009274, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicatePlu(table);
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string appType, string storeId, bool fullReplication, int batchSize, ref string lastKey, ref int recordsRemaining)
        {
            // fields to get 
            List<string> fields = new List<string>()
            {
                "Item No.", "Variant Code", "Store No.", "Serial No.", "Lot No.", "Net Inventory", "Replication Counter"
            };

            // filter
            List<XMLFieldData> filter = new List<XMLFieldData>();
            filter.Add(new XMLFieldData()
            {
                FieldName = "Store No.",
                Values = new List<string>() { storeId }
            });

            if (fullReplication == false)
            {
                if (string.IsNullOrWhiteSpace(lastKey))
                    lastKey = "0";

                filter.Add(new XMLFieldData()
                {
                    FieldName = "Replication Counter",
                    Values = new List<string>() { ">" + lastKey }
                });
            }

            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetBatchWebRequestXML("Inventory Lookup Table", fields, filter, (fullReplication) ? lastKey : string.Empty, batchSize);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            recordsRemaining = (table.NumberOfValues == batchSize) ? 1 : 0;

            string maxKey = string.Empty;
            ReplicateRepository rep = new ReplicateRepository();
            List<ReplInvStatus> list = rep.ReplicateInvStatus(table, ref lastKey, ref maxKey);

            if (table.NumberOfValues < batchSize || fullReplication == false)
                lastKey = maxKey;

            return list;
        }
    }
}
