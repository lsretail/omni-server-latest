using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;

namespace LSOmni.DataAccess.BOConnection.NavCommon.Mapping
{
    public class ActivityMapping : BaseMapping
    {
        public ActivityMapping(bool json)
        {
            IsJson = json;
        }

        public List<ActivityProduct> MapRootToActivityProducts(LSActivity15.ActivityUploadProducts root)
        {
            List<ActivityProduct> list = new List<ActivityProduct>();
            if (root.ActivityProducts == null)
                return list;

            foreach (LSActivity15.ActivityProducts rec in root.ActivityProducts)
            {
                list.Add(new ActivityProduct()
                {
                    Id = rec.ProductNo,
                    Description = rec.Description,
                    ActivityType = rec.ActivityType,
                    RetailItem = rec.RetailItem,
                    DefaultQty = rec.DefaultQty,
                    PricedPerPerson = rec.PricedPerPerson,
                    AllowQuantityChange = rec.AllowQuantityChange,
                    QuantityCaption = string.Concat(rec.QuantityCaption),
                    AllowNoOfPersonChange = rec.AllowNoOfPersonChange,
                    MinQty = rec.MinQty,
                    MaxQty = rec.MaxQty,
                    MinPersons = rec.MinPersons,
                    MaxPersons = rec.MaxPersons,
                    PaymentRequired = string.Concat(rec.PaymentRequired),
                    DefaultUnitPrice = rec.DefaultUnitPrice,
                    PriceCurrency = string.Concat(rec.PriceCurrency),
                    CancelPolicy = rec.CancelPolicy,
                    CancelPolicyDescription = string.Concat(rec.CancelPolicyDescription),
                    ProductType = rec.ProductType,
                    FixedLocation = rec.FixedLocation
                });
            }
            return list;
        }

        public List<ActivityType> MapRootToActivityType(LSActivity15.ActivityUploadTypes root)
        {
            List<ActivityType> list = new List<ActivityType>();
            if (root.ActivityTypes == null)
                return list;

            foreach (LSActivity15.ActivityTypes rec in root.ActivityTypes)
            {
                list.Add(new ActivityType()
                {
                    Id = rec.ActivityCode,
                    Description = rec.Description
                });
            }
            return list;
        }

        public List<ActivityLocation> MapRootToActivityLocation(LSActivity15.ActivityUploadLocations root)
        {
            List<ActivityLocation> list = new List<ActivityLocation>();
            if (root.ActivityLocations == null)
                return list;

            foreach (LSActivity15.ActivityLocations rec in root.ActivityLocations)
            {
                list.Add(new ActivityLocation()
                {
                    Id = rec.Code,
                    Description = rec.Description,
                    RetailStore = rec.RetailStore
                });
            }
            return list;
        }

        public List<Booking> MapRootToReservations(LSActivity15.ActivityUploadReservations root)
        {
            List<Booking> list = new List<Booking>();
            if (root.Activities == null)
                return list;

            foreach (LSActivity15.Activities rec in root.Activities)
            {
                list.Add(new Booking()
                {
                    ActivityNo = rec.ActivityNo,
                    ItemNo = rec.ProductNo,
                    Description = rec.Description,
                    DateFrom = ConvertTo.SafeJsonDate(rec.DateFrom, IsJson),
                    DateTo = ConvertTo.SafeJsonDate(rec.DateTo, IsJson),
                    TimeFrom = ConvertTo.SafeJsonDate(rec.TimeFrom, IsJson),
                    TimeTo = ConvertTo.SafeJsonDate(rec.TimeTo, IsJson),
                    Quantity = rec.Quantity,
                    UnitPrice = rec.UnitPrice,
                    LineDiscountAmount = rec.LineDiscountAmount,
                    LineDiscountPercentage = rec.LineDiscountPercentage,
                    TotalAmount = rec.TotalAmount,
                    Status = rec.Status,
                    PaymentStatus = rec.PaymentStatus,
                    Location = rec.Location,
                    ClientNo = rec.ClientNo,
                    ClientName = rec.ClientName,
                    NoOfPersons = rec.NoOfPersons,
                    Comment = rec.Comment,
                    AllowanceNo = rec.AllowanceNo,
                    PriceCurrency = string.Concat(rec.PriceCurrency),
                    CancelPolicy = rec.CancelPolicy,
                    CancelPolicyDescription = string.Concat(rec.CancelPolicyDescription),
                    CancelAmount = rec.CancelAmount
                });
            }
            return list;
        }

