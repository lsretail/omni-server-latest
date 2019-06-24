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
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

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
        /// <param name="cardId">Member Card Id to look for</param>
        /// <param name="itemId">Only show Offers for this item</param>
        /// <returns></returns>
        [OperationContract]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        /// <summary>
        /// Get related items in a published offer
        /// </summary>
        /// <param name="pubOfferId">Published offer id</param>
        /// <param name="numberOfItems">Number of items to return</param>
        /// <returns></returns>
        [OperationContract]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        /// <summary>
        /// Get discounts for items. Send in empty string for loyaltyschemecode if getting anonymously.
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

        #region One List

        /// <summary>
        /// Delete Basket or Wish List By OneList Id
        /// </summary>
        /// <param name="oneListId"></param>
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <returns></returns>
        [OperationContract]
        bool OneListDeleteById(string oneListId, ListType listType);

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
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
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
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        ///<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///  <soapenv:Header/>
        ///   <soapenv:Body>
        ///      <ser:OneListSave>
        ///         <ser:oneList>
        ///            <!--Not needed for anonymous basket:-->
        ///            <ns1:CardId>10021</ns1:CardId>
        ///            <ns1:Items>
        ///               <!--Zero or more repetitions:-->
        ///               <ns1:OneListItem>
        ///                  <ns1:Item>
        ///                     <ns:Id>40020</ns:Id>
        ///                  </ns1:Item>
        ///                  <ns1:Quantity>2</ns1:Quantity>
        ///                  <!--Optional:-->
        ///                  <ns1:VariantReg>
        ///                      <ns:Id>002</ns:Id>
        ///                  </ns1:VariantReg>
        ///               </ns1:OneListItem>
        ///            </ns1:Items>
        ///            <ns1:ListType>Basket</ns1:ListType>
        ///            <ns1:StoreId>S0001</ns1:StoreId>
        ///         </ser:oneList>
        ///         <ser:calculate>true</ser:calculate>
        ///      </ser:OneListSave>
        ///   </soapenv:Body>
        ///</soapenv:Envelope>
        /// ]]>
        /// </code>
        /// Response Data from OMNI after Request has been sent
        /// <code language="soap" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///    <s:Body>
        ///       <OneListSaveResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///          < OneListSaveResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///             <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017">16c57444-49c8-424a-b7bd-5be4adb5608f</Id>
        ///             <a:CardId>10021</a:CardId>
        ///             <a:ContactId>MO000008</a:ContactId>
        ///             <a:CreateDate>2018-11-20T12:46:42.79</a:CreateDate>
        ///             <a:CustomerId/>
        ///             <a:Description>List MO000008</a:Description>
        ///             <a:IsDefaultList>true</a:IsDefaultList>
        ///             <a:Items>
        ///                <a:OneListItem>
        ///                   <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > a7e973a9 - 3dad-4c93-9f10-08ba82b244b9</Id>
        ///                   <a:Amount>80.00000000</a:Amount>
        ///                   <a:BarcodeId/>
        ///                   <a:CreateDate>2018-11-20T12:46:42.803</a:CreateDate>
        ///                   <a:DiscountAmount>0.00000000</a:DiscountAmount>
        ///                   <a:DiscountPercent>0.00000000</a:DiscountPercent>
        ///                   <a:DisplayOrderId>1</a:DisplayOrderId>
        ///                   <a:Item>
        ///                      <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > 40020 </ Id >
        ///                      < a:AllowedToSell>true</a:AllowedToSell>
        ///                      <a:Description>Skirt Linda Professional Wear</a:Description>
        ///                      <a:Details>Skirt - Professional Wear from the Linda Line, Elegant skirt suitable for different occasions
        ///                      This skirt is available in many sizes and colors. This is one of our most sold skirts. Come and try it, you will find out how well it suits you.</a:Details>
        ///                      <a:Images xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                         < b:ImageView>
        ///                            <b:Id>40020</b:Id>
        ///                            <b:AvgColor/>
        ///                            <b:DisplayOrder>0</b:DisplayOrder>
        ///                            <b:Format/>
        ///                            <b:Image/>
        ///                            <b:ImgSize>
        ///                               <b:Height>0</b:Height>
        ///                               <b:Width>0</b:Width>
        ///                            </b:ImgSize>
        ///                            <b:LoadFromFile>false</b:LoadFromFile>
        ///                            <b:Location>http://hq-lsotest-d01.lsretail.local/lsomniservice/ucservice.svc/ImageStreamGetById?id=40020&amp;width={0}&amp;height={1}</b:Location>
        ///                            <b:LocationType>Image</b:LocationType>
        ///                            <b:ObjectId/>
        ///                         </b:ImageView>
        ///                      </a:Images>
        ///                      <a:IsDeleted>false</a:IsDeleted>
        ///                      <a:ItemAttributes/>
        ///                      <a:Price/>
        ///                      <a:Prices/>
        ///                      <a:ProductGroupId>WOMEN-S</a:ProductGroupId>
        ///                      <a:SalesUomId>PCS</a:SalesUomId>
        ///                      <a:UnitOfMeasures xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" />
        ///                      < a:VariantsExt xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" />
        ///                      < a:VariantsRegistration xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" />
        ///                   </ a:Item>
        ///                   <a:NetAmount>64.00000000</a:NetAmount>
        ///                   <a:NetPrice>64.00000000</a:NetPrice>
        ///                   <a:OnelistItemDiscounts/>
        ///                   <a:Price>80.00000000</a:Price>
        ///                   <a:Quantity>1.00000000</a:Quantity>
        ///                   <a:TaxAmount>16.00000000</a:TaxAmount>
        ///                   <a:UnitOfMeasure i:nil= "true" xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" />
        ///                   < a:VariantReg xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                      < b:Id>002</b:Id>
        ///                      <b:Dimension1>YELLOW</b:Dimension1>
        ///                      <b:Dimension2>38</b:Dimension2>
        ///                      <b:Dimension3/>
        ///                      <b:Dimension4/>
        ///                      <b:Dimension5/>
        ///                      <b:Dimension6/>
        ///                      <b:FrameworkCode>WOMEN</b:FrameworkCode>
        ///                      <b:Images>
        ///                         <b:ImageView>
        ///                            <b:Id>40020-Y</b:Id>
        ///                            <b:AvgColor/>
        ///                            <b:DisplayOrder>0</b:DisplayOrder>
        ///                            <b:Format/>
        ///                            <b:Image/>
        ///                            <b:ImgSize>
        ///                               <b:Height>0</b:Height>
        ///                               <b:Width>0</b:Width>
        ///                            </b:ImgSize>
        ///                            <b:LoadFromFile>false</b:LoadFromFile>
        ///                            <b:Location>http://hq-lsotest-d01.lsretail.local/lsomniservice/ucservice.svc/ImageStreamGetById?id=40020-Y&amp;width={0}&amp;height={1}</b:Location>
        ///                            <b:LocationType>Image</b:LocationType>
        ///                            <b:ObjectId/>
        ///                         </b:ImageView>
        ///                      </b:Images>
        ///                      <b:ItemId>40020</b:ItemId>
        ///                   </a:VariantReg>
        ///                </a:OneListItem>
        ///             </a:Items>
        ///             <a:ListType>Basket</a:ListType>
        ///             <a:PublishedOffers/>
        ///             <a:ShippingAmount>0.00000000</a:ShippingAmount>
        ///             <a:StoreId>S0001</a:StoreId>
        ///             <a:TotalAmount>80.00000000</a:TotalAmount>
        ///             <a:TotalDiscAmount>0.00000000</a:TotalDiscAmount>
        ///             <a:TotalNetAmount>64.00000000</a:TotalNetAmount>
        ///             <a:TotalTaxAmount>16.00000000</a:TotalTaxAmount>
        ///          </OneListSaveResult>
        ///       </OneListSaveResponse>
        ///    </s:Body>
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
        /// This function can be used to send in Basket and convert it to Order.<p/>
        /// Basic Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation
        /// </remarks>
        /// <example>
        /// Shows how Omni Object is mapped to LS Nav/Central WS Request
        /// <code language="xml" title="NAV WS Mapping">
        /// <![CDATA[
        /// <Request_ID>ECOMM_CALCULATE_BASKET</Request_ID>
        ///  <Request_Body>
        ///    <MobileTransaction>
        ///      <Id>oneList.Id</Id>
        ///      <StoreId>oneList.StoreId</StoreId>
        ///      <MemberContactNo>oneList.ContactId</MemberContactNo>
        ///      <MemberCardNo>oneList.CardId</MemberCardNo>
        ///    </MobileTransaction>
        ///    <MobileTransactionLine>
        ///      <Id>oneList.Id</Id>
        ///      <LineNo>lineCount</LineNo>
        ///      <LineType>0</LineType>
        ///      <Barcode></Barcode>
        ///      <Number>oneList.Items.Item.Id</Number>
        ///      <VariantCode>oneList.Items.VariantReg.Id</VariantCode>
        ///      <UomId>oneList.Items.UnitOfMeasure.Id</UomId>
        ///      <Quantity>oneList.Items.Quantity</Quantity>
        ///      <ExternalId>0</ExternalId>
        ///    </MobileTransactionLine>
        ///  </Request_Body>
        /// ]]>
        /// </code>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        /// <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///     <ser:OneListCalculate>
        ///        <ser:oneList>
        ///           <!--Not needed for anonymous basket:-->
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <!--Not needed for anonymous basket:-->
        ///           <ns1:ContactId>MO000008</ns1:ContactId>
        ///           <ns1:Items>
        ///              <!--Zero or more repetitions:-->
        ///              <ns1:OneListItem>
        ///                 <ns1:Item>
        ///                    <ns:Id>40020</ns:Id>
        ///                 </ns1:Item>
        ///                 <ns1:Quantity>2</ns1:Quantity>
        ///                 <!--Optional:-->
        ///                 <ns1:VariantReg>
        ///                     <ns:Id>002</ns:Id>
        ///                 </ns1:VariantReg>
        ///              </ns1:OneListItem>
        ///           </ns1:Items>
        ///           <ns1:ListType>Basket</ns1:ListType>
        ///           <ns1:StoreId>S0013</ns1:StoreId>
        ///        </ser:oneList>
        ///     </ser:OneListCalculate>
        ///    </soapenv:Body>
        /// </soapenv:Envelope>
        /// ]]>
        /// </code>
        /// Response Data from OMNI after Request has been sent
        /// <code language="xml" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///    <s:Body>
        ///       <OneListCalculateResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///          < OneListCalculateResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///             <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" >{6281701B-7C81-4FB6-9A17-C445DC522390}</Id>
        ///             <a:AnonymousOrder>false</a:AnonymousOrder>
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
        ///             <a:DocumentRegTime>0001-01-01T00:00:00</a:DocumentRegTime>
        ///             <a:Email i:nil="true"/>
        ///             <a:LineItemCount>0</a:LineItemCount>
        ///             <a:MobileNumber i:nil="true"/>
        ///             <a:OrderDiscountLines/>
        ///             <a:OrderLines>
        ///                <a:OrderLine>
        ///                   <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///                   < a:Amount>160.00</a:Amount>
        ///                   <a:DiscountAmount>0.00</a:DiscountAmount>
        ///                   <a:DiscountPercent>0.00</a:DiscountPercent>
        ///                   <a:ItemDescription>Skirt Linda Professional Wear</a:ItemDescription>
        ///                   <a:ItemId>40020</a:ItemId>
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
        ///             <a:PhoneNumber i:nil= "true" />
        ///             < a:PointAmount>1600.00</a:PointAmount>
        ///             <a:PointBalance>53928.00</a:PointBalance>
        ///             <a:PointCashAmountNeeded>0.00</a:PointCashAmountNeeded>
        ///             <a:PointsRewarded>160.00</a:PointsRewarded>
        ///             <a:PointsUsedInOrder>0.00</a:PointsUsedInOrder>
        ///             <a:Posted>false</a:Posted>
        ///             <a:ReceiptNo/>
        ///             <a:ShipClickAndCollect>false</a:ShipClickAndCollect>
        ///             <a:ShipToAddress xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                < b:Address1/>
        ///                <b:Address2/>
        ///                <b:CellPhoneNumber i:nil= "true" />
        ///                < b:City/>
        ///                <b:Country/>
        ///                <b:HouseNo i:nil= "true" />
        ///                < b:Id/>
        ///                <b:PhoneNumber i:nil= "true" />
        ///                < b:PostCode/>
        ///                <b:StateProvinceRegion/>
        ///                <b:Type>Residential</b:Type>
        ///             </a:ShipToAddress>
        ///             <a:ShipToEmail i:nil= "true" />
        ///             < a:ShipToName i:nil= "true" />
        ///             < a:ShipToPhoneNumber i:nil= "true" />
        ///             < a:ShippingAgentCode i:nil= "true" />
        ///             < a:ShippingAgentServiceCode i:nil= "true" />
        ///             < a:ShippingStatus>ShippigNotRequired</a:ShippingStatus>
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

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity available of items in order for certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <example>
        /// Shows how Omni Object is mapped to LS Nav/Central WS Request
        /// <code language="xml" title="NAV WS mapping">
        /// <![CDATA[
        /// <Request_ID>CO_QTY_AVAILABILITY_EXT</Request_ID>
        /// <Request_Body>
        ///   <Item_No_Type>request.ItemNumberType</Item_No_Type>
        ///   <Customer_Order_Header>
        ///     <Document_Id>request.Id</Document_Id>
        ///     <Store_No.>request.StoreId</Store_No.>
        ///     <Member_Card_No.>request.CardId</Member_Card_No.>
        ///     <Anonymous_Order>request.AnonymousOrder</Anonymous_Order>
        ///     <Source_Type>request.SourceType</Source_Type>
        ///   </Customer_Order_Header>
        ///   <Customer_Order_Line>
        ///     <Document_Id>request.Id</Document_Id>
        ///     <Line_No.>request.OrderLine.LineNumber</Line_No.>
        ///     <Line_Type>request.OrderLine.LineType</Line_Type>
        ///     <Number>request.OrderLine.ItemId</Number>
        ///     <Variant_Code>request.OrderLine.VariantId</Variant_Code>
        ///     <Unit_of_Measure_Code>request.OrderLine.UomId</Unit_of_Measure_Code>
        ///     <Quantity>request.OrderLine.Quantity</Quantity>
        ///   </Customer_Order_Line>
        /// </Request_Body>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request);

        /// <summary>
        /// Create Customer Order for ClickAndCollect or BasketPostSales 
        /// </summary>
        /// <example>
        /// Shows how Omni Object is mapped to LS Nav/Central WS Request
        /// <code language="xml" title="NAV WS mapping">
        /// <![CDATA[
        ///  <Request_ID>CUSTOMER_ORDER_CREATE_EXT</Request_ID> 
        ///  <Request_Body>
        ///   <Item_No_Type>request.ItemNumberType</Item_No_Type>
        ///   <Customer_Order_Header>
        ///     <Document_Id>request.Id</Document_Id>
        ///     <Store_No.>request.StoreId</Store_No.>
        ///     <Member_Card_No.>request.CardId</Member_Card_No.>
        ///     <Source_Type>request.SourceType</Source_Type>
        ///     <Full_Name>request.ContactName</Full_Name>
        ///     <Address>request.ContactAddress.Address1</Address>
        ///     <Address_2>request.ContactAddress.Address2</Address_2>
        ///     <City>request.ContactAddress.City</City>
        ///     <County>request.ContactAddress.StateProvinceRegion</County>
        ///     <Post_Code>request.ContactAddress.PostCode</Post_Code>
        ///     <Country_Region_Code>request.ContactAddress.Country</Country_Region_Code>
        ///     <Phone_No.>request.PhoneNumber</Phone_No.>
        ///     <Email>request.Email</Email>
        ///     <House_Apartment_No.>request.ContactAddress.HouseNo</House_Apartment_No.>
        ///     <Mobile_Phone_No.>request.MobileNumber</Mobile_Phone_No.>
        ///     <Daytime_Phone_No.>request.DayPhoneNumber</Daytime_Phone_No.>
        ///     <Ship_To_Full_Name>request.ShipToName</Ship_To_Full_Name>
        ///     <Ship_To_Address>request.ShipToAddress.Address1</Ship_To_Address>
        ///     <Ship_To_Address_2>request.ShipToAddress.Address2</Ship_To_Address_2>
        ///     <Ship_To_City>request.ShipToAddress.City</Ship_To_City>
        ///     <Ship_To_County>request.ShipToAddress.StateProvinceRegion</Ship_To_County>
        ///     <Ship_To_Post_Code>request.ShipToAddress.PostCode</Ship_To_Post_Code>
        ///     <Ship_To_Country_Region_Code>request.ShipToAddress.Country</Ship_To_Country_Region_Code>
        ///     <Ship_To_Phone_No.>request.ShipToPhoneNumber</Ship_To_Phone_No.>
        ///     <Ship_To_Email>request.ShipToEmail</Ship_To_Email>
        ///     <Ship_To_House_Apartment_No.>request.ShipToAddress.HouseNo</Ship_To_House_Apartment_No.>
        ///     <Click_And_Collect_Order>request.ClickAndCollectOrder</Click_And_Collect_Order>
        ///     <Anonymous_Order>request.AnonymousOrder</Anonymous_Order>
        ///     <Shipping_Agent_Code>request.ShippingAgentCode</Shipping_Agent_Code>
        ///     <Shipping_Agent_Service_Code>request.ShippingAgentServiceCode</Shipping_Agent_Service_Code>
        ///   </Customer_Order_Header>
        ///   <Customer_Order_Line>
        ///     <Document_Id>request.Id</Document_Id>
        ///     <Line_No.>request.OrderLineCreateRequests.LineNumber</Line_No.>
        ///     <Line_Type>request.OrderLineCreateRequests.LineType</Line_Type>
        ///     <Number>request.OrderLineCreateRequests.ItemId</Number>
        ///     <Variant_Code>request.OrderLineCreateRequests.VariantId</Variant_Code>
        ///     <Unit_of_Measure_Code>request.OrderLineCreateRequests.UomId</Unit_of_Measure_Code>
        ///     <Net_Price>request.OrderLineCreateRequests.NetPrice</Net_Price>
        ///     <Price>request.OrderLineCreateRequests.Price</Price>
        ///     <Quantity>request.OrderLineCreateRequests.Quantity</Quantity>
        ///     <Discount_Amount>request.OrderLineCreateRequests.DiscountAmount</Discount_Amount>
        ///     <Discount_Percent>request.OrderLineCreateRequests.DiscountPercent</Discount_Percent>
        ///     <Net_Amount>request.OrderLineCreateRequests.NetAmount</Net_Amount>
        ///     <Vat_Amount>request.OrderLineCreateRequests.TaxAmount</Vat_Amount>
        ///     <Amount>request.OrderLineCreateRequests.Amount</Amount>
        ///   </Customer_Order_Line>
        ///   <Customer_Order_Discount_Line>
        ///     <Document_Id>request.Id</Document_Id>
        ///     <Line_No.>request.OrderDiscountLineCreateRequests.LineNumber</Line_No.>
        ///     <Entry_No.>request.OrderDiscountLineCreateRequests.No</Entry_No.>
        ///     <Discount_Type>request.OrderDiscountLineCreateRequests.DiscountType</Discount_Type>
        ///     <Offer_No.>request.OrderDiscountLineCreateRequests.OfferNumber</Offer_No.>
        ///     <Periodic_Disc._Type>request.OrderDiscountLineCreateRequests.PeriodicDiscType</Periodic_Disc._Type>
        ///     <Periodic_Disc._Group>request.OrderDiscountLineCreateRequests.PeriodicDiscGroup</Periodic_Disc._Group>
        ///     <Description>request.OrderDiscountLineCreateRequests.Description</Description>
        ///     <Discount_Percent>request.OrderDiscountLineCreateRequests.DiscountPercent</Discount_Percent>
        ///     <Discount_Amount>request.OrderDiscountLineCreateRequests.DiscountAmount</Discount_Amount>
        ///   </Customer_Order_Discount_Line>
        ///   <Customer_Order_Payment>
        ///     <Document_Id>request.Id</Document_Id>
        ///     <Line_No.>request.OrderPaymentCreateRequests.LineNumber</Line_No.>
        ///     <Pre_Approved_Amount>request.OrderPaymentCreateRequests.PreApprovedAmount</Pre_Approved_Amount>
        ///     <Tender_Type>request.OrderPaymentCreateRequests.TenderType</Tender_Type>
        ///     <Card_Type>request.OrderPaymentCreateRequests.CardType</Card_Type> 
        ///     <Currency_Code>request.OrderPaymentCreateRequests.CurrencyCode</Currency_Code>
        ///     <Currency_Factor>request.OrderPaymentCreateRequests.CurrencyFactor</Currency_Factor>
        ///     <Authorisation_Code>request.OrderPaymentCreateRequests.AuthorisationCode</Authorisation_Code>
        ///     <Pre_Approved_Valid_Date>request.OrderPaymentCreateRequests.PreApprovedValidDate</Pre_Approved_Valid_Date>
        ///     <Card_or_Customer_number>request.OrderPaymentCreateRequests.CardNumber</Card_or_Customer_number>
        ///   </Customer_Order_Payment>
        /// </Request_Body>
        /// ]]>
        /// </code>
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
        ///           <ns1:AnonymousOrder>false</ns1:AnonymousOrder>
        ///            <!--Not needed when AnonymouseOrder == false:-->
        ///           <ns1:CardId>10021</ns1:CardId>
        ///           <ns1:ClickAndCollectOrder>false</ns1:ClickAndCollectOrder>
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
        ///                 <ns1:AuthorisationCode>123456</ns1:AuthorisationCode>
        ///                 <ns1:CardNumber>45XX..5555</ns1:CardNumber>
        ///                 <ns1:CardType>VISA</ns1:CardType>
        ///                 <ns1:CurrencyCode></ns1:CurrencyCode>
        ///                 <ns1:CurrencyFactor>1</ns1:CurrencyFactor>
        ///                 <ns1:FinalizedAmount>0</ns1:FinalizedAmount>
        ///                 <ns1:LineNumber>1</ns1:LineNumber>
        ///                 <ns1:No></ns1:No>
        ///                 <ns1:OrderId></ns1:OrderId>
        ///                 <ns1:PreApprovedAmount>160.00</ns1:PreApprovedAmount>
        ///                 <ns1:PreApprovedValidDate>2030-01-01</ns1:PreApprovedValidDate>
        ///                 <ns1:TenderType>1</ns1:TenderType>
        ///              </ns1:OrderPayment>
        ///           </ns1:OrderPayments>
        ///           <ns1:OrderStatus>Pending</ns1:OrderStatus>
        ///           <ns1:PaymentStatus>PreApproved</ns1:PaymentStatus>
        ///           <ns1:Posted>false</ns1:Posted>
        ///           <ns1:ShipClickAndCollect>false</ns1:ShipClickAndCollect>
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
        /// Response Data from OMNI after Request has been sent
        /// <code language="xml" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///   <s:Body>
        ///      <OrderCreateResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///         < OrderCreateResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///            <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > 1874c7fa-48f9-4417-a2b2-7b51fba07b08</Id>
        ///            <a:AnonymousOrder>false</a:AnonymousOrder>
        ///            <a:CardId>10021</a:CardId>
        ///            <a:ClickAndCollectOrder>false</a:ClickAndCollectOrder>
        ///            <a:CollectLocation i:nil="true"/>
        ///            <a:ContactAddress xmlns:b="http://lsretail.com/LSOmniService/Base/2017">
        ///               <b:Address1/>
        ///               <b:Address2/>
        ///               <b:CellPhoneNumber i:nil="true"/>
        ///               <b:City/>
        ///               <b:Country/>
        ///               <b:HouseNo/>
        ///               <b:Id/>
        ///               <b:PhoneNumber i:nil="true"/>
        ///               <b:PostCode/>
        ///               <b:StateProvinceRegion/>
        ///               <b:Type>Residential</b:Type>
        ///            </a:ContactAddress>
        ///            <a:ContactId/>
        ///            <a:ContactName/>
        ///            <a:DayPhoneNumber/>
        ///            <a:DocumentId>CO000011</a:DocumentId>
        ///            <a:DocumentRegTime>2018-11-20T12:41:53.763</a:DocumentRegTime>
        ///            <a:Email>tom @tom.xxx</a:Email>
        ///            <a:LineItemCount>2</a:LineItemCount>
        ///            <a:MobileNumber/>
        ///            <a:OrderDiscountLines/>
        ///            <a:OrderLines>
        ///               <a:OrderLine>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///                  < a:Amount>160.00000000000000000000</a:Amount>
        ///                  <a:DiscountAmount>0.00000000000000000000</a:DiscountAmount>
        ///                  <a:DiscountPercent>0.00000000000000000000</a:DiscountPercent>
        ///                  <a:ItemDescription>Skirt Linda Professional Wear</a:ItemDescription>
        ///                  <a:ItemId>40020</a:ItemId>
        ///                  <a:LineNumber>10000</a:LineNumber>
        ///                  <a:LineType>Item</a:LineType>
        ///                  <a:NetAmount>128.00000000000000000000</a:NetAmount>
        ///                  <a:NetPrice>64.00000000000000000000</a:NetPrice>
        ///                  <a:OrderId/>
        ///                  <a:Price>80.00000000000000000000</a:Price>
        ///                  <a:Quantity>2.00000000000000000000</a:Quantity>
        ///                  <a:QuantityOutstanding>0</a:QuantityOutstanding>
        ///                  <a:QuantityToInvoice>0</a:QuantityToInvoice>
        ///                  <a:QuantityToShip>0</a:QuantityToShip>
        ///                  <a:TaxAmount>32.00000000000000000000</a:TaxAmount>
        ///                  <a:UomId/>
        ///                  <a:VariantDescription>YELLOW/38</a:VariantDescription>
        ///                  <a:VariantId>002</a:VariantId>
        ///               </a:OrderLine>
        ///            </a:OrderLines>
        ///            <a:OrderPayments>
        ///               <a:OrderPayment>
        ///                  <a:AuthorisationCode>123456</a:AuthorisationCode>
        ///                  <a:CardNumber>10xx xxxx xxxx 1475</a:CardNumber>
        ///                  <a:CardType>VISA</a:CardType>
        ///                  <a:CurrencyCode>GBP</a:CurrencyCode>
        ///                  <a:CurrencyFactor>1.00000000000000000000</a:CurrencyFactor>
        ///                  <a:FinalizedAmount>0.00000000000000000000</a:FinalizedAmount>
        ///                  <a:LineNumber>10000</a:LineNumber>
        ///                  <a:No>1</a:No>
        ///                  <a:OrderId>CO000011</a:OrderId>
        ///                  <a:PreApprovedAmount>160.00000000000000000000</a:PreApprovedAmount>
        ///                  <a:PreApprovedValidDate>2030-01-01T00:00:00</a:PreApprovedValidDate>
        ///                  <a:TenderType>1</a:TenderType>
        ///               </a:OrderPayment>
        ///            </a:OrderPayments>
        ///            <a:OrderStatus>Created</a:OrderStatus>
        ///            <a:PaymentStatus>PreApproved</a:PaymentStatus>
        ///            <a:PhoneNumber>98545468</a:PhoneNumber>
        ///            <a:PointAmount>0</a:PointAmount>
        ///            <a:PointBalance>0</a:PointBalance>
        ///            <a:PointCashAmountNeeded>0</a:PointCashAmountNeeded>
        ///            <a:PointsRewarded>0</a:PointsRewarded>
        ///            <a:PointsUsedInOrder>0</a:PointsUsedInOrder>
        ///            <a:Posted>false</a:Posted>
        ///            <a:ReceiptNo i:nil= "true" />
        ///            < a:ShipClickAndCollect>false</a:ShipClickAndCollect>
        ///            <a:ShipToAddress xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               < b:Address1>Some Address</b:Address1>
        ///               <b:Address2/>
        ///               <b:CellPhoneNumber i:nil= "true" />
        ///               < b:City>Some City</b:City>
        ///               <b:Country/>
        ///               <b:HouseNo/>
        ///               <b:Id/>
        ///               <b:PhoneNumber i:nil= "true" />
        ///               < b:PostCode>999</b:PostCode>
        ///               <b:StateProvinceRegion/>
        ///               <b:Type>Residential</b:Type>
        ///            </a:ShipToAddress>
        ///            <a:ShipToEmail/>
        ///            <a:ShipToName/>
        ///            <a:ShipToPhoneNumber/>
        ///            <a:ShippingAgentCode/>
        ///            <a:ShippingAgentServiceCode/>
        ///            <a:ShippingStatus>NotYetShipped</a:ShippingStatus>
        ///            <a:SourceType>eCommerce</a:SourceType>
        ///            <a:StoreId>S0013</a:StoreId>
        ///            <a:TotalAmount>160.00000000000000000000</a:TotalAmount>
        ///            <a:TotalDiscount>0.00000000000000000000</a:TotalDiscount>
        ///            <a:TotalNetAmount>128.00000000000000000000</a:TotalNetAmount>
        ///         </OrderCreateResult>
        ///      </OrderCreateResponse>
        ///   </s:Body>
        /// </s:Envelope>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>SalesEntry object for order if order creation was successful</returns>
        [OperationContract]
        SalesEntry OrderCreate(Order request);

        /// <summary>
        /// Check Status of an Order made with BasketPostSale (LSNAV 10.02 and older)
        /// </summary>
        /// <example>
        /// Shows how Omni Object is mapped to LS Nav/Central WS Request
        /// <code language="xml" title="NAV WS mapping">
        /// <![CDATA[
        /// <Request_ID>WI_NC_ORDER_STATUS</Request_ID>
        /// <Request_Body>
        ///   <WebTransactionGUID>transactionId</WebTransactionGUID>
        /// </Request_Body>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="transactionId">Order GUID Id</param>
        /// <returns></returns>
        [OperationContract]
        OrderStatusResponse OrderStatusCheck(string transactionId);

        /// <summary>
        /// Cancel Order made with BasketPostSale
        /// </summary>
        /// <example>
        /// Shows how Omni Object is mapped to LS Nav/Central WS Request
        /// <code language="xml" title="NAV WS mapping">
        /// <![CDATA[
        /// <Request_ID>WEB_POS_CANCEL_CUSTOMER_ORDER</Request_ID>
        /// <Request_Body>
        ///   <WebTransactionGUID>transactionId</WebTransactionGUID>
        /// </Request_Body>
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [OperationContract]
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
        List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfEntries);

        /// <summary>
        /// Get the Sale details (order/transaction)
        /// </summary>
        /// <param name="entryId">Sales Entry ID</param>
        /// <param name="type">Document Id type of the Sale entry</param>
        /// <returns>SalesEntry with Lines</returns>
        [OperationContract]
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type);

        #endregion

        #region OrderQueue

        [OperationContract]
        OrderQueue OrderQueueSave(OrderQueue order);

        [OperationContract]
        OrderQueue OrderQueueGetById(string orderId);

        [OperationContract]
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
        /// Shows how Omni Object is mapped to LS Nav/Central WS Request
        /// <code language="xml" title="NAV WS mapping">
        /// <![CDATA[
        /// <Request_ID>MM_MOBILE_CONTACT_CREATE</Request_ID>
        /// <Request_Body>
        ///	  <LoginID>contact.UserName</LoginID>
        ///	  <Password>contact.Password</Password>
        ///	  <Email>contact.Email</Email>
        ///	  <FirstName>contact.FirstName</FirstName>
        ///	  <LastName>contact.LastName</LastName>
        ///	  <MiddleName>contact.MiddleName</MiddleName>
        ///	  <Gender>contact.Gender</Gender>
        ///	  <Phone>contact.Phone</Phone>
        ///	  <Address1>contact.Addresses.Address1</Address1>
        ///	  <Address2>contact.Addresses.Address2</Address2>
        ///	  <City>contact.Addresses.City</City>
        ///	  <PostCode>contact.Addresses.PostCode</PostCode>
        ///	  <StateProvinceRegion>contact.Addresses.StateProvinceRegion</StateProvinceRegion>
        ///	  <Country>contact.Addresses.Country</Country>
        ///	  <ClubID></ClubID>
        ///	  <SchemeID></SchemeID>
        ///	  <AccountID></AccountID>
        ///	  <ContactID></ContactID>
        ///	  <DeviceID>contact.Device.Id</DeviceID>
        ///	  <DeviceFriendlyName>contact.Device.DeviceFriendlyName</DeviceFriendlyName>
        ///	  <ExternalID></ExternalID>
        ///	  <ExternalSystem></ExternalSystem>
        /// </Request_Body>
        /// ]]>
        /// </code>
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        ///<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017" xmlns:lsr="http://schemas.datacontract.org/2004/07/LSRetail.Omni.Domain.DataModel.Loyalty.Baskets" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
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
        ///                  <ns:City>Hollywood</ns:City>
        ///                  <ns:Country>US</ns:Country>
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
        ///            <ns1:MobilePhone>555-5551</ns1:MobilePhone>
        ///            <ns1:Password>SxxInTheCity</ns1:Password>
        ///            <ns1:Phone>666-6661</ns1:Phone>
        ///            <ns1:UserName>sarah</ns1:UserName>
        ///         </ser:contact>
        ///      </ser:ContactCreate>
        ///   </soapenv:Body>            
        ///</soapenv:Envelope>
        /// ]]>
        /// </code>
        /// Response Data from OMNI after Request has been sent
        /// <code language="xml" title="SOAP Response">
        /// <![CDATA[
        /// <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
        ///   <s:Header>
        ///      <ActivityId CorrelationId = "f6fac723-17d5-4592-8cf9-70f614208dfd" xmlns="http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics">00000000-0000-0000-0000-000000000000</ActivityId>
        ///   </s:Header>
        ///   <s:Body>
        ///      <ContactCreateResponse xmlns = "http://lsretail.com/LSOmniService/EComm/2017/Service" >
        ///         < ContactCreateResult xmlns:a="http://lsretail.com/LSOmniService/Loy/2017" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
        ///            <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MO000072 </ Id >
        ///            < a:Account>
        ///               <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MA000070 </ Id >
        ///               < a:PointBalance>0</a:PointBalance>
        ///               <a:Scheme>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CR1 - BRONZE </ Id >
        ///                  < a:Club>
        ///                     <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > CRONUS </ Id >
        ///                     < a:Name>Cronus Loyalty Club</a:Name>
        ///                  </a:Club>
        ///                  <a:Description>Cronus Bronze</a:Description>
        ///                  <a:NextScheme>
        ///                     <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///                     < a:Club>
        ///                        <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///                        < a:Name/>
        ///                     </a:Club>
        ///                     <a:Description/>
        ///                     <a:NextScheme i:nil= "true" />
        ///                     < a:Perks/>
        ///                     <a:PointsNeeded>0</a:PointsNeeded>
        ///                  </a:NextScheme>
        ///                  <a:Perks>Free MP3 Player - 64 Gb Silver when you upgrade to Cronus Silver Scheme</a:Perks>
        ///                  <a:PointsNeeded>0</a:PointsNeeded>
        ///               </a:Scheme>
        ///            </a:Account>
        ///            <a:Addresses xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               < b:Address>
        ///                  <b:Address1>Santa Monica</b:Address1>
        ///                  <b:Address2/>
        ///                  <b:CellPhoneNumber i:nil= "true" />
        ///                  <b:City>Hollywood</b:City>
        ///                  <b:Country/>
        ///                  <b:HouseNo i:nil= "true" />
        ///                  < b:Id/>
        ///                  <b:PhoneNumber i:nil= "true" />
        ///                  < b:PostCode/>
        ///                  <b:StateProvinceRegion/>
        ///                  <b:Type>Residential</b:Type>
        ///               </b:Address>
        ///            </a:Addresses>
        ///            <a:AlternateId i:nil= "true" />
        ///            < a:Basket xmlns:b= "http://schemas.datacontract.org/2004/07/LSRetail.Omni.Domain.DataModel.Loyalty.Baskets" >
        ///               < Id xmlns= "http://lsretail.com/LSOmniService/Base/2017" > F8F76574 - 1111 - 4FBA-9424-023957776A94</Id>
        ///               <b:Amount>0</b:Amount>
        ///               <b:Items/>
        ///               <b:NetAmount>0</b:NetAmount>
        ///               <b:PublishedOffers xmlns:c= "http://lsretail.com/LSOmniService/Base/2017" />
        ///               < b:ShippingPrice>0</b:ShippingPrice>
        ///               <b:State>Dirty</b:State>
        ///               <b:TAXAmount>0</b:TAXAmount>
        ///            </a:Basket>
        ///            <a:BirthDay>1753-01-01T00:00:00</a:BirthDay>
        ///            <a:Card xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               < b:Id>10084</b:Id>
        ///               <b:BlockedBy/>
        ///               <b:BlockedReason/>
        ///               <b:ClubId>CRONUS</b:ClubId>
        ///               <b:ContactId>MO000072</b:ContactId>
        ///               <b:DateBlocked>1753-01-01T00:00:00</b:DateBlocked>
        ///               <b:LinkedToAccount>false</b:LinkedToAccount>
        ///               <b:Status>Active</b:Status>
        ///            </a:Card>
        ///            <a:Email>email @email.xxx</a:Email>
        ///            <a:Environment xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///               < b:Currency>
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
        ///               <b:Version>2.4.0.0</b:Version>
        ///            </a:Environment>
        ///            <a:FirstName>Sarah</a:FirstName>
        ///            <a:Gender>Female</a:Gender>
        ///            <a:Initials/>
        ///            <a:LastName>Parker</a:LastName>
        ///            <a:LoggedOnToDevice>
        ///               <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///               < a:BlockedBy i:nil="true"/>
        ///               <a:BlockedDate>0001-01-01T00:00:00</a:BlockedDate>
        ///               <a:BlockedReason i:nil="true"/>
        ///               <a:CardId i:nil="true"/>
        ///               <a:DeviceFriendlyName/>
        ///               <a:Manufacturer/>
        ///               <a:Model/>
        ///               <a:OsVersion/>
        ///               <a:Platform/>
        ///               <a:SecurityToken>A041B7165A7B4C1ABD357A854D377B88|17-09-29 13:41:32</a:SecurityToken>
        ///               <a:Status>0</a:Status>
        ///            </a:LoggedOnToDevice>
        ///            <a:MaritalStatus>Unknown</a:MaritalStatus>
        ///            <a:MiddleName/>
        ///            <a:MobilePhone/>
        ///            <a:Name>Sarah Parker</a:Name>
        ///            <a:Notifications>
        ///               <a:Notification>
        ///                  <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" > MN000001 </ Id >
        ///                  < a:ContactId>MO000072</a:ContactId>
        ///                  <a:Created>2017-09-29T13:41:32.86</a:Created>
        ///                  <a:Description>Remember our regular offers</a:Description>
        ///                  <a:Details>Cronus Club Members receive exra discounts</a:Details>
        ///                  <a:ExpiryDate>2018-03-29T13:41:32.82</a:ExpiryDate>
        ///                  <a:Images xmlns:b= "http://lsretail.com/LSOmniService/Base/2017" >
        ///                     < b:ImageView>
        ///                        <b:Id>OFFERS</b:Id>
        ///                        <b:AvgColor>F6F2E4</b:AvgColor>
        ///                        <b:DisplayOrder>0</b:DisplayOrder>
        ///                        <b:Format/>
        ///                        <b:Image/>
        ///                        <b:ImgSize>
        ///                           <b:Height>1000</b:Height>
        ///                           <b:Width>1500</b:Width>
        ///                        </b:ImgSize>
        ///                        <b:LoadFromFile>false</b:LoadFromFile>
        ///                        <b:Location>http://desktop-f9p79r1/lsomniservice/ucservice.svc/ImageStreamGetById?id=OFFERS&amp;width={0}&amp;height={1}</b:Location>
        ///                        <b:LocationType>Image</b:LocationType>
        ///                        <b:ObjectId/>
        ///                     </b:ImageView>
        ///                  </a:Images>
        ///                  <a:NotificationTextType>Plain</a:NotificationTextType>
        ///                  <a:QRText/>
        ///                  <a:Status>New</a:Status>
        ///               </a:Notification>
        ///            </a:Notifications>
        ///            <a:Password/>
        ///            <a:Phone/>
        ///            <a:Profiles/>
        ///            <a:PublishedOffers xmlns:b="http://lsretail.com/LSOmniService/Base/2017">
        ///               <b:PublishedOffer>
        ///                  <b:Id>PUB0038</b:Id>
        ///                  <b:Code>Deal</b:Code>
        ///                  <b:Description>Chicken, Salad and Fries</b:Description>
        ///                  <b:Details>For only 9,50. Grilled Chicken marinated with our own recipe rich with a tasty flavor.Served with fresh salad, steak fries and our classic barbeque sauce</b:Details>
        ///                  <b:ExpirationDate>1970-01-01T00:00:00</b:ExpirationDate>
        ///                  <b:Images>
        ///                     <b:ImageView>
        ///                        <b:Id>PUBOFF-CHICKENS</b:Id>
        ///                        <b:AvgColor/>
        ///                        <b:DisplayOrder>0</b:DisplayOrder>
        ///                        <b:Format/>
        ///                        <b:Image/>
        ///                        <b:ImgSize>
        ///                           <b:Height>0</b:Height>
        ///                           <b:Width>0</b:Width>
        ///                        </b:ImgSize>
        ///                        <b:LoadFromFile>false</b:LoadFromFile>
        ///                        <b:Location>http://desktop-f9p79r1/lsomniservice/ucservice.svc/ImageStreamGetById?id=PUBOFF-CHICKENS&amp;width={0}&amp;height={1}</b:Location>
        ///                        <b:LocationType>Image</b:LocationType>
        ///                        <b:ObjectId/>
        ///                     </b:ImageView>
        ///                  </b:Images>
        ///                  <b:OfferDetails/>
        ///                  <b:OfferId>S10017</b:OfferId>
        ///                  <b:Selected>false</b:Selected>
        ///                  <b:Type>General</b:Type>
        ///                  <b:ValidationText/>
        ///               </b:PublishedOffer>
        ///            </a:PublishedOffers>
        ///            <a:RV>0</a:RV>
        ///            <a:Transactions/>
        ///            <a:UserName>sarah</a:UserName>
        ///            <a:WishList>
        ///               <Id xmlns = "http://lsretail.com/LSOmniService/Base/2017" />
        ///               < a:CardId>10084</a:CardId>
        ///               <a:ContactId>MO000072</a:ContactId>
        ///               <a:CreateDate>2017-09-29T13:41:32.9856902+00:00</a:CreateDate>
        ///               <a:CustomerId/>
        ///               <a:Description/>
        ///               <a:IsDefaultList>false</a:IsDefaultList>
        ///               <a:Items/>
        ///               <a:ListType>Wish</a:ListType>
        ///               <a:PublishedOffers/>
        ///               <a:TotalAmount>0</a:TotalAmount>
        ///               <a:TotalDiscAmount>0</a:TotalDiscAmount>
        ///               <a:TotalNetAmount>0</a:TotalNetAmount>
        ///               <a:TotalTaxAmount>0</a:TotalTaxAmount>
        ///            </a:WishList>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        long CardGetPointBalance(string cardId);

        /// <summary>
        /// Gets Rate value for points (f.ex. 1 point = 0.01 kr)
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        decimal GetPointRate();

        [OperationContract]
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
        /// This Sample request can be used in SOAP UI application to send request to OMNI.<p/>
        /// Include minimum data needed to be able to process the request
        /// <code language="xml" title="SOAP Sample Request">
        /// <![CDATA[
        ///<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://lsretail.com/LSOmniService/EComm/2017/Service" xmlns:ns="http://lsretail.com/LSOmniService/Base/2017" xmlns:ns1="http://lsretail.com/LSOmniService/Loy/2017" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
        ///   <soapenv:Header/>
        ///   <soapenv:Body>
        ///      <ser:ContactUpdate>
        ///         <ser:contact>
        ///            <ns:Id>MO000012</ns:Id>
        ///            <ns1:Addresses>
        ///               <ns:Address>
        ///                  <ns:Address1>Santa Monica</ns:Address1>
        ///                  <ns:City>Hollywood</ns:City>
        ///                  <ns:Country>US</ns:Country>
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
        ///            <ns1:MobilePhone>555-5551</ns1:MobilePhone>
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

        [OperationContract]
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
        bool ResetPassword(string userName, string resetCode, string newPassword);

        [OperationContract]
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
        List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays);

        /// <summary>
        /// Get stock status for list of items from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Nav/Central WS2 : GetInventoryMultiple<p/><p/>
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
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        [OperationContract]
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
        List<Profile> ProfilesGetAll();

        /// <summary>
        /// Gets all Member Attributes for a Contact
        /// </summary>
        /// <remarks>
        /// Member Attribute Value has to have Value Yes or No, even in other languages as Omni Server uses that Text to determent if the Attribute is selected or not for the Contact
        /// </remarks>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [OperationContract]
        List<Profile> ProfilesGetByCardId(string contactId);

        #endregion

        #region Basket LOY

        #endregion

        #region Images COMMON

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
        ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest);

        #endregion

        #region search

        [OperationContract]
        SearchRs Search(string cardId, string search, SearchType searchTypes);

        #endregion search

        #region LS Recommends

        /// <summary>
        /// Checks if LS Recommend is active in Omni Server
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
    }
}