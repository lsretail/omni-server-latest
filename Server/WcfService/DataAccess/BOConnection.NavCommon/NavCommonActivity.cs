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
                activity15WS.ConfirmActivityV3(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                             XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                             ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item);
            }
            else
            {
                activity15WS.ConfirmActivityV2(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
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

            activity15WS.CancelActivity(activityNo, ref error, ref prod, ref price, ref qty, ref discount, ref amount, ref cur, ref bookgRef);
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

        public List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string optionalResource, string promoCode, string activityNo)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, string.Format("ActivityAvailabilityGet: locationNo:{0}, productNo:{1}, activityDate:{2}, contactNo:{3}", 
                locationNo, productNo, activityDate, contactNo));

            LSActivity15.ActivityAvailabilityResponse root = new LSActivity15.ActivityAvailabilityResponse();
            activity15WS.GetAvailabilityV2(locationNo, productNo, activityDate, XMLHelper.GetString(contactNo), XMLHelper.GetString(optionalResource), XMLHelper.GetString(promoCode), XMLHelper.GetString(activityNo), ref error, ref root);
            logger.Debug(config.LSKey.Key, "ActivityAvailabilityResponse - " + Serialization.ToXml(root, true));
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToAvailabilityResponse(root);
        }

        public AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAdditionalChargesGet: activityNo:{0}", activityNo));

            LSActivity15.ActivityChargeRespond root = new LSActivity15.ActivityChargeRespond();
            activity15WS.GetAdditionalCharges(activityNo, ref root);

            logger.Debug(config.LSKey.Key, "ActivityChargeRespond - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToAdditionalCharge(root);
        }

        public bool ActivityAdditionalChargesSet(AdditionalCharge req)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, "ActivityAdditionalChargesSet - " + Serialization.ToXml(req, true));

            LSActivity15.ActivityChargeRespond root = new LSActivity15.ActivityChargeRespond();
            activity15WS.SetAdditionalChargesV2(req.ActivityNo, req.LineNo, (int)req.ProductType, req.ItemNo, req.Quantity, req.Price, req.DiscountPercentage, XMLHelper.GetString(req.UnitOfMeasure), ref error);
            logger.Debug(config.LSKey.Key, "SetAdditionalChargesV2 - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return true;
        }

        public AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAttributesGet: type:{0}, linkNo:{1}", type, linkNo));

            LSActivity15.ActivityAttributeRespond root = new LSActivity15.ActivityAttributeRespond();
            activity15WS.GetAttributes((int)type, linkNo, ref root);

            logger.Debug(config.LSKey.Key, "ActivityAttributeRespond - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToAttributeResponse(root);
        }

        public int ActivityAttributeSet(AttributeType type, string linkNo, string attrCode, string attrValue)
        {
            string error = string.Empty;
            int seq = 0;

            logger.Debug(config.LSKey.Key, string.Format("ActivityAttributeSet: type:{0}, linkNo:{1}, attrCode:{2}, attrValue:{3}", type, linkNo, attrCode, attrValue));

            activity15WS.SetAttribute((int)type, linkNo, XMLHelper.GetString(attrCode), XMLHelper.GetString(attrValue), ref seq, ref error);
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

            activity15WS.InsertReservation(ref resNo, req.ReservationType, 
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

            activity15WS.UpdateReservation(req.Id, req.ReservationType,
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
            activity15WS.SellMembership(contactNo, type, ref no, ref itemNo, ref price, ref qty, ref discount, ref error);

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
            activity15WS.CancelMembership(contactNo, memNo, XMLHelper.GetString(comment), ref error);

            logger.Debug(config.LSKey.Key, "CancelMembership - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return true;
        }

        #endregion

        #region Data Get (Replication)

        public List<ActivityProduct> ActivityProductsGet()
        {
            LSActivity15.ActivityUploadProducts root = new LSActivity15.ActivityUploadProducts();
            activity15WS.UploadActivityProducts(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityProducts Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToActivityProducts(root);
        }

        public List<ActivityType> ActivityTypesGet()
        {
            LSActivity15.ActivityUploadTypes root = new LSActivity15.ActivityUploadTypes();
            activity15WS.UploadActivityTypes(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityTypes Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToActivityType(root);
        }

        public List<ActivityLocation> ActivityLocationsGet()
        {
            LSActivity15.ActivityUploadLocations root = new LSActivity15.ActivityUploadLocations();
            activity15WS.UploadActivityLocations(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityLocations Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToActivityLocation(root);
        }

        public List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            if (NAVVersion < new Version("16.1"))
                return new List<ResHeader>();

            logger.Debug(config.LSKey.Key, "GetActReservations Request - ResNo:{0} LocNo:{1} FromDate:{2}", reservationNo, locationNo, fromDate);

            LSActivity15.ActivityUploadResHeaders root = new LSActivity15.ActivityUploadResHeaders();
            activity15WS.GetActReservations(reservationNo, locationNo, reservationType, status, fromDate, ref root);

            logger.Debug(config.LSKey.Key, "GetActReservations Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToResHeader(root);
        }

        public List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityReservationsGet: contactNo:{0}, ResNo:{1}, actType:{2}", contactNo, reservationNo, activityType));

            LSActivity15.ActivityUploadReservations root = new LSActivity15.ActivityUploadReservations();
            if (string.IsNullOrWhiteSpace(reservationNo))
            {
                activity15WS.UploadClientBookingsV2(contactNo, activityType, ref root);
                logger.Debug(config.LSKey.Key, "UploadClientBookingsV2 Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                activity15WS.UploadReservationActivities(reservationNo, ref root);
                logger.Debug(config.LSKey.Key, "UploadReservationActivities Response - " + Serialization.ToXml(root, true));
            }

            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToReservations(root);
        }

        public List<Promotion> ActivityPromotionsGet()
        {
            LSActivity15.ActivityUploadPromotions root = new LSActivity15.ActivityUploadPromotions();
            activity15WS.UploadPromotions(ref root);

            logger.Debug(config.LSKey.Key, "UploadPromotions Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToPromotions(root);
        }

        public List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAllowancesGet: contactNo:{0}", contactNo));

            LSActivity15.ActivityUploadAllowance root = new LSActivity15.ActivityUploadAllowance();
            activity15WS.UploadPurchasedAllowances(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadPurchasedAllowances Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToAllowances(root);
        }

        public List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityCustomerEntriesGet: contactNo:{0}, customerNo:{1}", contactNo, customerNo));

            LSActivity15.ActivityCustomerEntries root = new LSActivity15.ActivityCustomerEntries();
            activity15WS.UploadCustomerEntries(contactNo, customerNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadCustomerEntries Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToCustomerEntry(root);
        }

        public List<MemberProduct> ActivityMembershipProductsGet()
        {
            if (NAVVersion > new Version("15.1.0.0"))
            {
                LSActivity15.ActivityMembershipProducts root = new LSActivity15.ActivityMembershipProducts();
                activity15WS.UploadMembershipProducts(ref root);

                logger.Debug(config.LSKey.Key, "UploadMembershipProducts Response - " + Serialization.ToXml(root, true));
                ActivityMapping map = new ActivityMapping(config.IsJson);
                return map.MapRootToMemberProduct(root);
            }
            else
            {
                LSActivity.ActivityMembershipProducts root = new LSActivity.ActivityMembershipProducts();
                activityWS.UploadMembershipProducts(ref root);

                logger.Debug(config.LSKey.Key, "UploadMembershipProducts Response - " + Serialization.ToXml(root, true));
                ActivityMapping map = new ActivityMapping(config.IsJson);
                List<MemberProduct> list = map.MapRootToMemberProduct(root);
                logger.Debug(config.LSKey.Key, "Return list - " + Serialization.ToXml(list, true));
                return list;
            }
        }

        public List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivitySubscriptionChargesGet: contactNo:{0}", contactNo));

            LSActivity15.ActivitySubscriptionEntries root = new LSActivity15.ActivitySubscriptionEntries();
            activity15WS.UploadMembershipSubscriptionCharges(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadMembershipSubscriptionCharges Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToSubscriptionEntry(root);
        }

        public List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityAdmissionEntriesGet: contactNo:{0}", contactNo));

            LSActivity15.ActivityAdmissionEntries root = new LSActivity15.ActivityAdmissionEntries();
            activity15WS.UploadAdmissionEntries(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadAdmissionEntries Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(config.IsJson);
            return map.MapRootToAdmissionEntry(root);
        }

        public List<Membership> ActivityMembershipsGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, string.Format("ActivityMembershipsGet: contactNo:{0}", contactNo));

            ActivityMapping map = new ActivityMapping(config.IsJson);
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
