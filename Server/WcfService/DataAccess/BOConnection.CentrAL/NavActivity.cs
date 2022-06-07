using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentrAL
{
    public class NavActivity : NavBase, IActivityBO
    {
        public NavActivity(BOConfiguration config) : base(config)
        {
        }

        #region Functions

        public virtual ActivityResponse ActivityConfirm(ActivityRequest request)
        {
            return NavWSBase.ActivityConfirm(request);
        }

        public virtual ActivityResponse ActivityCancel(string activityNo)
        {
            return NavWSBase.ActivityCancel(activityNo);
        }

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons)
        {
            return NavWSBase.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, contactAccount, optionalResource, promoCode, activityNo, noOfPersons);
        }

        public virtual AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            return NavWSBase.ActivityAdditionalChargesGet(activityNo);
        }

        public virtual AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            return NavWSBase.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            return NavWSBase.ActivityAdditionalChargesSet(request);
        }

        public virtual AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
        {
            return NavWSBase.ActivityAttributesGet(type, linkNo);
        }

        public virtual int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue)
        {
            return NavWSBase.ActivityAttributeSet(type, linkNo, attributeCode, attributeValue);
        }

        public virtual string ActivityReservationInsert(Reservation request)
        {
            return NavWSBase.ActivityInsertReservation(request);
        }

        public virtual string ActivityReservationUpdate(Reservation request)
        {
            return NavWSBase.ActivityUpdateReservation(request);
        }

        public virtual MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType)
        {
            return NavWSBase.ActivityMembershipSell(contactNo, membersShipType);
        }

        public virtual bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment)
        {
            return NavWSBase.ActivityMembershipCancel(contactNo, memberShipNo, comment);
        }

        public virtual List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activiyTime, string optionalResource, int quantity)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual string ActivityInsertGroupReservation(Reservation request)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual string ActivityUpdateGroupReservation(Reservation request)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual ActivityResponse ActivityConfirmGroup(ActivityRequest request)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual bool ActivityDeleteGroup(string groupNo, int lineNo)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        #endregion

        #region Data Get (Replication)

        public virtual List<ActivityProduct> ActivityProductsGet()
        {
            return NavWSBase.ActivityProductsGet();
        }

        public virtual List<ActivityType> ActivityTypesGet()
        {
            return NavWSBase.ActivityTypesGet();
        }

        public virtual List<ActivityLocation> ActivityLocationsGet()
        {
            return NavWSBase.ActivityLocationsGet();
        }

        public virtual List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            return NavWSBase.ActivityReservationsGet(reservationNo, contactNo, activityType);
        }

        public virtual List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            return NavWSBase.ActivityReservationsHeaderGet(reservationNo, reservationType, status, locationNo, fromDate);
        }

        public virtual List<Promotion> ActivityPromotionsGet()
        {
            return NavWSBase.ActivityPromotionsGet();
        }

        public virtual List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            return NavWSBase.ActivityAllowancesGet(contactNo);
        }

        public virtual List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            return NavWSBase.ActivityCustomerEntriesGet(contactNo, customerNo);
        }

        public virtual List<MemberProduct> ActivityMembershipProductsGet()
        {
            return NavWSBase.ActivityMembershipProductsGet();
        }

        public virtual List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            return NavWSBase.ActivitySubscriptionChargesGet(contactNo);
        }

        public virtual List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            return NavWSBase.ActivityAdmissionEntriesGet(contactNo);
        }

        public virtual List<Membership> ActivityMembershipsGet(string contactNo)
        {
            return NavWSBase.ActivityMembershipsGet(contactNo);
        }

        public virtual List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        public virtual List<ActivityResource> ActivityResourceGet()
        {
            throw new NotImplementedException("Only available in Central Pre");
        }

        #endregion
    }
}
