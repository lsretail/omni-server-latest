using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralPre
{
    public class NavActivity : NavBase, IActivityBO
    {
        public NavActivity(BOConfiguration config) : base(config)
        {
        }

        #region Functions

        public virtual ActivityResponse ActivityConfirm(ActivityRequest request)
        {
            return LSCentralWSBase.ActivityConfirm(request);
        }

        public virtual ActivityResponse ActivityCancel(string activityNo)
        {
            return LSCentralWSBase.ActivityCancel(activityNo);
        }

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons, string guestType)
        {
            return LSCentralWSBase.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, contactAccount, optionalResource, promoCode, activityNo, noOfPersons, guestType);
        }

        public virtual List<AdditionalCharge> ActivityAdditionalChargesGet(string activityNo)
        {
            return LSCentralWSBase.ActivityAdditionalChargesGet(activityNo);
        }

        public virtual List<AdditionalCharge> ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            return LSCentralWSBase.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            return LSCentralWSBase.ActivityAdditionalChargesSet(request);
        }

        public virtual List<AttributeResponse> ActivityAttributesGet(AttributeType type, string linkNo)
        {
            return LSCentralWSBase.ActivityAttributesGet(type, linkNo);
        }

        public virtual int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue)
        {
            return LSCentralWSBase.ActivityAttributeSet(type, linkNo, attributeCode, attributeValue);
        }

        public virtual string ActivityReservationInsert(Reservation request)
        {
            return LSCentralWSBase.ActivityInsertReservation(request);
        }

        public virtual string ActivityReservationUpdate(Reservation request)
        {
            return LSCentralWSBase.ActivityUpdateReservation(request);
        }

        public virtual bool ActivityUpdateReservationStatus(string reservationNo, string setStatusCode)
        {
            return LSCentralWSBase.ActivityUpdateReservationStatus(reservationNo, setStatusCode);
        }

        public virtual bool ActivityUpdateActivityStatus(string activityNo, string setStatusCode)
        {
            return LSCentralWSBase.ActivityUpdateActivityStatus(activityNo, setStatusCode);
        }

        public virtual MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType)
        {
            return LSCentralWSBase.ActivityMembershipSell(contactNo, membersShipType);
        }

        public virtual bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment)
        {
            return LSCentralWSBase.ActivityMembershipCancel(contactNo, memberShipNo, comment);
        }

        public virtual List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays)
        {
            return LSCentralWSBase.ActivityResourceAvailabilityGet(locationNo, activityDate, resourceNo, intervalType, noOfDays);
        }

        public virtual List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays)
        {
            return LSCentralWSBase.ActivityResourceGroupAvailabilityGet(locationNo, activityDate, groupNo, intervalType, noOfDays);
        }

        public virtual bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString)
        {
            return LSCentralWSBase.ActivityCheckAccess(searchReference, locationNo, gateNo, registerAccessEntry, checkType, out messageString);
        }

        public virtual string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activityTime, string optionalResource, int quantity)
        {
            return LSCentralWSBase.ActivityGetAvailabilityToken(locationNo, productNo, activityTime, optionalResource, quantity);
        }

        public virtual string ActivityInsertGroupReservation(Reservation request)
        { 
            return LSCentralWSBase.ActivityInsertGroupReservation(request); 
        }

        public virtual string ActivityUpdateGroupReservation(Reservation request)
        { 
            return LSCentralWSBase.ActivityUpdateGroupReservation(request);
        }

        public virtual ActivityResponse ActivityConfirmGroup(ActivityRequest request)
        { 
            return LSCentralWSBase.ActivityConfirmGroup(request); 
        }

        public virtual bool ActivityDeleteGroup(string groupNo, int lineNo)
        { 
            return LSCentralWSBase.ActivityDeleteGroup(groupNo, lineNo); 
        }

        public virtual string ActivityUpdateGroupHeaderStatus(string groupNo, string statusCode)
        {
            return LSCentralWSBase.ActivityUpdateGroupHeaderStatus(groupNo, statusCode);
        }

        public virtual ActivityResponse ActivityPreSellProduct(string locationNo, string productNo, string promoCode, string contactNo, int quantity)
        {
            return LSCentralWSBase.ActivityPreSellProduct(locationNo, productNo, promoCode, contactNo, quantity);
        }

        #endregion

        #region Data Get (Replication)

        public virtual List<ActivityProduct> ActivityProductsGet()
        {
            return LSCentralWSBase.ActivityProductsGet();
        }

        public virtual List<ActivityType> ActivityTypesGet()
        {
            return LSCentralWSBase.ActivityTypesGet();
        }

        public virtual List<ActivityLocation> ActivityLocationsGet()
        {
            return LSCentralWSBase.ActivityLocationsGet();
        }

        public virtual List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            return LSCentralWSBase.ActivityReservationsGet(reservationNo, contactNo, activityType);
        }

        public virtual List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            return LSCentralWSBase.ActivityReservationsHeaderGet(reservationNo, reservationType, status, locationNo, fromDate);
        }

        public virtual List<Promotion> ActivityPromotionsGet()
        {
            return LSCentralWSBase.ActivityPromotionsGet();
        }

        public virtual List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            return LSCentralWSBase.ActivityAllowancesGet(contactNo);
        }

        public virtual List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            return LSCentralWSBase.ActivityCustomerEntriesGet(contactNo, customerNo);
        }

        public virtual List<MemberProduct> ActivityMembershipProductsGet()
        {
            return LSCentralWSBase.ActivityMembershipProductsGet();
        }

        public virtual List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            return LSCentralWSBase.ActivitySubscriptionChargesGet(contactNo);
        }

        public virtual List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            return LSCentralWSBase.ActivityAdmissionEntriesGet(contactNo);
        }

        public virtual List<Membership> ActivityMembershipsGet(string contactNo)
        {
            return LSCentralWSBase.ActivityMembershipsGet(contactNo);
        }

        public virtual List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate)
        {
            return LSCentralWSBase.ActivityGetByResource(locationNo, resourceNo, fromDate, toDate);
        }

        public virtual List<ActivityResource> ActivityResourceGet()
        {
            return LSCentralWSBase.ActivityResourceGet();
        }

        #endregion
    }
}
