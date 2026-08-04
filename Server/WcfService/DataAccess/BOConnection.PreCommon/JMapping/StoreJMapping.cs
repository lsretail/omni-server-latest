using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.ContentMenu;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class StoreJMapping : BaseJMapping
    {
        public StoreJMapping(bool json) 
        {
            IsJson = json;
        }

        public List<Store> GetStores(string ret, int offset, out string resCode, out string resErr)
        {
            List<Store> list = new List<Store>();
            WSODataCollection result = null;

            try
            {
                result = JsonToWSOData(ret, "StoreInfo", out resCode, out resErr);
                if (result == null)
                    return list;

                // get data
                list = LoadStores(result.GetDataSet(99001470));
                if (list == null)
                    return list;

                List<LSKey> storeGroups = LoadFilters(result.GetDataSet(10000782));
                List<LSKey> priceGroups = LoadFilters(result.GetDataSet(99001575));
                List<SalesType> salesType = LoadSalesType(result.GetDataSet(10001439));
                List<HospitalityType> hospType = LoadHospType(result.GetDataSet(10001212));
                List<SourcingLocation> srcLocation = LoadSrcLocation(result.GetDataSet(10016664));
                List<ReplAttribute> attributes = LoadAttributes(result.GetDataSet(10000784));
                List<ReplAttributeValue> attrValue = LoadAttrValues(result.GetDataSet(10000786));
                List<StoreHours> storeHr = LoadHours(result.GetDataSet(99009052));
                List<ImageView> images = LoadImage(result.GetDataSet(99009064));
                List<LSKey> extraHr = LoadExtraHours(result.GetDataSet(99009050));

                foreach (Store store in list)
                {
                    store.PriceGroupCodes = GetFilterString(priceGroups, store.Id);
                    store.StoreGroupCodes = GetFilterString(storeGroups, store.Id);
                    store.SourcingLocations = srcLocation.FindAll(x => x.StoreNo == store.Id);
                    store.HospTypes = hospType.FindAll(x => x.StoreNo == store.Id);
                    store.Images = images.FindAll(x => x.Location == store.Id);

                    string[] stypes = store.SalesTypeCodes.Split('|');
                    foreach (string code in stypes)
                    {
                        SalesType st = salesType.Find(x => x.Code == code);
                        if (st != null)
                            store.HospSalesTypes.Add(st);
                    }

                    store.StoreHours = storeHr.FindAll(x => x.StoreId == store.Id);
                    foreach (LSKey key in extraHr)
                    {
                        if (key.Key != store.Id)
                            continue;

                        store.StoreHours.AddRange(storeHr.FindAll(x => x.StoreId == key.Description));
                    }

                    foreach (StoreHours sh in store.StoreHours)
                    {
                        if (sh.DayOfWeek == 7)
                            sh.DayOfWeek = 0; //NAV starts with Sunday as 1 but .Net Sunday=0

                        if (string.IsNullOrEmpty(sh.Description))
                            sh.Description = sh.NameOfDay;

                        sh.OpenFrom = ConvertTo.SafeDateTime(sh.OpenFrom.AddHours(offset));
                        sh.OpenTo = ConvertTo.SafeDateTime(sh.OpenTo.AddHours(offset));
                        sh.StartDate = ConvertTo.SafeDateTime(sh.StartDate.AddHours(offset));
                        sh.EndDate = ConvertTo.SafeDateTime(sh.EndDate.AddHours(offset));
                    }

                    List<ReplAttributeValue> aValue = attrValue.FindAll(x => x.LinkField1 == store.Id);
                    foreach (ReplAttributeValue v in aValue)
                    {
                        ReplAttribute at = attributes.Find(x => x.Code == v.Code);
                        store.Attributes.Add(new RetailAttribute()
                        {
                            Code = v.Code,
                            Value = v.Value,
                            NumericValue = v.NumbericValue,
                            LinkType = (AttributeLinkType)v.LinkType,
                            LinkField1 = v.LinkField1,
                            LinkField2 = v.LinkField2,
                            LinkField3 = v.LinkField3,
                            Sequence = v.Sequence,
                            DefaultValue = at.DefaultValue,
                            Description = at.Description,
                            ValueType = (AttributeValueType)at.ValueType
                        });
                    }
                }

                List<WSODataFlowField> flowfields = LoadFlowFields(result.GetDataSet(99001649));
            }
            finally
            {
                result?.Dispose();
                result = null;
            }
            return list;
        }

        public List<ReturnPolicy> GetReturnPolicy(string ret, out string resCode, out string resErr)
        {
            List<ReturnPolicy> list = new List<ReturnPolicy>();
            string result = GetJsonPageValue(ret, out resCode, out resErr);
            if (result == null)
                return list;

            JArray items = JArray.Parse(result);
            foreach (JObject item in items)
            {
                list.Add(new ReturnPolicy()
                {
                    StoreId = item.Value<string>("storeNo"),
                    StoreGroup = item.Value<string>("storeGroupCode"),
                    ItemCategory = item.Value<string>("itemCategoryCode"),
                    ProductGroup = item.Value<string>("retailProductCode"),
                    ItemId = item.Value<string>("itemNo"),
                    VariantCode = item.Value<string>("variantCode"),
                    VariantDimension1 = item.Value<string>("variantDimension1Code"),
                    RefundNotAllowed = item.Value<bool?>("refundNotAllowed") ?? false,
                    ManagerPrivileges = item.Value<bool?>("managerPrivileges") ?? false,
                    RefundPeriodLength = item.Value<string>("refundPeriodLength"),
                    Message1 = item.Value<string>("message1"),
                    Message2 = item.Value<string>("message2"),
                    ReturnPolicyHTML = item.Value<string>("returnPolicyHTML")
                });
            }
            return list;
        }

        public List<Hierarchy> GetContentMenu(string ret)
        {
            ContentMenuHierarchy result = JsonConvert.DeserializeObject<ContentMenuHierarchy>(ret);
            if (result == null || result.DynamicContentMenu == null)
                return new List<Hierarchy>();

            List<Hierarchy> list = new List<Hierarchy>();
            Hierarchy hierarchy = new Hierarchy();

            foreach (ContentMenu menu in result.DynamicContentMenu)
            {
                if (menu.Level == 0)
                {
                    hierarchy.Id = menu.NodeID;
                    hierarchy.Description = menu.Description;
                    continue;
                }

                if (menu.Level == 1)
                {
                    if (menu.NodeType == "Node")
                    {
                        HierarchyNode node = new HierarchyNode()
                        {
                            Id = menu.NodeID,
                            Description = menu.Description,
                            HierarchyCode = hierarchy.Id,
                            ImageId = menu.Image,
                            Type = HierarchyDealType.Item
                        };
                        hierarchy.Nodes.Add(node);

                        foreach (ContentMenuItem item in menu.MenuItems)
                        {
                            if (item.NodeType == "Offer")
                            {
                                HierarchyNode dealNode = new HierarchyNode()
                                {
                                    Id = item.DealNo,
                                    Description = item.Description,
                                    HierarchyCode = hierarchy.Id,
                                    ImageId = item.Image,
                                    ParentNode = node.Id,
                                    Type = HierarchyDealType.Deal,
                                    AddedAmount = item.Price,
                                    Price = item.Price
                                };
                                node.Nodes.Add(dealNode);

                                foreach (ContentMenuOfferLine oline in item.OfferLines)
                                {
                                    if (oline.OfferLineType == "Item")
                                    {
                                        HierarchyNode offNode = new HierarchyNode()
                                        {
                                            Id = oline.ItemNo,
                                            Description = oline.Description,
                                            UnitOfMeasure = oline.ItemUOM,
                                            MaxSelection = (int)oline.MaxQuantity,
                                            MinSelection = (int)oline.MinQuantity,
                                            ImageId = oline.Image,
                                            LineNo = oline.OfferLine,
                                            AddedAmount = oline.Price,
                                            Price = oline.Price
                                        };
                                        dealNode.Nodes.Add(offNode);
                                        FillModifierRecipes(offNode, oline.ModifierGroups, oline.Ingredients);
                                        continue;
                                    }

                                    HierarchyNode offfNode = new HierarchyNode()
                                    {
                                        Id = oline.DealModGroupId,
                                        Description = oline.DealModGroupDescription,
                                        MaxSelection = (int)oline.DealModGroupMaxQuantity,
                                        MinSelection = (int)oline.DealModGroupMinQuantity,
                                        DisplayPrompt = oline.DealModDisplayPrompt,
                                        ImageId = oline.Image,
                                        LineNo = oline.OfferLine,
                                        AddedAmount = oline.Price,
                                        Price = oline.Price,
                                        Type = HierarchyDealType.Modifier
                                    };
                                    dealNode.Nodes.Add(offfNode);

                                    foreach (ContentMenuDealModGroupItem modItem in oline.DealModGroupItems)
                                    {
                                        HierarchyLeaf ooLeaf = new HierarchyLeaf()
                                        {
                                            Id = modItem.ItemNo,
                                            Description = modItem.Description,
                                            ImageId = modItem.Image,
                                            ParentNode = dealNode.Id,
                                            ItemUOMs = GetItemUOM(modItem)
                                        };
                                        offfNode.Leafs.Add(ooLeaf);
                                    }
                                }
                                continue;
                            }

                            HierarchyNode subNode = new HierarchyNode()
                            {
                                Id = item.ItemNo,
                                Description = item.Description,
                                HierarchyCode = hierarchy.Id,
                                ImageId = item.Image,
                                ParentNode = node.Id,
                                Type = HierarchyDealType.Item,
                                AddedAmount = item.Price,
                                Price = item.Price
                            };
                            if (item.NodeType == "Recipe")
                                subNode.Type = HierarchyDealType.Recipe;
                            node.Nodes.Add(subNode);
                            FillModifierRecipes(subNode, item.ModifierGroups, item.Ingredients);
                        }
                    }
                }
            }
            list.Add(hierarchy);
            return list;
        }

        private void FillModifierRecipes(HierarchyNode node, List<ContentMenuModifierGroup> modGroups, List<ContentMenuIngredient> ingrList)
        {
            foreach (ContentMenuModifierGroup mgroup in modGroups)
            {
                HierarchyLeaf dealLeaf = new HierarchyLeaf()
                {
                    Id = mgroup.ModGroupId,
                    Description = mgroup.ModGroupDescription,
                    DisplayPrompt = mgroup.ModGroupDisplayPrompt,
                    MaxSelection = (int)mgroup.ModGroupMaxQuantity,
                    MinSelection = (int)mgroup.ModGroupMinQuantity,
                    ImageId = mgroup.Image
                };
                node.Leafs.Add(dealLeaf);

                foreach (ContentMenuModifierItem mitem in mgroup.ModifierItems)
                {
                    ItemModifier modLeaf = new ItemModifier()
                    {
                        Id = mitem.ItemId,
                        Description = mitem.Description,
                        ExplanatoryHeaderText = mgroup.ModGroupDisplayPrompt,
                        Selection = mitem.SelQuantity,
                        MaxSelection = (int)mitem.MaxQuantity,
                        MinSelection = (int)mitem.MinQuantity,
                        GroupMinSelection = (int)mgroup.ModGroupMinQuantity,
                        GroupMaxSelection = (int)mgroup.ModGroupMaxQuantity,
                        UnitOfMeasure = mitem.ItemModUom,
                        PriceType = ItemModifierPriceType.Amount,
                        AmountPercent = mitem.ItemModPriceAmount
                    };
                    if (mitem.ItemModPriceType == "Percentage")
                        modLeaf.PriceType = ItemModifierPriceType.Percent;

                    dealLeaf.Modifiers.Add(modLeaf);
                }
            }
            foreach (ContentMenuIngredient ingr in ingrList)
            {
                ItemRecipe recLeaf = new ItemRecipe()
                {
                    Id = ingr.ItemNo,
                    Description = ingr.Description,
                    ExclusionPrice = ingr.PriceReduction,
                    ImageId = ingr.Image,
                    QuantityPer = ingr.SelQuantity,
                    UnitOfMeasure = ingr.ItemUOM
                };
                node.Recipies.Add(recLeaf);
            }
        }

        private List<UnitOfMeasure> GetItemUOM(ContentMenuDealModGroupItem mitem)
        {
            List<UnitOfMeasure> list = new List<UnitOfMeasure>();
            foreach (ContentMenuItemUom uom in mitem.ItemUom)
            {
                UnitOfMeasure itemUOM = new UnitOfMeasure()
                {
                    Id = uom.ItemUOM,
                    Description = uom.UoMDescription,
                    ItemId = mitem.ItemNo,
                    QtyPerUom = uom.UoMFactor,
                    Price = uom.Price,
                    DealLine = uom.DealLine,
                    DealModLine = uom.DealModLine
                };
                list.Add(itemUOM);
            }
            return list;
        }

        private List<Store> LoadStores(ReplODataSetRecRef dynDataSet)
        {
            List<Store> list = new List<Store>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Store rec = new Store();
                rec.Address.Type = AddressType.Store;
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "No.": rec.Id = col.FieldValue; break;
                        case "Name": rec.Description = col.FieldValue; break;
                        case "Address": rec.Address.Address1 = col.FieldValue; break;
                        case "Address 2": rec.Address.Address2 = col.FieldValue; break;
                        case "Post Code": rec.Address.PostCode = col.FieldValue; break;
                        case "City": rec.Address.City = col.FieldValue; break;
                        case "County": rec.Address.Country = col.FieldValue; break;
                        case "Country Code": rec.Address.Country = col.FieldValue; break;
                        case "Phone No.": rec.Phone = col.FieldValue; break;
                        case "Latitude": rec.Latitude = ConvertTo.SafeDouble(col.FieldValue); break;
                        case "Longitude": rec.Longitude = ConvertTo.SafeDouble(col.FieldValue); break;
                        case "Currency Code": rec.Currency = new Currency(col.FieldValue); break;
                        case "Functionality Profile": rec.FunctionalityProfileId = col.FieldValue; break;
                        case "Store VAT Bus. Post. Gr.": rec.TaxGroupId = col.FieldValue; break;
                        case "Click and Collect": rec.IsClickAndCollect = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Loyalty": rec.IsLoyalty = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Web Store": rec.IsWebStore = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Web Store POS Terminal": rec.WebOmniTerminal = col.FieldValue; break;
                        case "Web Store Staff ID": rec.WebOmniStaff = col.FieldValue; break;
                        case "Calc Inv for Sourcing Location": rec.UseSourcingLocation = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Store Sales Type Filter": rec.SalesTypeCodes = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<SourcingLocation> LoadSrcLocation(ReplODataSetRecRef dynDataSet)
        {
            List<SourcingLocation> list = new List<SourcingLocation>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SourcingLocation rec = new SourcingLocation();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Delivery Store": rec.StoreNo = col.FieldValue; break;
                        case "Sourcing Location": rec.Id = col.FieldValue; break;
                        case "Priority": rec.Priority = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Will Ship Orders": rec.CanShip = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Orders can be Collected": rec.CanCollect = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Description": rec.Description = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<ReplAttribute> LoadAttributes(ReplODataSetRecRef dynDataSet)
        {
            List<ReplAttribute> list = new List<ReplAttribute>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                ReplAttribute rec = new ReplAttribute();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rec.Code = col.FieldValue; break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Value Type": rec.ValueType = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Default Value": rec.DefaultValue = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<ReplAttributeValue> LoadAttrValues(ReplODataSetRecRef dynDataSet)
        {
            List<ReplAttributeValue> list = new List<ReplAttributeValue>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                ReplAttributeValue rec = new ReplAttributeValue();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Link Type": rec.LinkType = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Link Field 1": rec.LinkField1 = col.FieldValue; break;
                        case "Link Field 2": rec.LinkField2 = col.FieldValue; break;
                        case "Link Field 3": rec.LinkField3 = col.FieldValue; break;
                        case "Attribute Code": rec.Code = col.FieldValue; break;
                        case "Attribute Value": rec.Value = col.FieldValue; break;
                        case "Sequence": rec.Sequence = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Numeric Value": rec.NumbericValue = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<StoreHours> LoadHours(ReplODataSetRecRef dynDataSet)
        {
            List<StoreHours> list = new List<StoreHours>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                StoreHours rec = new StoreHours();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Calendar Type": rec.CalendarType = (StoreHourCalendarType)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Calendar ID": rec.StoreId = col.FieldValue; break;
                        case "Time From": rec.OpenFrom = ConvertTo.SafeTime(col.FieldValue); break;
                        case "Time To": rec.OpenTo = ConvertTo.SafeTime(col.FieldValue); break;
                        case "Starting Date": rec.StartDate = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Ending Date": rec.EndDate = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Day No.": rec.DayOfWeek = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Day - Name": rec.NameOfDay = col.FieldValue; break;
                        case "Reason Closed": rec.Description = col.FieldValue; break;
                        case "Line Type": rec.Type = (StoreHourOpeningType)ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<LSKey> LoadExtraHours(ReplODataSetRecRef dynDataSet)
        {
            List<LSKey> list = new List<LSKey>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                LSKey rec = new LSKey();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Store No.": rec.Key = col.FieldValue; break;
                        case "Calendar ID": rec.Description = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<SalesType> LoadSalesType(ReplODataSetRecRef dynDataSet)
        {
            List<SalesType> list = new List<SalesType>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesType rec = new SalesType();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rec.Code = col.FieldValue; break;
                        case "Description": rec.Description = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<HospitalityType> LoadHospType(ReplODataSetRecRef dynDataSet)
        {
            List<HospitalityType> list = new List<HospitalityType>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                HospitalityType rec = new HospitalityType();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Restaurant No.": rec.StoreNo = col.FieldValue; break;
                        case "Sequence": rec.Sequence = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Sales Type": rec.SalesType = col.FieldValue; break;
                        case "Tips Calculation": rec.TipInclTax = (ConvertTo.SafeInt(col.FieldValue) == 0); break;
                        case "Tips Income Acc. 1": rec.IncomeAccount1 = col.FieldValue; break;
                        case "Tips Income Acc. 2": rec.IncomeAccount2 = col.FieldValue; break;
                        case "Suggested Tips 1 Percentage": rec.Tip1Percentage = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Suggested Tips 2 Percentage": rec.Tip2Percentage = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case "Suggested Tips 3 Percentage": rec.Tip3Percentage = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<ImageView> LoadImage(ReplODataSetRecRef dynDataSet)
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
            return list;
        }

        private List<LSKey> LoadFilters(ReplODataSetRecRef dynDataSet)
        {
            List<LSKey> list = new List<LSKey>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                LSKey rec = new LSKey();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Store Code": rec.Key = col.FieldValue; break;
                        case "Store Group": rec.Description = col.FieldValue; break;
                        case "Store": rec.Key = col.FieldValue; break;
                        case "Price Group Code": rec.Description = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private string GetFilterString(List<LSKey> list, string storeNo)
        {
            List<LSKey> items = list.FindAll(k => k.Key == storeNo);
            string data = string.Empty;
            foreach (LSKey rec in items)
                data += rec.Description + ";";
            return data;
        }
    }
}
