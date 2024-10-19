using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Replication
{
    public class ContactRepository : BaseRepository
    {
        public ContactRepository(BOConfiguration config) : base(config)
        {
        }

        public List<Scheme> SchemeGetAll(XMLTableData table)
        {
            List<Scheme> list = new List<Scheme>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                Scheme rec = new Scheme();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Club Code": rec.Club = new Club(field.Values[i]); break;
                        case "Min. Point for Upgrade": rec.PointsNeeded = (long)ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Next Scheme Benefits": rec.Perks = field.Values[i]; break;
                        case "NextScheme": rec.NextScheme = new Scheme(field.Values[i]); break;
                        case "Update Sequence": rec.UpdateSequence = Convert.ToInt32(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<MemberContact> ContactGet(XMLTableData table)
        {
            List<MemberContact> list = new List<MemberContact>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                MemberContact rec = new MemberContact();
                Address add = new Address();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Account No.": rec.Account = new Account(field.Values[i]); break;
                        case "Contact No.": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                        case "First Name": rec.FirstName = field.Values[i]; break;
                        case "Surname": rec.LastName = field.Values[i]; break;
                        case "Address": add.Address1 = field.Values[i]; break;
                        case "Address 2": add.Address2 = field.Values[i]; break;
                        case "Post Code": add.PostCode = field.Values[i]; break;
                        case "City": add.City = field.Values[i]; break;
                        case "Country": add.Country = field.Values[i]; break;
                        case "Phone No.": add.PhoneNumber = field.Values[i]; break;
                        case "Mobile Phone No.": add.CellPhoneNumber = field.Values[i]; break;
                        case "E-Mail": rec.Email = field.Values[i]; break;
                    }
                }
                rec.Addresses = new List<Address>();
                rec.Addresses.Add(add);
                list.Add(rec);
            }
            return list;
        }

        public List<Profile> ProfileGet(XMLTableData table)
        {
            List<Profile> list = new List<Profile>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                Profile rec = new Profile();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Code": rec.Id = field.Values[i]; break;
                        case "Description": rec.Description = field.Values[i]; break;
                        case "Attribute Type": rec.DataType = (ProfileDataType)ConvertTo.SafeInt(field.Values[i]); break;
                        case "Default Value": rec.DefaultValue = field.Values[i]; break;
                        case "Mandatory": rec.Mandatory = ConvertTo.SafeBoolean(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public List<Customer> CustomerGet(XMLTableData table)
        {
            List<Customer> list = new List<Customer>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                Customer rec = new Customer();
                rec.Address = new Address();
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "No.": rec.Id = field.Values[i]; break;
                        case "Name": rec.Name = field.Values[i]; break;
                        case "Address": rec.Address.Address1 = field.Values[i]; break;
                        case "City": rec.Address.City = field.Values[i]; break;
                        case "Post Code": rec.Address.PostCode = field.Values[i]; break;
                        case "Country/Region Code": rec.Address.StateProvinceRegion = field.Values[i]; break;
                        case "Mobile Phone No.": rec.Address.CellPhoneNumber = field.Values[i]; break;
                        case "Phone No.": rec.Address.PhoneNumber = field.Values[i]; break;
                        case "E-Mail": rec.Email = field.Values[i]; break;
                        case "Currency Code": rec.Currency = new Currency(field.Values[i]); break;
                        case "Blocked": rec.IsBlocked = ConvertTo.SafeBoolean(field.Values[i]); break;
                        case "Prices Including VAT": rec.InclTax = XMLHelper.GetWebBoolInt(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        public Device DeviceGetById(XMLTableData table)
        {
            if (table == null || table.NumberOfValues == 0)
                return null;

            Device rec = new Device();
            foreach (XMLFieldData field in table.FieldList)
            {
                switch (field.FieldName)
                {
                    case "ID": rec.Id = field.Values[0]; break;
                    case "Security Token": rec.SecurityToken = field.Values[0]; break;
                    case "Friendly Name": rec.DeviceFriendlyName = field.Values[0]; break;
                    case "Status": rec.Status = ConvertTo.SafeInt(field.Values[0]); break;
                    case "Reason Blocked": rec.BlockedReason = field.Values[0]; break;
                    case "Date Blocked": rec.BlockedDate = XMLHelper.GetWebDateTime(field.Values[0]); break;
                    case "Blocked By": rec.BlockedBy = field.Values[0]; break;
                }
            }
            return rec;
        }

        public List<PointEntry> PointEntryGet(XMLTableData table)
        {
            List<PointEntry> list = new List<PointEntry>();
            if (table == null)
                return list;

            for (int i = 0; i < table.NumberOfValues; i++)
            {
                PointEntry rec = new PointEntry(string.Empty);
                foreach (XMLFieldData field in table.FieldList)
                {
                    switch (field.FieldName)
                    {
                        case "Entry No.": rec.Id = field.Values[i]; break;
                        case "Document No.": rec.DocumentNo = field.Values[i]; break;
                        case "Store No.": rec.StoreNo = field.Values[i]; break;
                        case "Name": rec.StoreName = field.Values[i]; break;
                        case "Date": rec.Date = ConvertTo.SafeJsonDate(XMLHelper.GetWebDateTime(field.Values[i]), config.IsJson); break;
                        case "Expiration Date": rec.ExpirationDate = ConvertTo.SafeJsonDate(XMLHelper.GetWebDateTime(field.Values[i]), config.IsJson); break;
                        case "Source Type": rec.SourceType = (MemberPointSourceType)Convert.ToInt32(field.Values[i]); break;
                        case "Entry Type": rec.EntryType = (MemberPointEntryType)Convert.ToInt32(field.Values[i]); break;
                        case "Point Type": rec.PointType = (MemberPointType)Convert.ToInt32(field.Values[i]); break;
                        case "Points": rec.Points = ConvertTo.SafeDecimal(field.Values[i]); break;
                        case "Remaining Points": rec.RemainingPoints = ConvertTo.SafeDecimal(field.Values[i]); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
