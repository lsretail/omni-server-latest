using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication
{
    public class ReplicateRepository : BaseRepository
    {
        public List<ReplItem> ReplicateItems(XMLTableData table)
        {
            List<ReplItem> list = new List<ReplItem>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplItem rec = new ReplItem();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Base Unit of Measure": rec.BaseUnitOfMeasure = field.Values[i]; break;

                        case "Scale Item": rec.ScaleItem = GetWebBoolInt(field.Values[i]); break;
                        case "VAT Prod. Posting Group": rec.TaxItemGroupId = field.Values[i]; break;
                        case "Zero Price Valid": rec.ZeroPriceValId = GetWebBoolInt(field.Values[i]); break;

                        case "Unit Price": rec.UnitPrice = GetWebDecimal(field.Values[i]); break;
                        case "Purch. Unit of Measure": rec.PurchUnitOfMeasure = field.Values[i]; break;
                        case "Sales Unit of Measure": rec.SalseUnitOfMeasure = field.Values[i]; break;
                        case "Vendor No.": rec.VendorId = field.Values[i]; break;
                        case "Vendor Item No.": rec.VendorItemId = field.Values[i]; break;

                        case "Season Code": rec.SeasonCode = field.Values[i]; break;
                        case "Item Category Code": rec.ItemCategoryCode = field.Values[i]; break;
                        case "Item Family Code": rec.ItemFamilyCode = field.Values[i]; break;

                        case "Gross Weight": rec.GrossWeight = GetWebDecimal(field.Values[i]); break;
                        case "Units per Parcel": rec.UnitsPerParcel = GetWebDecimal(field.Values[i]); break;
                        case "Unit Volume": rec.UnitVolume = GetWebDecimal(field.Values[i]); break;
                    }
                }

                // TODO: Get HTML Description

                list.Add(rec);
            }
            return list;
        }

        public List<ReplBarcode> ReplicateBarcodes(XMLTableData table)
        {
            List<ReplBarcode> list = new List<ReplBarcode>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplBarcode rec = new ReplBarcode();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Barcode No.": rec.Id = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Variant Code": rec.VariantId = field.Values[i]; break;
                        case "Unit of Measure Code": rec.UnitOfMeasure = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(XMLTableData table)
        {
            List<ReplBarcodeMaskSegment> list = new List<ReplBarcodeMaskSegment>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplBarcodeMaskSegment rec = new ReplBarcodeMaskSegment();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Mask Entry No.": rec.MaskId = GetWebInt(field.Values[i]); break;
                        case "Segment No": rec.Number = GetWebInt(field.Values[i]); break;
                        case "Type": rec.SegmentType = GetWebInt(field.Values[i]); break;
                        case "Length": rec.Length = GetWebInt(field.Values[i]); break;
                        case "Decimals": rec.Decimals = GetWebInt(field.Values[i]); break;
                        case "Char": rec.Char = field.Values[i]; break;
                    }
                }
                rec.Id = Convert.ToInt32(string.Format("{0}{1}", rec.MaskId, rec.Number));
                list.Add(rec);
            }
            return list;
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(XMLTableData table)
        {
            List<ReplBarcodeMask> list = new List<ReplBarcodeMask>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplBarcodeMask rec = new ReplBarcodeMask();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Entry No.": rec.Id = GetWebInt(field.Values[i]); break;
                        case "Mask": rec.Mask = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Type": rec.MaskType = GetWebInt(field.Values[i]); break;
                        case "Prefix": rec.Prefix = field.Values[i]; break;
                        case "Symbology": rec.Symbology = GetWebInt(field.Values[i]); break;
                        case "Number Series": rec.NumberSeries = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(XMLTableData table)
        {
            List<ReplExtendedVariantValue> list = new List<ReplExtendedVariantValue>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplExtendedVariantValue rec = new ReplExtendedVariantValue();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Dimension": rec.Dimensions = field.Values[i]; break;
                        case "Code": rec.Code = field.Values[i]; break;
                        case "Value": rec.Value = field.Values[i]; break;
                        case "Framework Code": rec.FrameworkCode = field.Values[i]; break;
                        case "Logical Order": rec.LogicalOrder = GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplItemCategory> ReplicateItemCategory(XMLTableData table)
        {
            List<ReplItemCategory> list = new List<ReplItemCategory>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplItemCategory rec = new ReplItemCategory();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplProductGroup> ReplicateProductGroups(XMLTableData table)
        {
            List<ReplProductGroup> list = new List<ReplProductGroup>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplProductGroup rec = new ReplProductGroup();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Item Category Code": rec.ItemCategoryID = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(XMLTableData table)
        {
            List<ReplItemVariantRegistration> list = new List<ReplItemVariantRegistration>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplItemVariantRegistration rec = new ReplItemVariantRegistration();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Variant": rec.VariantId = field.Values[i]; break;
                        case "Framework Code": rec.FrameworkCode = field.Values[i]; break;
                        case "Variant Dimension 1": rec.VariantDimension1 = field.Values[i]; break;
                        case "Variant Dimension 2": rec.VariantDimension2 = field.Values[i]; break;
                        case "Variant Dimension 3": rec.VariantDimension3 = field.Values[i]; break;
                        case "Variant Dimension 4": rec.VariantDimension4 = field.Values[i]; break;
                        case "Variant Dimension 5": rec.VariantDimension5 = field.Values[i]; break;
                        case "Variant Dimension 6": rec.VariantDimension6 = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUnitOfMeasure(XMLTableData table)
        {
            List<ReplItemUnitOfMeasure> list = new List<ReplItemUnitOfMeasure>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplItemUnitOfMeasure rec = new ReplItemUnitOfMeasure();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Code = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Qty. per Unit of Measure": rec.QtyPrUOM = GetWebDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplPrice> ReplicatePrice(XMLTableData table)
        {
            List<ReplPrice> list = new List<ReplPrice>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplPrice rec = new ReplPrice();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Unit of Measure Code": rec.UnitOfMeasure = field.Values[i]; break;
                        case "Variant Code": rec.VariantId = field.Values[i]; break;
                        case "Customer Disc. Group": rec.CustomerDiscountGroup = field.Values[i]; break;
                        case "Loyalty Scheme Code": rec.LoyaltySchemeCode = field.Values[i]; break;
                        case "Currency Code": rec.CurrencyCode = field.Values[i]; break;
                        case "Unit Price": rec.UnitPrice = GetWebDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplDiscount> ReplicateDiscounts(XMLTableData table)
        {
            List<ReplDiscount> list = new List<ReplDiscount>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplDiscount rec = new ReplDiscount();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Unit of Measure Code": rec.UnitOfMeasureId = field.Values[i]; break;
                        case "Variant Code": rec.VariantId = field.Values[i]; break;
                        case "Customer Disc. Group": rec.CustomerDiscountGroup = field.Values[i]; break;
                        case "Loyalty Scheme Code": rec.LoyaltySchemeCode = field.Values[i]; break;
                        case "Currency Code": rec.CurrencyCode = field.Values[i]; break;
                        case "Priority No.": rec.PriorityNo = GetWebInt(field.Values[i]); break;
                        case "Minimum Quantity": rec.MinimumQuantity = GetWebDecimal(field.Values[i]); break;
                        case "Discount %": rec.DiscountValue = GetWebDecimal(field.Values[i]); break;
                        case "From Date": rec.FromDate = GetWebDateTime(field.Values[i]); break;
                        case "To Date": rec.ToDate = GetWebDateTime(field.Values[i]); break;
                        case "Offer No.": rec.OfferNo = field.Values[i]; break;
                    }
                }

                // TODO: get detailed info for discount - update NAV CU?

                list.Add(rec);
            }
            return list;
        }

        public List<ReplHierarchy> ReplicateHierarchy(XMLTableData table)
        {
            List<ReplHierarchy> list = new List<ReplHierarchy>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplHierarchy rec = new ReplHierarchy();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Hierarchy Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Type": rec.Type = (HierarchyType)GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplHierarchyNode> ReplicateHierarchyNode(XMLTableData table)
        {
            List<ReplHierarchyNode> list = new List<ReplHierarchyNode>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplHierarchyNode rec = new ReplHierarchyNode();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Hierarchy Code": rec.HierarchyCode = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Node ID": rec.Id = field.Values[i]; break;
                        case "Parent Node ID": rec.ParentNode = field.Values[i]; break;
                        case "Children Order": rec.ChildrenOrder = GetWebInt(field.Values[i]); break;
                        case "Indentation": rec.Indentation = GetWebInt(field.Values[i]); break;
                        case "Presentation Order": rec.PresentationOrder = GetWebInt(field.Values[i]); break;
                        case "Retail Image Code": rec.ImageId = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(XMLTableData table)
        {
            List<ReplHierarchyLeaf> list = new List<ReplHierarchyLeaf>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplHierarchyLeaf rec = new ReplHierarchyLeaf();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Hierarchy Code": rec.HierarchyCode = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Node ID": rec.NodeId = field.Values[i]; break;
                        case "Type": rec.Type = (HierarchyLeafType)GetWebInt(field.Values[i]); break;
                        case "No.": rec.Id = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplAttribute> ReplicateAttribute(XMLTableData table)
        {
            List<ReplAttribute> list = new List<ReplAttribute>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplAttribute rec = new ReplAttribute();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Code = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Default Value": rec.DefaultValue = field.Values[i]; break;
                        case "Value Type": rec.ValueType = GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplAttributeValue> ReplicateAttributeValue(XMLTableData table)
        {
            List<ReplAttributeValue> list = new List<ReplAttributeValue>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplAttributeValue rec = new ReplAttributeValue();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Attribute Code": rec.Code = field.Values[i]; break;
                        case "Attribute Value": rec.Value = field.Values[i]; break;
                        case "Numeric Value": rec.NumbericValue = GetWebInt(field.Values[i]); break;
                        case "Link Type": rec.LinkType = GetWebInt(field.Values[i]); break;
                        case "Link Field 1": rec.LinkField1 = field.Values[i]; break;
                        case "Link Field 2": rec.LinkField2 = field.Values[i]; break;
                        case "Link Field 3": rec.LinkField3 = field.Values[i]; break;
                        case "Sequence": rec.Sequence = GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplAttributeOptionValue> ReplicateAttributeOptionValue(XMLTableData table)
        {
            List<ReplAttributeOptionValue> list = new List<ReplAttributeOptionValue>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplAttributeOptionValue rec = new ReplAttributeOptionValue();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Attribute Code": rec.Code = field.Values[i]; break;
                        case "Option Value": rec.Value = field.Values[i]; break;
                        case "Sequence": rec.Sequence = GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplCustomer> ReplicateMembers(XMLTableData table)
        {
            List<ReplCustomer> list = new List<ReplCustomer>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplCustomer rec = new ReplCustomer();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Contact No.": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                        case "Address": rec.Street = field.Values[i]; break;
                        case "City": rec.City = field.Values[i]; break;
                        case "Country": rec.Country = field.Values[i]; break;
                        case "County": rec.County = field.Values[i]; break;
                        case "Home Page": rec.URL = field.Values[i]; break;
                        case "E-Mail": rec.Email = field.Values[i]; break;
                        case "Blocked": rec.Blocked = GetWebBoolInt(field.Values[i]); break;
                        case "Account No.": rec.AccountNumber = field.Values[i]; break;
                        case "Post Code": rec.ZipCode = field.Values[i]; break;
                        case "Mobile Phone No.": rec.CellularPhone = field.Values[i]; break;
                        case "Phone No.": rec.PhoneLocal = field.Values[i]; break;
                        case "Club Code": rec.ClubCode = field.Values[i]; break;
                        case "Scheme Code": rec.SchemeCode = field.Values[i]; break;
                        case "Login ID": rec.UserName = field.Values[i]; break;
                        case "Card No.": rec.Cards.Add(new Card(field.Values[i])); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplCustomer> ReplicateCustomer(XMLTableData table)
        {
            List<ReplCustomer> list = new List<ReplCustomer>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplCustomer rec = new ReplCustomer();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Name": rec.Name = field.Values[i]; break;
                        case "Address": rec.Street = field.Values[i]; break;
                        case "City": rec.City = field.Values[i]; break;
                        case "Country/Region Code": rec.Country = field.Values[i]; break;
                        case "County": rec.County = field.Values[i]; break;
                        case "Home Page": rec.URL = field.Values[i]; break;
                        case "E-Mail": rec.Email = field.Values[i]; break;
                        case "Blocked": rec.Blocked = GetWebBoolInt(field.Values[i]); break;
                        case "No.": rec.AccountNumber = field.Values[i]; break;
                        case "Post Code": rec.ZipCode = field.Values[i]; break;
                        case "Mobile Phone No.": rec.CellularPhone = field.Values[i]; break;
                        case "Phone No.": rec.PhoneLocal = field.Values[i]; break;
                        case "Currency Code": rec.Currency = field.Values[i]; break;
                        case "VAT Bus. Posting Group": rec.TaxGroup = field.Values[i]; break;
                        case "IncludeTax": rec.IncludeTax = GetWebBoolInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(XMLTableData table)
        {
            List<ReplUnitOfMeasure> list = new List<ReplUnitOfMeasure>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplUnitOfMeasure rec = new ReplUnitOfMeasure();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                    }
                }
                rec.ShortDescription = rec.Description;
                list.Add(rec);
            }
            return list;
        }

        public List<ReplCurrency> ReplicateCurrency(XMLTableData table)
        {
            List<ReplCurrency> list = new List<ReplCurrency>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplCurrency rec = new ReplCurrency();
                int curplace = 0;
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.CurrencyCode = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Amount Rounding Precision": rec.RoundOfSales = GetWebDecimal(field.Values[i]); break;
                        case "Invoice Rounding Type": rec.RoundOfTypeAmount = GetWebInt(field.Values[i]); break;
                        case "Invoice Rounding Precision": rec.RoundOfAmount = GetWebDecimal(field.Values[i]); break;
                        case "POS Currency Symbol": rec.Symbol = field.Values[i]; break;
                        case "Placement Of Currency Symbol": curplace = GetWebInt(field.Values[i]); break;
                    }
                }

                if (curplace == 0)
                {
                    rec.CurrencyPrefix = string.Empty;
                    rec.CurrencySuffix = rec.Symbol;
                }
                else
                {
                    rec.CurrencyPrefix = rec.Symbol;
                    rec.CurrencySuffix = string.Empty;
                }

                list.Add(rec);
            }
            return list;
        }

        public List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(XMLTableData table)
        {
            List<ReplCurrencyExchRate> list = new List<ReplCurrencyExchRate>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplCurrencyExchRate rec = new ReplCurrencyExchRate();
                decimal posextamt = 0;
                decimal posrelamt = 0;
                decimal extamt = 0;
                decimal relamt = 0;
                string code = string.Empty;

                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Currency Code": rec.CurrencyCode = field.Values[i]; break;
                        case "Starting Date": rec.StartingDate = GetWebDateTime(field.Values[i]); break;
                        case "POS Exchange Rate Amount": posextamt = GetWebDecimal(field.Values[i]); break;
                        case "POS Rel. Exch. Rate Amount": posrelamt = GetWebDecimal(field.Values[i]); break;
                        case "Relational Currency Code": code = field.Values[i]; break;
                        case "Exchange Rate Amount": extamt = GetWebDecimal(field.Values[i]); break;
                        case "Relational Exch. Rate Amount": relamt = GetWebDecimal(field.Values[i]); break;
                    }
                }

                if (posextamt != 0)
                {
                    rec.CurrencyFactor = ((1 / posextamt) * posrelamt);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(code) && extamt != 0)
                    {
                        rec.CurrencyFactor = ((1 / extamt) * relamt);
                    }
                    else
                    {
                        rec.RelationalCurrencyCode = code;
                    }
                }

                list.Add(rec);
            }

            foreach (ReplCurrencyExchRate rate in list)
            {
                if (string.IsNullOrEmpty(rate.RelationalCurrencyCode) == false)
                {
                    ReplCurrencyExchRate ex = list.Find(r => r.CurrencyCode.Equals(rate.RelationalCurrencyCode));
                    rate.CurrencyFactor = ex.CurrencyFactor;
                }
            }
            return list;
        }

        public List<ReplStore> ReplicateStores(XMLTableData table)
        {
            List<ReplStore> list = new List<ReplStore>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplStore rec = new ReplStore();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                        case "Address": rec.Street = field.Values[i]; break;
                        case "Post Code": rec.ZipCode = field.Values[i]; break;
                        case "City": rec.City = field.Values[i]; break;
                        case "County": rec.County = field.Values[i]; break;
                        case "Country Code": rec.Country = field.Values[i]; break;
                        case "Latitude": rec.Latitute = GetWebDecimal(field.Values[i]); break;
                        case "Longitude": rec.Longitude = GetWebDecimal(field.Values[i]); break;
                        case "Phone No.": rec.Phone = field.Values[i]; break;
                        case "Currency Code": rec.Currency = field.Values[i]; break;
                        case "Functionality Profile": rec.FunctionalityProfile = field.Values[i]; break;
                        case "Store VAT Bus. Post. Gr.": rec.TaxGroup = field.Values[i]; break;
                        case "Click and Collect": rec.ClickAndCollect = GetWebBool(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplStore> ReplicateInvStores(XMLTableData table, string terminalId)
        {
            List<ReplStore> list = new List<ReplStore>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplStore rec = new ReplStore();
                string terminal = string.Empty;
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Terminal No.": terminal = field.Values[i]; break;
                        case "Store": rec.Id = field.Values[i]; break;
                        case "Store Name": rec.Name = field.Values[i]; break;
                    }
                }
                if (terminal == terminalId)
                    list.Add(rec);
            }
            return list;
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(XMLTableData table)
        {
            List<ReplStoreTenderType> list = new List<ReplStoreTenderType>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplStoreTenderType rec = new ReplStoreTenderType();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Store No.": rec.StoreID = field.Values[i]; break;
                        case "Code": rec.TenderTypeId = field.Values[i]; break;
                        case "Description": rec.Name = field.Values[i]; break;
                        case "Function": rec.TenderFunction = GetWebInt(field.Values[i]); break;
                        case "Valid on Mobile POS": rec.ValidOnMobilePOS = GetWebBoolInt(field.Values[i]); break;
                        case "Change Tend. Code": rec.ChangeTenderId = field.Values[i]; break;
                        case "Above Min. Change Tender Type": rec.AboveMinimumTenderId = field.Values[i]; break;
                        case "Min. Change": rec.MinimumChangeAmount = GetWebDecimal(field.Values[i]); break;
                        case "Rounding": rec.RoundingMethode = GetWebInt(field.Values[i]); break;
                        case "Rounding To": rec.Rounding = GetWebDecimal(field.Values[i]); break;
                        case "Overtender Allowed": rec.AllowOverTender = GetWebBoolInt(field.Values[i]); break;
                        case "Overtender Max. Amt.": rec.MaximumOverTenderAmount = GetWebDecimal(field.Values[i]); break;
                        case "Undertender Allowed": rec.AllowUnderTender = GetWebBoolInt(field.Values[i]); break;
                        case "Return/Minus Allowed": rec.ReturnAllowed = GetWebBoolInt(field.Values[i]); break;
                        case "Drawer Opens": rec.OpenDrawer = GetWebBoolInt(field.Values[i]); break;
                        case "Counting Required": rec.CountingRequired = GetWebBoolInt(field.Values[i]); break;
                        case "Foreign Currency": rec.ForeignCurrency = GetWebBoolInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplStaff> ReplicateStaff(XMLTableData table)
        {
            List<ReplStaff> list = new List<ReplStaff>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplStaff rec = new ReplStaff();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "ID": rec.Id = field.Values[i]; break;
                        case "Password": rec.Password = field.Values[i]; break;
                        case "Change Password": rec.ChangePassword = GetWebBoolInt(field.Values[i]); break;
                        case "Store No.": rec.StoreID = field.Values[i]; break;
                        case "First Name": rec.FirstName = field.Values[i]; break;
                        case "Last Name": rec.LastName = field.Values[i]; break;
                        case "Name on Receipt": rec.NameOnReceipt = field.Values[i]; break;
                        case "Blocked": rec.Blocked = GetWebBoolInt(field.Values[i]); break;
                        case "Date to Be Blocked": rec.BlockingDate = GetWebDateTime(field.Values[i]); break;
                        case "Inventory Active": rec.InventoryActive = GetWebBool(field.Values[i]); break;
                        case "Inventory Main Menu": rec.InventoryMainMenu = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplStaffStoreLink> ReplicateStaffStoreLink(XMLTableData table)
        {
            List<ReplStaffStoreLink> list = new List<ReplStaffStoreLink>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplStaffStoreLink rec = new ReplStaffStoreLink();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Staff ID": rec.StaffId = field.Values[i]; break;
                        case "Default Sales Type": rec.DefaultHospType = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplStoreTenderTypeCurrency> ReplicateStoreTenderTypeCurrency(XMLTableData table)
        {
            List<ReplStoreTenderTypeCurrency> list = new List<ReplStoreTenderTypeCurrency>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplStoreTenderTypeCurrency rec = new ReplStoreTenderTypeCurrency();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Store No.": rec.StoreID = field.Values[i]; break;
                        case "Tender Type Code": rec.TenderTypeId = field.Values[i]; break;
                        case "Currency Code": rec.CurrencyCode = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplTenderType> ReplicateTenderTypes(XMLTableData table)
        {
            List<ReplTenderType> list = new List<ReplTenderType>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplTenderType rec = new ReplTenderType();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Nam = field.Values[i]; break;
                        case "Default Function": rec.DefaultFunction = Convert.ToInt32(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplTaxSetup> ReplicateTaxSetup(XMLTableData table)
        {
            List<ReplTaxSetup> list = new List<ReplTaxSetup>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplTaxSetup rec = new ReplTaxSetup();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "VAT Bus. Posting Group": rec.BusinessTaxGroup = field.Values[i]; break;
                        case "VAT Prod. Posting Group": rec.ProductTaxGroup = field.Values[i]; break;
                        case "VAT %": rec.TaxPercent = Convert.ToDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplVendor> ReplicateVendor(XMLTableData table)
        {
            List<ReplVendor> list = new List<ReplVendor>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplVendor rec = new ReplVendor()
                {
                    AllowCustomersToSelectPageSize = false,
                    DisplayOrder = 1,
                    ManufacturerTemplateId = 1,
                    PageSize = 4,
                    PictureId = 0,
                    Published = true
                };

                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                        case "Blocked": rec.Blocked = GetWebBool(field.Values[i]); break;
                        case "Last Date Modified": rec.UpdatedOnUtc = GetWebDateTime(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplImageLink> ReplicateImageLink(XMLTableData table)
        {
            List<ReplImageLink> list = new List<ReplImageLink>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplImageLink rec = new ReplImageLink();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Image Id": rec.ImageId = field.Values[i]; break;
                        case "Display Order": rec.DisplayOrder = GetWebInt(field.Values[i]); break;
                        case "TableName": rec.TableName = field.Values[i]; break;
                        case "KeyValue": rec.KeyValue = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplImage> ReplicateImage(XMLTableData table)
        {
            List<ReplImage> list = new List<ReplImage>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplImage rec = new ReplImage();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Image Location": rec.Location = field.Values[i]; break;
                        case "Type": rec.LocationType = (LocationType)GetWebInt(field.Values[i]); break;
                        case "Image Blob": rec.Image64 = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplDataTranslation> ReplicateDataTranslation(XMLTableData table)
        {
            List<ReplDataTranslation> list = new List<ReplDataTranslation>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplDataTranslation rec = new ReplDataTranslation();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Key": rec.Key = field.Values[i]; break;
                        case "Translation ID": rec.TranslationId = field.Values[i]; break;
                        case "Language Code": rec.LanguageCode = field.Values[i]; break;
                        case "Translation": rec.Text = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplDiscountValidation> ReplicateDiscountValidations(XMLTableData table)
        {
            List<ReplDiscountValidation> list = new List<ReplDiscountValidation>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplDiscountValidation rec = new ReplDiscountValidation();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "ID": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "StartingDate": rec.StartDate = GetWebDateTime(field.Values[i]); break;
                        case "EndingDate": rec.EndDate = GetWebDateTime(field.Values[i]); break;
                        case "StartingTime": rec.StartTime = GetWebDateTime(field.Values[i]); break;
                        case "EndingTime": rec.EndTime = GetWebDateTime(field.Values[i]); break;
                        case "MondayStartingTime": rec.MondayStart = GetWebDateTime(field.Values[i]); break;
                        case "MondayEndingTime": rec.MondayEnd = GetWebDateTime(field.Values[i]); break;
                        case "TuesdayStartingTime": rec.TuesdayStart = GetWebDateTime(field.Values[i]); break;
                        case "TuesdayEndingTime": rec.TuesdayEnd = GetWebDateTime(field.Values[i]); break;
                        case "WednesdayStartingTime": rec.WednesdayStart = GetWebDateTime(field.Values[i]); break;
                        case "WednesdayEndingTime": rec.WednesdayEnd = GetWebDateTime(field.Values[i]); break;
                        case "ThursdayStartingTime": rec.ThursdayStart = GetWebDateTime(field.Values[i]); break;
                        case "ThursdayEndingTime": rec.ThursdayEnd = GetWebDateTime(field.Values[i]); break;
                        case "FridayStartingTime": rec.FridayStart = GetWebDateTime(field.Values[i]); break;
                        case "FridayEndingTime": rec.FridayEnd = GetWebDateTime(field.Values[i]); break;
                        case "SaturdayStartingTime": rec.SaturdayStart = GetWebDateTime(field.Values[i]); break;
                        case "SaturdayEndingTime": rec.SaturdayEnd = GetWebDateTime(field.Values[i]); break;
                        case "SundayStartingTime": rec.SundayStart = GetWebDateTime(field.Values[i]); break;
                        case "SundayEndingTime": rec.SundayEnd = GetWebDateTime(field.Values[i]); break;
                        case "TimewithinBounds": rec.TimeWithinBounds = GetWebBool(field.Values[i]); break;
                        case "EndingTimeAfterMidnight": rec.EndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Mon_TimewithinBounds": rec.MondayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Mon_End_TimeAfterMidnight": rec.MondayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Tue_TimewithinBounds": rec.TuesdayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Tue_End_TimeAfterMidnight": rec.TuesdayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Wed_TimewithinBounds": rec.WednesdayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Wed_End_TimeAfterMidnight": rec.WednesdayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Thu_TimewithinBounds": rec.ThursdayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Thu_End_TimeAfterMidnight": rec.ThursdayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Fri_TimewithinBounds": rec.FridayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Fri_End_TimeAfterMidnight": rec.FridayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Sat_TimewithinBounds": rec.SaturdayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Sat_End_TimeAfterMidnight": rec.SaturdayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                        case "Sun_TimewithinBounds": rec.SundayWithinBounds = GetWebBool(field.Values[i]); break;
                        case "Sun_End_TimeAfterMidnight": rec.SundayEndAfterMidnight = GetWebBool(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplShippingAgent> ReplicateShippingAgent(XMLTableData table)
        {
            List<ReplShippingAgent> list = new List<ReplShippingAgent>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplShippingAgent rec = new ReplShippingAgent();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                        case "Internet Address": rec.InternetAddress = field.Values[i]; break;
                        case "Account No.": rec.AccountNo = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplTerminal> ReplicateTerminals(XMLTableData table)
        {
            List<ReplTerminal> list = new List<ReplTerminal>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplTerminal rec = new ReplTerminal();
                rec.Features = new FeatureFlags();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Description": rec.Name = field.Values[i]; break;
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Terminal Type": rec.TerminalType = Convert.ToInt32(field.Values[i]); break;
                        case "EFT Store No.": rec.EFTStoreId = field.Values[i]; break;
                        case "EFT POS Terminal No.": rec.SFTTerminalId = field.Values[i]; break;
                        case "Hardware Profile": rec.HardwareProfile = field.Values[i]; break;
                        case "Functionality Profile": rec.FunctionalityProfile = field.Values[i]; break;
                        case "Interface Profile": rec.VisualProfile = field.Values[i]; break;
                        case "Sales Type Filter": rec.HospTypeFilter = field.Values[i]; break;
                        case "Default Sales Type": rec.DefaultHospType = field.Values[i]; break;

                        case "Exit After Each Trans.": rec.Features.AddFlag(FeatureFlagName.ExitAfterEachTransaction, field.Values[i]); break;
                        case "AutoLogoff After (Min.)": rec.Features.AddFlag(FeatureFlagName.AutoLogOffAfterMin, field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplCountryCode> ReplicateCountryCode(XMLTableData table)
        {
            List<ReplCountryCode> list = new List<ReplCountryCode>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplCountryCode rec = new ReplCountryCode();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplPlu> ReplicatePlu(XMLTableData table)
        {
            List<ReplPlu> list = new List<ReplPlu>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplPlu rec = new ReplPlu();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "StoreId": rec.StoreId = field.Values[i]; break;
                        case "PageId": rec.PageId = GetWebInt(field.Values[i]); break;
                        case "PageIndex": rec.PageIndex = GetWebInt(field.Values[i]); break;
                        case "ItemId": rec.ItemId = field.Values[i]; break;
                        case "Description": rec.Descritpion = field.Values[i]; break;
                        case "ItemImageId": rec.ItemImageId = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplInvStatus> ReplicateInvStatus(XMLTableData table)
        {
            List<ReplInvStatus> list = new List<ReplInvStatus>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplInvStatus rec = new ReplInvStatus();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Item No.": rec.ItemId = field.Values[i]; break;
                        case "Variant Code": rec.VariantId = field.Values[i]; break;
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Net Inventory": rec.Quantity = GetWebDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
