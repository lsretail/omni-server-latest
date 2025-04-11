﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

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
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.Service
{
    // Returns good old SOAP
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/EComm/2017/Service")]
    [ServiceKnownType(typeof(StatusCode))]
    public interface IUCService
    {
        // COMMON ////////////////////////////////////////////////////////////

        #region Helpers Common

        [OperationContract]
        string Ping();

        [OperationContract]
        OmniEnvironment Environment();

        #endregion

        #region Discount, Offers and GiftCards

        /// <summary>
        /// Get Published Offers for Member Card Id
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_MEMBER_DIR_MARK_INFO<p/>
        /// LS Central WS2 : GetDirectMarketingInfo
        /// </remarks>
        /// <param name="cardId">Member Card Id to look for</param>
        /// <param name="itemId">Only show Offers for this item</param>
        /// <returns></returns>
        [OperationContract]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        /// <summary>
        /// Get related items in a published offer
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_PUBLISHED_OFFER_ITEMS
        /// </remarks>
        /// <param name="pubOfferId">Published offer id</param>
        /// <param name="numberOfItems">Number of items to return</param>
        /// <returns></returns>
        [OperationContract]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        /// <summary>
        /// Get discounts for items. Send in empty string for loyaltySchemeCode if getting anonymously.
        /// </summary>
        /// <remarks>
        /// LS Central OData : GetDiscount
        /// </remarks>
        /// <param name="storeId">Store Id</param>
        /// <param name="itemIds">List of item ids to check for discounts</param>
        /// <param name="loyaltySchemeCode">[OPTIONAL] Loyalty scheme code for a user</param>
        /// <returns></returns>
        [OperationContract]
        List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode);

        /// <summary>
        /// Get balance of a gift card.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetDataEntryBalance
        /// </remarks>
        /// <param name="cardNo">Gift card number</param>
        /// <param name="pin">Gift card pin number</param>
        /// <param name="entryType">Gift card Entry type. If empty, GiftCard_DataEntryType from TenantConfig is used</param>
        /// <returns></returns>
        [OperationContract]
        GiftCard GiftCardGetBalance(string cardNo, int pin, string entryType);

        /// <summary>
        /// Get activity history for Gift Card
        /// </summary>
        /// <param name="cardNo">Gift card number</param>
        /// <param name="pin">Gift card pin number</param>
        /// <param name="entryType">Gift card Entry type. If empty, GiftCard_DataEntryType from TenantConfig is used</param>
        /// <returns></returns>
        [OperationContract]
        List<GiftCardEntry> GiftCardGetHistory(string cardNo, int pin, string entryType);

        [OperationContract]
        List<Advertisement> AdvertisementsGetById(string id, string contactId);

        #endregion

        #region Notification

        /// <summary>
        /// Get all Order Notification for a Contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_MEMBER_DIR_MARK_INFO<p/>
        /// LS Central WS2 : GetDirectMarketingInfo
        /// </remarks>
        /// <param name="cardId">Card Id</param>
        /// <param name="numberOfNotifications">Number of notifications to return</param>
        /// <returns></returns>
        [OperationContract]
        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications);

        [OperationContract]
        bool NotificationsUpdateStatus(string cardId, List<string> notificationIds, NotificationStatus notificationStatus);

        [OperationContract]
        Notification NotificationGetById(string notificationId);

        [OperationContract]
        bool PushNotificationSave(PushNotificationRequest pushNotificationRequest);

        #endregion

        #region OneList

        /// <summary>
        /// Delete Basket or Wish List By OneList Id
        /// </summary>
        /// <param name="oneListId"></param>
        [OperationContract]
        bool OneListDeleteById(string oneListId);

        /// <summary>
        /// Get Basket or all Wish Lists by Member Card Id
        /// </summary>
        /// <param name="cardId">Contact Id</param>
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);

        /// <summary>
        /// Get Basket or Wish List by OneList Id
        /// </summary>
        /// <param name="id">List Id</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        OneList OneListGetById(string id, bool includeLines);

        /// <summary>
        /// Save Basket or Wish List
        /// </summary>
        /// <remarks>
        /// OneList can be saved, for both Member Contact and Anonymous Users.
        /// Member Contact can have one or more Member Cards and each Card can have one WishList and one Basket
        /// For Anonymous User, keep CardId empty and OneListSave will return OneList Id back that should be store with the session for the Anonymous user, 
        /// as Commerce Service for LS Central does not store any information for Anonymous Users.<p/>
        /// Used OneListGetById to get the OneList back.<p/>
        /// NOTE: If no Name is provided with OneList, system will look up contact to pull the name, this can slow the process.
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///  <soapenv:Header/>
        ///   <soapenv:Body>
        ///      <ser:OneListSave>
        ///         <ser:oneList>
        ///            <!--Empty for anonymous basket:-->
        ///            <ns1:CardId>10021</ns1:CardId>
        ///            <ns1:Items>
        ///               <ns1:OneListItem>
        ///                  <ns1:Image>
        ///                     <ns:Id>40020</ns:Id>
        ///                  </ns1:Image>
        ///                  <ns1:ItemDescription>Skirt Linda Professional Wear</ns1:ItemDescription>
        ///                  <ns1:ItemId>40020</ns1:ItemId>
        ///                  <ns1:Quantity>2</ns1:Quantity>
        ///                  <ns1:VariantDescription>YELLOW/38</ns1:VariantDescription>
        ///                  <ns1:VariantId>002</ns1:VariantId>
        ///               </ns1:OneListItem>
        ///            </ns1:Items>
        ///            <ns1:ListType>Basket</ns1:ListType>
        ///            <ns1:Name>Tom Tomsson</ns1:Name>
        ///            <ns1:StoreId>S0001</ns1:StoreId>
        ///         </ser:oneList>
        ///         <ser:calculate>true</ser:calculate>
        ///      </ser:OneListSave>
        ///   </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">List Id</param>
        /// <param name="calculate">Perform Calculation on a Basket and save result with Basket</param>
        /// <returns></returns>
        [OperationContract]
        OneList OneListSave(OneList oneList, bool calculate);

        /// <summary>
        /// Calculates OneList Basket Object and returns Order Object
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : EcomCalculateBasket<p/><p/>
        /// This function can be used to send in Basket and convert it to Order.<p/>
        /// Basic Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation<p/>
        /// NOTE: Image Ids are added if not provided with the item or returned from Central, this will result in extra calls, so to speed up things, provide Image object with Image Id only (not including blob)
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///     <ser:OneListCalculate>
        ///        <ser:oneList>
        ///           <!--Empty for anonymous basket:-->
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <ns1:Items>
        ///               <ns1:OneListItem>
        ///                  <ns1:Image>
        ///                     <ns:Id>40020</ns:Id>
        ///                  </ns1:Image>
        ///                  <ns1:ItemDescription>Skirt Linda Professional Wear</ns1:ItemDescription>
        ///                  <ns1:ItemId>40020</ns1:ItemId>
        ///                  <ns1:Quantity>2</ns1:Quantity>
        ///                  <ns1:VariantDescription>YELLOW/38</ns1:VariantDescription>
        ///                  <ns1:VariantId>002</ns1:VariantId>
        ///               </ns1:OneListItem>
        ///           </ns1:Items>
        ///           <ns1:ListType>Basket</ns1:ListType>
        ///           <ns1:StoreId>S0013</ns1:StoreId>
        ///        </ser:oneList>
        ///     </ser:OneListCalculate>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">OneList Object</param>
        /// <returns>Order Object that can be used to Create Order</returns>
        [OperationContract]
        Order OneListCalculate(OneList oneList);

        /// <summary>
        /// Calculates OneList Basket for Hospitality and returns Hospitality Order Object
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MobilePosCalculate<p/><p/>
        /// This function can be used to send in Basket and convert it to Hospitality Order.<p/>
        /// Basic Hospitality Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Basket for EasyBurger Restaurant, Cheese Burger Meal, with Jalapeno Popper and Reg Orange Soda
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///     <ser:OneListHospCalculate>
        ///        <ser:oneList>
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <ns1:IsHospitality>true</ns1:IsHospitality>
        ///           <ns1:Items>
        ///              <ns1:OneListItem>
        ///                 <ns1:IsADeal>true</ns1:IsADeal>
        ///                 <ns1:ItemId>S10025</ns1:ItemId>
        ///                 <ns1:OnelistSubLines>
        ///                    <ns1:OneListItemSubLine>
        ///                       <ns1:DealLineId>10000</ns1:DealLineId>
        ///                       <ns1:DealModLineId>0</ns1:DealModLineId>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Deal</ns1:Type>
        ///                    </ns1:OneListItemSubLine>
        ///                    <ns1:OneListItemSubLine>
        ///                       <ns1:DealLineId>20000</ns1:DealLineId>
        ///                       <ns1:DealModLineId>70000</ns1:DealModLineId>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Deal</ns1:Type>
        ///                       <ns1:Uom>PORTION</ns1:Uom>
        ///                    </ns1:OneListItemSubLine>
        ///                    <ns1:OneListItemSubLine>
        ///                       <ns1:DealLineId>30000</ns1:DealLineId>
        ///                       <ns1:DealModLineId>70000</ns1:DealModLineId>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Deal</ns1:Type>
        ///                       <ns1:Uom>REG</ns1:Uom>
        ///                    </ns1:OneListItemSubLine>
        ///                 </ns1:OnelistSubLines>
        ///                 <ns1:Quantity>1</ns1:Quantity>
        ///                 <ns1:UnitOfMeasureId></ns1:UnitOfMeasureId>
        ///              </ns1:OneListItem>
        ///           </ns1:Items>
        ///           <ns1:ListType>Basket</ns1:ListType>
        ///           <ns1:SalesType>TAKEAWAY</ns1:SalesType>
        ///           <ns1:StoreId>S0017</ns1:StoreId>
        ///        </ser:oneList>
        ///     </ser:OneListHospCalculate>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">OneList Object</param>
        /// <returns>Order Object that can be used to Create Order</returns>
        [OperationContract]
        OrderHosp OneListHospCalculate(OneList oneList);

        /// <summary>
        /// Add or remove Item in OneList without sending whole list
        /// </summary>
        /// <param name="oneListId">OneList Id</param>
        /// <param name="item">OneList Item to add or remove</param>
        /// <param name="cardId">Card Id of the person making the changes</param>
        /// <param name="remove">true if remove item, else false</param>
        /// <param name="calculate">Recalculate OneList</param>
        /// <returns>Updated OneList</returns>
        [OperationContract]
        OneList OneListItemModify(string oneListId, OneListItem item, string cardId, bool remove, bool calculate);

        /// <summary>
        /// Link or remove a Member to/from existing OneList
        /// </summary>
        /// <param name="oneListId">OneList Id to link</param>
        /// <param name="cardId">Card Id to link or remove</param>
        /// <param name="email">Email address to look up Card Id when requesting a Linking</param>
        /// <param name="phone">Phone number to look up Card Id when requesting a Linking</param>
        /// <param name="status">Link action</param>
        /// <returns></returns>
        [OperationContract]
        bool OneListLinking(string oneListId, string cardId, string email, string phone, LinkStatus status);

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity available of items in order for certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : COQtyAvailabilityV2
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="shippingOrder">true if order is to be shipped, false if click and collect</param>
        /// <returns></returns>
        [OperationContract]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request, bool shippingOrder);

        /// <summary>
        /// Create Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCreateVx
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request for Sales Order">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///  <soapenv:Header/>
        ///  <soapenv:Body>
        ///     <ser:OrderCreate>
        ///        <ser:request>
        ///           <ns:Id></ns:Id>
        ///            <!--AnonymousOrder leave empty-->
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <ns1:CollectLocation></ns1:CollectLocation>
        ///           <!--OrderLines need to have minimum Price and Net Price set for Order to be valid.Price is unit price and will be used when calculate the Line Total Amount -->
        ///           <ns1:OrderDiscountLines>
        ///           </ns1:OrderDiscountLines>
        ///           <ns1:OrderLines>
        ///              <ns1:OrderLine>
        ///                 <ns1:Amount>160.00</ns1:Amount>
        ///                 <ns1:ClickAndCollectLine>false</ns1:ClickAndCollectLine>
        ///                 <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                 <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                 <ns1:ItemId>40020</ns1:ItemId>
        ///                 <ns1:LineNumber>1</ns1:LineNumber>
        ///                 <ns1:LineType>Item</ns1:LineType>
        ///                 <ns1:NetAmount>128.00</ns1:NetAmount>
        ///                 <ns1:NetPrice>64.00</ns1:NetPrice>
        ///                 <ns1:Price>80.00</ns1:Price>
        ///                 <ns1:Quantity>2.00</ns1:Quantity>
        ///                 <ns1:TaxAmount>32.00</ns1:TaxAmount>
        ///                 <ns1:UomId/>
        ///                 <ns1:VariantId>002</ns1:VariantId>
        ///              </ns1:OrderLine>
        ///           </ns1:OrderLines>
        ///            <!--Optional: Used for PrePayment information for Shipped orders-->
        ///           <ns1:OrderPayments>
        ///              <!--Zero or more repetitions:-->
        ///              <ns1:OrderPayment>
        ///                 <ns1:Amount>160.00</ns1:Amount>
        ///                 <ns1:AuthorizationCode>123456</ns1:AuthorizationCode>
        ///                 <ns1:CardNumber>45XX..5555</ns1:CardNumber>
        ///                 <ns1:CardType>VISA</ns1:CardType>
        ///                 <ns1:CurrencyCode></ns1:CurrencyCode>
        ///                 <ns1:CurrencyFactor>1</ns1:CurrencyFactor>
        ///                 <ns1:ExternalReference>My123456</ns1:ExternalReference>
        ///                 <ns1:LineNumber>1</ns1:LineNumber>
        ///                 <ns1:PaymentType>PreAuthorization</ns1:PaymentType>
        ///                 <ns1:PreApprovedValidDate>2030-01-01</ns1:PreApprovedValidDate>
        ///                 <ns1:TenderType>1</ns1:TenderType>
        ///                 <ns1:TokenNumber>123456</ns1:TokenNumber>
        ///              </ns1:OrderPayment>
        ///           </ns1:OrderPayments>
        ///           <ns1:OrderType>Sale</ns1:OrderType>
        ///           <ns1:PaymentStatus>PreApproved</ns1:PaymentStatus>
        ///           <ns1:ShipOrder>true</ns1:ShipOrder>
        ///            <!--Optional: ShipToAddress can not be null if ClickAndCollectOrder == false-->
        ///           <ns1:ShipToAddress>
        ///              <ns:Address1>Some Address</ns:Address1>
        ///              <ns:Address2></ns:Address2>
        ///              <ns:CellPhoneNumber></ns:CellPhoneNumber>
        ///              <ns:City>Some City</ns:City>
        ///              <ns:Country></ns:Country>
        ///              <ns:HouseNo></ns:HouseNo>
        ///              <ns:PhoneNumber></ns:PhoneNumber>
        ///              <ns:PostCode>999</ns:PostCode>
        ///              <ns:StateProvinceRegion></ns:StateProvinceRegion>
        ///              <ns:Type>Residential</ns:Type>
        ///           </ns1:ShipToAddress>
        ///           <ns1:StoreId>S0013</ns1:StoreId>
        ///           <ns1:TotalAmount>160</ns1:TotalAmount>
        ///           <ns1:TotalDiscount>0</ns1:TotalDiscount>
        ///           <ns1:TotalNetAmount>128</ns1:TotalNetAmount>
        ///        </ser:request>
        ///        <ser:returnOrderIdOnly>true</ser:returnOrderIdOnly>
        ///     </ser:OrderCreate>
        ///  </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// <code language="xml" title="SOAP Sample Request for Click and Collect Order">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///  <soapenv:Header/>
        ///  <soapenv:Body>
        ///     <ser:OrderCreate>
        ///        <ser:request>
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <ns1:CollectLocation>S0001</ns1:CollectLocation>
        ///           <ns1:OrderDiscountLines>
        ///           </ns1:OrderDiscountLines>
        ///           <ns1:OrderLines>
        ///              <ns1:OrderLine>
        ///                 <ns1:Amount>320.00</ns1:Amount>
        ///                 <ns1:ClickAndCollectLine>true</ns1:ClickAndCollectLine>
        ///                 <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                 <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                 <ns1:ItemId>40020</ns1:ItemId>
        ///                 <ns1:LineNumber>1</ns1:LineNumber>
        ///                 <ns1:LineType>Item</ns1:LineType>
        ///                 <ns1:NetAmount>256.00</ns1:NetAmount>
        ///                 <ns1:NetPrice>64.00</ns1:NetPrice>
        ///                 <ns1:Price>80.00</ns1:Price>
        ///                 <ns1:Quantity>4.00</ns1:Quantity>
        ///                 <ns1:StoreId>S0001</ns1:StoreId>
        ///                 <ns1:TaxAmount>64.00</ns1:TaxAmount>
        ///                 <ns1:UomId/>
        ///                 <ns1:VariantId>002</ns1:VariantId>
        ///              </ns1:OrderLine>
        ///           </ns1:OrderLines>
        ///           <ns1:OrderType>ClickAndCollect</ns1:OrderType>
        ///           <ns1:PaymentStatus>PreApproved</ns1:PaymentStatus>
        ///           <ns1:ShipOrder>false</ns1:ShipOrder>
        ///           <ns1:StoreId>S0013</ns1:StoreId>
        ///           <ns1:TotalAmount>160</ns1:TotalAmount>
        ///           <ns1:TotalDiscount>0</ns1:TotalDiscount>
        ///           <ns1:TotalNetAmount>128</ns1:TotalNetAmount>
        ///        </ser:request>
        ///        <ser:returnOrderIdOnly>false</ser:returnOrderIdOnly>
        ///     </ser:OrderCreate>
        ///  </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <param name="returnOrderIdOnly">Only return Order Id back, not full order object</param>
        /// <returns>SalesEntry object for order if order creation was successful</returns>
        [OperationContract]
        SalesEntry OrderCreate(Order request, bool returnOrderIdOnly);

        /// <summary>
        /// Edit Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderEdit
        /// </remarks>
        /// <param name="request">Updated Order object</param>
        /// <param name="orderId">Order Id to edit</param>
        /// <param name="editType">Type of Order Edit</param>
        /// <param name="returnOrderIdOnly"></param>
        /// <returns></returns>
        [OperationContract]
        SalesEntry OrderEdit(Order request, string orderId, OrderEditType editType, bool returnOrderIdOnly);

        /// <summary>
        /// Update payments for Customer Order
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId"></param>
        /// <returns>webPreAuthNotAuthorize</returns>
        [OperationContract]
        bool OrderUpdatePayment(string orderId, string storeId, OrderPayment payment);

        /// <summary>
        /// Create a Hospitality Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CreateHospOrder
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request, 
        /// Sample Order for EasyBurger Restaurant, Cheese Burger Meal, with Jalapeno Popper and Reg Orange Soda.
        /// Based on OneListHospCalculate result.
        /// <code language="xml" title="SOAP Sample Request for Delivery order">
        /// <![CDATA[
        ///<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2020" xmlns:ns2="http://lsretail.com/LSOmniService/Loy/2017">
        ///    <soapenv:Header/>
        ///    <soapenv:Body>
        ///        <ser:OrderHospCreate>
        ///            <ser:request>
        ///                <ns1:Address>
        ///                    <ns:Address1>SomeStreet 4</ns:Address1>
        ///                    <ns:CellPhoneNumber>555-1234</ns:CellPhoneNumber>
        ///                    <ns:City>Kopavogur</ns:City>
        ///                    <ns:Country/>
        ///                    <ns:PostCode>200</ns:PostCode>
        ///                    <ns:StateProvinceRegion/>
        ///                    <ns:Type>Residential</ns:Type>
        ///                </ns1:Address>
        ///                <ns1:CardId>10021</ns1:CardId>
        ///                <ns1:Comment>Im Hungry</ns1:Comment>
        ///                <ns1:DeliveryType>Home</ns1:DeliveryType>
        ///                <ns1:Directions>Go around the corner</ns1:Directions>
        ///                <ns1:Email>tom@xyz.com</ns1:Email>
        ///                <ns1:Name>Tom Tomsson</ns1:Name>
        ///                <ns1:OrderDiscountLines>
        ///                    <ns2:OrderDiscountLine>
        ///                        <ns2:Description>Deal</ns2:Description>
        ///                        <ns2:DiscountAmount>0.00</ns2:DiscountAmount>
        ///                        <ns2:DiscountPercent>0.00</ns2:DiscountPercent>
        ///                        <ns2:DiscountType>Deal</ns2:DiscountType>
        ///                        <ns2:LineNumber>9750</ns2:LineNumber>
        ///                        <ns2:No>{540B4057-A092-4356-B34D-433CCD3EADAE}</ns2:No>
        ///                        <ns2:OfferNumber>S10025</ns2:OfferNumber>
        ///                        <ns2:OrderId/>
        ///                        <ns2:PeriodicDiscGroup/>
        ///                        <ns2:PeriodicDiscType>Unknown</ns2:PeriodicDiscType>
        ///                    </ns2:OrderDiscountLine>
        ///                    <ns2:OrderDiscountLine>
        ///                        <ns2:Description>Deal</ns2:Description>
        ///                        <ns2:DiscountAmount>0.00</ns2:DiscountAmount>
        ///                        <ns2:DiscountPercent>0.00</ns2:DiscountPercent>
        ///                        <ns2:DiscountType>Deal</ns2:DiscountType>
        ///                        <ns2:LineNumber>10000</ns2:LineNumber>
        ///                        <ns2:No>{540B4057-A092-4356-B34D-433CCD3EADAE}</ns2:No>
        ///                        <ns2:OfferNumber>S10025</ns2:OfferNumber>
        ///                        <ns2:OrderId/>
        ///                        <ns2:PeriodicDiscGroup/>
        ///                        <ns2:PeriodicDiscType>Unknown</ns2:PeriodicDiscType>
        ///                    </ns2:OrderDiscountLine>
        ///                    <ns2:OrderDiscountLine>
        ///                        <ns2:Description>Deal</ns2:Description>
        ///                        <ns2:DiscountAmount>0.00</ns2:DiscountAmount>
        ///                        <ns2:DiscountPercent>0.00</ns2:DiscountPercent>
        ///                        <ns2:DiscountType>Deal</ns2:DiscountType>
        ///                        <ns2:LineNumber>20000</ns2:LineNumber>
        ///                        <ns2:No>{540B4057-A092-4356-B34D-433CCD3EADAE}</ns2:No>
        ///                        <ns2:OfferNumber>S10025</ns2:OfferNumber>
        ///                        <ns2:OrderId/>
        ///                        <ns2:PeriodicDiscGroup/>
        ///                        <ns2:PeriodicDiscType>Unknown</ns2:PeriodicDiscType>
        ///                    </ns2:OrderDiscountLine>
        ///                    <ns2:OrderDiscountLine>
        ///                        <ns2:Description>Deal</ns2:Description>
        ///                        <ns2:DiscountAmount>0.00</ns2:DiscountAmount>
        ///                        <ns2:DiscountPercent>0.00</ns2:DiscountPercent>
        ///                        <ns2:DiscountType>Deal</ns2:DiscountType>
        ///                        <ns2:LineNumber>30000</ns2:LineNumber>
        ///                        <ns2:No>{540B4057-A092-4356-B34D-433CCD3EADAE}</ns2:No>
        ///                        <ns2:OfferNumber>S10025</ns2:OfferNumber>
        ///                        <ns2:OrderId/>
        ///                        <ns2:PeriodicDiscGroup/>
        ///                        <ns2:PeriodicDiscType>Unknown</ns2:PeriodicDiscType>
        ///                    </ns2:OrderDiscountLine>
        ///                </ns1:OrderDiscountLines>
        ///                <ns1:OrderLines>
        ///                    <ns1:OrderHospLine>
        ///                        <Id>{540B4057-A092-4356-B34D-433CCD3EADAE}</Id>
        ///                        <ns1:Amount>7.50</ns1:Amount>
        ///                        <ns1:DiscountAmount>0.00</ns1:DiscountAmount>
        ///                        <ns1:DiscountPercent>0.00</ns1:DiscountPercent>
        ///                        <ns1:IsADeal>true</ns1:IsADeal>
        ///                        <ns1:ItemDescription>Cheese Burger Meal</ns1:ItemDescription>
        ///                        <ns1:ItemId>S10025</ns1:ItemId>
        ///                        <ns1:LineNumber>9750</ns1:LineNumber>
        ///                        <ns1:LineType>Item</ns1:LineType>
        ///                        <ns1:NetAmount>6.82</ns1:NetAmount>
        ///                        <ns1:NetPrice>6.82</ns1:NetPrice>
        ///                        <ns1:Price>7.50</ns1:Price>
        ///                        <ns1:PriceModified>false</ns1:PriceModified>
        ///                        <ns1:Quantity>1.00</ns1:Quantity>
        ///                        <ns1:SubLines>
        ///                            <ns1:OrderHospSubLine>
        ///                                <ns1:Amount>5.32</ns1:Amount>
        ///                                <ns1:DealCode>S10025</ns1:DealCode>
        ///                                <ns1:DealLineId>10000</ns1:DealLineId>
        ///                                <ns1:DealModifierLineId>0</ns1:DealModifierLineId>
        ///                                <ns1:Description>Cheese Burger</ns1:Description>
        ///                                <ns1:DiscountAmount>0.00</ns1:DiscountAmount>
        ///                                <ns1:DiscountPercent>0.00</ns1:DiscountPercent>
        ///                                <ns1:ItemId>R0024</ns1:ItemId>
        ///                                <ns1:LineNumber>10000</ns1:LineNumber>
        ///                                <ns1:ManualDiscountAmount>0.0</ns1:ManualDiscountAmount>
        ///                                <ns1:ManualDiscountPercent>0.0</ns1:ManualDiscountPercent>
        ///                                <ns1:ModifierGroupCode/>
        ///                                <ns1:ModifierSubCode/>
        ///                                <ns1:NetAmount>4.84</ns1:NetAmount>
        ///                                <ns1:NetPrice>4.84</ns1:NetPrice>
        ///                                <ns1:ParentSubLineId>0</ns1:ParentSubLineId>
        ///                                <ns1:Price>5.32</ns1:Price>
        ///                                <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                                <ns1:Quantity>1.00</ns1:Quantity>
        ///                                <ns1:TAXAmount>0.48</ns1:TAXAmount>
        ///                                <ns1:Type>Deal</ns1:Type>
        ///                                <ns1:Uom>PORTION</ns1:Uom>
        ///                            </ns1:OrderHospSubLine>
        ///                            <ns1:OrderHospSubLine>
        ///                                <ns1:Amount>1.28</ns1:Amount>
        ///                                <ns1:DealCode>S10025</ns1:DealCode>
        ///                                <ns1:DealLineId>20000</ns1:DealLineId>
        ///                                <ns1:DealModifierLineId>70000</ns1:DealModifierLineId>
        ///                                <ns1:Description>Jalapeno Popper</ns1:Description>
        ///                                <ns1:DiscountAmount>0.00</ns1:DiscountAmount>
        ///                                <ns1:DiscountPercent>0.00</ns1:DiscountPercent>
        ///                                <ns1:ItemId>33430</ns1:ItemId>
        ///                                <ns1:LineNumber>20000</ns1:LineNumber>
        ///                                <ns1:ManualDiscountAmount>0.0</ns1:ManualDiscountAmount>
        ///                                <ns1:ManualDiscountPercent>0.0</ns1:ManualDiscountPercent>
        ///                                <ns1:ModifierGroupCode/>
        ///                                <ns1:ModifierSubCode/>
        ///                                <ns1:NetAmount>1.16</ns1:NetAmount>
        ///                                <ns1:NetPrice>1.16</ns1:NetPrice>
        ///                                <ns1:ParentSubLineId>0</ns1:ParentSubLineId>
        ///                                <ns1:Price>1.28</ns1:Price>
        ///                                <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                                <ns1:Quantity>1.00</ns1:Quantity>
        ///                                <ns1:TAXAmount>0.12</ns1:TAXAmount>
        ///                                <ns1:Type>Deal</ns1:Type>
        ///                                <ns1:Uom>PORTION</ns1:Uom>
        ///                            </ns1:OrderHospSubLine>
        ///                            <ns1:OrderHospSubLine>
        ///                                <ns1:Amount>0.90</ns1:Amount>
        ///                                <ns1:DealCode>S10025</ns1:DealCode>
        ///                                <ns1:DealLineId>30000</ns1:DealLineId>
        ///                                <ns1:DealModifierLineId>70000</ns1:DealModifierLineId>
        ///                                <ns1:Description>Orange Soda</ns1:Description>
        ///                                <ns1:DiscountAmount>0.00</ns1:DiscountAmount>
        ///                                <ns1:DiscountPercent>0.00</ns1:DiscountPercent>
        ///                                <ns1:ItemId>30520</ns1:ItemId>
        ///                                <ns1:LineNumber>30000</ns1:LineNumber>
        ///                                <ns1:ManualDiscountAmount>0.0</ns1:ManualDiscountAmount>
        ///                                <ns1:ManualDiscountPercent>0.0</ns1:ManualDiscountPercent>
        ///                                <ns1:ModifierGroupCode/>
        ///                                <ns1:ModifierSubCode/>
        ///                                <ns1:NetAmount>0.82</ns1:NetAmount>
        ///                                <ns1:NetPrice>0.82</ns1:NetPrice>
        ///                                <ns1:ParentSubLineId>0</ns1:ParentSubLineId>
        ///                                <ns1:Price>0.90</ns1:Price>
        ///                                <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                                <ns1:Quantity>1.00</ns1:Quantity>
        ///                                <ns1:TAXAmount>0.08</ns1:TAXAmount>
        ///                                <ns1:Type>Deal</ns1:Type>
        ///                                <ns1:Uom>REG</ns1:Uom>
        ///                            </ns1:OrderHospSubLine>
        ///                        </ns1:SubLines>
        ///                        <ns1:TaxAmount>0.68</ns1:TaxAmount>
        ///                        <ns1:UomId/>
        ///                        <ns1:VariantDescription/>
        ///                        <ns1:VariantId/>
        ///                    </ns1:OrderHospLine>
        ///                </ns1:OrderLines>
        ///                <ns1:OrderPayment>
        ///                    <ns1:Amount>7.50</ns1:Amount>
        ///                    <ns1:AuthorizationCode>123456</ns1:AuthorizationCode>
        ///                    <ns1:CardNumber>45XX..5555</ns1:CardNumber>
        ///                    <ns1:CardType>VISA</ns1:CardType>
        ///                    <ns1:CurrencyCode/>
        ///                    <ns1:CurrencyFactor>1</ns1:CurrencyFactor>
        ///                    <ns1:ExternalReference>My123456</ns1:ExternalReference>
        ///                    <ns1:LineNumber>1</ns1:LineNumber>
        ///                    <ns1:PaymentType>PreAuthorization</ns1:PaymentType>
        ///                    <ns1:PreApprovedValidDate>2030-01-01</ns1:PreApprovedValidDate>
        ///                    <ns1:TenderType>1</ns1:TenderType>
        ///                    <ns1:TokenNumber>123456</ns1:TokenNumber>
        ///                </ns1:OrderPayment>
        ///                <ns1:PickupTime>2022-06-10T10:00:00</ns1:PickupTime>
        ///                <ns1:RestaurantNo>S0017</ns1:RestaurantNo>
        ///                <ns1:SalesType>TAKEAWAY</ns1:SalesType>
        ///                <ns1:StoreId>S0017</ns1:StoreId>
        ///                <ns1:TotalAmount>7.50</ns1:TotalAmount>
        ///                <ns1:TotalDiscount>0.00</ns1:TotalDiscount>
        ///                <ns1:TotalNetAmount>6.82</ns1:TotalNetAmount>
        ///            </ser:request>
        ///            <ser:returnOrderIdOnly>false</ser:returnOrderIdOnly>
        ///        </ser:OrderHospCreate>
        ///    </soapenv:Body>
        ///</soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <param name="returnOrderIdOnly">Only return Order Id back, not full order object</param>
        /// <returns></returns>
        [OperationContract]
        SalesEntry OrderHospCreate(OrderHosp request, bool returnOrderIdOnly);

        /// <summary>
        /// Cancel hospitality order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelHospOrder
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        [OperationContract]
        bool HospOrderCancel(string storeId, string orderId);

        /// <summary>
        /// Get Order status for hospitality order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetHospOrderEstimatedTime and GetKotStatus
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        List<OrderHospStatus> HospOrderStatus(string storeId, string orderId);

        /// <summary>
        /// Check Status of a Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderStatus
        /// </remarks>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        OrderStatusResponse OrderStatusCheck(string orderId);

        /// <summary>
        /// Cancel Customer Order with lineNo option to cancel individual lines
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCancel
        /// </remarks>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId">Web Store Id</param>
        /// <param name="userId">User who cancels the order, use Contact ID for logged in user</param>
        /// <param name="lineNo">List of Order Line numbers to cancel, if empty whole order will be canceled</param>
        /// <returns></returns>
        [OperationContract]
        bool OrderCancel(string orderId, string storeId, string userId, List<int> lineNo);

        /// <summary>
        /// Cancel Customer Order with lineNo and quantity to cancel items from individual lines
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCancel
        /// </remarks>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId">Web Store Id</param>
        /// <param name="userId">User who cancels the order, use Contact ID for logged in user</param>
        /// <param name="lines">List of Order Lines to cancel from, if empty whole order will be canceled</param>
        /// <returns></returns>
        [OperationContract]
        bool OrderCancelEx(string orderId, string storeId, string userId, List<OrderCancelLine> lines);

        /// <summary>
        /// Get All Sales Entries (Transactions and Orders) by card Id
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetMemberContactSalesHistory
        /// </remarks>
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
        List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfEntries);

        /// <summary>
        /// Get All Sales Entries (Transactions and Orders) by Card Id and optional filter by Store Id and Registration Date
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetMemContSalesHist
        /// </remarks>
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
        List<SalesEntry> SalesEntriesGetByCardIdEx(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries);

        /// <summary>
        /// Get the Sale details (order/transaction)
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetSelectedSalesDoc
        /// </remarks>
        /// <param name="entryId">Sales Entry ID</param>
        /// <param name="type">Document Id type of the Sale entry</param>
        /// <returns>SalesEntry with Lines</returns>
        [OperationContract]
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type);

        /// <summary>
        /// Get Return sales transactions based on orginal transaction with HasReturnSale = true
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetSalesReturnById
        /// </remarks>
        /// <param name="receiptNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<SalesEntry> SalesEntryGetReturnSales(string receiptNo);

        /// <summary>
        /// Get Transaction and Sales Invoices for Customer order
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetSalesInfoByOrderId
        /// </remarks>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        List<SalesEntry> SalesEntryGetSalesByOrderId(string orderId);

        /// <summary>
        /// Get Transaction, Sales Invoices and Shipments for Customer order
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetSalesInfoByOrderId
        /// </remarks>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        SalesEntryList SalesEntryGetSalesExtByOrderId(string orderId);

        #endregion

        #region Contact

        /// <summary>
        /// Create new Member Contact
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
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        ///<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///      <ser:ContactCreate>
        ///         <ser:contact>
        ///            <ns1:Account>
        ///               <ns:Id></ns:Id>
        ///               <ns1:Scheme>
        ///                  <ns:Id></ns:Id>
        ///                  <ns1:Club>
        ///                     <ns:Id>CRONUS</ns:Id>
        ///                  </ns1:Club>
        ///               </ns1:Scheme>
        ///            </ns1:Account>
        ///            <ns1:Addresses>
        ///               <ns:Address>
        ///                  <ns:Address1>Santa Monica</ns:Address1>
        ///                  <ns:CellPhoneNumber>555-5551</ns:CellPhoneNumber>
        ///                  <ns:City>Hollywood</ns:City>
        ///                  <ns:Country>US</ns:Country>
        ///                  <ns:PhoneNumber>666-6661</ns:PhoneNumber>
        ///                  <ns:PostCode>1001</ns:PostCode>
        ///                  <ns:StateProvinceRegion></ns:StateProvinceRegion>
        ///                  <ns:Type>Residential</ns:Type>
        ///               </ns:Address>
        ///            </ns1:Addresses>
        ///            <ns1:Email>Sarah@Hollywood.com</ns1:Email>
        ///            <ns1:FirstName>Sarah</ns1:FirstName>
        ///            <ns1:Gender>Female</ns1:Gender>
        ///            <ns1:Initials>Ms</ns1:Initials>
        ///            <ns1:LastName>Parker</ns1:LastName>
        ///            <ns1:MiddleName></ns1:MiddleName>
        ///            <ns1:Password>SxxInTheCity</ns1:Password>
        ///            <ns1:UserName>sarah</ns1:UserName>
        ///         </ser:contact>
        ///      </ser:ContactCreate>
        ///   </soapenv:Body>            
        ///</soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="contact">contact</param>
        /// <param name="doLogin">Perform Login after Create, this will return full Contact object with all information. If false, only contact, card, account ids are returned.</param>
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
        MemberContact ContactCreate(MemberContact contact, bool doLogin);

        /// <summary>
        /// Update Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberContactUpdate<p/><p/>
        /// Contact Id, User name and EMail are required values for the update command to work.<p/>
        /// Any field left out or sent in empty will wipe out that information. Always fill out all 
        /// Name field, Address and phone number even if it has not changed so it will not be wiped out from LS Central
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        ///<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///      <ser:ContactUpdate>
        ///         <ser:contact>
        ///            <ns:Id>MO000012</ns:Id>
        ///            <ns1:Addresses>
        ///               <ns:Address>
        ///                  <ns:Address1>Santa Monica</ns:Address1>
        ///                  <ns:CellPhoneNumber>555-5551</ns:CellPhoneNumber>
        ///                  <ns:City>Hollywood</ns:City>
        ///                  <ns:Country>US</ns:Country>
        ///                  <ns:PostCode>1001</ns:PostCode>
        ///                  <ns:StateProvinceRegion></ns:StateProvinceRegion>
        ///                  <ns:Type>Residential</ns:Type>
        ///               </ns:Address>
        ///            </ns1:Addresses>
        ///            <ns1:Cards>
        ///               <ns:Card>
        ///                  <ns:Id>10027</ns:Id>
        ///               </ns:Card>
        ///            </ns1:Cards>
        ///            <ns1:Email>Sarah@Hollywood.com</ns1:Email>
        ///            <ns1:FirstName>Sarah</ns1:FirstName>
        ///            <ns1:Gender>Female</ns1:Gender>
        ///            <ns1:Initials>Ms</ns1:Initials>
        ///            <ns1:LastName>Parker</ns1:LastName>
        ///            <ns1:MiddleName></ns1:MiddleName>
        ///            <ns1:UserName>sarah</ns1:UserName>
        ///         </ser:contact>
        ///      </ser:ContactUpdate>
        ///   </soapenv:Body>
        ///</soapenv:Envelope>            
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="contact">contact</param>
        /// <param name="getContact">Return contact object filled out after Update</param>
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
        MemberContact ContactUpdate(MemberContact contact, bool getContact);

        /// <summary>
        /// Get Member Contact by card Id. This function returns all information about the Member contact, 
        /// including Profiles, Offers, Sales history, Onelist baskets and notifications.
        /// To get basic information, use ContactGet.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberContact2<p/>
        /// LS Central WS4 : GetMemberContactInfo
        /// </remarks>
        /// <param name="cardId">Card Id</param>
        /// <param name="numberOfTransReturned">Number of Sales History to return, 0 = all</param>
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
        MemberContact ContactGetByCardId(string cardId, int numberOfTransReturned);

        /// <summary>
        /// Search for list of Member Contacts by different searchType methods, 
        /// will return any contact that will match the search value.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberContact2<p/>
        /// LS Central WS4 : GetMemberContactInfo
        /// </remarks>
        /// <param name="searchType">Field to search by</param>
        /// <param name="search">Search value</param>
        /// <param name="maxNumberOfRowsReturned">Max number of record, if set to 1 the exact search will be performed</param>
        /// <returns></returns>
        [OperationContract]
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        /// <summary>
        /// Search for Member Contact by different searchType methods.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberContact2<p/>
        /// LS Central WS4 : GetMemberContactInfo
        /// </remarks>
        /// <param name="searchType">Field to search by</param>
        /// <param name="search">Search value</param>
        /// <returns></returns>
        [OperationContract]
        MemberContact ContactGet(ContactSearchType searchType, string search);

        /// <summary>
        /// Add new card to existing Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberCardToContact
        /// </remarks>
        /// <param name="contactId"></param>
        /// <param name="cardId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract]
        double ContactAddCard(string contactId, string cardId, string accountId);

        /// <summary>
        /// Block Member Contact and remove information from LS Central and LS Commerce
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        bool ContactBlock(string accountId, string cardId);

        /// <summary>
        /// Get Point balance for Member Card
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberCard
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        long CardGetPointBalance(string cardId);

        /// <summary>
        /// Get Point entries for Member Card
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        [OperationContract]
        List<PointEntry> CardGetPointEntries(string cardId, DateTime dateFrom);

        /// <summary>
        /// Gets Rate value for points (f.ex. 1 point = 0.01 Kr)
        /// </summary>
        /// <param name="currency">Currency to convert Loy points to, if empty Base Currency will be used</param>
        /// <returns></returns>
        [OperationContract]
        decimal GetPointRate(string currency);

        [OperationContract]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);

        /// <summary>
        /// Change password
        /// <p/>NOTE: Its recommended to use PasswordReset and PasswordChange instead
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordChange
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
        [Obsolete("Its recommended to use PasswordReset and PasswordChange instead")]
        bool ChangePassword(string userName, string newPassword, string oldPassword);

        /// <summary>
        /// Request a ResetCode to use in Email to send to Member Contact
        /// <p/>NOTE: Its recommended to use PasswordReset and PasswordChange instead
        /// </summary>
        /// <remarks>
        /// Settings for this function are found in Commerce Service for LS Central Database - TenantConfig table
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
        [Obsolete("Its recommended to use PasswordReset and PasswordChange instead")]
        string ForgotPassword(string userNameOrEmail);

        /// <summary>
        /// Send in Reset Password request for Member Contact
        /// <p/>NOTE: Its recommended to use PasswordReset and PasswordChange instead
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
        [Obsolete("Its recommended to use PasswordReset and PasswordChange instead")]
        bool ResetPassword(string userName, string resetCode, string newPassword);

        /// <summary>
        /// Reset current password or request new password for new Member Contact.  
        /// Send either login or email depending on which function is required.
        /// If sendEmail is true, send only email address, login is not used in this mode
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordReset
        /// </remarks>
        /// <param name="userName">Provide Login Id (UserName) to reset existing password</param>
        /// <param name="email">Provide Email to create new login password for new Member Contact</param>
        /// <returns>Token to be included in Email to send to Member Contact.  Send the token with PasswordChange function</returns>
        [OperationContract]
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
        bool PasswordChange(string userName, string token, string newPassword, string oldPassword);

        /// <summary>
        /// Change password in SPG App
        /// </summary>
        /// <param name="email">EMail to send token to</param>
        /// <param name="token">Token from PasswordReset</param>
        /// <param name="newPassword">New Password</param>
        /// <returns></returns>
        [OperationContract]
        string SPGPassword(string email, string token, string newPassword);

        /// <summary>
        /// Change Login Id for Member Contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_LOGIN_CHANGE
        /// </remarks>
        /// <param name="oldUserName">Current Login Id</param>
        /// <param name="newUserName">New Login Id</param>
        /// <param name="password">Current Password</param>
        /// <returns></returns>
        [OperationContract]
        bool LoginChange(string oldUserName, string newUserName, string password);

        /// <summary>
        /// Login user
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberLogon
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
        MemberContact Login(string userName, string password, string deviceId);

        /// <summary>
        /// Social authentication login
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberAuthenticatorLogin
        /// </remarks>
        /// <param name="authenticator"></param>
        /// <param name="authenticationId"></param>
        /// <param name="deviceId">Device Id. Should be empty for non device user (web apps)</param>
        /// <param name="deviceName">Device name or description</param>
        /// <param name="includeDetails">Include detailed Contact information, like  Publish offer and transactions</param>
        /// <returns>Contact</returns>
        [OperationContract]
        MemberContact SocialLogon(string authenticator, string authenticationId, string deviceId, string deviceName, bool includeDetails);

        /// <summary>
        /// Login user from web page.  This function is light version of Login and returns less data.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberLogon
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
        MemberContact LoginWeb(string userName, string password);

        /// <summary>
        /// Search for Customer
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="search"></param>
        /// <param name="maxNumberOfRowsReturned"></param>
        /// <returns></returns>
        [OperationContract]
        List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned);

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
        List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays);

        /// <summary>
        /// Get stock status for list of items from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetInventoryMultiple<p/><p/>
        /// If storeId is empty, all store that have item available will be returned
        /// </remarks>
        /// <param name="items">Items to get status for</param>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <returns></returns>
        [OperationContract]
        List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId);

        /// <summary>
        /// Get stock status for list of items from Store and/or Sourcing Location
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetInventoryMultipleV2<p/><p/>
        /// If storeId is empty, all store that have item available will be returned.
        /// If locationId is set, only status for that location will be shown (with or without storeId)
        /// </remarks>
        /// <param name="items">Items to get status for</param>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="locationId">Sourcing Location to get status for</param>
        /// <param name="useSourcingLocation">Get Inventory status from all Sourcing Locations</param>
        /// <returns></returns>
        [OperationContract]
        List<InventoryResponse> ItemsInStoreGetEx(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation);

        /// <summary>
        /// Gets Hospitality Kitchen Current Availability
        /// </summary>
        /// <param name="request">List of items to get, if empty, get all items</param>
        /// <param name="storeId">Store to get, if empty get all stores</param>
        /// <returns></returns>
        [OperationContract]
        List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId);

        /// <summary>
        /// Search Items by Description
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetItem
        /// </remarks>
        /// <param name="search">Description search</param>
        /// <param name="maxNumberOfItems"></param>
        /// <param name="includeDetails"></param>
        /// <returns></returns>
        [OperationContract]
        List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails);

        /// <summary>
        /// Lookup Item
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetItem
        /// </remarks>
        /// <param name="itemId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        LoyItem ItemGetById(string itemId, string storeId);

        /// <summary>
        /// Look up Item by Barcode
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetItemWithBarcode
        /// LS Central OData: GetItem
        /// </remarks>
        /// <param name="barcode"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        LoyItem ItemGetByBarcode(string barcode, string storeId);

        [OperationContract]
        List<LoyItem> ItemsPage(string storeId, int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        /// <summary>
        /// Gets customer specific prices
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : EcomGetCustomerPrice
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="cardId"></param>
        /// <param name="items">list of items to get prices for</param>
        /// <returns></returns>
        [OperationContract]
        List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items);

        [OperationContract]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        /// <summary>
        /// Gets Hierarchy setup for a Store with all Leaves and Nodes
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetHierarchy, GetHierarchyNode<p/><p/>
        /// It is recommended for large hierarchies to use the hierarchy replication functions.
        /// It will give option on getting only changes, instead of always have to get the whole hierarchy like this function does.
        /// </remarks>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
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
        List<Profile> ProfilesGetAll();

        /// <summary>
        /// Gets all Member Attributes for a Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberCard<p/><p/>
        /// Member Attribute Value has to have Value Yes or No, even in other languages as Commerce Service for LS Central uses that Text to determent if the Attribute is selected or not for the Contact
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        List<Profile> ProfilesGetByCardId(string cardId);

        #endregion

        #region Images

        [OperationContract]
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
        List<Scheme> SchemesGetAll();

        #endregion

        #region Store Location

        /// <summary>
        /// Get store by Store Id
        /// </summary>
        /// <remarks>
        /// LS Central OData : GetStores<p/><p/>
        /// Data for Store Hours needs to be generated in LS Central by running COMMERCE_XXXX Scheduler Jobs
        /// </remarks>
        /// <param name="storeId">store Id</param>
        /// <param name="includeImages">Include Image blobs</param>
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
        Store StoreGetById(string storeId, bool includeImages);

        /// <summary>
        /// Get List of stores
        /// </summary>
        /// <param name="storeType">Type of stores to get</param>
        /// <param name="includeDetails">Include detail data, like store hours</param>
        /// <param name="includeImages">Include Image Blobs for stores</param>
        /// <remarks>
        /// LS Central OData : GetStores<p/><p/>
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
        List<Store> StoresGet(StoreGetType storeType, bool includeDetails, bool includeImages);

        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// LS Central OData : GetStores<p/><p/>
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
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance);

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude), that have the item available
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetItemInventory
        /// </remarks>
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
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance);

        /// <summary>
        /// Gets Return Policy
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetReturnPolicy
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="storeGroupCode"></param>
        /// <param name="itemCategory"></param>
        /// <param name="productGroup"></param>
        /// <param name="itemId"></param>
        /// <param name="variantCode"></param>
        /// <param name="variantDim1"></param>
        /// <returns></returns>
        [OperationContract]
        List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1);

        /// <summary>
        /// Get Currency by Code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [OperationContract]
        Currency CurrencyGet(string code);

        #endregion

        #region Replication

        /// <summary>
        /// Replicate Item Barcodes (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001451 - LSC Barcodes
        /// LS Central WS4 : GetBarcode
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.  
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of barcodes</returns>
        [OperationContract]
        ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest);

        /// <summary>
        /// Replicate Currency setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 4 - Currency
        /// LS Central WS4 : GetCurrency
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of currencies</returns>
        [OperationContract]
        ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest);

        /// <summary>
        /// Replicate Currency Rate Setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 330 - Currency Exchange Rate
        /// LS Central WS4 : GetCurrencyExchRate
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of currency rates</returns>
        [OperationContract]
        ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Extended Variants Setup (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001413 - LSC Extd. Variant Values
        /// LS Central WS4 : GetExtdVariantValues
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variants</returns>
        [OperationContract]
        ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Image links
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009064 - LSC Retail Image Link
        /// LS Central WS4 : GetImageLink
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of image links</returns>
        [OperationContract]
        ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Images
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009063 - LSC Retail Image
        /// LS Central WS4 : GetWIImageBuffer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of images</returns>
        [OperationContract]
        ReplImageResponse ReplEcommImages(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Categories (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5722 - Item Category
        /// LS Central WS4 : GetItemCategory
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of item categories</returns>
        [OperationContract]
        ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Items (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item
        /// LS Central WS4 : GetWIItemBuffer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of items</returns>
        [OperationContract]
        ReplItemResponse ReplEcommItems(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Unit of Measures (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5404 - Item Unit of Measure
        /// LS Central WS4 : GetItemUnitOfMeasure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of item unit of measures</returns>
        [OperationContract]
        ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Variant Registrations (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001414 - LSC Item Variant Registration
        /// LS Central WS4 : GetVariantRegWithStatus
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant registrations</returns>
        [OperationContract]
        ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Variant (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5401 - Item Variant
        /// LS Central WS4 : GetItemVariant
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant</returns>
        [OperationContract]
        ReplItemVariantResponse ReplEcommItemVariants(ReplRequest replRequest);

        /// <summary>
        /// Replicate Best Prices for Items from WI Price table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012861 - LSC WI Price
        /// LS Central WS4 : GetWIPrice
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs.  
        /// This will generate the Best price for product based on date and offers available at the time.<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item and Sales Price tables are used to find deleted changes.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of prices</returns>
        [OperationContract]
        ReplPriceResponse ReplEcommPrices(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Prices from Sales Price table (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 7002 - Sales Price
        /// LS Central WS4 : GetSalesPrice, GetPriceListLine
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of prices</returns>
        [OperationContract]
        ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest);

        /// <summary>
        /// Replicate Product groups (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000705 - LSC Retail Product Group
        /// LS Central WS4 : GetProductGroup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of product groups</returns>
        [OperationContract]
        ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest);

        /// <summary>
        /// Replicate Store setups
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001470 - LSC Store
        /// LS Central WS4 : GetStoreBuffer
        /// <p/><p/>
        /// Only store with Loyalty or Mobile Checked will be replicated
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of stores</returns>
        [OperationContract]
        ReplStoreResponse ReplEcommStores(ReplRequest replRequest);

        /// <summary>
        /// Replicate Unit of Measures
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 204 - Unit of Measure
        /// LS Central WS4 : GetUnitOfMeasure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of unit of measures</returns>
        [OperationContract]
        ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Collection for Unit of Measures
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001430 - LSC Collection Framework
        /// LS Central WS4 : GetCollection
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Collection</returns>
        [OperationContract]
        ReplCollectionResponse ReplEcommCollection(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendors
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 23 - Vendor
        /// LS Central WS4 : GetVendor
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of vendors</returns>
        [OperationContract]
        ReplVendorResponse ReplEcommVendor(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendor Item Mapping (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item (Lookup by [Vendor No_])
        /// LS Central WS4 : GetVendorItem
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of vendor item mappings</returns>
        [OperationContract]
        ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attributes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000784 - LSC Attribute
        /// LS Central WS4 : GetAttribute
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attributes</returns>
        [OperationContract]
        ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Values
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000786 - LSC Attribute Value
        /// LS Central WS4 : GetAttributeValues
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attribute values</returns>
        [OperationContract]
        ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Option Values
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000785 - LSC Attribute Option Value
        /// LS Central WS4 : GetAttributeOptionValues
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attribute option values</returns>
        [OperationContract]
        ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000971 - LSC Data Translation
        /// LS Central WS4 : GetDataTranslation
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text for Item HTML table
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001410 - LSC Item HTML ML
        /// LS Central WS4 : GetItemHTML
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        ReplDataTranslationResponse ReplEcommHtmlTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text for Deal HTML table
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001410 - LSC Deal HTML ML
        /// LS Central WS4 : GetDealHTML
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        ReplDataTranslationResponse ReplEcommDealHtmlTranslation(ReplRequest replRequest);

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
        ReplDataTranslationLangCodeResponse ReplEcommDataTranslationLangCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Validation Scheduling data for Hierarchy
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000955 - LSC Validation Schedule
        /// <p/><p/>
        /// This function only checks if there are any available pre-actions for any of the tables involved in the Schedule data 
        /// and if there is, the whole Validation Schedule will be replicated again.
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Validation Schedule objects</returns>
        [OperationContract]
        ReplValidationScheduleResponse ReplEcommValidationSchedule(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy roots
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000920 - LSC Hierarchy
        /// LS Central WS4 : GetHierarchy
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Hierarchy objects</returns>
        [OperationContract]
        ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Nodes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000921 - LSC Hierar. Nodes
        /// LS Central WS4 : GetHierarchyNodes
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy nodes</returns>
        [OperationContract]
        ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Node Leaves
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000922 - LSC Hierar. Node Link
        /// LS Central WS4 : GetHierarchyLeaf
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy node leaves</returns>
        [OperationContract]
        ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deals for Node Leaf
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001503 - LSC Offer Line
        /// LS Central WS4 : GetHierarchyDeal
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deals</returns>
        [OperationContract]
        ReplHierarchyHospDealResponse ReplEcommHierarchyHospDeal(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deal lines for Deal
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001651 - LSC Deal Modifier Item
        /// LS Central WS4 : GetHierarchyDealLine
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deal lines</returns>
        [OperationContract]
        ReplHierarchyHospDealLineResponse ReplEcommHierarchyHospDealLine(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Recipe lines for Node Leaf
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 90 - BOM Component
        /// LS Central WS4 : GetWIItemRecipeBuffer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy recipe items</returns>
        [OperationContract]
        ReplItemRecipeResponse ReplEcommItemRecipe(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Modifier lines for Node Leaf
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001483 - LSC Information Subcode
        /// LS Central WS4 : GetWIItemModifier
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy modifier lines</returns>
        [OperationContract]
        ReplItemModifierResponse ReplEcommItemModifier(ReplRequest replRequest);

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
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML, Sales Price, Item Variant, Item Unit of Measure, Variants and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Item objects</returns>
        [OperationContract]
        ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts and MultiBuy for Items from WI Discount table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012862 - LSC WI Discounts
        /// LS Central WS4 : GetWIDiscounts
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest);

        /// <summary>
        /// Replicate Mix and Match Offers for Items from WI Mix and Match Offer table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012863 - LSC WI Mix and Match Offer
        /// LS Central WS4 : GetWIMixMatch
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        ReplMixMatchResponse ReplEcommMixAndMatch(ReplRequest replRequest);

        /// <summary>
        /// Replicate Discount Setup from Central<p/>
        /// Only Multibuy, Discount, Total and Tender discounts are replicated
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001453 - LSC Periodic Discount
        /// <p/><p/>
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Periodic Discounts</returns>
        [OperationContract]
        ReplDiscountSetupResponse ReplEcommDiscountSetup(ReplRequest replRequest);

        /// <summary>
        /// Replicate Validation Periods for Discounts<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001481 - LSC Validation Period
        /// LS Central WS4 : GetValidationPeriod
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
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
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of shipping agents</returns>
        [OperationContract]
        ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest);

        /// <summary>
        /// Replicate Member contacts
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009002 - LSC Member Contact (with valid Membership Card)
        /// LS Central WS4 : GetContact
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Member contacts</returns>
        [OperationContract]
        ReplCustomerResponse ReplEcommMember(ReplRequest replRequest);

        /// <summary>
        /// Replicate Customers
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 18 - Customer
        /// LS Central WS4 : GetCustomer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Customers</returns>
        [OperationContract]
        ReplCustomerResponse ReplEcommCustomer(ReplRequest replRequest);

        /// <summary>
        /// Replicate all Country Codes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 9 - Country/Region
        /// LS Central WS4 : GetCountryCode
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Country codes</returns>
        [OperationContract]
        ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Tender types for Store
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001466 - LSC Tender Type Setup
        /// LS Central WS4 : GetTenderType
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest);

        /// <summary>
        /// Replicate Tax setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 325 - VAT Posting Setup
        /// LS Central WS4 : GetVATPostingSetup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        ReplTaxSetupResponse ReplEcommTaxSetup(ReplRequest replRequest);

        /// <summary>
        /// Replicate Inventory Status
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001608 - LSC Inventory Lookup Table
        /// LS Central WS4 : GetInventoryStatus
        /// <p/><p/>
        /// Net Inventory field in Inventory Lookup Table must be updated before the replication can be done.  
        /// In Retail Product Group card, set up which products to check status for by click on Update POS Inventory Lookup button. Set store to be Web Store.
        /// Run Scheduler job with CodeUnit 10012871 - WI Update Inventory which will update the Net Inventory field.
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest);

        #endregion

        #region Search

        /// <summary>
        /// Search different data based on SearchType value
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="search"></param>
        /// <param name="searchTypes"></param>
        /// <returns></returns>
        [OperationContract]
        SearchRs Search(string cardId, string search, SearchType searchTypes);

        #endregion search

        #region LS Recommends

        /// <summary>
        /// Checks if LS Recommend is active in Commerce Service for LS Central
        /// <p/>NOTE: Not supported anymore
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [Obsolete("Not supported anymore", true)]
        bool RecommendedActive();

        /// <summary>
        /// Get Recommended Items based of list of items
        /// <p/>NOTE: Not supported anymore
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [OperationContract]
        [Obsolete("Not supported anymore", true)]
        List<RecommendedItem> RecommendedItemsGet(List<string> items);

        #endregion

        #region Activity

        /// <summary>
        /// Confirm Activity Booking
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ConfirmActivityVx<p/><p/>
        /// If property [Paid] is set, then returns details for the retail basket.<p/>
        /// [BookingRef] should be assigned to the OrderLine and passed in with Order so retrieved basket payment through Commerce Service for LS Central will update the Activities payment status and assign the sales order document as payment document.<p/> 
        /// If activity type does not require [contactNo] then it is sufficient to provide client name.<p/>
        /// If [ReservationNo] is blank the system will create new reservation and return the value to ReservationNo.  If ReservationNo is populated parameter then the system will try to add the activity to existing reservation if the reservation exists and is neither canceled or closed.<p/>
        /// [PromoCode] is validated and adjusts pricing accordingly.
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Activity/2017">
        ///    <soapenv:Header/>
        ///    <soapenv:Body>
        ///       <ser:ActivityConfirm>
        ///          <ser:request>
        ///             <ns:ActivityTime>2019-12-10T13:00:00</ns:ActivityTime>
        ///             <ns:ContactName>Tom</ns:ContactName>
        ///             <ns:ContactNo>MO000008</ns:ContactNo>
        ///             <ns:Location>CAMBRIDGE</ns:Location>
        ///             <ns:NoOfPeople>1</ns:NoOfPeople>
        ///             <ns:Paid>false</ns:Paid>
        ///             <ns:ProductNo>MASSAGE30</ns:ProductNo>
        ///             <ns:Quantity>1</ns:Quantity>
        ///             <ns:ReservationNo>RES0044</ns:ReservationNo>
        ///          </ser:request>
        ///       </ser:ActivityConfirm>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>Activity Number and Booking Reference</returns>
        [OperationContract]
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
        ActivityResponse ActivityCancel(string activityNo);

        /// <summary>
        /// Returns list of available time-slots/prices for a specific location,product and date 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAvailabilityVx<p/><p/>
        /// Optional to include required resource (if only specific resource) and contactNo for accurate pricing.
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="contactNo"></param>
        /// <param name="contactAccount"></param>
        /// <param name="optionalResource"></param>
        /// <param name="promoCode"></param>
        /// <param name="activityNo"></param>
        /// <param name="noOfPersons"></param>
        /// <param name="guestType"></param>
        /// <returns></returns>
        [OperationContract]
        List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons, string guestType);

        /// <summary>
        /// Returns list with the required or optional additional charges for the Activity as applied automatically according to the product
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAdditionalCharges
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<AdditionalCharge> ActivityAdditionalChargesGet(string activityNo);

        /// <summary>
        /// Returns list of charges for products
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetProductChargesV2
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="dateOfBooking"></param>
        /// <returns></returns>
        [OperationContract]
        List<AdditionalCharge> ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking);

        /// <summary>
        /// Change or insert additional charges to Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAdditionalChargesVx
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Activity/2017">
        ///    <soapenv:Header/>
        ///    <soapenv:Body>
        ///       <ser:ActivityAdditionalChargesSet>
        ///          <ser:request>
        ///             <ns:ActivityNo>ACT0034</ns:ActivityNo>
        ///             <ns:DiscountPercentage>0.0</ns:DiscountPercentage>
        ///             <ns:InvoiceReference></ns:InvoiceReference>
        ///             <ns:ItemNo>40020</ns:ItemNo>
        ///             <ns:LineNo>1</ns:LineNo>
        ///             <ns:Optional></ns:Optional>
        ///             <ns:OptionalComment></ns:OptionalComment>
        ///             <ns:ParentLine>0</ns:ParentLine>
        ///             <ns:Price>110.0</ns:Price>
        ///             <ns:ProductType>Item</ns:ProductType>
        ///             <ns:Quantity>1.0</ns:Quantity>
        ///             <ns:TotalAmount>110.0</ns:TotalAmount>
        ///             <ns:UnitOfMeasure></ns:UnitOfMeasure>
        ///             <ns:VariantCode></ns:VariantCode>
        ///          </ser:request>
        ///       </ser:ActivityAdditionalChargesSet>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityAdditionalChargesSet(AdditionalCharge request);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetReservationAdditionalCharges
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<AdditionalCharge> ActivityReservationAdditionalChargesGet(string reservationNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetGroupAdditionalCharges
        /// </remarks>
        /// <param name="reqest"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityGroupAdditionalChargesSet(AdditionalCharge reqest);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetGroupReservationAdditionalCharges
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<AdditionalCharge> ActivityGroupReservationAdditionalChargesGet(string reservationNo, int memberNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetGroupMembers
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberSequence"></param>
        /// <param name="contact"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="guestType"></param>
        /// <param name="optionalComment"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityGroupMemberSet(string reservationNo, int memberSequence, string contact, string name, string email, string phone, string guestType, string optionalComment);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : AssignGroupMember
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberSequence"></param>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityGroupMemberAssign(string reservationNo, int memberSequence, int lineNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : RemoveGroupMember
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberSequence"></param>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityGroupMemberRemove(string reservationNo, int memberSequence, int lineNo);

        /// <summary>
        /// Returns list of Attributes which are assigned on a given Activity product, reservation or activity entry
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAttributes
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<AttributeResponse> ActivityAttributesGet(AttributeType type, string linkNo);

        /// <summary>
        /// Action to set an attribute value on a given reservation or activity.  If attribute does not exist on the entry then its inserted otherwise updated
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAttribute
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <param name="attributeCode"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        [OperationContract]
        int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue);

        /// <summary>
        /// Action to create a Reservation header into the LS Reservation table
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : InsertReservationVx
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to LS Commerce.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Activity/2017">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///      <ser:ActivityReservationInsert>
        ///         <ser:request>
        ///            <ns1:ContactName>Tom</ns1:ContactName>
        ///            <ns1:ContactNo>MO000008</ns1:ContactNo>
        ///            <ns1:Description></ns1:Description>
        ///            <ns1:Email>tom@xxx.com</ns1:Email>
        ///            <ns1:EventNo></ns1:EventNo>
        ///            <ns1:Internalstatus>0</ns1:Internalstatus>
        ///            <ns1:Location>CAMBRIDGE</ns1:Location>
        ///            <ns1:NoOfPerson>1</ns1:NoOfPerson>
        ///            <ns1:ResDateFrom>2019-10-10</ns1:ResDateFrom>
        ///            <ns1:ResDateTo>2019-10-10</ns1:ResDateTo>
        ///            <ns1:ResTimeFrom>13:00:00</ns1:ResTimeFrom>
        ///            <ns1:ResTimeTo>14:00:00</ns1:ResTimeTo>
        ///            <ns1:ReservationType>SPA</ns1:ReservationType>
        ///            <ns1:SalesPerson>AH</ns1:SalesPerson>
        ///            <ns1:Status></ns1:Status>
        ///         </ser:request>
        ///      </ser:ActivityReservationInsert>
        ///   </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        string ActivityReservationInsert(Reservation request);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateReservationStatus
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="setStatusCode"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityUpdateReservationStatus(string reservationNo, string setStatusCode);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateActivityStatus
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <param name="setStatusCode"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityUpdateActivityStatus(string activityNo, string setStatusCode);

        /// <summary>
        /// Action to force update to a reservation header in the LS Reservation table.  Blank fields will be ignored
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateReservationVx
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        string ActivityReservationUpdate(Reservation request);

        /// <summary>
        /// Sell Membership (membership type) to Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SellMembership
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="membersShipType"></param>
        /// <returns></returns>
        [OperationContract]
        MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType);

        /// <summary>
        /// Cancels a specific membership and validates if cancellation is in order (i.e. compares to commitment period)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelMembership
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="memberShipNo"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment);

        /// <summary>
        /// Get availability for specific resource, for a specific date and location (all required parameters)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetResourceAvailability
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="resourceNo"></param>
        /// <param name="intervalType">Use specific intervals setup in the system or leave blank for whole day</param>
        /// <param name="noOfDays">Set how many days to return availability, if set to 0 then use default setting (10 days normally)</param>
        /// <returns></returns>
        [OperationContract]
        List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays);

        /// <summary>
        /// Get availability for all active resource in specific resource group, for a specific date and location (all required parameters)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetResourceGroupAvailability
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="groupNo"></param>
        /// <param name="intervalType">Use specific intervals setup in the system or leave blank for whole day</param>
        /// <param name="noOfDays">Set how many days to return availability, if set to 0 then use default setting (10 days normally)</param>
        /// <returns></returns>
        [OperationContract]
        List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays);

        /// <summary>
        /// Check if valid access for either membership or ticketing.  
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CheckAccess
        /// </remarks>
        /// <param name="searchReference">Either TicketBarcode, Member No. or Membership No. LocationNo</param>
        /// <param name="locationNo">Optional Activity Location</param>
        /// <param name="gateNo">Optional Gate number</param>
        /// <param name="registerAccessEntry">Set if to register admission</param>
        /// <param name="checkType">0 = All checks, 1 = CheckTicket only, 2 = CheckMembership only</param>
        /// <param name="messageString">Returned info</param>
        /// <returns>Returns true or false if access is valid</returns>
        [OperationContract]
        bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString);

        /// <summary>
        /// Get Availability Token
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAvailabilityToken
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="activityTime"></param>
        /// <param name="optionalResource"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [OperationContract]
        string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activityTime, string optionalResource, int quantity);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ExtendToken
        /// </remarks>
        /// <param name="tokenId"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityExtendToken(string tokenId, int seconds);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelToken
        /// </remarks>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityCancelToken(string tokenId);

        /// <summary>
        /// Create Group Reservation
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : InsertGroupReservationVx
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        string ActivityInsertGroupReservation(Reservation request);

        /// <summary>
        /// Update Group reservation header.  Blank fields will be ignored
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateGroupReservationVx
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        string ActivityUpdateGroupReservation(Reservation request);

        /// <summary>
        /// Confirm Group Activity Booking
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ConfirmGroupActivityVx<p/><p/>
        /// If property [Paid] is set, then returns details for the retail basket.<p/>
        /// [BookingRef] should be assigned to the OrderLine and passed in with Order so retrieved basket payment through Commerce Service for LS Central will update the Activities payment status and assign the sales order document as payment document.<p/> 
        /// If activity type does not require [contactNo] then it is sufficient to provide client name.<p/>
        /// If [ReservationNo] is blank the system will create new reservation and return the value to ReservationNo.  If ReservationNo is populated parameter then the system will try to add the activity to existing reservation if the reservation exists and is neither canceled or closed.<p/>
        /// [PromoCode] is validated and adjusts pricing accordingly.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>Activity Number and Booking Reference</returns>
        [OperationContract]
        ActivityResponse ActivityConfirmGroup(ActivityRequest request);

        /// <summary>
        /// Delete Group Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : DeleteGroupActivity
        /// </remarks>
        /// <param name="groupNo"></param>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        [OperationContract]
        bool ActivityDeleteGroup(string groupNo, int lineNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateGroupHeaderStatus
        /// </remarks>
        /// <param name="groupNo"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        [OperationContract]
        string ActivityUpdateGroupHeaderStatus(string groupNo, string statusCode);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : PreSellActivityProduct
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="promoCode"></param>
        /// <param name="contactNo"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [OperationContract]
        ActivityResponse ActivityPreSellProduct(string locationNo, string productNo, string promoCode, string contactNo, int quantity);

        #endregion

        #region Activity Data Get (Replication)

        /// <summary>
        /// Returns list of Activity Products
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityProducts
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<ActivityProduct> ActivityProductsGet();

        /// <summary>
        /// Returns list of Activity Types
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityTypes
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<ActivityType> ActivityTypesGet();

        /// <summary>
        /// Returns list of Activity Locations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityLocations
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<ActivityLocation> ActivityLocationsGet();

        /// <summary>
        /// Returns list of Reservations for Member Contact or list of Activities assigned to a Reservation
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : <p/>
        /// With [contactNo, activityType] UploadClientBookingsV2<p/>
        /// With [reservationNo] : UploadReservationActivities
        /// </remarks>
        /// <param name="reservationNo">Look up Activities for a Reservation</param>
        /// <param name="contactNo">Look up Reservations for a Contact</param>
        /// <param name="activityType">Activity type for Contact Lookup</param>
        /// <returns></returns>
        [OperationContract]
        List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType);

        /// <summary>
        /// Look up Reservation Headers
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetActReservations
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="reservationType"></param>
        /// <param name="status"></param>
        /// <param name="locationNo"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        [OperationContract]
        List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate);

        /// <summary>
        /// Returns list of Active Promotions (for information purposes only)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadPromotions
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<Promotion> ActivityPromotionsGet();

        /// <summary>
        /// Returns list of Member Contacts issued (sold) allowances
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadPurchasedAllowances
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        List<Allowance> ActivityAllowancesGet(string contactNo);

        /// <summary>
        /// Returns list of all entries charged to the Member Contact customer account (A/R). The Account no. is based on the contact business relation settings
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadCustomerEntries
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo);

        /// <summary>
        /// Returns list of Membership types (products) which are active and can be sold 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipProducts
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<MemberProduct> ActivityMembershipProductsGet();

        /// <summary>
        /// Returns list of all subscription charges posted towards their membership account. Draft unposted entries are not included
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipSubscriptionCharges
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo);

        /// <summary>
        /// Returns list of Member Contact visit registrations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadAdmissionEntries
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo);

        /// <summary>
        /// Returns list of the Member Contact current active or on hold memberships
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipEntries
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        List<Membership> ActivityMembershipsGet(string contactNo);

        /// <summary>
        /// Get list of activities assigned to a resource, required parameters Resource code (number), Date from and to date
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadResourceActivities
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="resourceNo"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [OperationContract]
        List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get list of all resources 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityResources
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<ActivityResource> ActivityResourceGet();

        #endregion

        #region ScanPayGo

        /// <summary>
        /// Creates a client token for payment provider
        /// </summary>
        /// <param name="customerId">Customer id, used to show saved cards</param>
        /// <returns></returns>
        [OperationContract]
        ClientToken PaymentClientTokenGet(string customerId);

        /// <summary>
        /// Gets Profile setup for SPG App
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SPGProfileGet
        /// </remarks>
        /// <param name="profileId"></param>
        /// <param name="storeNo"></param>
        /// <returns></returns>
        [OperationContract]
        ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo);

        /// <summary>
        /// Check security status of a profile
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SecurityCheckProfile
        /// </remarks>
        /// <param name="orderNo"></param>
        /// <param name="storeNo"></param>
        /// <returns></returns>
        [OperationContract]
        bool SecurityCheckProfile(string orderNo, string storeNo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [OperationContract]
        ScanPayGoSecurityLog SecurityCheckLog(string orderNo);

        /// <summary>
        /// Allow app to open Gate when exiting the store
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SPGOpenGate
        /// </remarks>
        /// <param name="qrCode"></param>
        /// <param name="storeNo"></param>
        /// <param name="devLocation"></param>
        /// <param name="memberAccount"></param>
        /// <param name="exitWithoutShopping"></param>
        /// <param name="isEntering"></param>
        /// <returns></returns>
        [OperationContract]
        string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering);

        /// <summary>
        /// Add Payment token
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetTokenEntry, SetMemberCardToken, DeleteMemberCardToken
        /// </remarks>
        /// <param name="token"></param>
        /// <param name="deleteToken">Delete token, Send token with token and cardId to delete</param>
        /// <returns></returns>
        [OperationContract]
        bool TokenEntrySet(ClientToken token, bool deleteToken);

        /// <summary>
        /// Get Payment token
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetTokenEntry, GetMemberCardToken
        /// </remarks>
        /// <param name="accountNo"></param>
        /// <param name="hotelToken">Get token for LS Hotels</param>
        /// <returns></returns>
        [OperationContract]
        List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken);

        /// <summary>
        /// Request to unlock Rod Device
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        bool SpgUnlockRodDevice(string storeId, string cardId);

        /// <summary>
        /// Used by Rod Device to check if there is request to unlock a device
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        string SpgUnlockRodDeviceCheck(string storeId);

        /// <summary>
        /// Register for Push Notifications when Wish list gets some updates, like new follower or items
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [OperationContract]
        bool SpgRegisterNotification(string cardId, string token);

        /// <summary>
        /// Un-Register Push Notifications for Wish list updates
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        bool SpgUnRegisterNotification(string cardId);

        /// <summary>
        /// Get codes
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string GetAuthCodes();
		
        [OperationContract]
        string MyCustomFunction(string data);

        #endregion
    }
}