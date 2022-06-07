using System;
using System.Collections.Generic;

using LSOmni.BLL;
using LSOmni.Common.Util;
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

        public virtual List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityAvailabilityGet(locationNo, productNo, activityDate, contactNo, contactAccount, optionalResource, promoCode, activityNo, noOfPersons);
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

        public virtual AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityProductChargesGet(locationNo, productNo, dateOfBooking);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityProductChargesGet");
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

        public virtual List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityResourceAvailabilityGet(locationNo, activityDate, resourceNo, intervalType, noOfDays);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return null; //never gets here
            }
        }

        public virtual List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityResourceGroupAvailabilityGet(locationNo, activityDate, groupNo, intervalType, noOfDays);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return null; //never gets here
            }
        }

        public virtual bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityCheckAccess(searchReference, locationNo, gateNo, registerAccessEntry, checkType, out messageString);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                messageString = "Error";
                return false; //never gets here
            }
        }

        public virtual string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activiyTime, string optionalResource, int quantity)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityGetAvailabilityToken(locationNo, productNo, activiyTime, optionalResource, quantity);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return string.Empty; //never gets here
            }
        }

        public virtual string ActivityInsertGroupReservation(Reservation request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityInsertGroupReservation(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return string.Empty; //never gets here
            }
        }

        public virtual string ActivityUpdateGroupReservation(Reservation request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityUpdateGroupReservation(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return string.Empty; //never gets here
            }
        }

        public virtual ActivityResponse ActivityConfirmGroup(ActivityRequest request)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityConfirmGroup(request);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipCancel");
                return null;
            }
        }

        public virtual bool ActivityDeleteGroup(string groupNo, int lineNo)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityDeleteGroup(groupNo, lineNo);
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

        public virtual List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityReservationsHeaderGet(reservationNo, reservationType, status, locationNo, fromDate);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityReservationsHeaderGet");
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

        public virtual List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate)
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityGetByResource(locationNo, resourceNo, fromDate, toDate);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "Failed to get ActivityMembershipsGet");
                return null; //never gets here
            }
        }

        public virtual List<ActivityResource> ActivityResourceGet()
        {
            try
            {
                ActivityBLL bll = new ActivityBLL(config);
                return bll.ActivityResourceGet();
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