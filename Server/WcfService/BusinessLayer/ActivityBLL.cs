using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.BLL
{
    public class ActivityBLL : BaseBLL
    {
        private IActivityBO iActBOConnection = null;
        protected int timeoutInSeconds = 0;

        public ActivityBLL(BOConfiguration config) : base(config)
        {
        }

        protected IActivityBO BOActConnection
        {
            get
            {
                if (iActBOConnection == null)
                    iActBOConnection = GetBORepository<IActivityBO>(config.LSKey.Key, config.IsJson);
                return iActBOConnection;
            }
        }

        public virtual ActivityResponse ActivityConfirm(ActivityRequest request)
        {
            return BOActConnection.ActivityConfirm(request);
        }

        public virtual ActivityResponse ActivityCancel(string activityNo)
        {
            return BOActConnection.ActivityCancel(activityNo);
        }

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons, string guestType)
        {
            return BOActConnection.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, contactAccount, optionalResource, promoCode, activityNo, noOfPersons, guestType);
        }

        public virtual List<AdditionalCharge> ActivityAdditionalChargesGet(string activityNo)
        {
            return BOActConnection.ActivityAdditionalChargesGet(activityNo);
        }

        public virtual List<AdditionalCharge> ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            return BOActConnection.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            return BOActConnection.ActivityAdditionalChargesSet(request);
        }

        public virtual List<AttributeResponse> ActivityAttributesGet(AttributeType type, string linkNo)
        {
            return BOActConnection.ActivityAttributesGet(type, linkNo);
        }

        public virtual int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue)
        {
            return BOActConnection.ActivityAttributeSet(type, linkNo, attributeCode, attributeValue);
        }

        public virtual string ActivityReservationInsert(Reservation request)
        {
            return BOActConnection.ActivityReservationInsert(request);
        }

        public virtual bool ActivityUpdateReservationStatus(string reservationNo, string setStatusCode)
        {
            return BOActConnection.ActivityUpdateReservationStatus(reservationNo, setStatusCode);
        }

        public virtual bool ActivityUpdateActivityStatus(string activityNo, string setStatusCode)
        {
            return BOActConnection.ActivityUpdateActivityStatus(activityNo, setStatusCode);
        }

        public virtual string ActivityReservationUpdate(Reservation request)
        {
            return BOActConnection.ActivityReservationUpdate(request);
        }

        public virtual MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType)
        {
            return BOActConnection.ActivityMembershipSell(contactNo, membersShipType);
        }

        public virtual bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment)
        {
            return BOActConnection.ActivityMembershipCancel(contactNo, memberShipNo, comment);
        }

        public virtual List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays)
        {
            return BOActConnection.ActivityResourceAvailabilityGet(locationNo, activityDate, resourceNo, intervalType, noOfDays);
        }

        public virtual List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays)
        {
            return BOActConnection.ActivityResourceGroupAvailabilityGet(locationNo, activityDate, groupNo, intervalType, noOfDays);
        }

        public virtual bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString)
        {
            return BOActConnection.ActivityCheckAccess(searchReference, locationNo, gateNo, registerAccessEntry, checkType, out messageString);
        }

        public virtual string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activityTime, string optionalResource, int quantity)
        {
            return BOActConnection.ActivityGetAvailabilityToken(locationNo, productNo, activityTime, optionalResource, quantity);
        }

        public virtual string ActivityInsertGroupReservation(Reservation request)
        {
            return BOActConnection.ActivityInsertGroupReservation(request);
        }

        public virtual string ActivityUpdateGroupReservation(Reservation request)
        {
            return BOActConnection.ActivityUpdateGroupReservation(request);
        }

        public virtual ActivityResponse ActivityConfirmGroup(ActivityRequest request)
        {
            return BOActConnection.ActivityConfirmGroup(request);
        }

        public virtual bool ActivityDeleteGroup(string groupNo, int lineNo)
        {
            return BOActConnection.ActivityDeleteGroup(groupNo, lineNo);
        }

        public virtual string ActivityUpdateGroupHeaderStatus(string groupNo, string statusCode)
        {
            return BOActConnection.ActivityUpdateGroupHeaderStatus(groupNo, statusCode);
        }

        public virtual ActivityResponse ActivityPreSellProduct(string locationNo, string productNo, string promoCode, string contactNo, int quantity)
        {
            return BOActConnection.ActivityPreSellProduct(locationNo, productNo, promoCode, contactNo, quantity);
        }

        #region Data Get (Replication)

        public virtual List<ActivityProduct> ActivityProductsGet()
        {
            return BOActConnection.ActivityProductsGet();
        }

        public virtual List<ActivityType> ActivityTypesGet()
        {
            return BOActConnection.ActivityTypesGet();
        }

        public virtual List<ActivityLocation> ActivityLocationsGet()
        {
            return BOActConnection.ActivityLocationsGet();
        }


        public virtual List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            return BOActConnection.ActivityReservationsGet(reservationNo, contactNo, activityType);
        }

        public virtual List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            return BOActConnection.ActivityReservationsHeaderGet(reservationNo, reservationType, status, locationNo, fromDate);
        }

        public virtual List<Promotion> ActivityPromotionsGet()
        {
            return BOActConnection.ActivityPromotionsGet();
        }

        public virtual List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            return BOActConnection.ActivityAllowancesGet(contactNo);
        }

        public virtual List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            return BOActConnection.ActivityCustomerEntriesGet(contactNo, customerNo);
        }

        public virtual List<MemberProduct> ActivityMembershipProductsGet()
        {
            return BOActConnection.ActivityMembershipProductsGet();
        }

        public virtual List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            return BOActConnection.ActivitySubscriptionChargesGet(contactNo);
        }

        public virtual List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            return BOActConnection.ActivityAdmissionEntriesGet(contactNo);
        }

        public virtual List<Membership> ActivityMembershipsGet(string contactNo)
        {
            return BOActConnection.ActivityMembershipsGet(contactNo);
        }

        public virtual List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate)
        {
            return BOActConnection.ActivityGetByResource(locationNo, resourceNo, fromDate, toDate);
        }

        public virtual List<ActivityResource> ActivityResourceGet()
        {
            return BOActConnection.ActivityResourceGet();
        }

        #endregion
    }
}
