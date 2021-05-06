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

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string optionalResource, string promoCode, string activityNo, int noOfPersons)
        {
            return LSCentralWSBase.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, optionalResource, promoCode, activityNo, noOfPersons);
        }

        public virtual AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            return LSCentralWSBase.ActivityAdditionalChargesGet(activityNo);
        }

        public virtual AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            return LSCentralWSBase.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            return LSCentralWSBase.ActivityAdditionalChargesSet(request);
        }

        public virtual AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
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

        public virtual MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType)
        {
            return LSCentralWSBase.ActivityMembershipSell(contactNo, membersShipType);
        }

        public virtual bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment)
        {
            return LSCentralWSBase.ActivityMembershipCancel(contactNo, memberShipNo, comment);
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

        #endregion
    }
}
