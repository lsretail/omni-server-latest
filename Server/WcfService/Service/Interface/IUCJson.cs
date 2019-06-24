using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

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
        string PingGet(); //REST http://localhost/LSOmniService/Json.svc/ping

        #endregion

        #region Discount, Offers and GiftCards

        /// <summary>
        /// Get Published Offers for Member Card Id
        /// </summary>
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
        /// Get discounts for items. Send in empty string for loyaltyschemecode if getting anonymously.
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

        #region One List

        /// <summary>
        /// Delete Basket or Wish List By OneList Id
        /// </summary>
        /// <param name="oneListId"></param>
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId, ListType listType);

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
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string id, ListType listType, bool includeLines);

        /// <summary>
        /// Save Basket or Wish List
        /// </summary>
        /// <remarks>
        /// OneList can be saved, for both Registered Users and Anonymous Users.
        /// For Anonymous User, keep CardId and ContactId empty, 
        /// and OneListSave will return OneList Id back that should be store with the session for the Anonymous user, 
        /// as Omni does not store any information for Anonymous Users.<p/>
        /// Used OneListGetById to get the OneList back.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in OMNI
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"oneList": {
        /// 		"Id": null,
        /// 		"CardId": "10021",
        /// 		"Items": [{
        /// 			"Id": null,
        /// 			"Item": {
        /// 				"Id": "40020"
        ///             },
        /// 			"Quantity": 2,
        /// 			"VariantReg": {
        /// 				"Id": "002"
        /// 			}
        ///         }],
        /// 		"ListType": 0,
        /// 		"StoreId": "S0001"
        /// 	},
        /// 	"calculate": true
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
        /// This function can be used to send in Basket and convert it to Order.<p/>
        /// Basic Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in OMNI<p/>
        /// Basket with 2 of same Items
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"oneList": {
        /// 		"Id": null,
        /// 		"CardId": "10021",
        /// 		"Items": [{
        /// 			"Id": null,
        /// 			"Item": {
        /// 				"Id": "40020"
        ///             },
        /// 			"Quantity": 2,
        /// 			"VariantReg": {
        /// 				"Id": "002"
        /// 		    }
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
        /// 		"Id": null,
        /// 		"CardId": "10021",
        /// 		"Items": [{
        /// 			"Id": null,
        /// 			"Item": {
        /// 				"Id": "40020"
        /// 
        ///                },
        /// 			"Quantity": 2,
        /// 			"VariantReg": {
        /// 				"Id": "002"
        /// 			}
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

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity available of items in order for certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request);

        /// <summary>
        /// Create Customer Order for ClickAndCollect or BasketPostSales 
        /// </summary>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in OMNI<p/>
        /// Order to be shipped to Customer
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"request": {
        /// 		"AnonymousOrder": "false",
        /// 		"CardId": "10021",
        /// 		"ClickAndCollectOrder": "false",
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
        /// 			"AuthorisationCode": "123456",
        /// 			"CardNumber": "10xx xxxx xxxx 1475",
        /// 			"CardType": "VISA",
        /// 			"CurrencyCode": "GBP",
        /// 			"CurrencyFactor": 1,
        /// 			"FinalizedAmount": "0",
        /// 			"LineNumber": 1,
        /// 			"PreApprovedAmount": "160.0",
        /// 			"PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        /// 			"TenderType": "1"
        ///         }],
        /// 		"OrderStatus": "1",
        /// 		"PaymentStatus": "10",
        /// 		"ShipClickAndCollect": "false",
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
        /// 		"AnonymousOrder": "false",
        /// 		"CardId": "10021",
        /// 		"ClickAndCollectOrder": "false",
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
        /// 			"AuthorisationCode": "123456",
        /// 			"CardNumber": "10xx xxxx xxxx 1475",
        /// 			"CardType": "VISA",
        /// 			"CurrencyCode": "GBP",
        /// 			"CurrencyFactor": 1,
        /// 			"FinalizedAmount": "0",
        /// 			"LineNumber": 1,
        /// 			"PreApprovedAmount": "144.0",
        /// 			"PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        /// 			"TenderType": "1"
        ///         }],
        /// 		"OrderStatus": "1",
        /// 		"PaymentStatus": "10",
        /// 		"ShipClickAndCollect": "false",
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
        /// 		"AnonymousOrder": "false",
        /// 		"CardId": "10021",
        /// 		"ClickAndCollectOrder": "false",
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
        ///        		"AuthorisationCode": "123456",
        ///        		"CardNumber": "10xx xxxx xxxx 1475",
        ///        		"CardType": "VISA",
        ///        		"CurrencyCode": "GBP",
        ///        		"CurrencyFactor": "1.0",
        ///        		"FinalizedAmount": "0",
        ///        		"LineNumber": 1,
        ///        		"PreApprovedAmount": "120.0",
        ///        		"PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        ///        		"TenderType": "1"
        ///         },
        ///         {
        ///        		"CardNumber": "10021",
        ///        		"CurrencyCode": "LOY",
        ///        		"CurrencyFactor": "0.1",
        ///        		"LineNumber": 2,
        ///        		"PreApprovedAmount": "200.0",
        ///       		"TenderType": "3"
        ///         },
        ///         {
        ///        		"CardNumber": "123456",
        ///        		"CurrencyCode": "GBP",
        ///        		"CurrencyFactor": "1.0",
        ///        		"LineNumber": 3,
        ///        		"PreApprovedAmount": "20.0",
        ///       		"TenderType": "4"
        ///         }],
        /// 		"OrderStatus": "1",
        /// 		"PaymentStatus": "10",
        /// 		"ShipClickAndCollect": "false",
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
        /// 		"AnonymousOrder": "false",
        /// 		"CardId": "10021",
        /// 		"ClickAndCollectOrder": "true",
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
        /// 		"OrderStatus": "1",
        /// 		"PaymentStatus": "0",
        /// 		"ShipClickAndCollect": "false",
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
        /// Check Status of an Order made with BasketPostSale (LSNAV 10.02 and older)
        /// </summary>
        /// <param name="transactionId">Order GUID Id</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderStatusResponse OrderStatusCheck(string transactionId);

        /// <summary>
        /// Cancel Order made with BasketPostSale
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OrderCancel(string transactionId);

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
        /// Get the Sale details (order/transaction)
        /// </summary>
        /// <param name="entryId">Sales Entry ID</param>
        /// <param name="type">Document Id type of the Sale entry</param>
        /// <returns>SalesEntry with Lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type);

        #endregion

        #region OrderQueue

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderQueue OrderQueueSave(OrderQueue order);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderQueue OrderQueueGetById(string orderId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderQueueUpdateStatus(string orderId, OrderQueueStatus status);

        #endregion OrderQueue

        #region Contact

        /// <summary>
        /// Change password
        /// </summary>
        /// <remarks>
        /// Nav/Central WS: MM_MOBILE_PWD_CHANGE
        /// </remarks>
        /// <param name="userName">user name (Nav/Central:LoginID)</param>
        /// <param name="newPassword">new password (Nav/Central:NewPassword)</param>
        /// <param name="oldPassword">old password (Nav/Central:OldPassword)</param>
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
        /// Create a new contact
        /// </summary>
        /// <remarks>
        /// Nav/Central WS: MM_MOBILE_CONTACT_CREATE<p/><p/>
        /// Contact will get new Card that should be used when dealing with Orders.  Card Id is the unique identifier for Contacts in LS Nav/Central<p/>
        /// Contact will be assigned to a Member Account.
        /// Member Account has Club and Club has Scheme level.<p/>
        /// If No Account is provided, New Account will be created.
        /// If No Club level for the Account is set, then default Club and Scheme level will be set.<p/>
        /// Valid UserName, Password and Email address is determined by LS Nav/Central and can be found in CU 99009001.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in OMNI
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
        /// 			"City": "Hollywood",
        /// 			"Country": "US",
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
        /// 		"MobilePhone": "555-5551",
        /// 		"Name": "Sarah Parker",
        /// 		"Password": "SxxInTheCity",
        /// 		"Phone": "666-6661",
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        long CardGetPointBalance(string cardId);

        /// <summary>
        /// Gets Rate value for points (f.ex. 1 point = 0.01 kr)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        decimal GetPointRate();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <remarks>
        /// Nav/Central WS: MM_MOBILE_CONTACT_UPDATE<p/><p/>
        /// Contact Id, Username and EMail are required values for the update command to work.<p/>
        /// Any field left out or sent in empty will wipe out that information. Always fill out all 
        /// Name field, Address and phone number even if it has not changed so it will not be wiped out from LS Nav/Central
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in OMNI
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        /// 	"contact": {
        /// 		"Id": "MO000012",
        /// 		"Addresses": [{
        /// 			"Address1": "Santa Monica",
        /// 			"City": "Hollywood",
        /// 			"Country": "US",
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
        /// 		"MobilePhone": "555-5551",
        /// 		"Name": "Sarah Parker",
        /// 		"Password": "SxxInTheCity",
        /// 		"Phone": "666-6661",
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

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        double ContactAddCard(string contactId, string cardId, string accountId);

        /// <summary>
        /// Request a ResetCode to use in Email to send to Member Contact
        /// </summary>
        /// <remarks>
        /// Settings for this function are found in Omni Database - AppSettings table
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
        /// If anything fails, simply ask the user to go thru the forgotpassword again..<p/>
        /// Error PasswordInvalid = ask user for better pwd<p/>
        /// Error ParameterInvalid = ask user for correct username since it does not match resetcode<p/>
        /// All other errors should as the user to go thru the forgotPassword flow again
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

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);

        /// <summary>
        /// Login user
        /// </summary>
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
        /// Login user from web page.
        /// </summary>
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
        /// LS Nav/Central WS1 : MM_MOBILE_GET_ITEMS_IN_STOCK<p/>
        /// LS Nav/Central WS2 : GetItemInventory<p/><p/>
        /// If storeId is empty, only store that are marked in LS Nav/Central with check box Loyalty or Mobile checked (Omni Section) will be returned
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
        /// LS Nav/Central WS2 : GetInventoryMultiple<p/><p/>
        /// If storeId is empty, all store that have item available will be returned
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in OMNI
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
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="items">Items to get status for</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId);

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
        List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MobileMenu MenusGetAll(string id, string lastVersion);

        /// <summary>
        /// Gets Hierarchy setup for a Store with all Leafs and Nodes
        /// </summary>
        /// <remarks>
        /// It is recommended for large hierarchies to use the hierarchy replication functions.
        /// It will give option on getting only changes, instead of always have to get the whole hierarchy like this function does.
        /// </remarks>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion

        #region Profile

        /// <summary>
        /// Gets all Member Attributes that are available to assign to a Member Contact
        /// </summary>
        /// <remarks>
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
        /// Member Attribute Value has to have Value Yes or No, even in other languages as Omni Server uses that Text to determent if the Attribute is selected or not for the Contact
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetByCardId(string cardId);

        #endregion

        #region Basket LOY

        #endregion

        #region Images COMMON

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
        /// Data for Store Hours needs to be generated in LS Nav/Central by running OMNI_XXXX Scheduler Jobs
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
        /// Data for Store Hours needs to be generated in LS Nav/Central by running OMNI_XXXX Scheduler Jobs
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
        /// <param name="maxNumberOfStores">max number of stores returned</param>
        /// <returns>List of stores marked as ClickAndCollect within max distance of coords</returns>
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
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance, int maxNumberOfStores);

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude), that have the item available
        /// </summary>
        /// <param name="itemId">item Id</param>
        /// <param name="variantId">variant Id</param>
        /// <param name="latitude">current latitude</param>
        /// <param name="longitude">current longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers, 0 = no limit</param>
        /// <param name="maxNumberOfStores">max number of stores returned</param>
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
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, int maxNumberOfStores);

        #endregion

        #region Replication

        /// <summary>
        /// Replicate Item Barcodes (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99001451 - Barcodes
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.  
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 4 - Currency
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 330 - Currency Exchange Rate
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10001413 - Extended Variant Values
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 99009064 - Retail Image Link
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 99009063 - Retail Image
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 5722 - Item Category
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 27 - Item
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.<p/>
        /// For update, actions for Item, Item html and Distirbution tables are used to find changes,
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
        /// LS Nav/Central Main Table data: 5404 - Item Unit of Measure
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10001414 - Item Variant Registration
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant registrations</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest);

        /// <summary>
        /// Replicate Best Prices for Items from WI Price table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99009258 - WI Price
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs.  
        /// This will generate the Best price for product based on date and offers available at the time.<p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.<p/>
        /// For update, actions for Item,Sales Price,Item Variant and Item Unit of Measure tables are used to find changes,
        /// and it may return empty list of prices while records remaining are still not 0.  Keep on calling the function till Records Remaining become 0.
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
        /// LS Nav/Central Main Table data: 7002 - Sales Price
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 5723 - Product Group
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10012866 - Store
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 204 - Unit of Measure
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of unit of measures</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendors
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 23 - Vendor
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 27 - Item (Lookup by [Vendor No_])
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10000784 - Attribute
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attributes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Values
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10000786 - Attribute Value
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10000785 - Attribute Option Value
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10000971 - Data Translation
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy roots
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10000920 - Hierarchy (Where Hierarchy Date is active)
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10000921 - Hierarchy Nodes
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 10000922 - Hierarchy Node Link
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy node leafs</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item with full detailed data (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 27 - Item
        /// <p/><p/>
        /// FullItem replication includes all variants, unit of measures, attributes and prices for an item<p/>
        /// NOTE: It is recommended to replicate item data separately using<p/>
        /// ReplEcomm Item / Prices / ItemUnitOfMeasures / ItemVariantRegistrations / ExtendedVariants / Attribute / AttributeValue / AttributeOptionValue<p/>
        /// Price Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.<p/>
        /// For update, actions for Item, Item html, Sales Price, Item Variant, Item Unit of Measure, Variants and Distirbution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Item objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts and MultiBuy for Items from WI Discount table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10012862 - WI Discounts
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest);

        /// <summary>
        /// Replicate Mix and Match Offers for Items from WI Mix and Match Offer table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10012863 - WI Mix and Match Offer
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountResponse ReplEcommMixAndMatch(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts for Items from WI Discount table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distirbution to that store.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 291 - Shipping Agent
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 99009002 - Member Contact (with valid Membership Card)
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 9 - Country/Region
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
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
        /// LS Nav/Central Main Table data: 99001462 - Tender Type
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 325 - VAT Posting Setup
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// For full replication of all data, set FullReplication to true and lastkey and maxkey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI, both during full or delta replication.
        /// To reset replication and get all data again, set LastKey to 0 and perform a full replication.
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
        /// LS Nav/Central Main Table data: 99001608 - Inventory Lookup Table
        /// <p/><p/>
        /// Net Inventory field in Inventory Lookup Table must be updated before the replication can be done.  
        /// In Retail Product Group card, set up which products to check status for by click on Update POS Inventory Lookup button.
        /// Run Scheduler job with CodeUnit 10012871 - WI Update Inventory which will update the Net Inventory field.
        /// <p/><p/>
        /// All ReplEcommXX web methods work the same.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calles to OMNI.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest);

        #endregion

        #region search

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SearchRs Search(string cardId, string search, SearchType searchTypes);

        #endregion search

        #region LS Recommends

        /// <summary>
        /// Checks if LS Recommend is active in Omni Server
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool RecommendedActive();

        /// <summary>
        /// Not used anymore, support for older system
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="items"></param>
        /// <param name="maxNumberOfItems"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<RecommendedItem> RecommendedItemsGetByUserId(string userId, List<LoyItem> items, int maxNumberOfItems);

        /// <summary>
        /// Gets Recommended Items for Items
        /// </summary>
        /// <param name="userId">Member contact Id to get recommended Item for. Empty string for anonymous requests</param>
        /// <param name="storeId">Store Id</param>
        /// <param name="items">List of Items to get recommend items for.  Format of string: ITEMID,ITEMID,...)</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items);

        #endregion
    }
}