        public List<ResHeader> MapRootToResHeader(LSActivity15.ActivityUploadResHeaders root)
        {
            List<ResHeader> list = new List<ResHeader>();
            if (root.Reservations == null)
                return list;

            foreach (LSActivity15.Reservations rec in root.Reservations)
            {
                list.Add(new ResHeader()
                {
                    Id = rec.ReservationNo,
                    Description = rec.Description,
                    ContactNo = rec.ClientNo,
                    ContactName = rec.ClientName,
                    DateFrom = ConvertTo.SafeJsonDate(rec.FromDate, IsJson),
                    DateTo = ConvertTo.SafeJsonDate(rec.ToDate, IsJson),
                    TimeFrom = ConvertTo.SafeJsonDate(rec.TimeFrom, IsJson),
                    TimeTo = ConvertTo.SafeJsonDate(rec.TimeTo, IsJson),
                    Status = rec.Status,
                    PaymentStatus = rec.PaymentStatus,
                    InternalStatus = rec.InternalStatus,
                    Location = rec.Location,
                    NoOfPersons = rec.NoPersons,
                    Comment = rec.Comment,
                    EMail = rec.Email,
                    Balance = string.Concat(rec.Balance),
                    CustomerAccount = rec.CustomerAccount,
                    DepositsBalance = rec.DepositsBalance,
                    GroupNo = rec.GroupNo,
                    InternalContact = rec.InternalContact,
                    Language = rec.Language,
                    MobileNo = rec.Mobile,
                    POSSale = rec.POSsales,
                    Reference = rec.Reference,
                    ReservationType = rec.ReservationType,
                    TotalActivityCharges = rec.TotalActivitiesAmount,
                    TotalAdditionalCharges = rec.TotalAdditionalAmount
                });
            }
            return list;
        }

        public List<Promotion> MapRootToPromotions(LSActivity15.ActivityUploadPromotions root)
        {
            List<Promotion> list = new List<Promotion>();
            if (root.ActivityPromotions == null)
                return list;

            foreach (LSActivity15.ActivityPromotions rec in root.ActivityPromotions)
            {
                list.Add(new Promotion()
                {
                    ItemNo = rec.ProductNo,
                    DateFrom = ConvertTo.SafeJsonDate(rec.DateFrom, IsJson),
                    DateTo = ConvertTo.SafeJsonDate(rec.DateTo, IsJson),
                    TimeFrom = ConvertTo.SafeJsonDate(rec.TimeFrom, IsJson),
                    TimeTo = ConvertTo.SafeJsonDate(rec.TimeTo, IsJson),
                    ClubMembersOnly = rec.ClubMembersOnly,
                    IsPriceOrDiscount = rec.ClubMembersOnly,
                    PriceOrDiscountValue = rec.PriceOrDiscountValue,
                    DaySetting = rec.DaySetting,
                    PriceDescription = rec.PriceDescription,
                    Location = rec.Location,
                    ProductName = rec.ProductName,
                    PriceCurrency = string.Concat(rec.PriceCurrency)
                });
            }
            return list;
        }

        public List<Allowance> MapRootToAllowances(LSActivity15.ActivityUploadAllowance root)
        {
            List<Allowance> list = new List<Allowance>();
            if (root.IssuedAllowance == null)
                return list;

            foreach (LSActivity15.IssuedAllowance rec in root.IssuedAllowance)
            {
                list.Add(new Allowance()
                {
                    Id = rec.AllowanceNo,
                    ItemNo = rec.ProductNo,
                    ValidLocation = rec.ValidLocation,
                    Description = rec.Description,
                    QtyIssued = rec.QtyIssued,
                    DateIssued = ConvertTo.SafeJsonDate(rec.DateIssued, IsJson),
                    UnitPrice = rec.UnitPrice,
                    ClientNo = rec.ClientNo,
                    ClientName = rec.ClientName,
                    ExpiryDate = ConvertTo.SafeJsonDate(rec.ExpiryDate, IsJson),
                    QuantityConsumed = rec.QuantityConsumed,
                    IssuedTotalAmount = rec.IssuedTotalAmount
                });
            }
            return list;
        }

