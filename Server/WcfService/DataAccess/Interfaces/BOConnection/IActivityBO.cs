using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using System;
using System.Collections.Generic;

namespace LSOmni.DataAccess.Interface.BOConnection
{
    public interface IActivityBO
    {
        #region Functions

        ActivityResponse ActivityConfirm(ActivityRequest request);
        ActivityResponse ActivityCancel(string activityNo);
        List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons);
        AdditionalCharge ActivityAdditionalChargesGet(string activityNo);
        AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking);
        bool ActivityAdditionalChargesSet(AdditionalCharge request);
        AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo);
        int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue);
        string ActivityReservationInsert(Reservation request);
        string ActivityReservationUpdate(Reservation request);
        MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType);
        bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment);
        List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays);
        List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays);

        #endregion

        #region Data Get (Replication)

        List<ActivityProduct> ActivityProductsGet();
        List<ActivityType> ActivityTypesGet();
        List<ActivityLocation> ActivityLocationsGet();
        List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType);
        List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate);
        List<Promotion> ActivityPromotionsGet();
        List<Allowance> ActivityAllowancesGet(string contactNo);
        List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo);
        List<MemberProduct> ActivityMembershipProductsGet();
        List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo);
        List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo);
        List<Membership> ActivityMembershipsGet(string contactNo);
        List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate);
        List<ActivityResource> ActivityResourceGet();

        #endregion
    }
}
