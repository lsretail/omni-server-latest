using System;
using System.Linq;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Mapping
{
    public class ContactMapping : BaseMapping
    {
        public NavWS.RootMemberContactCreate MapToRoot(MemberContact contact)
        {
            Address addr = (contact.Addresses == null || contact.Addresses.Count == 0) ? new Address() : contact.Addresses[0];

            List<NavWS.ContactCreateParameters> member = new List<NavWS.ContactCreateParameters>()
            {
                new NavWS.ContactCreateParameters()
                {
                    AccountID = GetString(contact.Account?.Id),
                    ClubID = GetString(contact.Account?.Scheme?.Club?.Id),

                    FirstName = contact.FirstName,
                    MiddleName = GetString(contact.MiddleName),
                    LastName = contact.LastName,
                    DateOfBirth = contact.BirthDay,
                    Email = contact.Email,
                    Gender = ((int)contact.Gender).ToString(),
                    Phone = GetString(contact.Phone),
                    MobilePhoneNo = GetString(contact.MobilePhone),

                    Address1 = GetString(addr.Address1),
                    Address2 = GetString(addr.Address2),
                    City = GetString(addr.City),
                    Country = GetString(addr.Country),
                    PostCode = GetString(addr.PostCode),
                    StateProvinceRegion = GetString(addr.StateProvinceRegion),

                    LoginID = contact.UserName,
                    Password = contact.Password,
                    DeviceID = contact.LoggedOnToDevice.Id,
                    DeviceFriendlyName = contact.LoggedOnToDevice.DeviceFriendlyName,

                    ContactID = string.Empty,
                    SchemeID = string.Empty,
                    ExternalID = string.Empty,
                    ExternalSystem = string.Empty
                }
            };

            NavWS.RootMemberContactCreate root = new NavWS.RootMemberContactCreate();
            root.ContactCreateParameters = member.ToArray();

            List<NavWS.MemberAttributeValue> attr = new List<NavWS.MemberAttributeValue>();
            foreach (Profile prof in contact.Profiles)
            {
                attr.Add(new NavWS.MemberAttributeValue()
                {
                    AttributeCode = prof.Id,
                    AttributeValue = (prof.ContactValue) ? "Yes" : "No"
                });
            }

            root.MemberAttributeValue = attr.ToArray();
            root.Text = new string[1];
            root.Text[0] = string.Empty;
            return root;
        }

        public NavWS.RootMemberContactCreate1 MapToRoot1(MemberContact contact, string accountId)
        {
            Address addr = (contact.Addresses == null || contact.Addresses.Count == 0) ? new Address() : contact.Addresses[0];

            List<NavWS.ContactCreateParameters1> member = new List<NavWS.ContactCreateParameters1>()
            {
                new NavWS.ContactCreateParameters1()
                {
                    ContactID = contact.Id,
                    AccountID = accountId,

                    FirstName = contact.FirstName,
                    MiddleName = GetString(contact.MiddleName),
                    LastName = contact.LastName,
                    DateOfBirth = contact.BirthDay,
                    Email = contact.Email,
                    Gender = ((int)contact.Gender).ToString(),
                    Phone = GetString(contact.Phone),
                    MobilePhoneNo = GetString(contact.MobilePhone),

                    Address1 = GetString(addr.Address1),
                    Address2 = GetString(addr.Address2),
                    City = GetString(addr.City),
                    Country = GetString(addr.Country),
                    PostCode = GetString(addr.PostCode),
                    StateProvinceRegion = GetString(addr.StateProvinceRegion),

                    ExternalID = string.Empty,
                    ExternalSystem = string.Empty
                }
            };

            List<NavWS.MemberAttributeValue1> attr = new List<NavWS.MemberAttributeValue1>();
            foreach (Profile prof in contact.Profiles)
            {
                attr.Add(new NavWS.MemberAttributeValue1()
                {
                    AttributeCode = prof.Id,
                    AttributeValue = (prof.ContactValue) ? "Yes" : "No"
                });
            }

            NavWS.RootMemberContactCreate1 root = new NavWS.RootMemberContactCreate1();
            root.ContactCreateParameters = member.ToArray();
            root.MemberAttributeValue = attr.ToArray();
            root.Text = new string[1];
            root.Text[0] = string.Empty;
            return root;
        }

        public MemberContact MapFromRootToContact(NavWS.RootGetMemberContact root)
        {
            NavWS.MemberContact contact = root.MemberContact.FirstOrDefault();
            MemberContact memberContact = new MemberContact()
            {
                Id = contact.ContactNo,
                Email = contact.EMail,
                FirstName = contact.FirstName,
                MiddleName = contact.MiddleName,
                LastName = contact.Surname,
                Gender =  (Gender)Convert.ToInt32(contact.Gender),
                MaritalStatus = (MaritalStatus)Convert.ToInt32(contact.MaritalStatus),
                Phone = contact.PhoneNo,
                MobilePhone = contact.MobilePhoneNo,
                BirthDay = contact.DateofBirth
            };

            memberContact.Addresses = new List<Address>();
            memberContact.Addresses.Add(new Address()
            {
                Type = AddressType.Residential,
                Address1 = contact.Address,
                Address2 = contact.Address2,
                City = contact.City,
                PostCode = contact.PostCode,
                StateProvinceRegion = contact.TerritoryCode,
                Country = contact.Country
            });

            memberContact.Account = new Account(contact.AccountNo);
            memberContact.Account.Scheme = new Scheme(contact.SchemeCode);
            memberContact.Account.Scheme.Club = new Club(contact.ClubCode);

            NavWS.MembershipCard1 card = root.MembershipCard.FirstOrDefault();
            memberContact.Card = new Card()
            {
                Id = card.CardNo,
                BlockedBy = card.Blockedby,
                BlockedReason = card.ReasonBlocked,
                DateBlocked = card.DateBlocked,
                LinkedToAccount = card.LinkedtoAccount,
                Status = (CardStatus)Convert.ToInt32(card.Status)
            };
            return memberContact;
        }

        public List<LoyTransaction> MapFromRootToSalesEntries(NavWS.RootGetMemberSalesHistory root)
        {
            List<LoyTransaction> list = new List<LoyTransaction>();
            if (root.MemberSalesEntry == null)
                return null;

            foreach (NavWS.MemberSalesEntry trans in root.MemberSalesEntry)
            {
                LoyTransaction tr = new LoyTransaction()
                {
                    Id = trans.EntryNo.ToString(),
                    Terminal = trans.POSTerminalNo,
                    Date = trans.Date,
                    Amt = trans.GrossAmount,
                    TotalQty = trans.Quantity,
                    DocumentType = (EntryDocumentType)Convert.ToInt32(trans.SourceType),
                    DocumentNumber = trans.DocumentNo
                };

                if (trans.SourceType == "0")
                {
                    tr.ReceiptNumber = trans.DocumentNo;
                }

                tr.Store = new Store(trans.StoreNo)
                {
                    Description = trans.StoreName
                };
                list.Add(tr);
            }
            return list;
        }

        public List<PublishedOffer> MapFromRootToPublishedOffers(NavWS.RootGetDirectMarketingInfo root)
        {
            List<PublishedOffer> list = new List<PublishedOffer>();
            if (root.PublishedOffer == null)
                return list;

            foreach (NavWS.PublishedOffer offer in root.PublishedOffer)
            {
                list.Add(new PublishedOffer()
                {
                    Id = offer.No,
                    Description = offer.PrimaryText,
                    Details = offer.SecondaryText,
                    ExpirationDate = GetSQLNAVDate(offer.EndingDate),
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

        public List<Notification> MapFromRootToNotifications(NavWS.RootGetDirectMarketingInfo root)
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
                    ExpiryDate = GetSQLNAVDate(notification.ValidToDate),
                    Created = GetSQLNAVDate(notification.ValidFromDate),
                    Status = NotificationStatus.New,
                    QRText = string.Empty,
                    NotificationTextType = NotificationTextType.Plain,
                    Images = GetMemberNotificationImages(root.MemberNotificationImages, notification.No)
                });
            }
            return list;
        }

        #region Private

        private List<ImageView> GetPublishedOfferImages(NavWS.PublishedOfferImages[] imgs, string offerId)
        {
            List<ImageView> list = new List<ImageView>();
            foreach (NavWS.PublishedOfferImages img in imgs)
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

        private List<OfferDetails> GetPublishedOfferDetails(NavWS.RootGetDirectMarketingInfo root, string offerId)
        {
            List<OfferDetails> list = new List<OfferDetails>();
            if (root.PublishedOfferDetailLine == null)
                return list;

            foreach (NavWS.PublishedOfferDetailLine line in root.PublishedOfferDetailLine)
            {
                if (line.OfferNo == offerId)
                {
                    list.Add(new OfferDetails()
                    {
                        Description = line.Description,
                        LineNumber = line.LineNo.ToString(),
                        Image = new ImageView(root.PublishedOfferDetailLineImages.FirstOrDefault(x => x.KeyValue == offerId)?.ImageId),
                        OfferId = line.OfferNo
                    });
                }
            }
            return list;
        }

        private List<ImageView> GetMemberNotificationImages(NavWS.MemberNotificationImages[] imgs, string notificationId)
        {
            List<ImageView> list = new List<ImageView>();

            foreach (NavWS.MemberNotificationImages img in imgs)
            {
                if(img.KeyValue == notificationId)
                    list.Add(new ImageView()
                    {
                        Id = img.ImageId,
                        DisplayOrder = img.DisplayOrder
                    });
            }

            return list;
        }

        private List<PublishedOfferLine> GetPublishedOfferLines(NavWS.PublishedOfferLine[] lines, string offerId)
        {
            List<PublishedOfferLine> list = new List<PublishedOfferLine>();
            if (lines == null)
                return list;

            foreach (NavWS.PublishedOfferLine line in lines.Where(x => x.PublishedOfferNo == offerId))
            {
                OfferDiscountType discountType = (OfferDiscountType) Convert.ToInt32(line.DiscountType);
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
