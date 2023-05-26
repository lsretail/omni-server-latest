using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class OrderJMapping : BaseJMapping
    {
        public OrderJMapping(bool json)
        {
            IsJson = json;
        }

        public List<SalesEntry> GetSalesEntry(string ret)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            WSODataCollection result = JsonToWSOData(ret, "MemberContactSalesHistoryDaCo");
            if (result == null)
                return list;

            // get data
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000818);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntry rec = new SalesEntry();
                rec.ContactAddress = new Address();
                rec.ShipToAddress = new Address();

                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldIndex)
                    {
                        case 2: rec.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                        case 3: rec.Id = col.FieldValue; break;
                        case 4: rec.StoreId = col.FieldValue; break;
                        case 5: rec.DocumentRegTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case 6: rec.ReturnSale = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case 7: rec.CustomerOrderNo = col.FieldValue; break;
                        case 8: rec.TerminalId = col.FieldValue; break;
                        case 9: rec.LineItemCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 10: rec.LineCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 11: rec.TotalNetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 12: rec.TotalAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 13: rec.TotalDiscount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 14: rec.CardId = col.FieldValue; break;
                        case 15: rec.CustomerId = col.FieldValue; break;
                        case 16: rec.ExternalId = col.FieldValue; break;
                        case 17: rec.HasReturnSale = col.FieldValue != string.Empty; break;
                        case 18: rec.StoreName = col.FieldValue; break;
                        case 19: rec.StoreCurrency = col.FieldValue; break;
                        case 20: rec.ContactName = col.FieldValue; break;
                        case 21: rec.ContactAddress.Address1 = col.FieldValue; break;
                        case 22: rec.ContactAddress.Address2 = col.FieldValue; break;
                        case 23: rec.ContactAddress.City = col.FieldValue; break;
                        case 24: rec.ContactAddress.StateProvinceRegion = col.FieldValue; break;
                        case 25: rec.ContactAddress.PostCode = col.FieldValue; break;
                        case 26: rec.ContactAddress.Country = col.FieldValue; break;
                        case 27: rec.ContactAddress.PhoneNumber = col.FieldValue; break;
                        case 28: rec.ContactEmail = col.FieldValue; break;
                        case 29: rec.ContactAddress.HouseNo = col.FieldValue; break;
                        case 30: rec.ContactAddress.CellPhoneNumber = col.FieldValue; break;
                        case 31: rec.ContactDayTimePhoneNo = col.FieldValue; break;
                        case 32: rec.ShipToName = col.FieldValue; break;
                        case 33: rec.ShipToAddress.Address1 = col.FieldValue; break;
                        case 34: rec.ShipToAddress.Address2 = col.FieldValue; break;
                        case 35: rec.ShipToAddress.City = col.FieldValue; break;
                        case 36: rec.ShipToAddress.StateProvinceRegion = col.FieldValue; break;
                        case 37: rec.ShipToAddress.PostCode = col.FieldValue; break;
                        case 38: rec.ShipToAddress.Country = col.FieldValue; break;
                        case 39: rec.ShipToAddress.PhoneNumber = col.FieldValue; break;
                        case 40: rec.ShipToEmail = col.FieldValue; break;
                        case 41: rec.ShipToAddress.HouseNo = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<SalesEntry> GetSalesEntry2(string ret)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            WSODataCollection result = JsonToWSOData(ret, "MemberContactSalesHistoryDaCo");
            if (result == null)
                return list;

            // get data
            ReplODataSetRecRef dynDataSet = result.GetDataSet(10000818);
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                SalesEntry rec = new SalesEntry();
                rec.ContactAddress = new Address();
                rec.ShipToAddress = new Address();

                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldIndex)
                    {
                        case 2: rec.IdType = GetDocIdType(ConvertTo.SafeInt(col.FieldValue)); break;
                        case 3: rec.Id = col.FieldValue; break;
                        case 4: rec.StoreId = col.FieldValue; break;
                        case 5: rec.DocumentRegTime = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case 6: rec.ReturnSale = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case 7: rec.CustomerOrderNo = col.FieldValue; break;
                        case 8: rec.TerminalId = col.FieldValue; break;
                        case 9: rec.LineItemCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 10: rec.LineCount = ConvertTo.SafeInt(col.FieldValue); break;
                        case 11: rec.TotalNetAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 12: rec.TotalAmount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 13: rec.TotalDiscount = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 14: rec.CardId = col.FieldValue; break;
                        case 15: rec.CustomerId = col.FieldValue; break;
                        case 16: rec.ExternalId = col.FieldValue; break;
                        case 17: rec.HasReturnSale = col.FieldValue != string.Empty; break;
                        case 18: rec.Quantity = ConvertTo.SafeDecimal(col.FieldValue); break;
                        case 19: rec.StoreName = col.FieldValue; break;
                        case 20: rec.StoreCurrency = col.FieldValue; break;
                        case 21: rec.ContactName = col.FieldValue; break;
                        case 22: rec.ContactAddress.Address1 = col.FieldValue; break;
                        case 23: rec.ContactAddress.Address2 = col.FieldValue; break;
                        case 24: rec.ContactAddress.City = col.FieldValue; break;
                        case 25: rec.ContactAddress.StateProvinceRegion = col.FieldValue; break;
                        case 26: rec.ContactAddress.PostCode = col.FieldValue; break;
                        case 27: rec.ContactAddress.Country = col.FieldValue; break;
                        case 28: rec.ContactAddress.PhoneNumber = col.FieldValue; break;
                        case 29: rec.ContactEmail = col.FieldValue; break;
                        case 30: rec.ContactAddress.HouseNo = col.FieldValue; break;
                        case 31: rec.ContactAddress.CellPhoneNumber = col.FieldValue; break;
                        case 32: rec.ContactDayTimePhoneNo = col.FieldValue; break;
                        case 33: rec.ShipToName = col.FieldValue; break;
                        case 34: rec.ShipToAddress.Address1 = col.FieldValue; break;
                        case 35: rec.ShipToAddress.Address2 = col.FieldValue; break;
                        case 36: rec.ShipToAddress.City = col.FieldValue; break;
                        case 37: rec.ShipToAddress.StateProvinceRegion = col.FieldValue; break;
                        case 38: rec.ShipToAddress.PostCode = col.FieldValue; break;
                        case 39: rec.ShipToAddress.Country = col.FieldValue; break;
                        case 40: rec.ShipToAddress.PhoneNumber = col.FieldValue; break;
                        case 41: rec.ShipToEmail = col.FieldValue; break;
                        case 42: rec.ShipToAddress.HouseNo = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private DocumentIdType GetDocIdType(int type)
        {
            switch (type)
            {
                case 1:
                    return DocumentIdType.Order;
                case 2:
                    return DocumentIdType.HospOrder;
            }
            return DocumentIdType.Receipt;
        }
    }
}