        public List<CustomerEntry> MapRootToCustomerEntry(LSActivity15.ActivityCustomerEntries root)
        {
            List<CustomerEntry> list = new List<CustomerEntry>();
            if (root.CustomerEntries == null)
                return list;

            foreach (LSActivity15.CustomerEntries rec in root.CustomerEntries)
            {
                list.Add(new CustomerEntry()
                {
                    Id = rec.EntryNo.ToString(),
                    CustomerNo = rec.CustomerNo,
                    PostingDate = ConvertTo.SafeJsonDate(rec.PostingDate, IsJson),
                    DocumentType = rec.DocumentType,
                    DocumentNo = rec.DocumentNo,
                    Description = rec.Description,
                    Currency = rec.Currency,
                    Amount = rec.Amount,
                    AmountLCY = rec.AmountLCY,
                    ExternalRef = rec.ExternalRef,
                    ContactNo = rec.MemberNo
                });
            }
            return list;
        }

        public List<MemberProduct> MapRootToMemberProduct(LSActivity.ActivityMembershipProducts root)
        {
            List<MemberProduct> list = new List<MemberProduct>();
            if (root.MembershipTypes == null)
                return list;

            foreach (LSActivity.MembershipTypes rec in root.MembershipTypes)
            {
                list.Add(new MemberProduct()
                {
                    Id = rec.Code,
                    Description = rec.Description,
                    ChargeType = rec.ChargeType,
                    AccessType = rec.AccessType,
                    Status = rec.Status,
                    ExpirationCalculation = rec.ExpirationCalculation,
                    CommitmentPeriod = rec.CommitmentPeriod,
                    NoOfTimes = rec.NoOfTimes,
                    RetailItem = rec.RetailItem,
                    Price = rec.Price,
                    MinAge = rec.MinAge,
                    MaxAge = rec.MaxAge,
                    FixedEndDate = ConvertTo.SafeJsonDate(rec.FixedEndDate, IsJson),
                    FixedIssueDate = ConvertTo.SafeJsonDate(rec.FixedIssueDate, IsJson),
                    RequiresMemberShip = rec.RequiresMemberShip,
                    SellingOption = rec.SellingOption,
                    EntryType = rec.EntryType,
                    SubscriptionType = rec.SubscriptionType,
                    MemberClub = rec.MemberClub,
                    PricingUpdate = rec.PricingUpdate,
                    PriceCommitmentPeriod = rec.PriceCommitmentPeriod
                });
            }
            return list;
        }

        public List<MemberProduct> MapRootToMemberProduct(LSActivity15.ActivityMembershipProducts root)
        {
            List<MemberProduct> list = new List<MemberProduct>();
            if (root.MembershipTypes == null)
                return list;

            foreach (LSActivity15.MembershipTypes rec in root.MembershipTypes)
            {
                list.Add(new MemberProduct()
                {
                    Id = rec.Code,
                    Description = rec.Description,
                    ChargeType = rec.ChargeType,
                    AccessType = rec.AccessType,
                    Status = rec.Status,
                    ExpirationCalculation = rec.ExpirationCalculation,
                    CommitmentPeriod = rec.CommitmentPeriod,
                    NoOfTimes = rec.NoOfTimes,
                    RetailItem = rec.RetailItem,
                    Price = rec.Price,
                    MinAge = rec.MinAge,
                    MaxAge = rec.MaxAge,
                    FixedEndDate = ConvertTo.SafeJsonDate(rec.FixedEndDate, IsJson),
                    FixedIssueDate = ConvertTo.SafeJsonDate(rec.FixedIssueDate, IsJson),
                    RequiresMemberShip = rec.RequiresMemberShip,
                    SellingOption = rec.SellingOption,
                    EntryType = rec.EntryType,
                    SubscriptionType = rec.SubscriptionType,
                    MemberClub = rec.MemberClub,
                    PricingUpdate = rec.PricingUpdate,
                    PriceCommitmentPeriod = rec.PriceCommitmentPeriod
                });
            }
            return list;
        }

        public List<SubscriptionEntry> MapRootToSubscriptionEntry(LSActivity15.ActivitySubscriptionEntries root)
        {
            List<SubscriptionEntry> list = new List<SubscriptionEntry>();
            if (root.MembershipBatchLines == null)
                return list;

            foreach (LSActivity15.MembershipBatchLines rec in root.MembershipBatchLines)
            {
                list.Add(new SubscriptionEntry()
                {
                    ContactNo = rec.MemberNo,
                    Price = rec.Price,
                    Discount = rec.Discount,
                    Quantity = rec.Qty,
                    Amount = rec.Amount,
                    ProductType = rec.ProductType,
                    ItemNo = rec.ProductNo,
                    Description = rec.Description,
                    Comment = rec.Comment,
                    InvoiceNo = rec.InvoiceNo,
                    MembershipNo = rec.MembershipNo,
                    PostingDate = ConvertTo.SafeJsonDate(rec.PostingDate, IsJson),
                    AdditionalCharges = rec.AdditionalCharges,
                    MembershipType = rec.MembershipType,
                    PeriodFrom = rec.PeriodFrom,
                    PeriodTo = rec.PeriodTo
                });
            }
            return list;
        }

