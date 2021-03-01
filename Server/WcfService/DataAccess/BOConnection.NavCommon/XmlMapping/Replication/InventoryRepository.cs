using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Inventory.Replication;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication
{
    public class InventoryRepository : BaseRepository
    {
        public List<ReplInvLocation> ReplicateLocations(XMLTableData table, string storeId)
        {
            List<ReplInvLocation> list = new List<ReplInvLocation>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplInvLocation rec = new ReplInvLocation();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Type": rec.Type = GetWebInt(field.Values[i]); break;
                        case "Location Code": rec.LocationCode = field.Values[i]; break;
                        case "Location Name": rec.Name = field.Values[i]; break;
                    }
                }
                if (rec.StoreId == storeId)
                    list.Add(rec);
            }
            return list;
        }

        public List<ReplGS1BarcodeSetup> ReplicateGS1Setup(XMLTableData table)
        {
            List<ReplGS1BarcodeSetup> list = new List<ReplGS1BarcodeSetup>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplGS1BarcodeSetup rec = new ReplGS1BarcodeSetup();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Type": rec.Type = GetWebInt(field.Values[i]); break;
                        case "Identifier": rec.Identifier = field.Values[i]; break;
                        case "Section Type": rec.SectionType = GetWebInt(field.Values[i]); break;
                        case "Section Size": rec.SectionSize = GetWebInt(field.Values[i]); break;
                        case "Identifier Size": rec.IdentifierSize = GetWebInt(field.Values[i]); break;
                        case "Section Mapping": rec.SectionMapping = GetWebInt(field.Values[i]); break;
                        case "Mapping Starting Char": rec.MappingStartingChar = GetWebInt(field.Values[i]); break;
                        case "Preferred Sequence": rec.PreferredSequence = GetWebInt(field.Values[i]); break;
                        case "Decimals": rec.Decimals = GetWebDecimal(field.Values[i]); break;
                        case "Value Type": rec.ValueType = GetWebInt(field.Values[i]); break;
                        case "Value": rec.Value = field.Values[i]; break;
                        case "Value (Dec)": rec.ValueDec = GetWebDecimal(field.Values[i]); break;
                        case "Value (Date)": rec.ValueDate = GetWebDateTime(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    
        public List<ReplInvMask> ReplicateInventoryMasks(XMLTableData table, int maxlines)
        {
            List<ReplInvMask> list = new List<ReplInvMask>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplInvMask rec = new ReplInvMask();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "WorksheetSeqNo": rec.SeqNumber = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Location Code": rec.Location = field.Values[i]; break;
                        case "Default UoM": rec.Unit = GetWebInt(field.Values[i]); break;
                        case "Use Area": rec.UseArea = GetWebBool(field.Values[i]); break;
                        case "StoreInvTransaction Max Lines": rec.MaxLinesToSend = GetWebInt(field.Values[i]); break;
                        case "Quantity Method": rec.QuantityMethod = GetWebInt(field.Values[i]); break;
                        case "Quick-default Quantity": rec.QuickDefaultQuantity = GetWebInt(field.Values[i]); break;
                        case "Reason Code": rec.ReasonCode = field.Values[i]; break;
                        case "Type": rec.EntryType = GetWebInt(field.Values[i]); break;
                        case "Type of Entering": rec.TypeOfEntering = GetWebInt(field.Values[i]); break;
                    }
                }
                rec.SearchForItemBy = 4;
                if (rec.MaxLinesToSend == 0)
                    rec.MaxLinesToSend = maxlines;

                //Central flipped enum values 1 & 2 when using store inventory 
                if (rec.Unit == 1)
                {
                    rec.Unit = 2;
                }
                else if (rec.Unit == 2)
                {
                    rec.Unit = 1;
                }

                list.Add(rec);
            }
            return list;
        }

        public List<ReplInvCountingArea> ReplicateCountingAreas(XMLTableData table, int seqNo)
        {
            List<ReplInvCountingArea> list = new List<ReplInvCountingArea>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplInvCountingArea rec = new ReplInvCountingArea();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Area": rec.Area = field.Values[i]; break;
                        case "Area Description": rec.Name = field.Values[i]; break;
                        case "Counted by": rec.CountBy = field.Values[i]; break;
                    }
                }
                rec.MaskId = seqNo.ToString();
                list.Add(rec);
            }
            return list;
        }

        public List<ReplInvMenu> ReplicateInventoryMenus(XMLTableData table)
        {
            List<ReplInvMenu> list = new List<ReplInvMenu>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplInvMenu rec = new ReplInvMenu();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Device Type": rec.DeviceType = GetWebInt(field.Values[i]); break;
                        case "Code": rec.Code = field.Values[i]; break;
                        case "Store No.": rec.StoreId = field.Values[i]; break;
                        case "Text": rec.Text = field.Values[i]; break;
                        case "Bitmap": rec.Bitmap = field.Values[i]; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<ReplInvMenuLine> ReplicateInventoryMenuLines(XMLTableData table)
        {
            List<ReplInvMenuLine> list = new List<ReplInvMenuLine>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                ReplInvMenuLine rec = new ReplInvMenuLine();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Menu Code": rec.MenuId = field.Values[i]; break;
                        case "Line No.": rec.LineId = GetWebInt(field.Values[i]); break;
                        case "Text": rec.Text = field.Values[i]; break;
                        case "Line Type": rec.LineType = GetWebInt(field.Values[i]); break;
                        case "Code": rec.Code = field.Values[i]; break;
                        case "Bitmap": rec.Bitmap = field.Values[i]; break;
                        case "Status": rec.Status = GetWebInt(field.Values[i]); break;
                        case "Code Type": rec.CodeType = GetWebInt(field.Values[i]); break;
                        case "Location Code": rec.LocationCode = field.Values[i]; break;
                        case "Vendor No.": rec.VendorId = field.Values[i]; break;
                        case "Customer No.": rec.CustomerId = field.Values[i]; break;
                        case "Card View Code": rec.CardViewId = field.Values[i]; break;
                        case "Item No.": rec.ItemNo = field.Values[i]; break;
                        case "Worksheet Type": rec.MaskType = GetWebInt(field.Values[i]); break;
                        case "WorksheetSeqNo": rec.MaskId = GetWebInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
