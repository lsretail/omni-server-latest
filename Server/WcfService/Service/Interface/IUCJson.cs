using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.Service
{
    // Returns json
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/EComm/2017/Json")]
    [ServiceKnownType(typeof(StatusCode))]
    public interface IUCJson
    {
        // COMMON ////////////////////////////////////////////////////////////

        #region Helpers Common

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Ping();

        [OperationContract]
        [WebGet(UriTemplate = "/Ping", ResponseFormat = WebMessageFormat.Json)]
        string PingGet(); //REST http://localhost/LSCommerceService/ucjson.svc/ping

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OmniEnvironment Environment();

        #endregion

        #region Discount, Offers and GiftCards

        /// <summary>
        /// Get Published Offers for Member Card Id
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_MEMBER_DIR_MARK_INFO<p/>
        /// LS Central WS2 : GetDirectMarketingInfo<p/><p/>
        /// </remarks>
        /// <param name="cardId">Member Card Id to look for</param>
        /// <param name="itemId">Only show Offers for this item</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        /// <summary>
        /// Get related items in a published offer
        /// </summary>
        /// <param name="pubOfferId">Published offer id</param>
        /// <param name="numberOfItems">Number of items to return</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        /// <summary>
        /// Get discounts for items. Send in empty string for loyaltySchemeCode if getting anonymously.
        /// </summary>
        /// <param name="storeId">Store Id</param>
        /// <param name="itemiIds">List of item ids to check for discounts</param>
        /// <param name="loyaltySchemeCode">[OPTIONAL] Loyalty scheme code for a user</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemiIds, string loyaltySchemeCode);

        /// <summary>
        /// Get balance of a gift card.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetDataEntryBalance<p/><p/>
        /// </remarks>
        /// <param name="cardNo">Gift card number</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        GiftCard GiftCardGetBalance(string cardNo);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Advertisement> AdvertisementsGetById(string id, string contactId);

        #endregion

        #region Notification

        /// <summary>
        /// Get all Order Notification for a Contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_MEMBER_DIR_MARK_INFO<p/>
        /// LS Central WS2 : GetDirectMarketingInfo<p/><p/>
        /// </remarks>
        /// <param name="cardId">Card Id</param>
        /// <param name="numberOfNotifications">Number of notifications to return</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool NotificationsUpdateStatus(string cardId, List<string> notificationIds, NotificationStatus notificationStatus);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Notification NotificationGetById(string notificationId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool PushNotificationSave(PushNotificationRequest pushNotificationRequest);

        #endregion

        #region OneList

        /// <summary>
        /// Delete Basket or Wish List By OneList Id
        /// </summary>
        /// <param name="oneListId"></param>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId);

        /// <summary>
        /// Get Basket or all Wish Lists by Member Card Id
        /// </summary>
        /// <param name="cardId">Contact Id</param>
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);

        /// <summary>
        /// Get Basket or Wish List by OneList Id
        /// </summary>
        /// <param name="id">List Id</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string id, bool includeLines);

        /// <summary>
        /// Save Basket or Wish List
        /// </summary>
        /// <remarks>
        /// OneList can be saved, for both Member Contact and Anonymous Users.
        /// Member Contact can have one or more Member Cards and each Card can have one WishList and one Basket
        /// For Anonymous User, keep CardId empty and OneListSave will return OneList Id back that should be store with the session for the Anonymous user, 
        /// as LS Commerce Service does not store any information for Anonymous Users.<p/>
        /// Used OneListGetById to get the OneList back.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "oneList": {
        ///         "CardId": "10021",
        ///         "Items": [{
        ///             "ItemDescription": "Skirt Linda Professional Wear",
        ///             "ItemId": "40020",
        ///             "Quantity": 2,
        ///             "VariantDescription": "YELLOW/38",
        ///             "VariantId": "002"
        ///         }],
        ///         "ListType": 0,
        ///         "StoreId": "S0001"
        ///     },
        ///     "calculate": true
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">List Id</param>
        /// <param name="calculate">Perform Calculation on a Basket and save result with Basket</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListSave(OneList oneList, bool calculate);

        /// <summary>
        /// Calculates OneList Basket Object and returns Order Object
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : EcomCalculateBasket<p/><p/>
        /// This function can be used to send in Basket and convert it to Order.<p/>
        /// Basic Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// Basket with 2 of same Items
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "oneList": {
        /// 		"CardId": "10021",
        ///         "Items": [{
        ///             "ItemDescription": "Skirt Linda Professional Wear",
        ///             "ItemId": "40020",
        ///             "Quantity": 2,
        ///             "VariantDescription": "YELLOW/38",
        ///             "VariantId": "002"
        ///         }],
        /// 		"ListType": 0,
        /// 		"StoreId": "S0001"
        /// 	},
        /// 	"calculate": true
        /// }
        /// ]]>
        /// </code>
        /// Basket with 2 of same Items and with XMAS Coupon that gives discount 
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"oneList": {
        /// 		"CardId": "10021",
        ///         "Items": [{
        ///             "ItemDescription": "Skirt Linda Professional Wear",
        ///             "ItemId": "40020",
        ///             "Quantity": 2,
        ///             "VariantDescription": "YELLOW/38",
        ///             "VariantId": "002"
        ///         }],
        /// 		"PublishedOffers": [{
        /// 			"Id": "XMAS",
        /// 			"Type": "9",
        ///         }],
        /// 		"ListType": 0,
        /// 		"StoreId": "S0001"
        /// 	},
        /// 	"calculate": true
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">OneList Object</param>
        /// <returns>Order Object that can be used to Create Order</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OneListCalculate(OneList oneList);

        /// <summary>
        /// Calculates OneList Basket for Hospitality and returns Hospitality Order Object
        /// </summary>
        /// <param name="oneList"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderHosp OneListHospCalculate(OneList oneList);

        /// <summary>
        /// Add or remove Item in OneList without sending whole list
        /// </summary>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "onelistId":"1117AC57-10BD-4F7C-974D-FA19B1B027FB",
        ///     "item": {
        /// 	    "ItemDescription": "T-shirt Linda Wear",
        /// 	    "ItemId": "40045",
        ///         "Quantity": 1,
        ///         "VariantDescription": "BLACK/40",
        /// 	    "VariantId": "015"
        ///     },
        /// 	"remove": false,
        /// 	"calculate": true
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="onelistId">OneList Id</param>
        /// <param name="item">OneList Item to add or remove</param>
        /// <param name="remove">true if remove item, else false</param>
        /// <param name="calculate">Recalculate OneList</param>
        /// <returns>Updated OneList</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate);

        /// <summary>
        /// Link or remove a Member to/from existing OneList
        /// </summary>
        /// <param name="oneListId">OneList Id to link</param>
        /// <param name="cardId">Card Id to link or remove</param>
        /// <param name="email">Email address to look up Card Id when requesting a Linking</param>
        /// <param name="status">Link action</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListLinking(string oneListId, string cardId, string email, LinkStatus status);

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity available of items in order for certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : COQtyAvailabilityV2<p/><p/>
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="shippingOrder">true if order is to be shipped, false if click and collect</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request, bool shippingOrder);

        /// <summary>
        /// Create Customer Order for ClickAndCollect or BasketPostSales 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCreateVx<p/><p/>
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// Order to be shipped to Customer
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"request": {
        /// 		"CardId": "10021",
        /// 		"LineItemCount": "1",
        /// 		"OrderDiscountLines": [],
        /// 		"OrderLines": [{
        /// 			"Amount": "160.0",
        /// 			"DiscountAmount": "0",
        /// 			"DiscountPercent": "0",
        /// 			"ItemId": "40020",
        /// 			"LineNumber": "1",
        /// 			"LineType": "0",
        /// 			"NetAmount": "128.00",
        /// 			"NetPrice": "64.0",
        /// 			"Price": "80.0",
        /// 			"Quantity": "2",
        /// 			"QuantityOutstanding": "0",
        /// 			"QuantityToInvoice": "2.0",
        /// 			"QuantityToShip": "0",
        /// 			"TaxAmount": "32.0",
        /// 			"VariantId": "002"
        ///         }],
        /// 		"OrderPayments": [{
        /// 			"Amount": "160.0",
        /// 			"AuthorisationCode": "123456",
        /// 			"CardNumber": "10xx xxxx xxxx 1475",
        /// 			"CardType": "VISA",
        /// 			"CurrencyCode": "GBP",
        /// 			"CurrencyFactor": 1,
        /// 			"ExternalReference": "MyRef123",
        /// 			"LineNumber": 1,
        /// 			"PaymentType": "2",
        /// 			"PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        /// 			"TenderType": "1",
        /// 			"TokenNumber": "123456"
        ///         }],
        /// 		"OrderType": "0",
        /// 		"PaymentStatus": "10",
        /// 		"ShipToAddress": {
        /// 			"Address1": "600 Lue Via",
        /// 			"Address2": "None",
        /// 			"City": "North Viola",
        /// 			"Country": "Belgium",
        /// 			"PhoneNumber": "555-555-5555",
        /// 			"PostCode": "88391-4289",
        /// 			"StateProvinceRegion": "None",
        /// 			"Type": "0"
        /// 		},
        /// 		"ShippingStatus": "20",
        /// 		"SourceType": "2",
        /// 		"StoreId": "S0013",
        /// 		"TotalAmount": "160.0",
        /// 		"TotalDiscount": "0",
        /// 		"TotalNetAmount": "128.0"
        /// 	}
        /// }
        /// ]]>
        /// </code>
        /// Order with Manual 10% Discount to be shipped to Customer
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"request": {
        /// 		"CardId": "10021",
        /// 		"LineItemCount": "1",
        /// 		"OrderDiscountLines": [{
        /// 			"DiscountAmount": "16.0",
        /// 			"DiscountPercent": "10.0",
        /// 			"DiscountType": "4",
        /// 			"LineNumber": "1",
        /// 			"No": "10000"
        ///         }],
        /// 		"OrderLines": [{
        /// 			"Amount": "144.0",
        /// 			"DiscountAmount": "16.3",
        /// 			"DiscountPercent": "10.0",
        /// 			"ItemId": "40020",
        /// 			"LineNumber": "1",
        /// 			"LineType": "0",
        /// 			"NetAmount": "115.20",
        /// 			"NetPrice": "64.0",
        /// 			"Price": "80.0",
        /// 			"Quantity": "2",
        /// 			"QuantityOutstanding": "0",
        /// 			"QuantityToInvoice": "2.0",
        /// 			"QuantityToShip": "0",
        /// 			"TaxAmount": "28.0",
        /// 			"VariantId": "002"
        ///         }],
        /// 		"OrderPayments": [{
        /// 			"Amount": "160.0",
        /// 			"AuthorisationCode": "123456",
        /// 			"CardNumber": "10xx xxxx xxxx 1475",
        /// 			"CardType": "VISA",
        /// 			"CurrencyCode": "GBP",
        /// 			"CurrencyFactor": 1,
        /// 			"ExternalReference": "MyRef123",
        /// 			"LineNumber": 1,
        /// 			"PaymentType": "2",
        /// 			"PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        /// 			"TenderType": "1",
        /// 			"TokenNumber": "123456"
        ///         }],
        /// 		"OrderType": "0",
        /// 		"PaymentStatus": "10",
        /// 		"ShipToAddress": {
        /// 			"Address1": "600 Lue Via",
        /// 			"Address2": "None",
        /// 			"City": "North Viola",
        /// 			"Country": "Belgium",
        /// 			"PhoneNumber": "555-555-5555",
        /// 			"PostCode": "88391-4289",
        /// 			"StateProvinceRegion": "None",
        /// 			"Type": "0"
        /// 		},
        /// 		"ShippingStatus": "20",
        /// 		"SourceType": "2",
        /// 		"StoreId": "S0013",
        /// 		"TotalAmount": "144.0",
        /// 		"TotalDiscount": "16.0",
        /// 		"TotalNetAmount": "115.20"
        /// 	}
        /// }
        /// ]]>
        /// </code>
        /// Order 3 Different payments, Credit Card, Loyalty Points (Currency is LOY and CardNumber is MemberContact Card ID) and Gift Card (CardNumber is the Gift Card Id)
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"request": {
        /// 		"CardId": "10021",
        /// 		"LineItemCount": "1",
        /// 		"OrderDiscountLines": [],
        /// 		"OrderLines": [{
        /// 			"Amount": "160.0",
        /// 			"DiscountAmount": "0",
        /// 			"DiscountPercent": "0",
        /// 			"ItemId": "40020",
        /// 			"LineNumber": "1",
        /// 			"LineType": "0",
        /// 			"NetAmount": "128.00",
        /// 			"NetPrice": "64.0",
        /// 			"Price": "80.0",
        /// 			"Quantity": "2",
        /// 			"QuantityOutstanding": "0",
        /// 			"QuantityToInvoice": "2.0",
        /// 			"QuantityToShip": "0",
        /// 			"TaxAmount": "32.0",
        /// 			"VariantId": "002"
        ///         }],
        ///        	"OrderPayments": [{
        /// 			"Amount": "160.0",
        /// 			"AuthorisationCode": "123456",
        /// 			"CardNumber": "10xx xxxx xxxx 1475",
        /// 			"CardType": "VISA",
        /// 			"CurrencyCode": "GBP",
        /// 			"CurrencyFactor": 1,
        /// 			"ExternalReference": "MyRef123",
        /// 			"LineNumber": 1,
        /// 			"PaymentType": "2",
        /// 			"PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        /// 			"TenderType": "1",
        /// 			"TokenNumber": "123456"
        ///         },
        ///         {
        ///        		"Amount": "200.0",
        ///        		"CardNumber": "10021",
        ///        		"CurrencyCode": "LOY",
        ///        		"CurrencyFactor": "0.1",
        ///        		"LineNumber": 2,
        ///       		"TenderType": "3"
        ///         },
        ///         {
        ///        		"Amount": "20.0",
        ///        		"CardNumber": "123456",
        ///        		"CurrencyCode": "GBP",
        ///        		"CurrencyFactor": "1.0",
        ///        		"LineNumber": 3,
        ///       		"TenderType": "4"
        ///         }],
        /// 		"OrderType": "0",
        /// 		"PaymentStatus": "10",
        /// 		"ShipToAddress": {
        /// 			"Address1": "600 Lue Via",
        /// 			"Address2": "None",
        /// 			"City": "North Viola",
        /// 			"Country": "Belgium",
        /// 			"PhoneNumber": "555-555-5555",
        /// 			"PostCode": "88391-4289",
        /// 			"StateProvinceRegion": "None",
        /// 			"Type": "0"
        /// 		},
        /// 		"ShippingStatus": "20",
        /// 		"SourceType": "2",
        /// 		"StoreId": "S0013",
        /// 		"TotalAmount": "160.0",
        /// 		"TotalDiscount": "0",
        /// 		"TotalNetAmount": "128.0"
        /// 	}
        /// }
        /// ]]>
        /// </code>
        /// Order to be collected at Store S0001
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"request": {
        /// 		"CardId": "10021",
        /// 		"CollectLocation": "S0001",
        /// 		"LineItemCount": "1",
        /// 		"OrderDiscountLines": [],
        /// 		"OrderLines": [{
        /// 			"Amount": "160.0",
        /// 			"DiscountAmount": "0",
        /// 			"DiscountPercent": "0",
        /// 			"ItemId": "40020",
        /// 			"LineNumber": "1",
        /// 			"LineType": "0",
        /// 			"NetAmount": "128.00",
        /// 			"NetPrice": "64.0",
        /// 			"Price": "80.0",
        /// 			"Quantity": "2",
        /// 			"QuantityOutstanding": "0",
        /// 			"QuantityToInvoice": "2.0",
        /// 			"QuantityToShip": "0",
        /// 			"TaxAmount": "32.0",
        /// 			"VariantId": "002"
        ///         }],
        /// 		"OrderPayments": [],
        /// 		"OrderType": "1",
        /// 		"PaymentStatus": "0",
        /// 		"ShipToAddress": {},
        /// 		"ShippingStatus": "10",
        /// 		"SourceType": "2",
        /// 		"StoreId": "S0013",
        /// 		"TotalAmount": "160.0",
        /// 		"TotalDiscount": "0",
        /// 		"TotalNetAmount": "128.0"
        /// 	}
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>SalesEntry object for order if order creation was successful</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry OrderCreate(Order request);

        /// <summary>
        /// Create a Hospitality Order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry OrderHospCreate(OrderHosp request);

        /// <summary>
        /// Cancel hospitality order
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool HospOrderCancel(string storeId, string orderId);

        /// <summary>
        /// Get Order status for hospitality order
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderHospStatus HospOrderStatus(string storeId, string orderId);

        /// <summary>
        /// Check Status of a Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderStatus<p/><p/>
        /// </remarks>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderStatusResponse OrderStatusCheck(string orderId);

        /// <summary>
        /// Cancel Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCancel<p/><p/>
        /// </remarks>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId">Web Store Id</param>
        /// <param name="userId">User who cancels the order, use Contact ID for logged in user</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OrderCancel(string orderId, string storeId, string userId);

        /// <summary>
        /// Get All Sales Entries (Transactions and Orders) by card Id
        /// </summary>
        /// <param name="cardId">Card Id</param>
        /// <param name="maxNumberOfEntries">max number of transactions returned</param>
        /// <returns>List of most recent Transactions for a contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfEntries);

        /// <summary>
        /// Get All Sales Entries (Transactions and Orders) by Card Id and optional filter by Store Id and Registration Date
        /// </summary>
        /// <param name="cardId">Card Id (Required)</param>
        /// <param name="storeId">Filter by Store Id</param>
        /// <param name="date">Filter by Registration Date.  Set Date value to MinValue (0001-01-01) to skip Date Filtering</param>
        /// <param name="dateGreaterThan">Get Entries Greater (true) or Less (false) than Filter Date</param>
        /// <param name="maxNumberOfEntries">max number of transactions returned</param>
        /// <returns>List of most recent Transactions for a contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SalesEntry> SalesEntriesGetByCardIdEx(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries);

        /// <summary>
        /// Get the Sale details (order/transaction)
        /// </summary>
        /// <param name="entryId">Sales Entry ID</param>
        /// <param name="type">Document Id type of the Sale entry</param>
        /// <returns>SalesEntry with Lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type);

        #endregion

        #region Contact

        /// <summary>
        /// Create a new contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberContactCreate<p/><p/>
        /// Contact will get new Card that should be used when dealing with Orders.  Card Id is the unique identifier for Contacts in LS Central<p/>
        /// Contact will be assigned to a Member Account.
        /// Member Account has Club and Club has Scheme level.<p/>
        /// If No Account is provided, New Account will be created.
        /// If No Club level for the Account is set, then default Club and Scheme level will be set.<p/>
        /// Valid UserName, Password and Email address is determined by LS Central and can be found in CU 99009001.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"contact": {
        /// 		"Id": "",
        /// 		"Account": {
        /// 			"Id": "",
        /// 			"Scheme": {
        /// 				"Id": "",
        /// 				"Club": {
        /// 					"Id": "CRONUS"
        /// 				}
        /// 			}
        /// 		},
        /// 		"Addresses": [{
        /// 			"Address1": "Santa Monica",
        ///             "CellPhoneNumber": "555-5551",
        /// 			"City": "Hollywood",
        /// 			"Country": "US",
        ///             "PhoneNumber": "666-6661",
        /// 			"PostCode": "1001",
        /// 			"StateProvinceRegion": "",
        /// 			"Type": "0"
        ///         }],
        /// 		"Email": "Sarah@Hollywood.com",
        /// 		"FirstName": "Sarah",
        /// 		"Gender": "2",
        /// 		"Initials": "Ms",
        /// 		"LastName": "Parker",
        /// 		"MaritalStatus": "0",
        /// 		"MiddleName": "",
        /// 		"Name": "Sarah Parker",
        /// 		"Password": "SxxInTheCity",
        /// 		"UserName": "sarah"
        /// 	}
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="contact">contact</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.EmailInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNameInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNameExists</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.MissingLastName</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.MissingFirstName</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccountNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactCreate(MemberContact contact);

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberContactUpdate<p/><p/>
        /// Contact Id, User name and EMail are required values for the update command to work.<p/>
        /// Any field left out or sent in empty will wipe out that information. Always fill out all 
        /// Name field, Address and phone number even if it has not changed so it will not be wiped out from LS Central
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///   "contact": {
        ///     "Id": "MO000012",
        ///     "Addresses": [{
        ///       "Address1": "Santa Monica",
        ///       "CellPhoneNumber": "555-5551",
        ///       "City": "Hollywood",
        ///       "Country": "US",
        ///       "PhoneNumber": "666-6661",
        ///       "PostCode": "1001",
        ///       "StateProvinceRegion": "",
        ///       "Type": "0"
        ///     }],
        ///     "Email": "Sarah@Hollywood.com",
        ///     "FirstName": "Sarah",
        ///     "Gender": "2",
        ///     "Initials": "Ms",
        ///     "LastName": "Parker",
        ///     "MaritalStatus": "0",
        ///     "MiddleName": "",
        ///     "Name": "Sarah Parker",
        ///     "Password": "SxxInTheCity",
        ///     "UserName": "sarah"
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="contact">contact</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ParameterInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.EmailInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactUpdate(MemberContact contact);

        /// <summary>
        /// Get contact by contact Id
        /// </summary>
        /// <param name="cardId">Card Id</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetByCardId(string cardId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        /// <summary>
        /// Add new card to existing Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberCardToContact<p/><p/>
        /// </remarks>
        /// <param name="contactId"></param>
        /// <param name="cardId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        double ContactAddCard(string contactId, string cardId, string accountId);

        /// <summary>
        /// Get Point balance for Member Card
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberCard<p/><p/>
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        long CardGetPointBalance(string cardId);

        /// <summary>
        /// Get Point entries for Member Card
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PointEntry> CardGetPointEnties(string cardId, DateTime dateFrom);

        /// <summary>
        /// Gets Rate value for points (f.ex. 1 point = 0.01 Kr)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        decimal GetPointRate();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);

        /// <summary>
        /// Change password
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordChange<p/><p/>
        /// </remarks>
        /// <param name="userName">user name (LS Central:LoginID)</param>
        /// <param name="newPassword">new password (LS Central:NewPassword)</param>
        /// <param name="oldPassword">old password (LS Central:OldPassword)</param>
        /// <returns>true/false</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordOldInvalid</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ChangePassword(string userName, string newPassword, string oldPassword);

        /// <summary>
        /// Request a ResetCode to use in Email to send to Member Contact
        /// </summary>
        /// <remarks>
        /// Settings for this function are found in LS Commerce Service Database - TenantConfig table
        /// <ul>
        /// <li>forgotpassword_code_encrypted: Reset Code is Encrypted</li>
        /// </ul>
        /// </remarks>
        /// <param name="userNameOrEmail">User Name or Email Address</param>
        /// <returns>ResetCode</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.UserNameNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeInvalid</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ForgotPassword(string userNameOrEmail);

        /// <summary>
        /// Send in Reset Password request for Member contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordReset<p/><p/>
        /// If anything fails, simply ask the user to go through the ForgotPassword again..<p/>
        /// Error PasswordInvalid = ask user for better password<p/>
        /// Error ParameterInvalid = ask user for correct userName since it does not match resetCode<p/>
        /// All other errors should as the user to go through the forgotPassword flow again
        /// </remarks>
        /// <param name="userName"></param>
        /// <param name="resetCode">Reset Code returned from ForgotPassword</param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.UserNameNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeExpired</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ParameterInvalid</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ResetPassword(string userName, string resetCode, string newPassword);

        /// <summary>
        /// Reset current password or request new password for new Member Contact.  
        /// Send either login or email depending on which function is required.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordReset<p/><p/>
        /// </remarks>
        /// <param name="userName">Provide Login Id (UserName) to reset existing password</param>
        /// <param name="email">Provide Email to create new login password for new Member Contact</param>
        /// <returns>Token to be included in Email to send to Member Contact.  Send the token with PasswordChange function</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string PasswordReset(string userName, string email);

        /// <summary>
        /// Change password for Member Contact.
        /// Call PasswordReset first if oldPassword is unknown or no login/password exist for Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordChange<p/><p/>
        /// To change password for existing contact: Send userName, newPassword, oldPassword<p/>
        /// To reset password for existing contact: Send userName, token, newPassword<p/>
        /// To register new login/password for new contact: Send userName, token, newPassword<p/>
        /// </remarks>
        /// <param name="userName">Login Id or UserName</param>
        /// <param name="token">Token from PasswordReset</param>
        /// <param name="newPassword">New Password</param>
        /// <param name="oldPassword">Previous Password</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool PasswordChange(string userName, string token, string newPassword, string oldPassword);

        /// <summary>
        /// Change Login Id for Member Contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_LOGIN_CHANGE<p/><p/>
        /// </remarks>
        /// <param name="oldUserName">Current Login Id</param>
        /// <param name="newUserName">New Login Id</param>
        /// <param name="password">Current Password</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool LoginChange(string oldUserName, string newUserName, string password);

        /// <summary>
        /// Login user
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberLogon<p/><p/>
        /// </remarks>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <param name="deviceId">device Id. Should be empty for non device user (web apps)</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AuthFailed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.InvalidPassword</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.LoginIdNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact Login(string userName, string password, string deviceId);

        /// <summary>
        /// Login user from web page.  This function is light version of Login and returns less data.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberLogon<p/><p/>
        /// </remarks>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <returns>Returns contact but only Contact and Card object have data</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AuthFailed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.InvalidPassword</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.LoginIdNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact LoginWeb(string userName, string password);

        #endregion

        #region Item

        /// <summary>
        /// Get stock status of an item from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetItemInventory<p/><p/>
        /// If storeId is empty, only store that are marked in LS Central with check box Loyalty or Mobile checked (Omni Section) will be returned
        /// </remarks>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="itemId">Item to get status for</param>
        /// <param name="variantId">Item variant</param>
        /// <param name="arrivingInStockInDays">Item Date Filter</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays);

        /// <summary>
        /// Get stock status for list of items from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetInventoryMultiple<p/><p/>
        /// If storeId is empty, all store that have item available will be returned
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"items": [{
        /// 		"ItemId": "40020",
        /// 		"VariantId": "010"
        ///     }],
        /// 	"storeId": "S0001"
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="items">Items to get status for</param>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId);

        /// <summary>
        /// Get stock status for list of items from Store and/or Sourcing Location
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetInventoryMultipleV2<p/><p/>
        /// If storeId is empty, all store that have item available will be returned.
        /// If locationId is set, only status for that location will be shown (with or without storeId)
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"items": [{
        /// 		"ItemId": "40020",
        /// 		"VariantId": "010"
        ///     }],
        ///     "locationId": "W0001",
        /// 	"storeId": "S0001",
        /// 	"useSourcingLocation": 0
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="items">Items to get status for</param>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="locationId">Sourcing Location to get status for</param>
        /// <param name="useSourcingLocation">Get Inventory status from all Sourcing Locations</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStoreGetEx(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation);

        /// <summary>
        /// Gets Hospitality Kitchen Current Availability
        /// </summary>
        /// <param name="request">List of items to get, if empty, get all items</param>
        /// <param name="storeId">Store to get, if empty get all stores</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails);

        /// <summary>
        /// Lookup Item
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetById(string itemId, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetByBarcode(string barcode, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsPage(string storeId, int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        /// <summary>
        /// Gets Hierarchy setup for a Store with all Leafs and Nodes
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetHierarchy, GetHierarchyNode<p/><p/>
        /// It is recommended for large hierarchies to use the hierarchy replication functions.
        /// It will give option on getting only changes, instead of always have to get the whole hierarchy like this function does.
        /// </remarks>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion

        #region Menu

        /// <summary>
        /// Load Hospitality Menu
        /// </summary>
        /// <param name="storeId">Store to load, empty loads all</param>
        /// <param name="salesType">Sales type to load, empty loads all</param>
        /// <param name="loadDetails">Load Item Details and Image data</param>
        /// <param name="imageSize">Size of Image if loadDetails is set to true</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MobileMenu MenuGet(string storeId, string salesType, bool loadDetails, ImageSize imageSize);

        #endregion menu

        #region Profile

        /// <summary>
        /// Gets all Member Attributes that are available to assign to a Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MobileGetProfiles<p/><p/>
        /// Only Member Attributes of type Boolean and Lookup Type None and are valid in Default Club will be selected
        /// </remarks>
        /// <returns>List of Member Attributes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetAll();

        /// <summary>
        /// Gets all Member Attributes for a Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberCard<p/><p/>
        /// Member Attribute Value has to have Value Yes or No, even in other languages as LS Commerce Service uses that Text to determent if the Attribute is selected or not for the Contact
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetByCardId(string cardId);

        #endregion

        #region Images

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ImageView ImageGetById(string id, ImageSize imageSize);

        [OperationContract]
        [WebGet(UriTemplate = "/ImageStreamGetById?id={id}&width={width}&height={height}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ImageStreamGetById(string id, int width, int height);

        #endregion

        #region Account

        /// <summary>
        /// Get all schemes in system
        /// </summary>
        /// <returns>List of schemes</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Scheme> SchemesGetAll();

        #endregion

        #region Store Location

        /// <summary>
        /// Get store by Store Id
        /// </summary>
        /// <remarks>
        /// Data for Store Hours needs to be generated in LS Central by running COMMERCE_XXXX Scheduler Jobs
        /// </remarks>
        /// <param name="storeId">store Id</param>
        /// <returns>Store</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Store StoreGetById(string storeId);

        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// Data for Store Hours needs to be generated in LS Central by running COMMERCE_XXXX Scheduler Jobs
        /// </remarks>
        /// <returns>List of stores</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetAll();

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude)
        /// </summary>
        /// <param name="latitude">current latitude</param>
        /// <param name="longitude">current longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers, 0 = no limit</param>
        /// <returns>List of stores marked as ClickAndCollect within max distance of coordinates</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance);

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude), that have the item available
        /// </summary>
        /// <param name="itemId">item Id</param>
        /// <param name="variantId">variant Id</param>
        /// <param name="latitude">current latitude</param>
        /// <param name="longitude">current longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers, 0 = no limit</param>
        /// <returns>List of stores marked as ClickAndCollect that have the item in stock</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance);

        /// <summary>
        /// Gets Return Policy
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="storeGroupCode"></param>
        /// <param name="itemCategory"></param>
        /// <param name="productGroup"></param>
        /// <param name="itemId"></param>
        /// <param name="variantCode"></param>
        /// <param name="variantDim1"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1);

        #endregion

        #region Replication

        /// <summary>
        /// Replicate Item Barcodes (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001451 - LSC Barcodes
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.  
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of barcodes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest);

        /// <summary>
        /// Replicate Currency setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 4 - Currency
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of currencies</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest);

        /// <summary>
        /// Replicate Currency Rate Setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 330 - Currency Exchange Rate
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of currency rates</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Extended Variants Setup (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001413 - LSC Extd. Variant Values
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variants</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Image links
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009064 - LSC Retail Image Link
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of image links</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Images
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009063 - LSC Retail Image
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of images</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplImageResponse ReplEcommImages(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Categories (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5722 - Item Category
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of item categories</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Items (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemResponse ReplEcommItems(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Unit of Measures (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5404 - Item Unit of Measure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of item unit of measures</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Variant Registrations (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001414 - LSC Item Variant Registration
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant registrations</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest);

        /// <summary>
        /// Replicate Best Prices for Items from WI Price table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012861 - LSC WI Price
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs.  
        /// This will generate the Best price for product based on date and offers available at the time.<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item and Sales Price tables are used to find deleted changes.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of prices</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplPriceResponse ReplEcommPrices(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Prices from Sales Price table (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 7002 - Sales Price
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of prices</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest);

        /// <summary>
        /// Replicate Product groups (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000705 - LSC Retail Product Group
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of product groups</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest);

        /// <summary>
        /// Replicate Store setups
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001470 - LSC Store
        /// <p/><p/>
        /// Only store with Loyalty or Mobile Checked will be replicated
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of stores</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplStoreResponse ReplEcommStores(ReplRequest replRequest);

        /// <summary>
        /// Replicate Unit of Measures
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 204 - Unit of Measure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of unit of measures</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Collection for Unit of Measures
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001430 - LSC Collection Framework
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Collection</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCollectionResponse ReplEcommCollection(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendors
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 23 - Vendor
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of vendors</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplVendorResponse ReplEcommVendor(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendor Item Mapping (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item (Lookup by [Vendor No_])
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of vendor item mappings</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attributes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000784 - LSC Attribute
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <example>
        /// Sample request can be used with all other Replication functions
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "replRequest": {
        ///         "BatchSize": 100,
        ///         "FullReplication": 1,
        ///         "LastKey": "",
        ///         "MaxKey": "",
        ///         "StoreId": "S0013",
        ///         "TerminalId": ""
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attributes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Values
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000786 - LSC Attribute Value
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attribute values</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Option Values
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000785 - LSC Attribute Option Value
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attribute option values</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000971 - LSC Data Translation
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation Language Codes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000972 - LSC Data Translation Language
        /// <p/><p/>
        /// This will always replicate all Code
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Codes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationLangCodeResponse ReplEcommDataTranslationLangCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Validation Scheduling data for Hierarchy
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000955 - LSC Validation Schedule
        /// <p/><p/>
        /// This function only checks if there are any available pre-actions for any of the tables involved in the Schedule data 
        /// and if there is, the whole Validation Schedule will be replicated again.
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Validation Schedule objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplValidationScheduleResponse ReplEcommValidationSchedule(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy roots
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000920 - LSC Hierarchy
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Hierarchy objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Nodes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000921 - LSC Hierar. Nodes
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy nodes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000922 - LSC Hierar. Node Link
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy node leafs</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deal lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001503 - LSC Offer Line
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deals</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyHospDealResponse ReplEcommHierarchyHospDeal(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deal lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001651 - LSC Deal Modifier Item
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deal lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyHospDealLineResponse ReplEcommHierarchyHospDealLine(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Recipe lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 90 - BOM Component
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy recipe items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemModifierResponse ReplEcommItemModifier(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Modifier lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001483 - LSC Information Subcode
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy modifier lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemRecipeResponse ReplEcommItemRecipe(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item with full detailed data (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item
        /// <p/><p/>
        /// FullItem replication includes all variants, unit of measures, attributes and prices for an item<p/>
        /// NOTE: It is recommended to replicate item data separately using<p/>
        /// ReplEcomm Item / Prices / ItemUnitOfMeasures / ItemVariantRegistrations / ExtendedVariants / Attribute / AttributeValue / AttributeOptionValue<p/>
        /// Price Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML, Sales Price, Item Variant, Item Unit of Measure, Variants and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Item objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts and MultiBuy for Items from WI Discount table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012862 - LSC WI Discounts
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest);

        /// <summary>
        /// Replicate Mix and Match Offers for Items from WI Mix and Match Offer table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012863 - LSC WI Mix and Match Offer
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplMixMatchResponse ReplEcommMixAndMatch(ReplRequest replRequest);

        /// <summary>
        /// Replicate Validation Periods for Discounts<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001481 - LSC Validation Period
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountValidationResponse ReplEcommDiscountValidations(ReplRequest replRequest);

        /// <summary>
        /// Replicate all Shipping agents and services
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 291 - Shipping Agent
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of shipping agents</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest);

        /// <summary>
        /// Replicate Member contacts
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009002 - LSC Member Contact (with valid Membership Card)
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Member contacts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCustomerResponse ReplEcommMember(ReplRequest replRequest);

        /// <summary>
        /// Replicate all Country Codes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 9 - Country/Region
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Country codes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Tender types for Store
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001466 - LSC Tender Type Setup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest);

        /// <summary>
        /// Replicate Tax setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 325 - VAT Posting Setup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplTaxSetupResponse ReplEcommTaxSetup(ReplRequest replRequest);

        /// <summary>
        /// Replicate Inventory Status
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001608 - LSC Inventory Lookup Table
        /// <p/><p/>
        /// Net Inventory field in Inventory Lookup Table must be updated before the replication can be done.  
        /// In Retail Product Group card, set up which products to check status for by click on Update POS Inventory Lookup button. Set store to be Web Store.
        /// Run Scheduler job with CodeUnit 10012871 - WI Update Inventory which will update the Net Inventory field.
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calls to LS Commerce Service.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest);

        #endregion

        #region Search

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SearchRs Search(string cardId, string search, SearchType searchTypes);

        #endregion search

        #region LS Recommends

        /// <summary>
        /// Checks if LS Recommend is active in LS Commerce Service
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool RecommendedActive();

        /// <summary>
        /// Get Recommended Items based of list of items
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<RecommendedItem> RecommendedItemsGet(List<string> items);

        #endregion

        #region Activity

        /// <summary>
        /// Confirm Activity Booking
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ConfirmActivityV2 or V3<p/><p/>
        /// If property [Paid] is set, then returns details for the retail basket.<p/>
        /// [BookingRef] should be assigned to the OrderLine and passed in with Order so retrieved basket payment through LS Commerce Service will update the Activities payment status and assign the sales order document as payment document.<p/> 
        /// If activity type does not require [contactNo] then it is sufficient to provide client name.<p/>
        /// If [ReservationNo] is blank the system will create new reservation and return the value to ReservationNo.  If ReservationNo is populated parameter then the system will try to add the activity to existing reservation if the reservation exists and is neither canceled or closed.<p/>
        /// [PromoCode] is validated and adjusts pricing accordingly.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///	  "request": {
        ///     "ActivityTime": "\/Date(1576011600000)\/",
        ///     "ContactName": "Tom",
        ///     "ContactNo": "MO000008",
        ///     "Location": "CAMBRIDGE",
        ///     "NoOfPeople": "1",
        ///     "Paid": "false",
        ///     "ProductNo": "MASSAGE30",
        ///     "Quantity": "1",
        ///     "ReservationNo": "RES0045"
        ///	  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>Activity Number and Booking Reference</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ActivityResponse ActivityConfirm(ActivityRequest request);

        /// <summary>
        /// Cancel Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelActivity<p/><p/>
        /// If cancellation charges apply, then those vales will be returned and could be applied to a retail basket.
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ActivityResponse ActivityCancel(string activityNo);

        /// <summary>
        /// Returns list of available time-slots/prices for a specific location,product and date 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAvailabilityV2<p/><p/>
        /// Optional to include required resource (if only specific resource) and contactNo for accurate pricing.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "locationNo": "CAMBRIDGE",
        ///     "productNo": "MASSAGE30",
        ///     "activityDate": "\/Date(1580580398000)\/",
        ///     "contactNo": "MO000008",
        ///     "optionalResource": "",
        ///     "promoCode": ""
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="contactNo"></param>
        /// <param name="contactAccount"></param>
        /// <param name="optionalResource"></param>
        /// <param name="promoCode"></param>
        /// <param name="activityNo"></param>
        /// <param name="noOfPersons"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons);

        /// <summary>
        /// Returns list with the required or optional additional charges for the Activity as applied automatically according to the product
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAdditionalCharges<p/><p/>
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        AdditionalCharge ActivityAdditionalChargesGet(string activityNo);

        /// <summary>
        /// Returns list of charges for products
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetProductChargesV2<p/><p/>
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="dateOfBooking"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        AdditionalCharge ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking);

        /// <summary>
        /// Change or insert additional charges to Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAdditionalChargesV2<p/><p/>
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///	  "request": {
        ///     "ActivityNo": "ACT0035",
        ///     "DiscountPercentage": "0.0",
        ///     "ItemNo": "40020",
        ///     "LineNo": "1",
        ///     "Price": "110.0",
        ///     "ProductType": "0",
        ///     "Quantity": "1",
        ///     "TotalAmount": "110.0",
        ///     "UnitOfMeasure": ""
        ///	  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityAdditionalChargesSet(AdditionalCharge request);

        /// <summary>
        /// Returns list of Attributes which are assigned on a given Activity product, reservation or activity entry
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAttributes<p/><p/>
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        AttributeResponse ActivityAttributesGet(AttributeType type, string linkNo);

        /// <summary>
        /// Action to set an attribute value on a given reservation or activity.  If attribute does not exist on the entry then its inserted otherwise updated
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAttribute<p/><p/>
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <param name="attributeCode"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue);

        /// <summary>
        /// Action to create a Reservation header into the LS Reservation table
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : InsertReservation<p/><p/>
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///	  "request": {
        ///	    "ClientName": "Tom",
        ///	    "ContactNo": "MO000008",
        ///	    "Description": "",
        ///	    "Email": "tom@xxx.com",
        ///	    "Internalstatus": "0",
        ///	    "Location": "CAMBRIDGE",
        ///	    "ResDateFrom": "\/Date(1570737600000)\/",
        ///	    "ResDateTo": "\/Date(1570741200000)\/",
        ///	    "ResTimeFrom": "\/Date(1570737600000)\/",
        ///	    "ResTimeTo": "\/Date(1570741200000)\/",
        ///	    "ReservationType": "SPA",
        ///	    "SalesPerson": "AH",
        ///	    "Status": ""
        ///	  }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityReservationInsert(Reservation request);

        /// <summary>
        /// Action to force update to a reservation header in the LS Reservation table.  Blank fields will be ignored
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateReservation<p/><p/>
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityReservationUpdate(Reservation request);

        /// <summary>
        /// Sell Membership (membership type) to Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SellMembership<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="membersShipType"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType);

        /// <summary>
        /// Cancels a specific membership and validates if cancellation is in order (i.e. compares to commitment period)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelMembership<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="memberShipNo"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment);

        /// <summary>
        /// Get availability for specific resource, for a specific date and location (all required parameters)
        /// Optional parameters - Interval Type - Use specific intervals setup in the system or leave blank for whole day
        /// NoOfDays - Set how many days to return availability, if set to 0 then use default setting (10 days normally)
        /// </summary>
        /// <param name="locationNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="resourceNo"></param>
        /// <param name="intervalType"></param>
        /// <param name="noOfDays"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays);

        /// <summary>
        /// Get availability for all active resource in specific resource group, for a specific date and location (all required parameters)
        /// Optional parameters - Interval Type - Use specific intervals setup in the system or leave blank for whole day
        /// NoOfDays - Set how many days to return availability, if set to 0 then use default setting (10 days normally)
        /// </summary>
        /// <param name="locationNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="groupNo"></param>
        /// <param name="intervalType"></param>
        /// <param name="noOfDays"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays);

        #endregion

        #region Activity Data Get (Replication)

        /// <summary>
        /// Returns list of Activity Products
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityProducts<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityProduct> ActivityProductsGet();

        /// <summary>
        /// Returns list of Activity Types
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityTypes<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityType> ActivityTypesGet();

        /// <summary>
        /// Returns list of Activity Locations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityLocations<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityLocation> ActivityLocationsGet();

        /// <summary>
        /// Returns list of Reservations for Member Contact or list of Activities assigned to a Reservation
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : <p/>
        /// With [contactNo, activityType] UploadClientBookingsV2<p/>
        /// With [reservationNo] : UploadReservationActivities<p/><p/>
        /// </remarks>
        /// <param name="reservationNo">Look up Activities for a Reservation</param>
        /// <param name="contactNo">Look up Reservations for a Contact</param>
        /// <param name="activityType">Activity type for Contact Lookup</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType);

        /// <summary>
        /// Look up Reservation Headers
        /// </summary>
        /// <param name="reservationNo"></param>
        /// <param name="reservationType"></param>
        /// <param name="status"></param>
        /// <param name="locationNo"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate);

        /// <summary>
        /// Returns list of Active Promotions (for information purposes only)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadPromotions<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Promotion> ActivityPromotionsGet();

        /// <summary>
        /// Returns list of Member Contacts issued (sold) allowances
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadPurchasedAllowances<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Allowance> ActivityAllowancesGet(string contactNo);

        /// <summary>
        /// Returns list of all entries charged to the Member Contact customer account (A/R). The Account no. is based on the contact business relation settings
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadCustomerEntries<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo);

        /// <summary>
        /// Returns list of Membership types (products) which are active and can be sold 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipProducts<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<MemberProduct> ActivityMembershipProductsGet();

        /// <summary>
        /// Returns list of all subscription charges posted towards their membership account. Draft unposted entries are not included
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipSubscriptionCharges<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo);

        /// <summary>
        /// Returns list of Member Contact visit registrations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadAdmissionEntries<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo);

        /// <summary>
        /// Returns list of the Member Contact current active or on hold memberships
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipEntries<p/><p/>
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Membership> ActivityMembershipsGet(string contactNo);

        /// <summary>
        /// Get list of activities assigned to a resource, required parameters Resource code (number), Date from and to date
        /// </summary>
        /// <param name="locationNo"></param>
        /// <param name="resourceNo"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get list of all resources 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityResource> ActivityResourceGet();

        #endregion

        #region ScanPayGo

        /// <summary>
        /// Creates a client token for payment provider
        /// </summary>
        /// <param name="customerId">Customer id, used to show saved cards</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ClientToken PaymentClientTokenGet(string customerId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool SecurityCheckProfile(string orderNo, string storeNo);

        #endregion
    }
}