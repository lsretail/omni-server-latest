using System;
using System.Collections.Generic;

using LSOmni.BLL;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;

namespace LSOmni.Service
{
    public partial class LSOmniBase
    {
        #region Functions

        public virtual ActivityResponse ActivityConfirm(ActivityRequest request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityConfirm(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityConfirm");
                return null; //never gets here
            }
        }

        public virtual ActivityResponse ActivityCancel(string activityNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityCancel(activityNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityCancel");
                return null; //never gets here
            }
        }

        public virtual AvailabilityResponse ActivityAvailabilityGet(string locationNo, string itemNo, DateTime activityDate, string contactNo, string optionalResource, string promoCode)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAvailabilityGet(locationNo, itemNo, activityDate, contactNo, optionalResource, promoCode);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityAvailabilityGet");
                return null; //never gets here
            }
        }

        public virtual AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAdditionalChargesGet(activityNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityAdditionalChargesGet");
                return null; //never gets here
            }
        }

        public virtual bool ActivityAdditionalChargesSet(AdditionalCharge request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAdditionalChargesSet(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityAdditionalChargesSet");
                return false; //never gets here
            }
        }

        public virtual AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAttributesGet(type, linkNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityAttributesGet");
                return null; //never gets here
            }
        }

        public virtual int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAttributeSet(type, linkNo, attributeCode, attributeValue);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityAttributeSet");
                return -1; //never gets here
            }
        }

        public virtual string ActivityReservationInsert(Reservation request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityReservationInsert(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityInsertReservation");
                return null; //never gets here
            }
        }

        public virtual string ActivityReservationUpdate(Reservation request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityReservationUpdate(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityUpdateReservation");
                return null; //never gets here
            }
        }

        public virtual MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityMembershipSell(contactNo, membersShipType);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipSell");
                return null; //never gets here
            }
        }

        public virtual bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityMembershipCancel(contactNo, memberShipNo, comment);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return false; //never gets here
            }
        }

        #endregion

        #region Data Get (Replication)

        public virtual List<ActivityProduct> ActivityProductsGet()
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityProductsGet();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityProducts");
                return null; //never gets here
            }
        }

        public virtual List<ActivityType> ActivityTypesGet()
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityTypesGet();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityTypes");
                return null; //never gets here
            }
        }

        public virtual List<ActivityLocation> ActivityLocationsGet()
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityLocationsGet();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityLocationsGet");
                return null; //never gets here
            }
        }

        public virtual List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityReservationsGet(reservationNo, contactNo, activityType);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityReservationsGet");
                return null; //never gets here
            }
        }

        public virtual List<Promotion> ActivityPromotionsGet()
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityPromotionsGet();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityPromotionsGet");
                return null; //never gets here
            }
        }

        public virtual List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAllowancesGet(contactNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityAllowancesGet");
                return null; //never gets here
            }
        }

        public virtual List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityCustomerEntriesGet(contactNo, customerNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityCustomerEntriesGet");
                return null; //never gets here
            }
        }

        public virtual List<MemberProduct> ActivityMembershipProductsGet()
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityMembershipProductsGet();
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipProductsGet");
                return null; //never gets here
            }
        }

        public virtual List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivitySubscriptionChargesGet(contactNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivitySubscriptionChargesGet");
                return null; //never gets here
            }
        }

        public virtual List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAdmissionEntriesGet(contactNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivitySubscriptionChargesGet");
                return null; //never gets here
            }
        }

        public virtual List<Membership> ActivityMembershipsGet(string contactNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityMembershipsGet(contactNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipsGet");
                return null; //never gets here
            }
        }

        #endregion
    }
}