        public List<AdmissionEntry> MapRootToAdmissionEntry(LSActivity15.ActivityAdmissionEntries root)
        {
            List<AdmissionEntry> list = new List<AdmissionEntry>();
            if (root.AdmissionEntries == null)
                return list;

            foreach (LSActivity15.AdmissionEntries rec in root.AdmissionEntries)
            {
                list.Add(new AdmissionEntry()
                {
                    LocationNo = rec.LocationNo,
                    GateNo = rec.GateNo,
                    ContactNo = rec.MemberNo,
                    MembershipNo = rec.MembershipNo,
                    LineNo = rec.LineNo,
                    EntryTime = ConvertTo.NavJoinDateAndTime(rec.Date, rec.Time),
                    MemberType = rec.MemberType,
                    ProductName = rec.ProductName,
                    Type = rec.Type,
                    Name = rec.Name
                });
            }
            return list;
        }

        public List<Membership> MapRootToMemberships(LSActivity.ActivityUploadMemberships root)
        {
            List<Membership> list = new List<Membership>();
            if (root.MembershipEntries == null)
                return list;

            foreach (LSActivity.MembershipEntries rec in root.MembershipEntries)
            {
                list.Add(new Membership()
                {
                    Id = rec.MembershipNo,
                    ContactNo = rec.MemberNo,
                    MemberName = rec.MemberName,
                    MembershipType = rec.MembershipType,
                    MembershipDescription = rec.MembershipDescription,
                    DateIssued = ConvertTo.SafeJsonDate(rec.DateIssued, IsJson),
                    DateCreated = ConvertTo.SafeJsonDate(rec.DateCreated, IsJson),
                    DateExpires = ConvertTo.SafeJsonDate(rec.DateExpires, IsJson),
                    LastProcessBatch = rec.LastProcessBatch,
                    Status = rec.Status,
                    UnitPrice = rec.UnitPrice,
                    Discount = rec.Discount,
                    Amount = rec.Amount,
                    DiscountReasonCode = rec.DiscountReasonCode, 
                    AccessID = rec.AccessID,
                    SalesPersonCode = rec.SalesPersonCode,
                    EntryType = rec.EntryType,
                    NoOfVisits = rec.NoofVisits,
                    ChargeTo = rec.ChargeTo,
                    ChargeToName = rec.ChargeToName,
                    StatusCode = rec.StatusCode,
                    PaymentMethodCode = rec.PaymentMethodCode,

                    CommitmentDate = ConvertTo.SafeJsonDate(rec.CommitmentDate, IsJson),
                    DateLastProcessed = ConvertTo.SafeJsonDate(rec.DateLastProcessed, IsJson),
                    StatusDate = ConvertTo.SafeJsonDate(rec.StatusDate, IsJson),
                    OnHoldUntil = ConvertTo.SafeJsonDate(rec.OnHoldUntil, IsJson),
                    AccessFrom = ConvertTo.SafeJsonDate(rec.AccessFrom, IsJson),
                    AccessUntil = ConvertTo.SafeJsonDate(rec.AccessUntil, IsJson),
                    PriceCommitmentExpires = ConvertTo.SafeJsonDate(rec.PriceCommitmentExpires, IsJson)
                });
            }
            return list;
        }

