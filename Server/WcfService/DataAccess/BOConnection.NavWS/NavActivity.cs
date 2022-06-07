using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Base;
using System;
using System.Collections.Generic;

namespace LSOmni.DataAccess.BOConnection.NavWS
{
    public class NavActivity : NavBase, IActivityBO
    {
        public NavActivity(BOConfiguration config) : base(config)
        {
        }

        #region Functions

        public virtual ActivityResponse ActivityConfirm(ActivityRequest request)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityConfirm(request);

            return LSCWSBase.ActivityConfirm(request);
        }

        public virtual ActivityResponse ActivityCancel(string activityNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityCancel(activityNo);

            return LSCWSBase.ActivityCancel(activityNo);
        }

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, contactAccount, optionalResource, promoCode, activityNo, noOfPersons);

            return LSCWSBase.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, contactAccount, optionalResource, promoCode, activityNo, noOfPersons);
        }

        public virtual AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAdditionalChargesGet(activityNo);

            return LSCWSBase.ActivityAdditionalChargesGet(activityNo);
        }

        public virtual AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);

            return LSCWSBase.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAdditionalChargesSet(request);

            return LSCWSBase.ActivityAdditionalChargesSet(request);
        }

        public virtual AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAttributesGet(type, linkNo);

            return LSCWSBase.ActivityAttributesGet(type, linkNo);
        }

        public virtual int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAttributeSet(type, linkNo, attributeCode, attributeValue);

            return LSCWSBase.ActivityAttributeSet(type, linkNo, attributeCode, attributeValue);
        }

        public virtual string ActivityReservationInsert(Reservation request)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityInsertReservation(request);

            return LSCWSBase.ActivityInsertReservation(request);
        }

        public virtual string ActivityReservationUpdate(Reservation request)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityUpdateReservation(request);

            return LSCWSBase.ActivityUpdateReservation(request);
        }

        public virtual MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityMembershipSell(contactNo, membersShipType);

            return LSCWSBase.ActivityMembershipSell(contactNo, membersShipType);
        }

        public virtual bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityMembershipCancel(contactNo, memberShipNo, comment);

            return LSCWSBase.ActivityMembershipCancel(contactNo, memberShipNo, comment);
        }

        public virtual List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays)
        {
            if (NAVVersion < new Version("17.5"))
                return new List<AvailabilityResponse>();

            return LSCWSBase.ActivityResourceAvailabilityGet(locationNo, activityDate, resourceNo, intervalType, noOfDays);
        }

        public virtual List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays)
        {
            if (NAVVersion < new Version("17.5"))
                return new List<AvailabilityResponse>();

            return LSCWSBase.ActivityResourceGroupAvailabilityGet(locationNo, activityDate, groupNo, intervalType, noOfDays);
        }

        public virtual bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString)
        {
            if (NAVVersion < new Version("17.5"))
            {
                messageString = "Not Supported";
                return false;
            }
            return LSCWSBase.ActivityCheckAccess(searchReference, locationNo, gateNo, registerAccessEntry, checkType, out messageString);
        }

        public virtual string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activiyTime, string optionalResource, int quantity)
        {
            if (NAVVersion < new Version("17.5"))
                return "Not Supported";

            return LSCWSBase.ActivityGetAvailabilityToken(locationNo, productNo, activiyTime, optionalResource, quantity);
        }

        public virtual string ActivityInsertGroupReservation(Reservation request)
        {
            if (NAVVersion < new Version("17.5"))
                return "Not Supported";
            
            return LSCWSBase.ActivityInsertGroupReservation(request);
        }

        public virtual string ActivityUpdateGroupReservation(Reservation request)
        {
            if (NAVVersion < new Version("17.5"))
                return "Not Supported";

            return LSCWSBase.ActivityUpdateGroupReservation(request);
        }

        public virtual ActivityResponse ActivityConfirmGroup(ActivityRequest request)
        {
            if (NAVVersion < new Version("17.5"))
                return new ActivityResponse();

            return LSCWSBase.ActivityConfirmGroup(request);
        }

        public virtual bool ActivityDeleteGroup(string groupNo, int lineNo)
        {
            if (NAVVersion < new Version("17.5"))
                return false;

            return LSCWSBase.ActivityDeleteGroup(groupNo, lineNo);
        }

        #endregion

        #region Data Get (Replication)

        public virtual List<ActivityProduct> ActivityProductsGet()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityProductsGet();

            return LSCWSBase.ActivityProductsGet();
        }

        public virtual List<ActivityType> ActivityTypesGet()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityTypesGet();

            return LSCWSBase.ActivityTypesGet();
        }

        public virtual List<ActivityLocation> ActivityLocationsGet()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityLocationsGet();

            return LSCWSBase.ActivityLocationsGet();
        }

        public virtual List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityReservationsGet(reservationNo, contactNo, activityType);

            return LSCWSBase.ActivityReservationsGet(reservationNo, contactNo, activityType);
        }

        public virtual List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityReservationsHeaderGet(reservationNo, reservationType, status, locationNo, fromDate);

            return LSCWSBase.ActivityReservationsHeaderGet(reservationNo, reservationType, status, locationNo, fromDate);
        }

        public virtual List<Promotion> ActivityPromotionsGet()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityPromotionsGet();

            return LSCWSBase.ActivityPromotionsGet();
        }

        public virtual List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAllowancesGet(contactNo);

            return LSCWSBase.ActivityAllowancesGet(contactNo);
        }

        public virtual List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityCustomerEntriesGet(contactNo, customerNo);

            return LSCWSBase.ActivityCustomerEntriesGet(contactNo, customerNo);
        }

        public virtual List<MemberProduct> ActivityMembershipProductsGet()
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityMembershipProductsGet();

            return LSCWSBase.ActivityMembershipProductsGet();
        }

        public virtual List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivitySubscriptionChargesGet(contactNo);

            return LSCWSBase.ActivitySubscriptionChargesGet(contactNo);
        }

        public virtual List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityAdmissionEntriesGet(contactNo);

            return LSCWSBase.ActivityAdmissionEntriesGet(contactNo);
        }

        public virtual List<Membership> ActivityMembershipsGet(string contactNo)
        {
            if (NAVVersion < new Version("17.5"))
                return NavWSBase.ActivityMembershipsGet(contactNo);

            return LSCWSBase.ActivityMembershipsGet(contactNo);
        }

        public virtual List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate)
        {
            if (NAVVersion < new Version("17.5"))
                return new List<Booking>();

            return LSCWSBase.ActivityGetByResource(locationNo, resourceNo, fromDate, toDate);
        }

        public virtual List<ActivityResource> ActivityResourceGet()
        {
            if (NAVVersion < new Version("17.5"))
                return new List<ActivityResource>();

            return LSCWSBase.ActivityResourceGet();
        }

        #endregion
    }
}
