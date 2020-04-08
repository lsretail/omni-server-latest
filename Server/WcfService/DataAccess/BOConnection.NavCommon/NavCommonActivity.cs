using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.NavCommon.Mapping;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavCommon
{
    public partial class NavCommonBase
    {
        #region Functions

        public ActivityResponse ActivityConfirm(ActivityRequest req)
        {
            if (req == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "Request missing");

            string actId = string.Empty;
            string error = string.Empty;
            decimal price = 0;
            decimal discount = 0;
            decimal amount = 0;
            string cur = string.Empty;
            string bookgRef = string.Empty;
            string resNo = XMLHelper.GetString(req.ReservationNo);
            string item = string.Empty;

            logger.Debug(config.LSKey.Key, "ActivityConfirm - " + Serialization.ToXml(req, true));

            if (NAVVersion > new Version("15.1.0.0"))
            {
                activity15WS.ConfirmActivityV3(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime), ConvertTo.NavGetTime(req.ActivityTime), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                             XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                             ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item);
            }
            else
            {
                activityWS.ConfirmActivityV2(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime), ConvertTo.NavGetTime(req.ActivityTime), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                            XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                            ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo);
            }
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            ActivityResponse result = new ActivityResponse()
            {
                Id = actId,
                ErrorString = error,
                UnitPrice = price,
                LineDiscount = discount,
                TotalAmount = amount,
                Currency = cur,
                BookingRef = bookgRef,
                ReservationNo = resNo,
                ItemNo = item
            };

            logger.Debug(config.LSKey.Key, "ActivityResponse - " + Serialization.ToXml(result, true));
            return result;
        }

        public ActivityResponse ActivityCancel(string activityNo)
        {
            if (string.IsNullOrEmpty(activityNo))
                throw new LSOmniException(StatusCode.ObjectMissing, "Activity No missing");

            string error = string.Empty;
            decimal price = 0;
            decimal discount = 0;
            decimal amount = 0;
            decimal qty = 0;
            string cur = string.Empty;
            string bookgRef = string.Empty;
            string prod = string.Empty;

            logger.Debug(config.LSKey.Key, string.Format("ActivityCancel: activityNo:{0}", activityNo));

            activityWS.CancelActivity(activityNo, ref error, ref prod, ref price, ref qty, ref discount, ref amount, ref cur, ref bookgRef);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            ActivityResponse result = new ActivityResponse()
            {
                ErrorString = error,
                UnitPrice = price,
                LineDiscount = discount,
                Quantity = qty,
                TotalAmount = amount,
                Currency = cur,
                BookingRef = bookgRef,
                ItemNo = prod
            };

            logger.Debug(config.LSKey.Key, "ActivityResponse - " + Serialization.ToXml(result, true));
            return result;
        }

        public List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string optionalResource, string promoCode)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, string.Format("ActivityAvailabilityGet: locationNo:{0}, productNo:{1}, activityDate:{2}, contactNo:{3}", 
                locationNo, productNo, activityDate, contactNo));

            LSActivity.ActivityAvailabilityResponse root = new LSActivity.ActivityAvailabilityResponse();
            activityWS.GetAvailabilityV2(locationNo, productNo, activityDate, XMLHelper.GetString(contactNo), XMLHelper.GetString(optionalResource), XMLHelper.GetString(promoCode), ref error, ref root);
            logger.Debug(config.LSKey.Key, "ActivityAvailabilityResponse - " + Serialization.ToXml(root, true));
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            ActivityMapping map = new ActivityMapping();
            return map.MapRootToAvailabilityResponse(root);
        }

        public AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAdditionalChargesGet: activityNo:{0}", activityNo));

            LSActivity.ActivityChargeRespond root = new LSActivity.ActivityChargeRespond();
            activityWS.GetAdditionalCharges(activityNo, ref root);

            logger.Debug(config.LSKey.Key, "ActivityChargeRespond - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToAdditionalCharge(root);
        }

        public bool ActivityAdditionalChargesSet(AdditionalCharge req)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, "ActivityAdditionalChargesSet - " + Serialization.ToXml(req, true));

            LSActivity.ActivityChargeRespond root = new LSActivity.ActivityChargeRespond();
            activityWS.SetAdditionalChargesV2(req.ActivityNo, req.LineNo, (int)req.ProductType, req.ItemNo, req.Quantity, req.Price, req.DiscountPercentage, XMLHelper.GetString(req.UnitOfMeasure), ref error);
            logger.Debug(config.LSKey.Key, "SetAdditionalChargesV2 - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return true;
        }

        public AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAttributesGet: type:{0}, linkNo:{1}", type, linkNo));

            LSActivity.ActivityAttributeRespond root = new LSActivity.ActivityAttributeRespond();
            activityWS.GetAttributes((int)type, linkNo, ref root);

            logger.Debug(config.LSKey.Key, "ActivityAttributeRespond - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToAttributeResponse(root);
        }

        public int ActivityAttributeSet(AttributeType type, string linkNo, string attrCode, string attrValue)
        {
            string error = string.Empty;
            int seq = 0;

            logger.Debug(config.LSKey.Key, string.Format("ActivityAttributeSet: type:{0}, linkNo:{1}, attrCode:{2}, attrValue:{3}", type, linkNo, attrCode, attrValue));

            activityWS.SetAttribute((int)type, linkNo, XMLHelper.GetString(attrCode), XMLHelper.GetString(attrValue), ref seq, ref error);
            logger.Debug(config.LSKey.Key, "SetAttribute - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return seq;
        }

        public string ActivityInsertReservation(Reservation req)
        {
            string error = string.Empty;
            string resNo = string.Empty;

            logger.Debug(config.LSKey.Key, "ActivityInsertReservation - " + Serialization.ToXml(req, true));

            activityWS.InsertReservation(ref resNo, req.ReservationType, 
                                         XMLHelper.GetSQLNAVDate(req.ResDateFrom), XMLHelper.GetSQLNAVTime(req.ResTimeFrom), XMLHelper.GetSQLNAVDate(req.ResDateTo), XMLHelper.GetSQLNAVTime(req.ResTimeTo), 
                                         XMLHelper.GetString(req.CustomerAccount), XMLHelper.GetString(req.Description), XMLHelper.GetString(req.Comment),
                                         XMLHelper.GetString(req.Reference), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         XMLHelper.GetString(req.Location), XMLHelper.GetString(req.SalesPerson), req.Internalstatus, XMLHelper.GetString(req.Status), 
                                         ref error);
            logger.Debug(config.LSKey.Key, "InsertReservation - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return resNo;
        }

        public string ActivityUpdateReservation(Reservation req)
        {
            string error = string.Empty;
            string resNo = string.Empty;

            logger.Debug(config.LSKey.Key, "ActivityUpdateReservation - " + Serialization.ToXml(req, true));

            activityWS.UpdateReservation(req.Id, req.ReservationType,
                                         XMLHelper.GetSQLNAVDate(req.ResDateFrom), XMLHelper.GetSQLNAVTime(req.ResTimeFrom), XMLHelper.GetSQLNAVDate(req.ResDateTo), XMLHelper.GetSQLNAVTime(req.ResTimeTo),
                                         XMLHelper.GetString(req.CustomerAccount), XMLHelper.GetString(req.Description), XMLHelper.GetString(req.Comment),
                                         XMLHelper.GetString(req.Reference), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         XMLHelper.GetString(req.Location), XMLHelper.GetString(req.SalesPerson), req.Internalstatus, XMLHelper.GetString(req.Status),
                                         ref error);
            logger.Debug(config.LSKey.Key, "UpdateReservation - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return resNo;
        }

        public MembershipResponse ActivityMembershipSell(string contactNo, string type)
        {
            string error = string.Empty;
            string no = string.Empty;
            string itemNo = string.Empty;
            decimal price = 0;
            decimal discount = 0;
            decimal qty = 0;

            logger.Debug(config.LSKey.Key, string.Format("ActivityMembershipCancel: contactNo:{0}, type:{1}", contactNo, type));

            if (NAVVersion > new Version("15.1.0.0"))
            {
                activity15WS.SellMembership(contactNo, type, ref no, ref itemNo, ref price, ref qty, ref discount, ref error);
            }
            else
            {
                activityWS.SellMembership(contactNo, type, ref no, ref itemNo, ref price, ref qty, ref discount, ref error);
            }
            logger.Debug(config.LSKey.Key, "SellMembership - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return new MembershipResponse()
            {
                Id = no,
                ItemNo = itemNo,
                Price = price,
                Discount = discount,
                Quantity = qty
            };
        }

        public bool ActivityMembershipCancel(string contactNo, string memNo, string comment)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, string.Format("ActivityMembershipCancel: contactNo:{0}, memNo:{1}, comment:{2}", contactNo, memNo, comment));

            if (NAVVersion > new Version("15.1.0.0"))
            {
                activity15WS.CancelMembership(contactNo, memNo, XMLHelper.GetString(comment), ref error);
            }
            else
            {
                activityWS.CancelMembership(contactNo, memNo, XMLHelper.GetString(comment), ref error);
            }
            logger.Debug(config.LSKey.Key, "CancelMembership - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return true;
        }

        #endregion

        #region Data Get (Replication)

        public List<ActivityProduct> ActivityProductsGet()
        {
            LSActivity.ActivityUploadProducts root = new LSActivity.ActivityUploadProducts();
            activityWS.UploadActivityProducts(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityProducts Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToActivityProducts(root);
        }

        public List<ActivityType> ActivityTypesGet()
        {
            LSActivity.ActivityUploadTypes root = new LSActivity.ActivityUploadTypes();
            activityWS.UploadActivityTypes(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityTypes Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToActivityType(root);
        }

        public List<ActivityLocation> ActivityLocationsGet()
        {
            LSActivity.ActivityUploadLocations root = new LSActivity.ActivityUploadLocations();
            activityWS.UploadActivityLocations(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityLocations Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToActivityLocation(root);
        }

        public List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityReservationsGet: contactNo:{0}, ResNo:{1}, actType:{2}", contactNo, reservationNo, activityType));

            LSActivity.ActivityUploadReservations root = new LSActivity.ActivityUploadReservations();
            if (string.IsNullOrWhiteSpace(reservationNo))
            {
                activityWS.UploadClientBookingsV2(contactNo, activityType, ref root);
                logger.Debug(config.LSKey.Key, "UploadClientBookingsV2 Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                activityWS.UploadReservationActivities(reservationNo, ref root);
                logger.Debug(config.LSKey.Key, "UploadClientBookingsV2 Response - " + Serialization.ToXml(root, true));
            }

            ActivityMapping map = new ActivityMapping();
            return map.MapRootToReservations(root);
        }

        public List<Promotion> ActivityPromotionsGet()
        {
            LSActivity.ActivityUploadPromotions root = new LSActivity.ActivityUploadPromotions();
            activityWS.UploadPromotions(ref root);

            logger.Debug(config.LSKey.Key, "UploadPromotions Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToPromotions(root);
        }

        public List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAllowancesGet: contactNo:{0}", contactNo));

            LSActivity.ActivityUploadAllowance root = new LSActivity.ActivityUploadAllowance();
            activityWS.UploadPurchasedAllowances(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadPurchasedAllowances Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToAllowances(root);
        }

        public List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityCustomerEntriesGet: contactNo:{0}, customerNo:{1}", contactNo, customerNo));

            LSActivity.ActivityCustomerEntries root = new LSActivity.ActivityCustomerEntries();
            activityWS.UploadCustomerEntries(contactNo, customerNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadCustomerEntries Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToCustomerEntry(root);
        }

        public List<MemberProduct> ActivityMembershipProductsGet()
        {
            if (NAVVersion > new Version("15.1.0.0"))
            {
                LSActivity15.ActivityMembershipProducts root = new LSActivity15.ActivityMembershipProducts();
                activity15WS.UploadMembershipProducts(ref root);

                logger.Debug(config.LSKey.Key, "UploadMembershipProducts Response - " + Serialization.ToXml(root, true));
                ActivityMapping map = new ActivityMapping();
                return map.MapRootToMemberProduct(root);
            }
            else
            {
                LSActivity.ActivityMembershipProducts root = new LSActivity.ActivityMembershipProducts();
                activityWS.UploadMembershipProducts(ref root);

                logger.Debug(config.LSKey.Key, "UploadMembershipProducts Response - " + Serialization.ToXml(root, true));
                ActivityMapping map = new ActivityMapping();
                List<MemberProduct> list = map.MapRootToMemberProduct(root);
                logger.Debug(config.LSKey.Key, "Return list - " + Serialization.ToXml(list, true));
                return list;
            }
        }

        public List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivitySubscriptionChargesGet: contactNo:{0}", contactNo));

            LSActivity.ActivitySubscriptionEntries root = new LSActivity.ActivitySubscriptionEntries();
            activityWS.UploadMembershipSubscriptionCharges(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadMembershipSubscriptionCharges Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToSubscriptionEntry(root);
        }

        public List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAdmissionEntriesGet: contactNo:{0}", contactNo));

            LSActivity.ActivityAdmissionEntries root = new LSActivity.ActivityAdmissionEntries();
            activityWS.UploadAdmissionEntries(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadAdmissionEntries Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping();
            return map.MapRootToAdmissionEntry(root);
        }

        public List<Membership> ActivityMembershipsGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityMembershipsGet: contactNo:{0}", contactNo));

            ActivityMapping map = new ActivityMapping();
            if (NAVVersion > new Version("15.1.0.0"))
            {
                LSActivity15.ActivityUploadMemberships root = new LSActivity15.ActivityUploadMemberships();
                activity15WS.UploadMembershipEntries(contactNo, ref root);

                logger.Debug(config.LSKey.Key, "UploadMembershipEntries Response - " + Serialization.ToXml(root, true));
                return map.MapRootToMemberships(root);
            }
            else
            {
                LSActivity.ActivityUploadMemberships root = new LSActivity.ActivityUploadMemberships();
                activityWS.UploadMembershipEntries(contactNo, ref root);

                logger.Debug(config.LSKey.Key, "UploadMembershipEntries Response - " + Serialization.ToXml(root, true));
                return map.MapRootToMemberships(root);
            }
        }

        #endregion
    }
}