        public List<Membership> MapRootToMemberships(LSActivity15.ActivityUploadMemberships root)
        {
            List<Membership> list = new List<Membership>();
            if (root.MembershipEntries == null)
                return list;

            foreach (LSActivity15.MembershipEntries rec in root.MembershipEntries)
            {
                list.Add(new Membership()
                {
                    Id = rec.MembershipNo,
                    ContactNo = rec.MemberNo,
                    MemberName = rec.MemberName,
                    MembershipType = rec.MembershipType,
                    MembershipDescription = rec.MembershipDescription,
                    DateIssued = ConvertTo.SafeJsonDate(rec.DateIssued, IsJson),
                    DateCreated = ConvertTo.SafeJsonDate(rec.DateCreated, IsJson),
                    DateExpires = ConvertTo.SafeJsonDate(rec.DateExpires, IsJson),
                    LastProcessBatch = rec.LastProcessBatch,
                    Status = rec.Status,
                    UnitPrice = rec.UnitPrice,
                    Discount = rec.Discount,
                    Amount = rec.Amount,
                    DiscountReasonCode = rec.DiscountReasonCode, 
                    AccessID = rec.AccessID,
                    SalesPersonCode = rec.SalesPersonCode,
                    EntryType = rec.EntryType,
                    NoOfVisits = rec.NoofVisits,
                    ChargeTo = rec.ChargeTo,
                    ChargeToName = rec.ChargeToName,
                    StatusCode = rec.StatusCode,
                    PaymentMethodCode = rec.PaymentMethodCode,

                    CommitmentDate = ConvertTo.SafeJsonDate(rec.CommitmentDate, IsJson),
                    DateLastProcessed = ConvertTo.SafeJsonDate(rec.DateLastProcessed, IsJson),
                    StatusDate = ConvertTo.SafeJsonDate(rec.StatusDate, IsJson),
                    OnHoldUntil = ConvertTo.SafeJsonDate(rec.OnHoldUntil, IsJson),
                    AccessFrom = ConvertTo.SafeJsonDate(rec.AccessFrom, IsJson),
                    AccessUntil = ConvertTo.SafeJsonDate(rec.AccessUntil, IsJson),
                    PriceCommitmentExpires = ConvertTo.SafeJsonDate(rec.PriceCommitmentExpires, IsJson)
                });
            }
            return list;
        }

        public List<AvailabilityResponse> MapRootToAvailabilityResponse(LSActivity15.ActivityAvailabilityResponse root)
        {
            if (root.AvailabilityWork == null || root.AvailabilityWork.Length == 0)
                return new List<AvailabilityResponse>();

            List<AvailabilityResponse> list = new List<AvailabilityResponse>();
            foreach (LSActivity15.AvailabilityWork rec in root.AvailabilityWork)
            {
                list.Add(new AvailabilityResponse()
                {
                    ItemNo = rec.ProductNo,
                    AvailDate = ConvertTo.SafeJsonDate(rec.AvailDate, IsJson),
                    AvailTime = ConvertTo.NavJoinDateAndTime(rec.AvailDate, rec.AvailTime),
                    WeekDay = rec.WeekDay,
                    Availability = rec.Availability,
                    TimeCaption = rec.TimeCaption,
                    Location = rec.Location,
                    Price = rec.Price,
                    OptionalResourceName = string.Concat(rec.OptionalResourceName),
                    OptionalResourceNo = rec.OptionalResourceNo,
                    PriceCurrency = string.Concat(rec.PriceCurrency),
                    Comment = rec.Comment
                });
            }
            return list;
        }

        public AdditionalCharge MapRootToAdditionalCharge(LSActivity15.ActivityChargeRespond root)
        {
            if (root.ChargeLines == null || root.ChargeLines.Length == 0)
                return new AdditionalCharge();

            LSActivity15.ChargeLines rec = root.ChargeLines[0];

            return new AdditionalCharge()
            {
                ActivityNo = string.Concat(rec.ActivityNo),
                LineNo = rec.LineNo,
                ItemNo = rec.ItemNo,
                Quantity = rec.Qty,
                Price = rec.Price,
                DiscountPercentage = rec.DiscountPercentage,
                TotalAmount = rec.Total,
                Optional = rec.Optional,
                UnitOfMeasure = rec.Uom,
                InvoiceReference = string.Concat(rec.InvoiceReference),
                ProductType = (rec.ProductType.Equals("Item")) ? ProductChargeType.Item : ProductChargeType.Deal
            };
        }

        public AttributeResponse MapRootToAttributeResponse(LSActivity15.ActivityAttributeRespond root)
        {
            if (root.AttributeLines == null || root.AttributeLines.Length == 0)
                return new AttributeResponse();

            LSActivity15.AttributeLines rec = root.AttributeLines[0];

            return new AttributeResponse()
            {
                LinkField = rec.LinkField,
                AttributeCode = rec.AttributeCode,
                AttributeValue = rec.AttributeValue,
                AttributeValueType = rec.AttributeValueType,
                Sequence = rec.Sequence
            };
        }
    }
}
