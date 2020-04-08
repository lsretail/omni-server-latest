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
                    iActBOConnection = GetBORepository<IActivityBO>(config.LSKey.Key);
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

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string optionalResource, string promoCode)
        {
            return BOActConnection.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, optionalResource, promoCode);
        }

        public virtual AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            return BOActConnection.ActivityAdditionalChargesGet(activityNo);
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            return BOActConnection.ActivityAdditionalChargesSet(request);
        }

        public virtual AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
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

        #endregion
    }
}
