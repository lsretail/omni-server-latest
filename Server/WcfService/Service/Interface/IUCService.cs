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
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        /// <summary>
        /// Get related items in a published offer
        /// </summary>
        /// <remarks>
        /// LS Nav  WS1 : LOAD_PUBLISHED_OFFER_ITEMS<p/><p/>
        /// </remarks>
        /// <param name="pubOfferId">Published offer id</param>
        /// <param name="numberOfItems">Number of items to return</param>
        /// <returns></returns>
        [OperationContract]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        /// <summary>
        /// Get discounts for items. Send in empty string for loyaltySchemeCode if getting anonymously.
        /// </summary>
        /// <param name="storeId">Store Id</param>
        /// <param name="itemiIds">List of item ids to check for discounts</param>
        /// <param name="loyaltySchemeCode">[OPTIONAL] Loyalty scheme code for a user</param>
        /// <returns></returns>
        [OperationContract]
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
        GiftCard GiftCardGetBalance(string cardNo);

        [OperationContract]
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
        /// as LS Commerce Service does not store any information for Anonymous Users.<p/>
        /// Used OneListGetById to get the OneList back.
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        ///                  <ns1:ItemDescription>Skirt Linda Professional Wear</ns1:ItemDescription>
        ///                  <ns1:ItemId>40020</ns1:ItemId>
        ///                  <ns1:Quantity>2</ns1:Quantity>
        ///                  <ns1:VariantDescription>YELLOW/38</ns1:VariantDescription>
        ///                  <ns1:VariantId>002</ns1:VariantId>
        ///               </ns1:OneListItem>
        ///            </ns1:Items>
        ///            <ns1:ListType>Basket</ns1:ListType>
        ///            <ns1:StoreId>S0001</ns1:StoreId>
        ///         </ser:oneList>
        ///         <ser:calculate>true</ser:calculate>
        ///      </ser:OneListSave>
        ///   </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// Response Data from LS Commerce Service after Request has been sent
        /// <code language="soap" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///   <s:Body>
        ///      <OneListSaveResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///         < OneListSaveResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///            <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > 4979E04C-D037-4791-B3AA-1885106160E3</Id>
        ///            <a:CardId>10021</a:CardId>
        ///            <a:CardLinks>
        ///               <a:OneListLink>
        ///                  <a:CardId>10021</a:CardId>
        ///                  <a:Name>Tom Thomson</a:Name>
        ///                  <a:Owner>true</a:Owner>
        ///                  <a:Status>Active</a:Status>
        ///               </a:OneListLink>
        ///            </a:CardLinks>
        ///            <a:CreateDate>2019-09-12T09:42:24.693</a:CreateDate>
        ///            <a:Description>Basket: 10021</a:Description>
        ///            <a:ExternalType>0</a:ExternalType>
        ///            <a:Items>
        ///               <a:OneListItem>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > A3C1C59D - F4E5 - 428E-8A3E-F80F91A507F6</Id>
        ///                  <a:Amount>160.00000000</a:Amount>
        ///                  <a:BarcodeId/>
        ///                  <a:CreateDate>2019-09-12T09:42:24.71</a:CreateDate>
        ///                  <a:Detail/>
        ///                  <a:DiscountAmount>0.00000000</a:DiscountAmount>
        ///                  <a:DiscountPercent>0.00000000</a:DiscountPercent>
        ///                  <a:DisplayOrderId>1</a:DisplayOrderId>
        ///                  <a:Image xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                     < b:Id>40020-Y</b:Id>
        ///                     <b:AvgColor/>
        ///                     <b:DisplayOrder>0</b:DisplayOrder>
        ///                     <b:Format/>
        ///                     <b:Image/>
        ///                     <b:ImgSize>
        ///                        <b:Height>0</b:Height>
        ///                        <b:UseMinHorVerSize>false</b:UseMinHorVerSize>
        ///                        <b:Width>0</b:Width>
        ///                     </b:ImgSize>
        ///                     <b:LoadFromFile>false</b:LoadFromFile>
        ///                     <b:Location>http://dhqsrvomni001.lsretail.local/lsomniservice/ucservice.svc/ImageStreamGetById?id=40020-Y&amp;width={0}&amp;height={1}</b:Location>
        ///                     <b:LocationType>Image</b:LocationType>
        ///                  </a:Image>
        ///                  <a:ItemDescription>Skirt Linda Professional Wear</a:ItemDescription>
        ///                  <a:ItemId>40020</a:ItemId>
        ///                  <a:NetAmount>128.00000000</a:NetAmount>
        ///                  <a:NetPrice>64.00000000</a:NetPrice>
        ///                  <a:OnelistItemDiscounts/>
        ///                  <a:Price>80.00000000</a:Price>
        ///                  <a:Quantity>2.00000000</a:Quantity>
        ///                  <a:TaxAmount>32.00000000</a:TaxAmount>
        ///                  <a:UnitOfMeasureDescription i:nil= "true" />
        ///                  <a:UnitOfMeasureId/>
        ///                  <a:VariantDescription>YELLOW/38</a:VariantDescription>
        ///                  <a:VariantId>002</a:VariantId>
        ///                  <a:VariantRegistration xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                     <b:Id>1BAC4050-C3CA-4ADB-A979-D80BC43EC10A</b:Id>
        ///                     <b:Dimension1/>
        ///                     <b:Dimension2/>
        ///                     <b:Dimension3/>
        ///                     <b:Dimension4/>
        ///                     <b:Dimension5/>
        ///                     <b:Dimension6/>
        ///                     <b:FrameworkCode/>
        ///                     <b:Images/>
        ///                     <b:ItemId/>
        ///                  </a:VariantRegistration>
        ///               </a:OneListItem>
        ///            </a:Items>
        ///            <a:ListType>Basket</a:ListType>
        ///            <a:PointAmount>0</a:PointAmount>
        ///            <a:PublishedOffers/>
        ///            <a:ShippingAmount>0.00000000</a:ShippingAmount>
        ///            <a:StoreId>S0001</a:StoreId>
        ///            <a:TotalAmount>160.00000000</a:TotalAmount>
        ///            <a:TotalDiscAmount>0.00000000</a:TotalDiscAmount>
        ///            <a:TotalNetAmount>128.00000000</a:TotalNetAmount>
        ///            <a:TotalTaxAmount>32.00000000</a:TotalTaxAmount>
        ///         </OneListSaveResult>
        ///      </OneListSaveResponse>
        ///   </s:Body>
        /// </s:Envelope>
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
        /// LS Nav  WS1 : ECOMM_CALCULATE_BASKET<p/>
        /// LS Central WS2 : EcomCalculateBasket<p/><p/>
        /// This function can be used to send in Basket and convert it to Order.<p/>
        /// Basic Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        /// Response Data from LS Commerce Service after Request has been sent
        /// <code language="xml" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///    <s:Body>
        ///       <OneListCalculateResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///          <OneListCalculateResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///             <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" >{16B13DED-C5BE-4462-9223-5A45A9569F84}</Id>
        ///             <a:CardId>10021</a:CardId>
        ///             <a:ClickAndCollectOrder>false</a:ClickAndCollectOrder>
        ///             <a:CollectLocation i:nil="true"/>
        ///             <a:ContactAddress xmlns:b="http://lsretail.com/LSOmniService/Base/2017">
        ///                <b:Address1/>
        ///                <b:Address2/>
        ///                <b:CellPhoneNumber i:nil="true"/>
        ///                <b:City/>
        ///                <b:Country/>
        ///                <b:HouseNo i:nil="true"/>
        ///                <b:Id/>
        ///                <b:PhoneNumber i:nil="true"/>
        ///                <b:PostCode/>
        ///                <b:StateProvinceRegion/>
        ///                <b:Type>Residential</b:Type>
        ///             </a:ContactAddress>
        ///             <a:ContactId i:nil="true"/>
        ///             <a:ContactName i:nil="true"/>
        ///             <a:DayPhoneNumber i:nil="true"/>
        ///             <a:DocumentId i:nil="true"/>
        ///             <a:Email i:nil="true"/>
        ///             <a:LineItemCount>0</a:LineItemCount>
        ///             <a:MobileNumber i:nil="true"/>
        ///             <a:OrderDiscountLines/>
        ///             <a:OrderLines>
        ///                <a:OrderLine>
        ///                   <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///                   <a:Amount>160.00</a:Amount>
        ///                   <a:DiscountAmount>0.00</a:DiscountAmount>
        ///                   <a:DiscountPercent>0.00</a:DiscountPercent>
        ///                   <a:ItemDescription>Skirt Linda Professional Wear</a:ItemDescription>
        ///                   <a:ItemId>40020</a:ItemId>
        ///                   <a:ItemImageId>40020-Y</a:ItemImageId>
        ///                   <a:LineNumber>1</a:LineNumber>
        ///                   <a:LineType>Item</a:LineType>
        ///                   <a:NetAmount>128.00</a:NetAmount>
        ///                   <a:NetPrice>64.00</a:NetPrice>
        ///                   <a:OrderId/>
        ///                   <a:Price>80.00</a:Price>
        ///                   <a:Quantity>2.00</a:Quantity>
        ///                   <a:QuantityOutstanding>0</a:QuantityOutstanding>
        ///                   <a:QuantityToInvoice>2.00</a:QuantityToInvoice>
        ///                   <a:QuantityToShip>0</a:QuantityToShip>
        ///                   <a:TaxAmount>32.00</a:TaxAmount>
        ///                   <a:UomId/>
        ///                   <a:VariantDescription>YELLOW/38</a:VariantDescription>
        ///                   <a:VariantId>002</a:VariantId>
        ///                </a:OrderLine>
        ///             </a:OrderLines>
        ///             <a:OrderPayments/>
        ///             <a:OrderStatus>Pending</a:OrderStatus>
        ///             <a:PaymentStatus>PreApproved</a:PaymentStatus>
        ///             <a:PointAmount>1600.00</a:PointAmount>
        ///             <a:PointBalance>53632.00</a:PointBalance>
        ///             <a:PointCashAmountNeeded>0.00</a:PointCashAmountNeeded>
        ///             <a:PointsRewarded>160.00</a:PointsRewarded>
        ///             <a:PointsUsedInOrder>0.00</a:PointsUsedInOrder>
        ///             <a:Posted>false</a:Posted>
        ///             <a:ReceiptNo/>
        ///             <a:ShipToAddress xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                <b:Address1/>
        ///                <b:Address2/>
        ///                <b:CellPhoneNumber i:nil= "true" />
        ///                <b:City/>
        ///                <b:Country/>
        ///                <b:HouseNo i:nil= "true" />
        ///                <b:Id/>
        ///                <b:PhoneNumber i:nil= "true" />
        ///                <b:PostCode/>
        ///                <b:StateProvinceRegion/>
        ///                <b:Type>Residential</b:Type>
        ///             </a:ShipToAddress>
        ///             <a:ShipToEmail i:nil= "true" />
        ///             <a:ShipToName i:nil= "true" />
        ///             <a:ShippingAgentCode i:nil= "true" />
        ///             <a:ShippingAgentServiceCode i:nil= "true" />
        ///             <a:ShippingStatus>ShippigNotRequired</a:ShippingStatus>
        ///             <a:StoreId>S0013</a:StoreId>
        ///             <a:TotalAmount>160.00</a:TotalAmount>
        ///             <a:TotalDiscount>0.00</a:TotalDiscount>
        ///             <a:TotalNetAmount>128.00</a:TotalNetAmount>
        ///          </OneListCalculateResult>
        ///       </OneListCalculateResponse>
        ///    </s:Body>
        /// </s:Envelope>
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
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Basket with Item and Recipe modification (remove) and Modifier add-on (add item)
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
        ///                 <!-- Chicken -->
        ///                 <ns1:IsADeal>false</ns1:IsADeal>
        ///                 <ns1:ItemId>R0001</ns1:ItemId>
        ///                 <ns1:OnelistSubLines>
        ///                    <ns1:OneListItemSubLine>
        ///                       <!-- Remove Recipe Item -->
        ///                       <ns1:ItemId>R0002</ns1:ItemId>
        ///                       <ns1:Quantity>0</ns1:Quantity>
        ///                       <ns1:Type>Modifier</ns1:Type>
        ///                    </ns1:OneListItemSubLine>
        ///                    <ns1:OneListItemSubLine>
        ///                       <!-- Add Modifier -->
        ///                       <ns1:ModifierGroupCode>POT+RICE</ns1:ModifierGroupCode>
        ///                       <ns1:ModifierSubCode>01</ns1:ModifierSubCode>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Modifier</ns1:Type>
        ///                    </ns1:OneListItemSubLine>
        ///                 </ns1:OnelistSubLines>
        ///                 <ns1:Quantity>1</ns1:Quantity>
        ///                 <ns1:UnitOfMeasureId></ns1:UnitOfMeasureId>
        ///              </ns1:OneListItem>
        ///           </ns1:Items>
        ///           <ns1:ListType>Basket</ns1:ListType>
        ///           <ns1:StoreId>S0005</ns1:StoreId>
        ///        </ser:oneList>
        ///     </ser:OneListHospCalculate>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// Basket to order SetMenu Deal (Burger with Drink and Fries) with burger item recipe changes (remove bacon)
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
        ///                 <!-- Burger Deal SetMenu -->
        ///                 <ns1:IsADeal>true</ns1:IsADeal>
        ///                 <ns1:ItemId>S10024</ns1:ItemId>
        ///                 <ns1:LineNumber>1</ns1:LineNumber>
        ///                 <ns1:OnelistSubLines>
        ///                    <ns1:OneListItemSubLine>
        ///                       <!-- Burger -->
        ///                       <ns1:DealLineId>1</ns1:DealLineId>
        ///                       <ns1:LineNumber>1</ns1:LineNumber>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Deal</ns1:Type>
        ///                    </ns1:OneListItemSubLine>
        ///                    <ns1:OneListItemSubLine>
        ///                       <!-- Remove Bacon -->
        ///                       <ns1:DealLineId>1</ns1:DealLineId>
        ///                       <ns1:ItemId>34420</ns1:ItemId>
        ///                       <ns1:LineNumber>2</ns1:LineNumber>
        ///                       <ns1:ParentSubLineId>1</ns1:ParentSubLineId>
        ///                       <ns1:Quantity>0</ns1:Quantity>
        ///                       <ns1:Type>Modifier</ns1:Type>
        ///                    </ns1:OneListItemSubLine>
        ///                    <ns1:OneListItemSubLine>
        ///                       <!-- Garlic Bread Sticks -->
        ///                       <ns1:DealLineId>2</ns1:DealLineId>
        ///                       <ns1:DealModLineId>2</ns1:DealModLineId>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Deal</ns1:Type>
        ///                       <ns1:Uom>LAR</ns1:Uom>
        ///                    </ns1:OneListItemSubLine>
        ///                    <ns1:OneListItemSubLine>
        ///                       <!-- Cherry Soda -->
        ///                       <ns1:DealLineId>3</ns1:DealLineId>
        ///                       <ns1:DealModLineId>2</ns1:DealModLineId>
        ///                       <ns1:Quantity>1</ns1:Quantity>
        ///                       <ns1:Type>Deal</ns1:Type>
        ///                       <ns1:Uom>LAR</ns1:Uom>
        ///                    </ns1:OneListItemSubLine>
        ///                 </ns1:OnelistSubLines>
        ///                 <ns1:Quantity>1</ns1:Quantity>
        ///                 <ns1:UnitOfMeasureId></ns1:UnitOfMeasureId>
        ///              </ns1:OneListItem>
        ///           </ns1:Items>
        ///           <ns1:ListType>Basket</ns1:ListType>
        ///           <ns1:StoreId>S0005</ns1:StoreId>
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
        /// <param name="onelistId">OneList Id</param>
        /// <param name="item">OneList Item to add or remove</param>
        /// <param name="remove">true if remove item, else false</param>
        /// <param name="calculate">Recalculate OneList</param>
        /// <returns>Updated OneList</returns>
        [OperationContract]
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
        bool OneListLinking(string oneListId, string cardId, string email, LinkStatus status);

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity available of items in order for certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <remarks>
        /// LS Nav  WS1 : CO_QTY_AVAILABILITY_EXT<p/>
        /// LS Central WS2 : COQtyAvailabilityV2<p/><p/>
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request);

        /// <summary>
        /// Create Customer Order for ClickAndCollect or BasketPostSales 
        /// </summary>
        /// <remarks>
        /// LS Nav  WS1 : CUSTOMER_ORDER_CREATE_EXT<p/>
        /// LS Central WS2 : CustomerOrderCreateVx<p/><p/>
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///  <soapenv:Header/>
        ///  <soapenv:Body>
        ///     <ser:OrderCreate>
        ///        <ser:request>
        ///           <ns:Id></ns:Id>
        ///            <!--AnonymouseOrder leave empty-->
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <ns1:CollectLocation></ns1:CollectLocation>
        ///           <ns1:LineItemCount>1</ns1:LineItemCount>
        ///           <!--OrderLines need to have minimum Price and Net Price set for Order to be valid.Price is unit price and will be used when calculate the Line Total Amount -->
        ///           <ns1:OrderDiscountLines>
        ///           </ns1:OrderDiscountLines>
        ///           <ns1:OrderLines>
        ///              <ns1:OrderLine>
        ///                 <ns1:Amount>160.00</ns1:Amount>
        ///                 <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                 <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                 <ns1:ItemId>40020</ns1:ItemId>
        ///                 <ns1:LineNumber>1</ns1:LineNumber>
        ///                 <ns1:LineType>Item</ns1:LineType>
        ///                 <ns1:NetAmount>128.00</ns1:NetAmount>
        ///                 <ns1:NetPrice>64.00</ns1:NetPrice>
        ///                 <ns1:Price>80.00</ns1:Price>
        ///                 <ns1:Quantity>2.00</ns1:Quantity>
        ///                 <ns1:QuantityOutstanding>0</ns1:QuantityOutstanding>
        ///                 <ns1:QuantityToInvoice>2.00</ns1:QuantityToInvoice>
        ///                 <ns1:QuantityToShip>0</ns1:QuantityToShip>
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
        ///                 <ns1:AuthorisationCode>123456</ns1:AuthorisationCode>
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
        ///           <ns1:OrderStatus>Pending</ns1:OrderStatus>
        ///           <ns1:OrderType>Sale</ns1:OrderType>
        ///           <ns1:PaymentStatus>PreApproved</ns1:PaymentStatus>
        ///           <ns1:Posted>false</ns1:Posted>
        ///            <!--Optional: ShipToAddress can not be null if ClickAndCollectOrder == false-->
        ///           <ns1:ShipToAddress>
        ///              <ns:Address1>Some Address</ns:Address1>
        ///              <ns:Address2></ns:Address2>
        ///              <ns:CellPhoneNumber></ns:CellPhoneNumber>
        ///              <ns:City>Some City</ns:City>
        ///              <ns:Country></ns:Country>
        ///              <ns:HouseNo></ns:HouseNo>
        ///              <ns:Id></ns:Id>
        ///              <ns:PhoneNumber></ns:PhoneNumber>
        ///              <ns:PostCode>999</ns:PostCode>
        ///              <ns:StateProvinceRegion></ns:StateProvinceRegion>
        ///              <ns:Type>Residential</ns:Type>
        ///           </ns1:ShipToAddress>
        ///           <ns1:ShippingStatus>NotYetShipped</ns1:ShippingStatus>
        ///           <ns1:SourceType>eCommerce</ns1:SourceType>
        ///           <ns1:StoreId>S0013</ns1:StoreId>
        ///           <ns1:TotalAmount>160</ns1:TotalAmount>
        ///           <ns1:TotalDiscount>0</ns1:TotalDiscount>
        ///           <ns1:TotalNetAmount>128</ns1:TotalNetAmount>
        ///        </ser:request>
        ///     </ser:OrderCreate>
        ///  </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// Response Data from LS Commerce Service after Request has been sent
        /// <code language="xml" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///    <s:Body>
        ///       <OrderCreateResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///          < OrderCreateResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///             <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CO000007 </ Id >
        ///             <a:AnonymousOrder>false</a:AnonymousOrder>
        ///             <a:CardId>10021</a:CardId>
        ///             <a:ClickAndCollectOrder>false</a:ClickAndCollectOrder>
        ///             <a:DiscountLines/>
        ///             <a:DocumentRegTime>2019-10-22T09:47:20.857</a:DocumentRegTime>
        ///             <a:ExternalId>16B01BE6-BA8A-462E-97D7-18F63D7AFE81</a:ExternalId>
        ///             <a:IdType>Order</a:IdType>
        ///             <a:LineItemCount>2</a:LineItemCount>
        ///             <a:Lines>
        ///                <a:SalesEntryLine>
        ///                   <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///                   <a:Amount>160.00000000000000000000</a:Amount>
        ///                   <a:DiscountAmount>0.00000000000000000000</a:DiscountAmount>
        ///                   <a:DiscountPercent>0.00000000000000000000</a:DiscountPercent>
        ///                   <a:ItemDescription>Skirt Linda Professional Wear</a:ItemDescription>
        ///                   <a:ItemId>40020</a:ItemId>
        ///                   <a:ItemImageId>40020-Y</a:ItemImageId>
        ///                   <a:LineNumber>10000</a:LineNumber>
        ///                   <a:LineType>Item</a:LineType>
        ///                   <a:NetAmount>128.00000000000000000000</a:NetAmount>
        ///                   <a:NetPrice>64.00000000000000000000</a:NetPrice>
        ///                   <a:Price>80.00000000000000000000</a:Price>
        ///                   <a:Quantity>2.00000000000000000000</a:Quantity>
        ///                   <a:TaxAmount>32.00000000000000000000</a:TaxAmount>
        ///                   <a:UomId/>
        ///                   <a:VariantDescription>YELLOW/38</a:VariantDescription>
        ///                   <a:VariantId>002</a:VariantId>
        ///                </a:SalesEntryLine>
        ///             </a:Lines>
        ///             <a:PaymentStatus>PreApproved</a:PaymentStatus>
        ///             <a:Payments>
        ///                <a:SalesEntryPayment>
        ///                   <a:Amount>160.00000000000000000000</a:Amount>
        ///                   <a:CardNo>45XX..5555</a:CardNo>
        ///                   <a:CurrencyCode/>
        ///                   <a:CurrencyFactor>1.00000000000000000000</a:CurrencyFactor>
        ///                   <a:LineNumber>10000</a:LineNumber>
        ///                   <a:TenderType>1</a:TenderType>
        ///                </a:SalesEntryPayment>
        ///             </a:Payments>
        ///             <a:PointsRewarded>0</a:PointsRewarded>
        ///             <a:PointsUsedInOrder>0</a:PointsUsedInOrder>
        ///             <a:Posted>false</a:Posted>
        ///             <a:ShipToAddress xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                <b:Address1>Some Address</b:Address1>
        ///                <b:Address2/>
        ///                <b:CellPhoneNumber i:nil= "true" />
        ///                <b:City>Some City</b:City>
        ///                <b:Country/>
        ///                <b:HouseNo/>
        ///                <b:Id/>
        ///                <b:PhoneNumber i:nil= "true" />
        ///                <b:PostCode>999</b:PostCode>
        ///                <b:StateProvinceRegion/>
        ///                <b:Type>Residential</b:Type>
        ///             </a:ShipToAddress>
        ///             <a:ShipToEmail/>
        ///             <a:ShipToName/>
        ///             <a:ShippingAgentCode/>
        ///             <a:ShippingAgentServiceCode/>
        ///             <a:ShippingStatus>NotYetShipped</a:ShippingStatus>
        ///             <a:Status>Created</a:Status>
        ///             <a:StoreId>S0013</a:StoreId>
        ///             <a:StoreName i:nil= "true" />
        ///             <a:TerminalId i:nil= "true" />
        ///             <a:TotalAmount>160.00000000000000000000</a:TotalAmount>
        ///             <a:TotalDiscount>0.00000000000000000000</a:TotalDiscount>
        ///             <a:TotalNetAmount>128.00000000000000000000</a:TotalNetAmount>
        ///          </OrderCreateResult>
        ///       </OrderCreateResponse>
        ///    </s:Body>
        /// </s:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>SalesEntry object for order if order creation was successful</returns>
        [OperationContract]
        SalesEntry OrderCreate(Order request);

        /// <summary>
        /// Create a Hospitality Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MobilePosPost<p/><p/>
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Include minimum data needed to be able to process the request, 
        /// Sample Order with SetMenu (Burger, Fries and Drink) and remove recipe item (bacon)
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///    <soapenv:Header/>
        ///    <soapenv:Body>
        ///       <ser:OrderHospCreate>
        ///          <ser:request>
        ///             <ns1:CardId>10021</ns1:CardId>
        ///             <ns1:OrderLines>
        ///                <ns1:OrderHospLine>
        ///                   <ns1:Amount>0.0</ns1:Amount>
        ///                   <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                   <ns1:DiscountLines>
        ///                   </ns1:DiscountLines>
        ///                   <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                   <ns1:IsADeal>true</ns1:IsADeal>
        ///                   <ns1:ItemId>S10024</ns1:ItemId>
        ///                   <ns1:LineNumber>9750</ns1:LineNumber>
        ///                   <ns1:LineType>Item</ns1:LineType>
        ///                   <ns1:NetAmount>7.1</ns1:NetAmount>
        ///                   <ns1:NetPrice>7.1</ns1:NetPrice>
        ///                   <ns1:Price>7.1</ns1:Price>
        ///                   <ns1:Quantity>1</ns1:Quantity>
        ///                   <ns1:SubLines>
        ///                      <ns1:OrderHospSubLine>
        ///                         <ns1:DealId>S10024</ns1:DealId>
        ///                         <ns1:DealLineCode>10000</ns1:DealLineCode>
        ///                         <ns1:DealModifierLineCode>0</ns1:DealModifierLineCode>
        ///                         <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                         <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                         <ns1:ItemId>R0025</ns1:ItemId>
        ///                         <ns1:LineNumber>10000</ns1:LineNumber>
        ///                         <ns1:ModifierGroupCode></ns1:ModifierGroupCode>
        ///                         <ns1:ModifierSubCode></ns1:ModifierSubCode>
        ///                         <ns1:NetAmount>4.23</ns1:NetAmount>
        ///                         <ns1:NetPrice>4.23</ns1:NetPrice>
        ///                         <ns1:ParentIsSubLine>false</ns1:ParentIsSubLine>
        ///                         <ns1:Price>4.23</ns1:Price>
        ///                         <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                         <ns1:Quantity>1</ns1:Quantity>
        ///                         <ns1:TAXAmount>0</ns1:TAXAmount>
        ///                         <ns1:TextModifiers>
        ///                         </ns1:TextModifiers>
        ///                         <ns1:Type>Deal</ns1:Type>
        ///                         <ns1:Uom/>
        ///                      </ns1:OrderHospSubLine>
        ///                      <ns1:OrderHospSubLine>
        ///                         <ns1:DealId>S10024</ns1:DealId>
        ///                         <ns1:DealLineCode>20000</ns1:DealLineCode>
        ///                         <ns1:DealModifierLineCode>20000</ns1:DealModifierLineCode>
        ///                         <ns1:Description>Fries</ns1:Description>
        ///                         <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                         <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                         <ns1:ItemId>33410</ns1:ItemId>
        ///                         <ns1:LineNumber>30000</ns1:LineNumber>
        ///                         <ns1:ModifierGroupCode></ns1:ModifierGroupCode>
        ///                         <ns1:ModifierSubCode></ns1:ModifierSubCode>
        ///                         <ns1:NetAmount>2.21</ns1:NetAmount>
        ///                         <ns1:NetPrice>2.21</ns1:NetPrice>
        ///                         <ns1:ParentIsSubLine>false</ns1:ParentIsSubLine>
        ///                         <ns1:Price>2.21</ns1:Price>
        ///                         <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                         <ns1:Quantity>1</ns1:Quantity>
        ///                         <ns1:TAXAmount>0</ns1:TAXAmount>
        ///                         <ns1:TextModifiers>
        ///                         </ns1:TextModifiers>
        ///                         <ns1:Type>Deal</ns1:Type>
        ///                         <ns1:Uom/>
        ///                      </ns1:OrderHospSubLine>
        ///                      <ns1:OrderHospSubLine>
        ///                         <ns1:DealId>S10024</ns1:DealId>
        ///                         <ns1:DealLineCode>30000</ns1:DealLineCode>
        ///                         <ns1:DealModifierLineCode>20000</ns1:DealModifierLineCode>
        ///                         <ns1:Description>Cola</ns1:Description>
        ///                         <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                         <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                         <ns1:ItemId>30500</ns1:ItemId>
        ///                         <ns1:LineNumber>40000</ns1:LineNumber>
        ///                         <ns1:ModifierGroupCode></ns1:ModifierGroupCode>
        ///                         <ns1:ModifierSubCode></ns1:ModifierSubCode>
        ///                         <ns1:NetAmount>0.66</ns1:NetAmount>
        ///                         <ns1:NetPrice>0.66</ns1:NetPrice>
        ///                         <ns1:ParentIsSubLine>false</ns1:ParentIsSubLine>
        ///                         <ns1:Price>0.66</ns1:Price>
        ///                         <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                         <ns1:Quantity>1</ns1:Quantity>
        ///                         <ns1:TAXAmount>0</ns1:TAXAmount>
        ///                         <ns1:TextModifiers>
        ///                         </ns1:TextModifiers>
        ///                         <ns1:Type>Deal</ns1:Type>
        ///                         <ns1:Uom/>
        ///                      </ns1:OrderHospSubLine>
        ///                      <ns1:OrderHospSubLine>
        ///                         <ns1:DealId/>
        ///                         <ns1:DealLineCode>10000</ns1:DealLineCode>
        ///                         <ns1:DealModifierLineCode>0</ns1:DealModifierLineCode>
        ///                         <ns1:Description>Bacon</ns1:Description>
        ///                         <ns1:DiscountAmount>0</ns1:DiscountAmount>
        ///                         <ns1:DiscountPercent>0</ns1:DiscountPercent>
        ///                         <ns1:ItemId>34420</ns1:ItemId>
        ///                         <ns1:LineNumber>20000</ns1:LineNumber>
        ///                         <ns1:ModifierGroupCode></ns1:ModifierGroupCode>
        ///                         <ns1:ModifierSubCode></ns1:ModifierSubCode>
        ///                         <ns1:NetAmount>0</ns1:NetAmount>
        ///                         <ns1:NetPrice>0</ns1:NetPrice>
        ///                         <ns1:ParentIsSubLine>false</ns1:ParentIsSubLine>
        ///                         <ns1:Price>0</ns1:Price>
        ///                         <ns1:PriceReductionOnExclusion>false</ns1:PriceReductionOnExclusion>
        ///                         <ns1:Quantity>0</ns1:Quantity>
        ///                         <ns1:TAXAmount>0</ns1:TAXAmount>
        ///                         <ns1:TextModifiers>
        ///                         </ns1:TextModifiers>
        ///                         <ns1:Type>Modifier</ns1:Type>
        ///                         <ns1:Uom/>
        ///                      </ns1:OrderHospSubLine>
        ///                   </ns1:SubLines>
        ///                   <ns1:TaxAmount>0</ns1:TaxAmount>
        ///                   <ns1:TextModifierLines>
        ///                   </ns1:TextModifierLines>
        ///                   <ns1:UomId></ns1:UomId>
        ///                   <ns1:VariantId></ns1:VariantId>
        ///                </ns1:OrderHospLine>
        ///             </ns1:OrderLines>
        ///             <ns1:OrderPayments>
        ///                <ns1:OrderPayment>
        ///                   <ns1:Amount>7.1</ns1:Amount>
        ///                   <ns1:AuthorizationCode>123456</ns1:AuthorizationCode>
        ///                   <ns1:CardNumber>45XX..5555</ns1:CardNumber>
        ///                   <ns1:CardType>VISA</ns1:CardType>
        ///                   <ns1:CurrencyCode></ns1:CurrencyCode>
        ///                   <ns1:CurrencyFactor>1</ns1:CurrencyFactor>
        ///                   <ns1:PaymentType>Payment</ns1:PaymentType>
        ///                   <ns1:PreApprovedValidDate>2030-01-01</ns1:PreApprovedValidDate>
        ///                   <ns1:TenderType>1</ns1:TenderType>
        ///                </ns1:OrderPayment>
        ///             </ns1:OrderPayments>
        ///             <ns1:StoreId>S0005</ns1:StoreId>
        ///             <ns1:TotalAmount>7.1</ns1:TotalAmount>
        ///             <ns1:TotalDiscount>0</ns1:TotalDiscount>
        ///             <ns1:TotalNetAmount>7.1</ns1:TotalNetAmount>
        ///          </ser:request>
        ///       </ser:OrderHospCreate>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        SalesEntry OrderHospCreate(OrderHosp request);

        /// <summary>
        /// Check Status of a Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderStatus<p/><p/>
        /// </remarks>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
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
        List<SalesEntry> SalesEntriesGetByCardIdEx(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries);

        /// <summary>
        /// Get the Sale details (order/transaction)
        /// </summary>
        /// <param name="entryId">Sales Entry ID</param>
        /// <param name="type">Document Id type of the Sale entry</param>
        /// <returns>SalesEntry with Lines</returns>
        [OperationContract]
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type);

        #endregion

        #region Contact

        /// <summary>
        /// Create a new contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_CONTACT_CREATE<p/>
        /// LS Central WS2 : MemberContactCreate<p/><p/>
        /// Contact will get new Card that should be used when dealing with Orders.  Card Id is the unique identifier for Contacts in LS Nav/Central<p/>
        /// Contact will be assigned to a Member Account.
        /// Member Account has Club and Club has Scheme level.<p/>
        /// If No Account is provided, New Account will be created.
        /// If No Club level for the Account is set, then default Club and Scheme level will be set.<p/>
        /// Valid UserName, Password and Email address is determined by LS Nav/Central and can be found in CU 99009001.
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        /// Response Data from LS Commerce Service after Request has been sent
        /// <code language="xml" title="SOAP Response">
        /// <![CDATA[
        ///<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///   <s:Body>
        ///      <ContactCreateResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///         < ContactCreateResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///            <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MO000090 </Id >
        ///            <a:Account>
        ///               <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MA000088 </Id >
        ///               <a:PointBalance>0</a:PointBalance>
        ///               <a:Scheme>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CR1 - BRONZE </Id >
        ///                  <a:Club>
        ///                     <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CRONUS </Id >
        ///                     <a:Name>Cronus Loyalty Club</a:Name>
        ///                  </a:Club>
        ///                  <a:Description>Cronus Bronze</a:Description>
        ///                  <a:NextScheme>
        ///                     <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CR2 - SILVER </Id >
        ///                     <a:Club>
        ///                        <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CRONUS </Id >
        ///                        <a:Name>Cronus Loyalty Club</a:Name>
        ///                     </a:Club>
        ///                     <a:Description>Cronus Silver</a:Description>
        ///                     <a:NextScheme>
        ///                     </a:NextScheme>
        ///                     <a:Perks>Discount 10% when you upgrade to Cronus Gold</a:Perks>
        ///                     <a:PointsNeeded>100000</a:PointsNeeded>
        ///                  </a:NextScheme>
        ///                  <a:Perks>Free MP3 Player - 64 GB Silver when you upgrade to Cronus Silver Scheme</a:Perks>
        ///                  <a:PointsNeeded>50000</a:PointsNeeded>
        ///               </a:Scheme>
        ///            </a:Account>
        ///            <a:Addresses xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               <b:Address>
        ///                  <b:Address1>Santa Monica</b:Address1>
        ///                  <b:Address2/>
        ///                  <b:CellPhoneNumber>555-5551</b:CellPhoneNumber>
        ///                  <b:City>Hollywood</b:City>
        ///                  <b:Country>US</b:Country>
        ///                  <b:HouseNo i:nil= "true" />
        ///                  <b:Id/>
        ///                  <b:PhoneNumber>666-6661</b:PhoneNumber>
        ///                  <b:PostCode>1001</b:PostCode>
        ///                  <b:StateProvinceRegion/>
        ///                  <b:Type>Residential</b:Type>
        ///               </b:Address>
        ///            </a:Addresses>
        ///            <a:AlternateId i:nil= "true" />
        ///            <a:BirthDay>1753-01-01T00:00:00</a:BirthDay>
        ///            <a:Cards xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               <b:Card>
        ///                  <b:Id>10102</b:Id>
        ///                  <b:BlockedBy/>
        ///                  <b:BlockedReason/>
        ///                  <b:ClubId>CRONUS</b:ClubId>
        ///                  <b:ContactId>MO000090</b:ContactId>
        ///                  <b:DateBlocked>1753-01-01T00:00:00</b:DateBlocked>
        ///                  <b:LinkedToAccount>false</b:LinkedToAccount>
        ///                  <b:LoginId>sarah1</b:LoginId>
        ///                  <b:Status>Active</b:Status>
        ///               </b:Card>
        ///            </a:Cards>
        ///            <a:Email>Sarah1 @Hollywood.com</a:Email>
        ///            <a:Environment xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               <b:Currency>
        ///                  <b:Id>GBP</b:Id>
        ///                  <b:AmountRoundingMethod>RoundNearest</b:AmountRoundingMethod>
        ///                  <b:Culture/>
        ///                  <b:DecimalPlaces>2</b:DecimalPlaces>
        ///                  <b:DecimalSeparator>.</b:DecimalSeparator>
        ///                  <b:Description>British Pound</b:Description>
        ///                  <b:Postfix/>
        ///                  <b:Prefix>£</b:Prefix>
        ///                  <b:RoundOffAmount>0.01000000000000000000</b:RoundOffAmount>
        ///                  <b:RoundOffSales>0.01000000000000000000</b:RoundOffSales>
        ///                  <b:SaleRoundingMethod>RoundNearest</b:SaleRoundingMethod>
        ///                  <b:Symbol>£</b:Symbol>
        ///                  <b:ThousandSeparator>,</b:ThousandSeparator>
        ///               </b:Currency>
        ///               <b:PasswordPolicy>5-character minimum; case sensitive</b:PasswordPolicy>
        ///               <b:Version>4.1.1</b:Version>
        ///            </a:Environment>
        ///            <a:FirstName>Sarah</a:FirstName>
        ///            <a:Gender>Female</a:Gender>
        ///            <a:Initials/>
        ///            <a:LastName>Parker</a:LastName>
        ///            <a:LoggedOnToDevice>
        ///               <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///               <a:BlockedBy i:nil="true"/>
        ///               <a:BlockedDate>1900-01-01T00:00:00</a:BlockedDate>
        ///               <a:BlockedReason i:nil="true"/>
        ///               <a:CardId i:nil="true"/>
        ///               <a:DeviceFriendlyName/>
        ///               <a:Manufacturer/>
        ///               <a:Model/>
        ///               <a:OsVersion/>
        ///               <a:Platform/>
        ///               <a:SecurityToken>5F2AEC894F4C40DABC552A9F053F3450|19-08-08 14:37:41</a:SecurityToken>
        ///               <a:Status>0</a:Status>
        ///            </a:LoggedOnToDevice>
        ///            <a:MaritalStatus>Unknown</a:MaritalStatus>
        ///            <a:MiddleName/>
        ///            <a:Name>Sarah Parker</a:Name>
        ///            <a:Notifications>
        ///               <a:Notification>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MN000001 </ Id >
        ///                  <a:ContactId/>
        ///                  <a:Created>2019-08-08T14:37:21.1</a:Created>
        ///                  <a:Description>Remember our regular offers</a:Description>
        ///                  <a:Details>Cronus Club Members receive extra discounts</a:Details>
        ///                  <a:ExpiryDate>1753-01-01T23:59:59</a:ExpiryDate>
        ///                  <a:Images xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                     < b:ImageView>
        ///                        <b:Id>OFFERS</b:Id>
        ///                        <b:AvgColor/>
        ///                        <b:DisplayOrder>0</b:DisplayOrder>
        ///                        <b:Format/>
        ///                        <b:Image/>
        ///                        <b:ImgSize>
        ///                           <b:Height>0</b:Height>
        ///                           <b:UseMinHorVerSize>false</b:UseMinHorVerSize>
        ///                           <b:Width>0</b:Width>
        ///                        </b:ImgSize>
        ///                        <b:LoadFromFile>false</b:LoadFromFile>
        ///                        <b:Location>http://localhost/lsomniservice/ucservice.svc/ImageStreamGetById?id=OFFERS&amp;width={0}&amp;height={1}</b:Location>
        ///                        <b:LocationType>Image</b:LocationType>
        ///                     </b:ImageView>
        ///                  </a:Images>
        ///                  <a:NotificationTextType>Plain</a:NotificationTextType>
        ///                  <a:QRText/>
        ///                  <a:Status>New</a:Status>
        ///               </a:Notification>
        ///               <a:Notification>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MN000002 </ Id >
        ///                  <a:ContactId/>
        ///                  <a:Created>2019-08-08T14:37:21.4</a:Created>
        ///                  <a:Description>Call our Help Desk for assistance</a:Description>
        ///                  <a:Details>Our help desk phone number is (212) 555-9999</a:Details>
        ///                  <a:ExpiryDate>1753-01-01T23:59:59</a:ExpiryDate>
        ///                  <a:Images xmlns:b="http://lsretail.com/LSOmniService/Base/2017">
        ///                     <b:ImageView>
        ///                        <b:Id>HELPDESK</b:Id>
        ///                        <b:AvgColor/>
        ///                        <b:DisplayOrder>0</b:DisplayOrder>
        ///                        <b:Format/>
        ///                        <b:Image/>
        ///                        <b:ImgSize>
        ///                           <b:Height>0</b:Height>
        ///                           <b:UseMinHorVerSize>false</b:UseMinHorVerSize>
        ///                           <b:Width>0</b:Width>
        ///                        </b:ImgSize>
        ///                        <b:LoadFromFile>false</b:LoadFromFile>
        ///                        <b:Location>http://localhost/lsomniservice/ucservice.svc/ImageStreamGetById?id=HELPDESK&amp;width={0}&amp;height={1}</b:Location>
        ///                        <b:LocationType>Image</b:LocationType>
        ///                     </b:ImageView>
        ///                  </a:Images>
        ///                  <a:NotificationTextType>Plain</a:NotificationTextType>
        ///                  <a:QRText/>
        ///                  <a:Status>New</a:Status>
        ///               </a:Notification>
        ///            </a:Notifications>
        ///            <a:OneLists/>
        ///            <a:Password>TI9nX0kjWX3f3LA1EPpwXmMC3cfBC0bKzpcmPlzAWLIJghSYLwbLxFvsakQiPf93kxw6xpY+4KUB41yBsAM/3w==</a:Password>
        ///            <a:Profiles/>
        ///            <a:PublishedOffers xmlns:b="http://lsretail.com/LSOmniService/Base/2017">
        ///               <b:PublishedOffer>
        ///                  <b:Id>PUB0047</b:Id>
        ///                  <b:Code>Coupon</b:Code>
        ///                  <b:Description>Leather Backpack -  20% off</b:Description>
        ///                  <b:Details>Receive 20% discount when you shop this fabulous bag. This bag is genuine leather, designed by our team.</b:Details>
        ///                  <b:ExpirationDate>2019-12-31T00:00:00</b:ExpirationDate>
        ///                  <b:Images>
        ///                     <b:ImageView>
        ///                        <b:Id>PUBOFF-UR-CBAG-OM</b:Id>
        ///                        <b:AvgColor/>
        ///                        <b:DisplayOrder>0</b:DisplayOrder>
        ///                        <b:Format/>
        ///                        <b:Image/>
        ///                        <b:ImgSize>
        ///                           <b:Height>0</b:Height>
        ///                           <b:UseMinHorVerSize>false</b:UseMinHorVerSize>
        ///                           <b:Width>0</b:Width>
        ///                        </b:ImgSize>
        ///                        <b:LoadFromFile>false</b:LoadFromFile>
        ///                        <b:Location>http://localhost/lsomniservice/ucservice.svc/ImageStreamGetById?id=PUBOFF-UR-CBAG-OM&amp;width={0}&amp;height={1}</b:Location>
        ///                        <b:LocationType>Image</b:LocationType>
        ///                     </b:ImageView>
        ///                  </b:Images>
        ///                  <b:OfferDetails/>
        ///                  <b:OfferId>COUP0119</b:OfferId>
        ///                  <b:OfferLines>
        ///                     <b:PublishedOfferLine>
        ///                        <b:Id>40180</b:Id>
        ///                        <b:Description>Leather Backpack</b:Description>
        ///                        <b:DiscountId>COUP0119</b:DiscountId>
        ///                        <b:DiscountType>Coupon</b:DiscountType>
        ///                        <b:Exclude>false</b:Exclude>
        ///                        <b:LineNo>10000</b:LineNo>
        ///                        <b:LineType>Item</b:LineType>
        ///                        <b:OfferId>PUB0047</b:OfferId>
        ///                        <b:UnitOfMeasure/>
        ///                        <b:Variant/>
        ///                        <b:VariantType>None</b:VariantType>
        ///                     </b:PublishedOfferLine>
        ///                  </b:OfferLines>
        ///                  <b:Selected>false</b:Selected>
        ///                  <b:Type>PointOffer</b:Type>
        ///                  <b:ValidationText/>
        ///               </b:PublishedOffer>
        ///            </a:PublishedOffers>
        ///            <a:SalesEntries/>
        ///            <a:UserName>sarah1</a:UserName>
        ///         </ContactCreateResult>
        ///      </ContactCreateResponse>
        ///   </s:Body>
        ///</s:Envelope>
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
        MemberContact ContactCreate(MemberContact contact);

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_CONTACT_UPDATE<p/>
        /// LS Central WS2 : MemberContactUpdate<p/><p/>
        /// Contact Id, User name and EMail are required values for the update command to work.<p/>
        /// Any field left out or sent in empty will wipe out that information. Always fill out all 
        /// Name field, Address and phone number even if it has not changed so it will not be wiped out from LS Nav/Central
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        MemberContact ContactGetByCardId(string cardId);

        [OperationContract]
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        /// <summary>
        /// Add new card to existing Member Contact
        /// </summary>
        /// <remarks>
        /// LS Nav  WS1 : MM_CARD_TO_CONTACT<p/>
        /// LS Central WS2 : MemberCardToContact<p/><p/>
        /// </remarks>
        /// <param name="contactId"></param>
        /// <param name="cardId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract]
        double ContactAddCard(string contactId, string cardId, string accountId);

        /// <summary>
        /// Get Point balance for Member Card
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : GET_MEMBER_CARD<p/>
        /// LS Central WS2 : GetMemberCard<p/><p/>
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        long CardGetPointBalance(string cardId);

        /// <summary>
        /// Gets Rate value for points (f.ex. 1 point = 0.01 Kr)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        decimal GetPointRate();

        [OperationContract]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);

        /// <summary>
        /// Change password
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_PWD_CHANGE<p/>
        /// LS Central WS2 : MemberPasswordChange<p/><p/>
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
        string ForgotPassword(string userNameOrEmail);

        /// <summary>
        /// Send in Reset Password request for Member contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_PWD_RESET<p/>
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
        bool ResetPassword(string userName, string resetCode, string newPassword);

        /// <summary>
        /// Reset current password or request new password for new Member Contact.  
        /// Send either login or email depending on which function is required.
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_PWD_RESET<p/>
        /// LS Central WS2 : MemberPasswordReset<p/><p/>
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
        /// LS Nav WS1 : MM_MOBILE_PWD_CHANGE<p/>
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
        /// Change Login Id for Member Contact
        /// </summary>
        /// <remarks>
        /// LS Nav  WS1 : MM_LOGIN_CHANGE<p/><p/>
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
        /// LS Nav WS1 : MM_MOBILE_LOGON<p/>
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
        MemberContact Login(string userName, string password, string deviceId);

        /// <summary>
        /// Login user from web page.  This function is light version of Login and returns less data.
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_LOGON<p/>
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
        MemberContact LoginWeb(string userName, string password);

        #endregion

        #region Item

        /// <summary>
        /// Get stock status of an item from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_MOBILE_GET_ITEMS_IN_STOCK<p/>
        /// LS Central WS2 : GetItemInventory<p/><p/>
        /// If storeId is empty, only store that are marked in LS Nav/Central with check box Loyalty or Mobile checked (Omni Section) will be returned
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
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="items">Items to get status for</param>
        /// <returns></returns>
        [OperationContract]
        List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId);

        [OperationContract]
        List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails);

        /// <summary>
        /// Lookup Item
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        LoyItem ItemGetById(string itemId, string storeId);

        [OperationContract]
        LoyItem ItemGetByBarcode(string barcode, string storeId);

        [OperationContract]
        List<LoyItem> ItemsPage(int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        [OperationContract]
        List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items);

        [OperationContract]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        /// <summary>
        /// Gets Hierarchy setup for a Store with all Leafs and Nodes
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : GET_HIERARCHY<p/>
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
        /// LS Nav WS1 : MM_MOBILE_GET_PROFILES<p/>
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
        /// Member Attribute Value has to have Value Yes or No, even in other languages as LS Commerce Service uses that Text to determent if the Attribute is selected or not for the Contact
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
        List<Store> StoresGetAll();

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude)
        /// </summary>
        /// <param name="latitude">current latitude</param>
        /// <param name="longitude">current longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers, 0 = no limit</param>
        /// <param name="maxNumberOfStores">max number of stores returned</param>
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
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance, int maxNumberOfStores);

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
        List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1);

        #endregion

        #region Replication

        /// <summary>
        /// Replicate Item Barcodes (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99001451 - Barcodes
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.  
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 4 - Currency
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 330 - Currency Exchange Rate
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10001413 - Extended Variant Values
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 99009064 - Retail Image Link
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 99009063 - Retail Image
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 5722 - Item Category
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 27 - Item
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 5404 - Item Unit of Measure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10001414 - Item Variant Registration
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant registrations</returns>
        [OperationContract]
        ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest);

        /// <summary>
        /// Replicate Best Prices for Items from WI Price table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99009258 - WI Price
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs.  
        /// This will generate the Best price for product based on date and offers available at the time.<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 7002 - Sales Price
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 5723 - Product Group
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10012866 - Store
        /// <p/><p/>
        /// Only store with Loyalty or Mobile Checked will be replicated
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 204 - Unit of Measure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of unit of measures</returns>
        [OperationContract]
        ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendors
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 23 - Vendor
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 27 - Item (Lookup by [Vendor No_])
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10000784 - Attribute
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10000786 - Attribute Value
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10000785 - Attribute Option Value
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10000971 - Data Translation
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation Language Codes
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10000972 - Data Translation
        /// <p/><p/>
        /// This will always replicate all Code
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Codes</returns>
        [OperationContract]
        ReplDataTranslationLangCodeResponse ReplEcommDataTranslationLangCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy roots
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10000920 - Hierarchy (Where Hierarchy Date is active)
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 10000921 - Hierarchy Nodes
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy nodes</returns>
        [OperationContract]
        ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10000922 - Hierarchy Node Link
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy node leafs</returns>
        [OperationContract]
        ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deal lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99001651 - Deal Modifier Item
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deals</returns>
        [OperationContract]
        ReplHierarchyHospDealResponse ReplEcommHierarchyHospDeal(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deal lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99001651 - Deal Modifier Item
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deal lines</returns>
        [OperationContract]
        ReplHierarchyHospDealLineResponse ReplEcommHierarchyHospDealLine(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Recipe lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10000768 - BOM Component
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy recipe items</returns>
        [OperationContract]
        ReplItemRecipeResponse ReplEcommItemRecipe(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Modifier lines for Node Leafs
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 99001483 - Information Subcode
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 27 - Item
        /// <p/><p/>
        /// FullItem replication includes all variants, unit of measures, attributes and prices for an item<p/>
        /// NOTE: It is recommended to replicate item data separately using<p/>
        /// ReplEcomm Item / Prices / ItemUnitOfMeasures / ItemVariantRegistrations / ExtendedVariants / Attribute / AttributeValue / AttributeOptionValue<p/>
        /// Price Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML, Sales Price, Item Variant, Item Unit of Measure, Variants and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Item objects</returns>
        [OperationContract]
        ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts and MultiBuy for Items from WI Discount table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10012862 - WI Discounts
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest);

        /// <summary>
        /// Replicate Mix and Match Offers for Items from WI Mix and Match Offer table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 10012863 - WI Mix and Match Offer
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        ReplDiscountResponse ReplEcommMixAndMatch(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts for Items from WI Discount table in LS Nav/Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// Data for this function needs to be generated in LS Nav/Central by running either OMNI_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 291 - Shipping Agent
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 99009002 - Member Contact (with valid Membership Card)
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Member contacts</returns>
        [OperationContract]
        ReplCustomerResponse ReplEcommMember(ReplRequest replRequest);

        /// <summary>
        /// Replicate all Country Codes
        /// </summary>
        /// <remarks>
        /// LS Nav/Central Main Table data: 9 - Country/Region
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
        /// LS Nav/Central Main Table data: 99001462 - Tender Type
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 325 - VAT Posting Setup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to OMNI, both during full or delta replication.
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
        /// LS Nav/Central Main Table data: 99001608 - Inventory Lookup Table
        /// <p/><p/>
        /// Net Inventory field in Inventory Lookup Table must be updated before the replication can be done.  
        /// In Retail Product Group card, set up which products to check status for by click on Update POS Inventory Lookup button. Set store to be Web Store.
        /// Run Scheduler job with CodeUnit 10012871 - WI Update Inventory which will update the Net Inventory field.
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calls to OMNI.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest);

        #endregion

        #region Search

        [OperationContract]
        SearchRs Search(string cardId, string search, SearchType searchTypes);

        #endregion search

        #region LS Recommends

        /// <summary>
        /// Checks if LS Recommend is active in LS Commerce Service
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool RecommendedActive();

        /// <summary>
        /// Not used anymore, support for older system
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="items"></param>
        /// <param name="maxNumberOfItems"></param>
        /// <returns></returns>
        [OperationContract]
        List<RecommendedItem> RecommendedItemsGetByUserId(string userId, List<LoyItem> items, int maxNumberOfItems);

        /// <summary>
        /// Gets Recommended Items for Items
        /// </summary>
        /// <param name="userId">Member contact Id to get recommended Item for. Empty string for anonymous requests</param>
        /// <param name="storeId">Store Id</param>
        /// <param name="items">List of Items to get recommend items for.  Format of string: ITEMID,ITEMID,...)</param>
        /// <returns></returns>
        [OperationContract]
        List<RecommendedItem> RecommendedItemsGet(string userId, string storeId, string items);

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
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        /// LS Central WS2 : GetAvailabilityV2<p/><p/>
        /// Optional to include required resource (if only specific resource) and contactNo for accurate pricing.
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="contactNo"></param>
        /// <param name="optionalResource"></param>
        /// <param name="promoCode"></param>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string optionalResource, string promoCode, string activityNo);

        /// <summary>
        /// Returns list with the required or optional additional charges for the Activity as applied automatically according to the product
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAdditionalCharges<p/><p/>
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        AdditionalCharge ActivityAdditionalChargesGet(string activityNo);

        /// <summary>
        /// Change or insert additional charges to Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAdditionalChargesV2<p/><p/>
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        ///             <ns:ItemNo>40020</ns:ItemNo>
        ///             <ns:LineNo>1</ns:LineNo>
        ///             <ns:Price>110.0</ns:Price>
        ///             <ns:ProductType>Item</ns:ProductType>
        ///             <ns:Quantity>1.0</ns:Quantity>
        ///             <ns:TotalAmount>110.0</ns:TotalAmount>
        ///             <ns:UnitOfMeasure></ns:UnitOfMeasure>
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
        /// Returns list of Attributes which are assigned on a given Activity product, reservation or activity entry
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAttributes<p/><p/>
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <returns></returns>
        [OperationContract]
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
        int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue);

        /// <summary>
        /// Action to create a Reservation header into the LS Reservation table
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : InsertReservation<p/><p/>
        /// </remarks>
        /// <example>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
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
        ///            <ns1:Internalstatus>0</ns1:Internalstatus>
        ///            <ns1:Location>CAMBRIDGE</ns1:Location>
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
        /// Action to force update to a reservation header in the LS Reservation table.  Blank fields will be ignored
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateReservation<p/><p/>
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
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
        bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment);

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
        List<ActivityProduct> ActivityProductsGet();

        /// <summary>
        /// Returns list of Activity Types
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityTypes<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        List<ActivityType> ActivityTypesGet();

        /// <summary>
        /// Returns list of Activity Locations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityLocations<p/><p/>
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
        /// With [reservationNo] : UploadReservationActivities<p/><p/>
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
        /// LS Central WS2 : UploadPromotions<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
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
        List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo);

        /// <summary>
        /// Returns list of Membership types (products) which are active and can be sold 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipProducts<p/><p/>
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
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
        List<Membership> ActivityMembershipsGet(string contactNo);

        #endregion

        [OperationContract]
        string MyCustomFunction(string data);
    }
}