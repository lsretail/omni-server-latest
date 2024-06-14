using System;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public class ContactJMapping : BaseJMapping
    {
        public ContactJMapping(bool json)
        {
            IsJson = json;
        }

        public List<MemberContact> GetMemberContact(string ret)
        {
            List<MemberContact> list = new List<MemberContact>();
            WSODataCollection result = JsonToWSOData(ret, "MemberContactInfoDaCo");
            if (result == null)
                return list;

            // get data
            list = LoadMemberContacts(result.GetDataSet(99009002));
            if (list == null)
                return list;

            List<Card> cards = LoadMemberCards(result.GetDataSet(99009003));
            List<Account> accounts = LoadMemberAccounts(result.GetDataSet(99009001));
            List<Scheme> schemes = LoadMemberSchemes(result.GetDataSet(99009024));
            List<Club> clubs = LoadClubs(result.GetDataSet(99009000));
            List<Profile> profiles = LoadMemberProfile(result.GetDataSet(10000817));
            List<WSODataFlowField> flowfields = LoadFlowFields(result.GetDataSet(99001649));

            // join data
            foreach (MemberContact cont in list)
            {
                cont.Cards = cards.FindAll(x => x.ContactId == cont.Id);
                foreach (Card card in cont.Cards)
                {
                    card.LoginId = LoadOneValue(result.GetDataSet(99009049), "Card No.", card.Id, "Login ID");
                }
                cont.UserName = LoadOneValue(result.GetDataSet(99009049), "Card No.", cont.Cards.FirstOrDefault().Id, "Login ID");
                cont.Account = accounts.Find(x => x.Id == cont.Account.Id);
                cont.Account.Scheme = schemes.Find(x => x.Id == cont.Account.SchemeCode);
                cont.Account.Scheme.Club = clubs.Find(x => x.Id == cont.Account.Scheme.ClubCode);
                cont.Profiles = profiles.FindAll(x => x.AccountNo == cont.Account.Id && x.ContactNo == cont.Id);

                string key = "LSC Member Account: " + cont.Account.Id;
                List<WSODataFlowField> fields = flowfields.FindAll(x => x.Key == key);
                foreach (WSODataFlowField rec in fields)
                {
                    cont.Account.PointBalance += (long)rec.DecimalValue;
                }
            }
            return list;
        }

        private List<MemberContact> LoadMemberContacts(ReplODataSetRecRef dynDataSet)
        {
            List<MemberContact> list = new List<MemberContact>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                MemberContact rec = new MemberContact();
                rec.Addresses = new List<Address>();
                rec.Addresses.Add(new Address());
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Account No.": rec.Account = new Account(col.FieldValue); break;
                        case "Contact No.": rec.Id = col.FieldValue; break;
                        case "Name": rec.Name = col.FieldValue; break;
                        case "First Name": rec.FirstName = col.FieldValue; break;
                        case "Middle Name": rec.MiddleName = col.FieldValue; break;
                        case "Surname": rec.LastName = col.FieldValue; break;
                        case "Gender": rec.Gender = (Gender)ConvertTo.SafeInt(col.FieldValue); break;
                        case "E-Mail": rec.Email = col.FieldValue; break;
                        case "Date of Birth": rec.BirthDay = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Marital Status": rec.MaritalStatus = (MaritalStatus)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Address": rec.Addresses[0].Address1 = col.FieldValue; break;
                        case "Address 2": rec.Addresses[0].Address2 = col.FieldValue; break;
                        case "Post Code": rec.Addresses[0].PostCode = col.FieldValue; break;
                        case "City": rec.Addresses[0].City = col.FieldValue; break;
                        case "Country/Region Code": rec.Addresses[0].Country = col.FieldValue; break;
                        case "Phone No.": rec.Addresses[0].PhoneNumber = col.FieldValue; break;
                        case "Mobile Phone No.": rec.Addresses[0].CellPhoneNumber = col.FieldValue; break;
                        case "Territory Code": rec.Addresses[0].StateProvinceRegion = col.FieldValue; break;
                        case "Send Receipt by E-mail": rec.SendReceiptByEMail = (SendEmail)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Blocked": rec.Blocked = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Reason Blocked": rec.BlockedReason = col.FieldValue; break;
                        case "Date Blocked": rec.DateBlocked = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Blocked by": rec.BlockedBy = col.FieldValue; break;
                        case "Guest Type": rec.GuestType = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<Card> LoadMemberCards(ReplODataSetRecRef dynDataSet)
        {
            List<Card> list = new List<Card>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Card rec = new Card();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Card No.": rec.Id = col.FieldValue; break;
                        case "Blocked by": rec.BlockedBy = col.FieldValue; break;
                        case "Reason Blocked": rec.BlockedReason = col.FieldValue; break;
                        case "Club Code": rec.ClubId = col.FieldValue; break;
                        case "Contact No.": rec.ContactId = col.FieldValue; break;
                        case "Date Blocked": rec.DateBlocked = ConvertTo.SafeDateTime(col.FieldValue); break;
                        case "Linked to Account": rec.LinkedToAccount = ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Status": rec.Status = (CardStatus)ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<Account> LoadMemberAccounts(ReplODataSetRecRef dynDataSet)
        {
            List<Account> list = new List<Account>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Account rec = new Account();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "No.": rec.Id = col.FieldValue; break;
                        case "Blocked": rec.Blocked =  ConvertTo.SafeBoolean(col.FieldValue); break;
                        case "Linked To Customer No.": rec.CustomerId = col.FieldValue; break;
                        case "Club Code": rec.ClubCode = col.FieldValue; break;
                        case "Scheme Code": rec.SchemeCode = col.FieldValue; break;
                        case "Status": rec.Status = (AccountStatus)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Account Type": rec.Type = (AccountType)ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<Scheme> LoadMemberSchemes(ReplODataSetRecRef dynDataSet)
        {
            List<Scheme> list = new List<Scheme>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Scheme rec = new Scheme();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rec.Id = col.FieldValue; break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Club Code": rec.ClubCode = col.FieldValue; break;
                        case "Next Scheme": rec.NextSchemeCode = col.FieldValue; break;
                        case "Next Scheme Benefits": rec.Perks = col.FieldValue; break;
                        case "Update Sequence": rec.UpdateSequence = ConvertTo.SafeInt(col.FieldValue); break;
                        case "Min. Point for Upgrade": rec.PointsNeeded = ConvertTo.SafeInt(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<Club> LoadClubs(ReplODataSetRecRef dynDataSet)
        {
            List<Club> list = new List<Club>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Club rec = new Club();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Code": rec.Id = col.FieldValue; break;
                        case "Description": rec.Name = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }

        private List<Profile> LoadMemberProfile(ReplODataSetRecRef dynDataSet)
        {
            List<Profile> list = new List<Profile>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                Profile rec = new Profile();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldName)
                    {
                        case "Account No.": rec.AccountNo = col.FieldValue; break;
                        case "Contact No.": rec.ContactNo = col.FieldValue; break;
                        case "Code": rec.Id = col.FieldValue; break;
                        case "Type": rec.DataType = (ProfileDataType)ConvertTo.SafeInt(col.FieldValue); break;
                        case "Description": rec.Description = col.FieldValue; break;
                        case "Value": rec.TextValue = col.FieldValue; break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
