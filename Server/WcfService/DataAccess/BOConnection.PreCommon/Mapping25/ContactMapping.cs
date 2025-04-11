using System;
using System.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.PreCommon.Mapping25
{
    public class ContactMapping25 : BaseMapping25
    {
        public ContactMapping25(bool json, Version version)
        {
            IsJson = json;
            LSCVersion = version;
        }

        public LSCentral25.RootMemberContactCreate MapToRoot(MemberContact contact)
        {
            Address addr = (contact.Addresses == null || contact.Addresses.Count == 0) ? new Address() : contact.Addresses[0];

            List<LSCentral25.ContactCreateParameters> members = new List<LSCentral25.ContactCreateParameters>();
            LSCentral25.ContactCreateParameters mem = new LSCentral25.ContactCreateParameters()
            {
                ContactID = XMLHelper.GetString(contact.Id),
                AccountID = XMLHelper.GetString(contact.Account?.Id),
                ClubID = XMLHelper.GetString(contact.Account?.Scheme?.Club?.Id),
                SchemeID = XMLHelper.GetString(contact.Account?.Scheme?.Id),
                ExternalID = XMLHelper.GetString(contact.AlternateId),

                FirstName = XMLHelper.GetString(contact.FirstName),
                MiddleName = XMLHelper.GetString(contact.MiddleName),
                LastName = XMLHelper.GetString(contact.LastName),
                DateOfBirth = contact.BirthDay,
                Email = contact.Email.ToLower(),
                Gender = contact.GetNewGender(LSCVersion),

                Address1 = XMLHelper.GetString(addr.Address1),
                Address2 = XMLHelper.GetString(addr.Address2),
                HouseApartmentNo = XMLHelper.GetString(addr.HouseNo),
                City = XMLHelper.GetString(addr.City),
                Country = XMLHelper.GetString(addr.Country),
                PostCode = XMLHelper.GetString(addr.PostCode),
                TerritoryCode = XMLHelper.GetString(addr.StateProvinceRegion),
                StateProvinceRegion = XMLHelper.GetString(addr.County),

                Phone = XMLHelper.GetString(addr.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(addr.CellPhoneNumber),

                LoginID = XMLHelper.GetString(contact.UserName.ToLower()),
                Password = XMLHelper.GetString(contact.Password),
                DeviceID = XMLHelper.GetString(contact.LoggedOnToDevice.Id),
                DeviceFriendlyName = XMLHelper.GetString(contact.LoggedOnToDevice.DeviceFriendlyName),
                AuthenticationId = XMLHelper.GetString(contact.AuthenticationId),
                Authenticator = XMLHelper.GetString(contact.Authenticator),

                GuestType = XMLHelper.GetString(contact.GuestType),
                SendReceiptbyEmail = ((int)contact.SendReceiptByEMail).ToString(),
                ExternalSystem = XMLHelper.GetString(contact.ExternalSystem)
            };

            LSCentral25.RootMemberContactCreate root = new LSCentral25.RootMemberContactCreate();
            members.Add(mem);
            root.ContactCreateParameters = members.ToArray();

            List<LSCentral25.MemberAttributeValue> attr = new List<LSCentral25.MemberAttributeValue>();
            if (contact.Profiles != null)
            {
                foreach (Profile prof in contact.Profiles)
                {
                    LSCentral25.MemberAttributeValue val = new LSCentral25.MemberAttributeValue()
                    {
                        AttributeCode = prof.Id,
                    };
                    if (prof.DataType == ProfileDataType.Text)
                        val.AttributeValue = prof.TextValue;
                    else
                        val.AttributeValue = (prof.ContactValue) ? "Yes" : "No";
                    attr.Add(val);
                }
            }

            root.MemberAttributeValue = attr.ToArray();
            root.Text = new string[1];
            root.Text[0] = string.Empty;
            return root;
        }

        public LSCentral25.RootMemberContactCreate1 MapToRoot1(MemberContact contact, string accountId)
        {
            Address addr = (contact.Addresses == null || contact.Addresses.Count == 0) ? new Address() : contact.Addresses[0];

            List<LSCentral25.ContactCreateParameters1> member = new List<LSCentral25.ContactCreateParameters1>();
            LSCentral25.ContactCreateParameters1 mem = new LSCentral25.ContactCreateParameters1()
            {
                ContactID = XMLHelper.GetString(contact.Id),
                AccountID = XMLHelper.GetString(accountId),
                ExternalID = XMLHelper.GetString(contact.AlternateId),

                FirstName = XMLHelper.GetString(contact.FirstName),
                MiddleName = XMLHelper.GetString(contact.MiddleName),
                LastName = XMLHelper.GetString(contact.LastName),
                DateOfBirth = contact.BirthDay,
                Email = XMLHelper.GetString(contact.Email),
                Gender = contact.GetNewGender(LSCVersion),

                Address1 = XMLHelper.GetString(addr.Address1),
                Address2 = XMLHelper.GetString(addr.Address2),
                HouseApartmentNo = XMLHelper.GetString(addr.HouseNo),
                City = XMLHelper.GetString(addr.City),
                Country = XMLHelper.GetString(addr.Country),
                PostCode = XMLHelper.GetString(addr.PostCode),
                TerritoryCode = XMLHelper.GetString(addr.StateProvinceRegion),
                StateProvinceRegion = XMLHelper.GetString(addr.County),

                Phone = XMLHelper.GetString(addr.PhoneNumber),
                MobilePhoneNo = XMLHelper.GetString(addr.CellPhoneNumber),
                SendReceiptbyEmail = ((int)contact.SendReceiptByEMail).ToString(),
                ExternalSystem = XMLHelper.GetString(contact.ExternalSystem),
                GuestType = XMLHelper.GetString(contact.GuestType)
            };

            List<LSCentral25.MemberAttributeValue1> attr = new List<LSCentral25.MemberAttributeValue1>();
            if (contact.Profiles != null)
            {
                foreach (Profile prof in contact.Profiles)
                {
                    LSCentral25.MemberAttributeValue1 val = new LSCentral25.MemberAttributeValue1()
                    {
                        AttributeCode = prof.Id,
                    };
                    if (prof.DataType == ProfileDataType.Text)
                        val.AttributeValue = prof.TextValue;
                    else
                        val.AttributeValue = (prof.ContactValue) ? "Yes" : "No";
                    attr.Add(val);
                }
            }

            member.Add(mem);
            LSCentral25.RootMemberContactCreate1 root = new LSCentral25.RootMemberContactCreate1()
            {
                ContactCreateParameters = member.ToArray(),
                MemberAttributeValue = attr.ToArray(),
                Text = new string[1]
            };
            root.Text[0] = string.Empty;
            return root;
        }

        public MemberContact MapFromRootToContact(LSCentral25.RootGetMemberContact root, List<Scheme> schemelist)
        {
            LSCentral25.MemberContact contact = root.MemberContact.FirstOrDefault();
            MemberContact memberContact = new MemberContact()
            {
                Id = contact.ContactNo,
                AlternateId = contact.ExternalID,
                ExternalSystem = contact.ExternalSystem,
                Email = contact.EMail,
                FirstName = contact.FirstName,
                MiddleName = contact.MiddleName,
                LastName = contact.Surname,
                MaritalStatus = (MaritalStatus)Convert.ToInt32(contact.MaritalStatus),
                BirthDay = ConvertTo.SafeJsonDate(contact.DateofBirth, IsJson),
                Account = new Account(contact.AccountNo),
                SendReceiptByEMail = (SendEmail)Convert.ToInt32(contact.SendReceiptbyEmail),
                Blocked = contact.Blocked,
                BlockedBy = contact.Blockedby,
                BlockedReason = contact.ReasonBlocked,
                DateBlocked = ConvertTo.SafeJsonDate(contact.DateBlocked, IsJson)
            };
            
            memberContact.SetNewGender(contact.Gender, LSCVersion);
            memberContact.Addresses = new List<Address>()
            {
                new Address()
                {
                    Type = AddressType.Residential,
                    Address1 = contact.Address,
                    Address2 = contact.Address2,
                    HouseNo = contact.HouseApartmentNo,
                    City = contact.City,
                    PostCode = contact.PostCode,
                    StateProvinceRegion = contact.TerritoryCode,
                    County = contact.County,
                    Country = contact.Country,
                    PhoneNumber = contact.PhoneNo,
                    CellPhoneNumber = contact.MobilePhoneNo
                }
            };

            Scheme scheme = schemelist.Find(s => s.Club.Id == contact.ClubCode && s.Id == contact.SchemeCode);
            memberContact.Account.Scheme = scheme;
            while (scheme != null)
            {
                scheme = GetNextScheme(schemelist, memberContact.Account.Scheme.Club.Id, scheme);
            }

            if (root.MembershipCard != null)
            {
                foreach (LSCentral25.MembershipCard1 card in root.MembershipCard)
                {
                    memberContact.Cards.Add(new Card()
                    {
                        Id = card.CardNo,
                        BlockedBy = card.Blockedby,
                        BlockedReason = card.ReasonBlocked,
                        DateBlocked = ConvertTo.SafeJsonDate(card.DateBlocked, IsJson),
                        LinkedToAccount = card.LinkedtoAccount,
                        ClubId = card.ClubCode,
                        ContactId = card.ContactNo,
                        Status = (CardStatus)Convert.ToInt32(card.Status)
                    });
                }
            }
            return memberContact;
        }

        private Scheme GetNextScheme(List<Scheme> schemelist, string club, Scheme cur)
        {
            Scheme sc = schemelist.Find(s => s.Club.Id == club && s.UpdateSequence == cur.UpdateSequence + 1);
            if (sc == null)
            {
                cur.PointsNeeded = 0;
                return null;
            }

            cur.NextScheme = sc;
            cur.PointsNeeded = sc.PointsNeeded;
            return sc;
        }

        public MemberContact MapFromRootToLogonContact(LSCentral25.RootMemberLogon root, decimal pointBalance)
        {
            if (root.MemberContact == null)
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, "No Contact found");

            LSCentral25.MemberContact3 contact = root.MemberContact.FirstOrDefault();
            MemberContact memberContact = new MemberContact()
            {
                Id = contact.ContactNo,
                AlternateId = contact.ExternalID,
                ExternalSystem = contact.ExternalSystem,
                Email = contact.EMail,
                FirstName = contact.FirstName,
                MiddleName = contact.MiddleName,
                LastName = contact.Surname,
                MaritalStatus = (MaritalStatus)Convert.ToInt32(contact.MaritalStatus),
                SendReceiptByEMail = (SendEmail)Convert.ToInt32(contact.SendReceiptbyEmail),
                BirthDay = ConvertTo.SafeJsonDate(contact.DateofBirth, IsJson),
                Blocked = contact.Blocked,
                BlockedBy = contact.Blockedby,
                BlockedReason = contact.ReasonBlocked,
                DateBlocked = ConvertTo.SafeJsonDate(contact.DateBlocked, IsJson)
            };

            memberContact.SetNewGender(contact.Gender, LSCVersion);
            memberContact.Addresses = new List<Address>()
            {
                new Address()
                {
                    Type = AddressType.Residential,
                    Address1 = contact.Address,
                    Address2 = contact.Address2,
                    HouseNo = contact.HouseApartmentNo,
                    City = contact.City,
                    PostCode = contact.PostCode,
                    StateProvinceRegion = contact.TerritoryCode,
                    County = contact.County,
                    Country = contact.Country,
                    PhoneNumber = contact.PhoneNo,
                    CellPhoneNumber = contact.MobilePhoneNo
                }
            };

            memberContact.Account = new Account(contact.AccountNo);
            memberContact.Account.PointBalance = (long)pointBalance;
            memberContact.Account.Scheme = new Scheme(contact.SchemeCode);
            memberContact.Account.Scheme.Club = new Club(contact.ClubCode);

            if (root.MemberClub != null)
            {
                memberContact.Account.Scheme.Club.Name = root.MemberClub.FirstOrDefault().Description;
            }

            if (root.MembershipCard != null)
            {
                foreach (LSCentral25.MembershipCard4 card in root.MembershipCard)
                {
                    memberContact.Cards.Add(new Card()
                    {
                        Id = card.CardNo,
                        BlockedBy = card.Blockedby,
                        BlockedReason = card.ReasonBlocked,
                        DateBlocked = ConvertTo.SafeJsonDate(card.DateBlocked, IsJson),
                        LinkedToAccount = card.LinkedtoAccount,
                        ClubId = card.ClubCode,
                        ContactId = card.ContactNo,
                        Status = (CardStatus)Convert.ToInt32(card.Status)
                    });
                }
            }

            if (root.MemberAccount != null)
            {
                memberContact.Account.Blocked = root.MemberAccount[0].Blocked;
                memberContact.Account.Status = (AccountStatus)ConvertTo.SafeInt(root.MemberAccount[0].Status);
                memberContact.Account.CustomerId = root.MemberAccount[0].CustomerNo;
                memberContact.Account.Type = (AccountType)ConvertTo.SafeInt(root.MemberAccount[0].AccountType);
            }

            if (root.MemberAttributeList != null)
            {
                foreach (LSCentral25.MemberAttributeList3 attr in root.MemberAttributeList)
                {
                    if (attr.Type != "0" || (attr.AttributeType != "0" && attr.AttributeType != "4"))
                        continue;

                    Profile pro = new Profile()
                    {
                        Id = attr.Code,
                        Description = attr.Description,
                        DataType = (ProfileDataType)Convert.ToInt32(attr.AttributeType)
                    };
                    if (pro.DataType == ProfileDataType.Text)
                        pro.TextValue = attr.Value;
                    else
                        pro.ContactValue = (attr.Value.ToUpper().Equals("YES"));

                    memberContact.Profiles.Add(pro);
                }
            }
            return memberContact;
        }

        public MemberContact MapFromRootToLogonAuth(LSCentral25.RootMemberauthLogin root, decimal pointBalance)
        {
            if (root.MemberContact == null)
                throw new LSOmniServiceException(StatusCode.ContactIdNotFound, "No Contact found");

            LSCentral25.MemberContact2 contact = root.MemberContact.FirstOrDefault();
            MemberContact memberContact = new MemberContact()
            {
                Id = contact.ContactNo,
                AlternateId = contact.ExternalID,
                ExternalSystem = contact.ExternalSystem,
                Email = contact.EMail,
                FirstName = contact.FirstName,
                MiddleName = contact.MiddleName,
                LastName = contact.Surname,
                MaritalStatus = (MaritalStatus)Convert.ToInt32(contact.MaritalStatus),
                SendReceiptByEMail = (SendEmail)Convert.ToInt32(contact.SendReceiptbyEmail),
                BirthDay = ConvertTo.SafeJsonDate(contact.DateofBirth, IsJson),
                Blocked = contact.Blocked,
                BlockedBy = contact.Blockedby,
                BlockedReason = contact.ReasonBlocked,
                DateBlocked = ConvertTo.SafeJsonDate(contact.DateBlocked, IsJson)
            };

            memberContact.SetNewGender(contact.Gender, LSCVersion);
            memberContact.Addresses = new List<Address>()
            {
                new Address()
                {
                    Type = AddressType.Residential,
                    Address1 = contact.Address,
                    Address2 = contact.Address2,
                    HouseNo = contact.HouseApartmentNo,
                    City = contact.City,
                    PostCode = contact.PostCode,
                    StateProvinceRegion = contact.TerritoryCode,
                    County = contact.County,
                    Country = contact.Country,
                    PhoneNumber = contact.PhoneNo,
                    CellPhoneNumber = contact.MobilePhoneNo
                }
            };

            memberContact.Account = new Account(contact.AccountNo);
            memberContact.Account.PointBalance = (long)pointBalance;
            memberContact.Account.Scheme = new Scheme(contact.SchemeCode);
            memberContact.Account.Scheme.Club = new Club(contact.ClubCode);

            if (root.MemberClub != null)
            {
                memberContact.Account.Scheme.Club.Name = root.MemberClub.FirstOrDefault().Description;
            }

            if (root.MembershipCard != null)
            {
                foreach (LSCentral25.MembershipCard3 card in root.MembershipCard)
                {
                    memberContact.Cards.Add(new Card()
                    {
                        Id = card.CardNo,
                        BlockedBy = card.Blockedby,
                        BlockedReason = card.ReasonBlocked,
                        DateBlocked = ConvertTo.SafeJsonDate(card.DateBlocked, IsJson),
                        LinkedToAccount = card.LinkedtoAccount,
                        ClubId = card.ClubCode,
                        ContactId = card.ContactNo,
                        Status = (CardStatus)Convert.ToInt32(card.Status)
                    });
                }
            }

            if (root.MemberAccount != null)
            {
                memberContact.Account.Blocked = root.MemberAccount[0].Blocked;
                memberContact.Account.Status = (AccountStatus)ConvertTo.SafeInt(root.MemberAccount[0].Status);
                memberContact.Account.CustomerId = root.MemberAccount[0].CustomerNo;
                memberContact.Account.Type = (AccountType)ConvertTo.SafeInt(root.MemberAccount[0].AccountType);
            }

            if (root.MemberAttributeList != null)
            {
                foreach (LSCentral25.MemberAttributeList2 attr in root.MemberAttributeList)
                {
                    if (attr.Type != "0" || (attr.AttributeType != "0" && attr.AttributeType != "4"))
                        continue;

                    Profile pro = new Profile()
                    {
                        Id = attr.Code,
                        Description = attr.Description,
                        DataType = (ProfileDataType)Convert.ToInt32(attr.AttributeType)
                    };
                    if (pro.DataType == ProfileDataType.Text)
                        pro.TextValue = attr.Value;
                    else
                        pro.ContactValue = (attr.Value.ToUpper().Equals("YES"));

                    memberContact.Profiles.Add(pro);
                }
            }
            return memberContact;
        }

        public List<PublishedOffer> MapFromRootToPublishedOffers(LSCentral25.RootGetDirectMarketingInfo root)
        {
            List<PublishedOffer> list = new List<PublishedOffer>();
            if (root.PublishedOffer == null)
                return list;

            foreach (LSCentral25.PublishedOffer offer in root.PublishedOffer)
            {
                list.Add(new PublishedOffer()
                {
                    Id = offer.No,
                    Description = offer.PrimaryText,
                    Details = offer.SecondaryText,
                    ExpirationDate = ConvertTo.SafeJsonDate(offer.EndingDate, IsJson),
                    OfferId = offer.DiscountNo,
                    Code = (OfferDiscountType)Convert.ToInt32(offer.DiscountType),
                    Type = (OfferType)Convert.ToInt32(offer.OfferCategory),
                    Images = GetPublishedOfferImages(root.PublishedOfferImages, offer.No),
                    OfferDetails = GetPublishedOfferDetails(root, offer.No),
                    OfferLines = GetPublishedOfferLines(root.PublishedOfferLine, offer.No)
                });
            }
            return list;
        }

        public List<Notification> MapFromRootToNotifications(LSCentral25.RootGetDirectMarketingInfo root)
        {
            List<Notification> list = new List<Notification>();
            if (root.MemberNotification == null)
                return list;

            foreach (var notification in root.MemberNotification)
            {
                list.Add(new Notification()
                {
                    Id = notification.No,
                    ContactId = notification.ContactNo,
                    Description = notification.PrimaryText,
                    Details = notification.SecondaryText,
                    ExpiryDate = ConvertTo.SafeJsonDate(notification.ValidToDate, IsJson),
                    Created = ConvertTo.SafeJsonDate(notification.ValidFromDate, IsJson),
                    Status = NotificationStatus.New,
                    NotificationTextType = NotificationTextType.Plain,
                    Images = GetMemberNotificationImages(root.MemberNotificationImages, notification.No)
                });
            }
            return list;
        }

        public List<Profile> MapFromRootToProfiles(LSCentral25.RootMobileGetProfiles root)
        {
            List<Profile> list = new List<Profile>();
            if (root.MemberAttribute == null)
                return list;

            foreach (var attr in root.MemberAttribute)
            {
                list.Add(new Profile()
                {
                    Id = attr.Code,
                    Description = attr.Description,
                    DataType = (ProfileDataType)Convert.ToInt32(attr.AttributeType),
                    DefaultValue = attr.DefaultValue,
                    Mandatory = attr.Mandatory

                });
            }
            return list;
        }

        #region Private

        private List<ImageView> GetPublishedOfferImages(LSCentral25.PublishedOfferImages[] imgs, string offerId)
        {
            List<ImageView> list = new List<ImageView>();
            if (imgs == null)
                return list;

            foreach (LSCentral25.PublishedOfferImages img in imgs)
            {
                if (img.KeyValue == offerId)
                {
                    list.Add(new ImageView()
                    {
                        Id = img.ImageId,
                        DisplayOrder = img.DisplayOrder
                    });
                }
            }
            return list;
        }

        private List<OfferDetails> GetPublishedOfferDetails(LSCentral25.RootGetDirectMarketingInfo root, string offerId)
        {
            List<OfferDetails> list = new List<OfferDetails>();
            if (root.PublishedOfferDetailLine == null)
                return list;

            foreach (LSCentral25.PublishedOfferDetailLine line in root.PublishedOfferDetailLine)
            {
                if (line.OfferNo == offerId)
                {
                    OfferDetails det = new OfferDetails()
                    {
                        Description = line.Description,
                        LineNumber = line.LineNo.ToString(),
                        OfferId = line.OfferNo
                    };
                    if (root.PublishedOfferDetailLineImages != null)
                        det.Image = new ImageView(root.PublishedOfferDetailLineImages.FirstOrDefault(x => x.KeyValue == offerId)?.ImageId);

                    list.Add(det);
                }
            }
            return list;
        }

        private List<ImageView> GetMemberNotificationImages(LSCentral25.MemberNotificationImages[] imgs, string notificationId)
        {
            List<ImageView> list = new List<ImageView>();
            if (imgs == null)
                return list;

            foreach (LSCentral25.MemberNotificationImages img in imgs)
            {
                if (img.KeyValue == notificationId)
                    list.Add(new ImageView()
                    {
                        Id = img.ImageId,
                        DisplayOrder = img.DisplayOrder
                    });
            }
            return list;
        }

        private List<PublishedOfferLine> GetPublishedOfferLines(LSCentral25.PublishedOfferLine[] lines, string offerId)
        {
            List<PublishedOfferLine> list = new List<PublishedOfferLine>();
            if (lines == null)
                return list;

            foreach (LSCentral25.PublishedOfferLine line in lines.Where(x => x.PublishedOfferNo == offerId))
            {
                OfferDiscountType discountType = (OfferDiscountType)Convert.ToInt32(line.DiscountType);
                list.Add(new PublishedOfferLine()
                {
                    Id = line.DiscountLineId,
                    OfferId = line.PublishedOfferNo,
                    DiscountId = line.DiscountNo,
                    DiscountType = discountType,
                    LineType = GetOfferDiscountLineType(discountType, line.DiscountLineType),
                    Description = line.DiscountLineDescription,
                    LineNo = line.DiscountLineNo,
                    VariantType = (OfferLineVariantType)Convert.ToInt32(line.VariantType),
                    Variant = line.VariantCode,
                    Exclude = line.Exclude,
                    UnitOfMeasure = line.UnitOfMeasure
                });
            }
            return list;
        }

        private OfferDiscountLineType GetOfferDiscountLineType(OfferDiscountType discountType, int discountLineType)
        {
            switch (discountType)
            {
                case OfferDiscountType.Promotion:
                case OfferDiscountType.Deal:
                    {
                        switch (discountLineType)
                        {
                            case 0: return OfferDiscountLineType.Item;
                            case 1: return OfferDiscountLineType.ProductGroup;
                            case 2: return OfferDiscountLineType.ItemCategory;
                            case 3: return OfferDiscountLineType.All;
                            case 4: return OfferDiscountLineType.PLUMenu;
                            case 5: return OfferDiscountLineType.DealModifier;
                            case 6: return OfferDiscountLineType.SpecialGroup;
                        }
                        break;
                    }
                case OfferDiscountType.Coupon:
                    {
                        switch (discountLineType)
                        {
                            case 0: return OfferDiscountLineType.Item;
                            case 1: return OfferDiscountLineType.ProductGroup;
                            case 2: return OfferDiscountLineType.ItemCategory;
                            case 3: return OfferDiscountLineType.SpecialGroup;
                            case 4: return OfferDiscountLineType.All;
                        }
                        break;
                    }
            }
            return (OfferDiscountLineType)discountLineType;
        }

        #endregion
    }
}
