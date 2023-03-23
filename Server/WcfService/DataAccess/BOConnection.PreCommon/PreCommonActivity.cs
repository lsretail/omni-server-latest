using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.Mapping;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
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

            if (LSCVersion < new Version("18.1"))
            {
                logger.Debug(config.LSKey.Key, "ConfirmActivityV3 - " + Serialization.ToXml(req, true));
                activityWS.ConfirmActivityV3(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                         XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item);
            }
            else if (LSCVersion.Major < 20)
            {
                logger.Debug(config.LSKey.Key, "ConfirmActivityV4 - " + Serialization.ToXml(req, true));
                activityWS.ConfirmActivityV4(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                         XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item, XMLHelper.GetString(req.ContactAccount));
            }
            else
            {
                logger.Debug(config.LSKey.Key, "ConfirmActivityV5 - " + Serialization.ToXml(req, true));
                activityWS.ConfirmActivityV5(XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                         XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item, XMLHelper.GetString(req.ContactAccount), XMLHelper.GetString(req.Token));
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

            logger.Debug(config.LSKey.Key, "CancelActivity: activityNo:{0}", activityNo);

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

        public List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons)
        {
            string error = string.Empty;

            LSActivity.ActivityAvailabilityResponse root = new LSActivity.ActivityAvailabilityResponse();
            if (LSCVersion < new Version("18.1"))
            {
                logger.Debug(config.LSKey.Key, "GetAvailabilityV3: locationNo:{0}, productNo:{1}, activityDate:{2}, contactNo:{3}",
                    locationNo, productNo, activityDate, contactNo);
                activityWS.GetAvailabilityV3(locationNo, productNo, activityDate, XMLHelper.GetString(contactNo), XMLHelper.GetString(optionalResource), XMLHelper.GetString(promoCode), XMLHelper.GetString(activityNo), noOfPersons, ref error, ref root);
            }
            else
            {
                logger.Debug(config.LSKey.Key, "GetAvailabilityV4: locationNo:{0}, productNo:{1}, activityDate:{2}, contactNo:{3}",
                    locationNo, productNo, activityDate, contactNo);
                activityWS.GetAvailabilityV4(locationNo, productNo, activityDate, XMLHelper.GetString(contactNo), XMLHelper.GetString(optionalResource), XMLHelper.GetString(promoCode), XMLHelper.GetString(activityNo), noOfPersons, XMLHelper.GetString(contactAccount),  ref error, ref root);
            }
            logger.Debug(config.LSKey.Key, "AvailabilityResponse - " + Serialization.ToXml(root, true));
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAvailabilityResponse(root);
        }

        public AdditionalCharge ActivityAdditionalChargesGet(string activityNo)
        {
            logger.Debug(config.LSKey.Key, "GetAdditionalCharges: activityNo:{0}", activityNo);

            LSActivity.ActivityChargeRespond root = new LSActivity.ActivityChargeRespond();
            activityWS.GetAdditionalCharges(activityNo, ref root);

            logger.Debug(config.LSKey.Key, "AdditionalChargeResponse - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAdditionalCharge(root);
        }

        public AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking)
        {
            logger.Debug(config.LSKey.Key, "GetProductChargesV2: productNo:{0}", productNo);

            LSActivity.ActivityChargeRespond root = new LSActivity.ActivityChargeRespond();
            activityWS.GetProductChargesV2(productNo, locationNo, dateOfBooking, ref root);

            logger.Debug(config.LSKey.Key, "AdditionalChargeResponse - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAdditionalCharge(root);
        }

        public ActivityResponse ActivityPreSellProduct(string locationNo, string productNo, string promoCode, string contactNo, int quantity)
        {
            logger.Debug(config.LSKey.Key, "PreSellActivityProduct: locationNo:{0} productNo:{1}", locationNo, productNo);

            string itemNo = string.Empty;
            string bookRef = string.Empty;
            string errMsg = string.Empty;
            decimal unitPrice = 0;

            activityWS.PreSellActivityProduct(locationNo, promoCode, contactNo, productNo, quantity, ref itemNo, ref unitPrice, ref bookRef, ref errMsg);
            if (string.IsNullOrEmpty(errMsg) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, errMsg);

            ActivityResponse resp = new ActivityResponse()
            {
                ItemNo = itemNo,
                BookingRef = bookRef,
                UnitPrice = unitPrice,
                ErrorString = errMsg
            };
            logger.Debug(config.LSKey.Key, "ActivityResponse - " + Serialization.ToXml(resp, true));
            return resp;
        }

        public bool ActivityAdditionalChargesSet(AdditionalCharge req)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, "SetAdditionalChargesV2 - " + Serialization.ToXml(req, true));

            LSActivity.ActivityChargeRespond root = new LSActivity.ActivityChargeRespond();
            activityWS.SetAdditionalChargesV2(req.ActivityNo, req.LineNo, (int)req.ProductType, req.ItemNo, req.Quantity, req.Price, req.DiscountPercentage, XMLHelper.GetString(req.UnitOfMeasure), ref error);
            logger.Debug(config.LSKey.Key, "SetAdditionalChargesV2 - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return true;
        }

        public List<AttributeResponse> ActivityAttributesGet(AttributeType type, string linkNo)
        {
            logger.Debug(config.LSKey.Key, "GetAttributes: type:{0}, linkNo:{1}", type, linkNo);

            LSActivity.ActivityAttributeRespond root = new LSActivity.ActivityAttributeRespond();
            activityWS.GetAttributes((int)type, linkNo, ref root);

            logger.Debug(config.LSKey.Key, "AttributeResponse - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAttributeResponse(root);
        }

        public int ActivityAttributeSet(AttributeType type, string linkNo, string attrCode, string attrValue)
        {
            string error = string.Empty;
            int seq = 0;

            logger.Debug(config.LSKey.Key, "SetAttribute: type:{0}, linkNo:{1}, attrCode:{2}, attrValue:{3}", type, linkNo, attrCode, attrValue);

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

            logger.Debug(config.LSKey.Key, "InsertReservation - " + Serialization.ToXml(req, true));

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

            logger.Debug(config.LSKey.Key, "UpdateReservation - " + Serialization.ToXml(req, true));

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

        public bool ActivityUpdateReservationStatus(string reservationNo, string setStatusCode)
        {
            string error = string.Empty;
            logger.Debug(config.LSKey.Key, "UpdateReservationStatus - ResNo:{0} Stat:{1}", reservationNo, setStatusCode);
            bool ret = activityWS.UpdateReservationStatus(reservationNo, setStatusCode, ref error);
            logger.Debug(config.LSKey.Key, "UpdateReservationStatus - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);
            return ret;
        }

        public bool ActivityUpdateActivityStatus(string activityNo, string setStatusCode)
        {
            string error = string.Empty;
            logger.Debug(config.LSKey.Key, "UpdateReservationStatus - ResNo:{0} Stat:{1}", activityNo, setStatusCode);
            bool ret = activityWS.UpdateActivityStatus(activityNo, setStatusCode, ref error);
            logger.Debug(config.LSKey.Key, "UpdateReservationStatus - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);
            return ret;
        }

        public MembershipResponse ActivityMembershipSell(string contactNo, string type)
        {
            string error = string.Empty;
            string no = string.Empty;
            string itemNo = string.Empty;
            string bookRef = string.Empty;
            decimal price = 0;
            decimal discount = 0;
            decimal qty = 0;

            logger.Debug(config.LSKey.Key, "SellMembershipV2: contactNo:{0}, type:{1}", contactNo, type);
            activityWS.SellMembershipV2(contactNo, type, ref no, ref itemNo, ref price, ref qty, ref discount, ref bookRef, ref error);
            logger.Debug(config.LSKey.Key, "SellMembership - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return new MembershipResponse()
            {
                Id = no,
                ItemNo = itemNo,
                Price = price,
                Discount = discount,
                Quantity = qty,
                BookingRef = bookRef
            };
        }

        public bool ActivityMembershipCancel(string contactNo, string memNo, string comment)
        {
            string error = string.Empty;

            logger.Debug(config.LSKey.Key, "CancelMembership: contactNo:{0}, memNo:{1}, comment:{2}", contactNo, memNo, comment);
            activityWS.CancelMembership(contactNo, memNo, XMLHelper.GetString(comment), ref error);

            logger.Debug(config.LSKey.Key, "CancelMembership - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return true;
        }

        public List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays)
        {
            logger.Debug(config.LSKey.Key, "GetResourceAvailability: locNo:{0}, actDate:{1}, resNo:{2}, intType:{3}, noOfDays:{4}", 
                locationNo, activityDate, resourceNo, intervalType, noOfDays);

            string error = string.Empty;
            LSActivity.ActivityAvailabilityResponse root = new LSActivity.ActivityAvailabilityResponse();
            activityWS.GetResourceAvailability(locationNo, activityDate, resourceNo, intervalType, noOfDays, ref error, ref root);

            logger.Debug(config.LSKey.Key, "GetResourceAvailability - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            logger.Debug(config.LSKey.Key, "GetResourceAvailability - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAvailabilityResponse(root);
        }

        public List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays)
        {
            logger.Debug(config.LSKey.Key, "GetResourceGroupAvailability: locNo:{0}, actDate:{1}, groupNo:{2}, intType:{3}, noOfDays:{4}",
                locationNo, activityDate, groupNo, intervalType, noOfDays);

            string error = string.Empty;
            LSActivity.ActivityAvailabilityResponse root = new LSActivity.ActivityAvailabilityResponse();
            activityWS.GetResourceGroupAvailability(locationNo, activityDate, groupNo, intervalType, noOfDays, ref error, ref root);

            logger.Debug(config.LSKey.Key, "GetResourceGroupAvailability - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            logger.Debug(config.LSKey.Key, "GetResourceGroupAvailability - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAvailabilityResponse(root);
        }

        public bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString)
        {
            messageString = string.Empty;

            logger.Debug(config.LSKey.Key, $"CheckAccess: SearchRef:{searchReference}, Loc:{locationNo}, Gate:{gateNo}, RegAccEntry:{registerAccessEntry}, Type:{checkType}");
            bool ret = activityWS.CheckAccess(searchReference, locationNo, gateNo, registerAccessEntry, checkType, ref messageString);
            logger.Debug(config.LSKey.Key, $"CheckAccess > Ret:{ret} Msg:{messageString}");
            return ret;
        }

        public string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activiyTime, string optionalResource, int quantity)
        {
            string error = string.Empty;
            string token = string.Empty;

            logger.Debug(config.LSKey.Key, $"GetAvailabilityToken: locationNo:{locationNo}, productNo:{productNo}, activiyTime:{activiyTime}, optionalResource:{optionalResource}, quantity:{quantity}");
            bool ret = activityWS.GetAvailabilityToken(locationNo, productNo, ConvertTo.NavGetDate(activiyTime, false), ConvertTo.NavGetTime(activiyTime, false), optionalResource, quantity, ref token, ref error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return token;
        }

        public string ActivityInsertGroupReservation(Reservation req)
        {
            string error = string.Empty;
            string resNo = string.Empty;

            logger.Debug(config.LSKey.Key, "InsertGroupReservation - " + Serialization.ToXml(req, true));

            activityWS.InsertGroupReservation(ref resNo, req.ReservationType,
                                         XMLHelper.GetSQLNAVDate(req.ResDateFrom), XMLHelper.GetSQLNAVTime(req.ResTimeFrom), XMLHelper.GetSQLNAVDate(req.ResDateTo), XMLHelper.GetSQLNAVTime(req.ResTimeTo),
                                         XMLHelper.GetString(req.CustomerAccount), XMLHelper.GetString(req.Description), XMLHelper.GetString(req.Comment),
                                         XMLHelper.GetString(req.Reference), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         XMLHelper.GetString(req.Location), XMLHelper.GetString(req.SalesPerson), req.Internalstatus, XMLHelper.GetString(req.Status),req.NoOfPerson,
                                         ref error);
            logger.Debug(config.LSKey.Key, "InsertGroupReservation - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return resNo;
        }

        public string ActivityUpdateGroupReservation(Reservation req)
        {
            string error = string.Empty;
            string resNo = string.Empty;

            logger.Debug(config.LSKey.Key, "UpdateGroupReservation - " + Serialization.ToXml(req, true));

            activityWS.UpdateGroupReservation(req.Id, req.ReservationType,
                                         XMLHelper.GetSQLNAVDate(req.ResDateFrom), XMLHelper.GetSQLNAVTime(req.ResTimeFrom), XMLHelper.GetSQLNAVDate(req.ResDateTo), XMLHelper.GetSQLNAVTime(req.ResTimeTo),
                                         XMLHelper.GetString(req.CustomerAccount), XMLHelper.GetString(req.Description), XMLHelper.GetString(req.Comment),
                                         XMLHelper.GetString(req.Reference), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email),
                                         XMLHelper.GetString(req.Location), XMLHelper.GetString(req.SalesPerson), req.Internalstatus, XMLHelper.GetString(req.Status),req.NoOfPerson,
                                         ref error);
            logger.Debug(config.LSKey.Key, "UpdateGroupReservation - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return resNo;
        }

        public string ActivityUpdateGroupHeaderStatus(string groupNo, string statusCode)
        {
            string error = string.Empty;
            logger.Debug(config.LSKey.Key, "UpdateGroupHeaderStatus - groupNo:{0}", groupNo);

            activityWS.UpdateGroupHeaderStatus(groupNo, statusCode, ref error);
            logger.Debug(config.LSKey.Key, "UpdateGroupHeaderStatus - " + error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return error;
        }

        public ActivityResponse ActivityConfirmGroup(ActivityRequest req)
        {
            if (req == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "Request missing");

            string actId = string.Empty;
            string error = string.Empty;
            decimal price = 0;
            decimal discount = 0;
            decimal amount = 0;
            int grLineNo = 0;
            string cur = string.Empty;
            string bookgRef = string.Empty;
            string groupCode = string.Empty;
            string resNo = XMLHelper.GetString(req.ReservationNo);
            string item = string.Empty;

            if (LSCVersion >= new Version("20.2"))
            {
                logger.Debug(config.LSKey.Key, "ConfirmGroupActivityV2 - " + Serialization.ToXml(req, true));
                activityWS.ConfirmGroupActivityV2(XMLHelper.GetString(req.GroupNo), XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                         XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.SetGroupReservation), XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email), req.UnitPrice,
                                         ref grLineNo, ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item, XMLHelper.GetString(req.ContactAccount), XMLHelper.GetString(req.Token), req.SetGroupHeaderStatus);
            }
            else
            {
                logger.Debug(config.LSKey.Key, "ConfirmGroupActivity - " + Serialization.ToXml(req, true));
                activityWS.ConfirmGroupActivity(XMLHelper.GetString(req.GroupNo), XMLHelper.GetString(req.Location), XMLHelper.GetString(req.ProductNo), ConvertTo.NavGetDate(req.ActivityTime, false), ConvertTo.NavGetTime(req.ActivityTime, false), XMLHelper.GetString(req.ContactNo), XMLHelper.GetString(req.OptionalResource),
                                         XMLHelper.GetString(req.OptionalComment), req.Quantity, req.NoOfPeople, req.Paid, XMLHelper.GetString(req.SetGroupReservation), XMLHelper.GetString(req.PromoCode), XMLHelper.GetString(req.ContactName), XMLHelper.GetString(req.Email), req.UnitPrice,
                                         ref grLineNo, ref actId, ref error, ref price, ref discount, ref amount, ref cur, ref bookgRef, ref resNo, ref item, XMLHelper.GetString(req.ContactAccount), XMLHelper.GetString(req.Token));
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
                ItemNo = item,
                GroupLineNo = grLineNo
            };

            logger.Debug(config.LSKey.Key, "ActivityResponse - " + Serialization.ToXml(result, true));
            return result;
        }

        public bool ActivityDeleteGroup(string groupNo, int lineNo)
        {
            if (string.IsNullOrEmpty(groupNo))
                throw new LSOmniException(StatusCode.ObjectMissing, "Group No missing");

            string error = string.Empty;
            bool ret = false;

            logger.Debug(config.LSKey.Key, $"DeleteGroupActivity: groupNo:{groupNo} line:{lineNo}");

            ret = activityWS.DeleteGroupActivity(groupNo, lineNo, ref error);
            if (string.IsNullOrEmpty(error) == false)
                throw new LSOmniServiceException(StatusCode.NavWSError, error);

            return ret;
        }

        #endregion

        #region Data Get (Replication)

        public List<ActivityProduct> ActivityProductsGet()
        {
            LSActivity.ActivityUploadProducts root = new LSActivity.ActivityUploadProducts();
            activityWS.UploadActivityProducts(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityProducts Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToActivityProducts(root);
        }

        public List<ActivityType> ActivityTypesGet()
        {
            LSActivity.ActivityUploadTypes root = new LSActivity.ActivityUploadTypes();
            activityWS.UploadActivityTypes(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityTypes Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToActivityType(root);
        }

        public List<ActivityLocation> ActivityLocationsGet()
        {
            LSActivity.ActivityUploadLocations root = new LSActivity.ActivityUploadLocations();
            activityWS.UploadActivityLocations(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityLocations Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToActivityLocation(root);
        }

        public List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate)
        {
            logger.Debug(config.LSKey.Key, "GetActReservations Request - ResNo:{0} LocNo:{1} FromDate:{2}", reservationNo, locationNo, fromDate);

            LSActivity.ActivityUploadResHeaders root = new LSActivity.ActivityUploadResHeaders();
            activityWS.GetActReservations(reservationNo, locationNo, reservationType, status, fromDate, ref root);

            logger.Debug(config.LSKey.Key, "GetActReservations Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToResHeader(root);
        }

        public List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType)
        {
            LSActivity.ActivityUploadReservations root = new LSActivity.ActivityUploadReservations();
            if (string.IsNullOrWhiteSpace(reservationNo))
            {
                logger.Debug(config.LSKey.Key, "UploadClientBookingsV2: contactNo:{0}, ResNo:{1}, actType:{2}", contactNo, reservationNo, activityType);
                activityWS.UploadClientBookingsV2(contactNo, activityType, ref root);
                logger.Debug(config.LSKey.Key, "UploadClientBookingsV2 Response - " + Serialization.ToXml(root, true));
            }
            else
            {
                logger.Debug(config.LSKey.Key, "UploadReservationActivities: contactNo:{0}, ResNo:{1}, actType:{2}", contactNo, reservationNo, activityType);
                activityWS.UploadReservationActivities(reservationNo, ref root);
                logger.Debug(config.LSKey.Key, "UploadReservationActivities Response - " + Serialization.ToXml(root, true));
            }

            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToReservations(root);
        }

        public List<Promotion> ActivityPromotionsGet()
        {
            LSActivity.ActivityUploadPromotions root = new LSActivity.ActivityUploadPromotions();
            activityWS.UploadPromotions(ref root);

            logger.Debug(config.LSKey.Key, "UploadPromotions Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToPromotions(root);
        }

        public List<Allowance> ActivityAllowancesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, "UploadPurchasedAllowances: contactNo:{0}", contactNo);

            LSActivity.ActivityUploadAllowance root = new LSActivity.ActivityUploadAllowance();
            activityWS.UploadPurchasedAllowances(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadPurchasedAllowances Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAllowances(root);
        }

        public List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo)
        {
            logger.Debug(config.LSKey.Key, "UploadCustomerEntries: contactNo:{0}, customerNo:{1}", contactNo, customerNo);

            LSActivity.ActivityCustomerEntries root = new LSActivity.ActivityCustomerEntries();
            activityWS.UploadCustomerEntries(contactNo, customerNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadCustomerEntries Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToCustomerEntry(root);
        }

        public List<MemberProduct> ActivityMembershipProductsGet()
        {
            LSActivity.ActivityMembershipProducts root = new LSActivity.ActivityMembershipProducts();
            activityWS.UploadMembershipProducts(ref root);

            logger.Debug(config.LSKey.Key, "UploadMembershipProducts Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToMemberProduct(root);
        }

        public List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, "UploadMembershipSubscriptionCharges: contactNo:{0}", contactNo);

            LSActivity.ActivitySubscriptionEntries root = new LSActivity.ActivitySubscriptionEntries();
            activityWS.UploadMembershipSubscriptionCharges(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadMembershipSubscriptionCharges Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToSubscriptionEntry(root);
        }

        public List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, "UploadAdmissionEntries: contactNo:{0}", contactNo);

            LSActivity.ActivityAdmissionEntries root = new LSActivity.ActivityAdmissionEntries();
            activityWS.UploadAdmissionEntries(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadAdmissionEntries Response - " + Serialization.ToXml(root, true));
            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            return map.MapRootToAdmissionEntry(root);
        }

        public List<Membership> ActivityMembershipsGet(string contactNo)
        {
            logger.Debug(config.LSKey.Key, "UploadMembershipEntries: contactNo:{0}", contactNo);

            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            LSActivity.ActivityUploadMemberships root = new LSActivity.ActivityUploadMemberships();
            activityWS.UploadMembershipEntries(contactNo, ref root);

            logger.Debug(config.LSKey.Key, "UploadMembershipEntries Response - " + Serialization.ToXml(root, true));
            return map.MapRootToMemberships(root);
        }

        public List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate)
        {
            logger.Debug(config.LSKey.Key, "UploadResourceActivities: locNo:{0}, resNo:{1}, fromDate:{2}, toDate:{3}", 
                locationNo, resourceNo, fromDate, toDate);

            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            LSActivity.ActivityUploadReservations root = new LSActivity.ActivityUploadReservations();
            activityWS.UploadResourceActivities(locationNo, resourceNo, fromDate, toDate, ref root);

            logger.Debug(config.LSKey.Key, "UploadResourceActivities Response - " + Serialization.ToXml(root, true));
            return map.MapRootToReservations(root);
        }

        public List<ActivityResource> ActivityResourceGet()
        {
            logger.Debug(config.LSKey.Key, "UploadActivityResources");

            ActivityMapping map = new ActivityMapping(LSCVersion, config.IsJson);
            LSActivity.ActivityUploadResources root = new LSActivity.ActivityUploadResources();
            activityWS.UploadActivityResources(ref root);

            logger.Debug(config.LSKey.Key, "UploadActivityResources Response - " + Serialization.ToXml(root, true));
            return map.MapRootToResource(root);
        }

        #endregion
    }
}
