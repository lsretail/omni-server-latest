using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    //Navision back office connection
    // 
    public class BasketXml : BaseXml
    {
        //Request IDs
        private const string CalculatieBasketRequestId = "WI_NC_CALCULATE_BASKET";
        private const string CalculatieBasketNewRequestId = "ECOMM_CALCULATE_BASKET";
        private const string CreateOrderRequestId = "WI_NC_CREATE_ORDER";

        public BasketXml()
        {
        }

        public string BasketCalcRequestXML(OneList Rq, int navVersion)
        {
            // Create the XML Declaration, and append it to XML document
            XElement root;

            if (navVersion > 10)
            {
                root = new XElement("Request",
                         new XElement("Request_ID", CalculatieBasketNewRequestId),
                         new XElement("Request_Body")
                       );
            }
            else
            {
                root = new XElement("Request",
                         new XElement("Request_ID", CalculatieBasketRequestId),
                         new XElement("Request_Body",
                            new XElement("ItemId_Type", "ItemNo"),
                            new XElement("Calculation_Type", "None"),
                            new XElement("Collect_Store_No", Rq.StoreId)
                       ));
            }

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");

            XElement mtransRoot = MobileTrans(Rq);
            body.Add(mtransRoot);
            int lineno = 1;
            foreach (OneListItem calcRq in Rq.Items)
            {
                body.Add(MobileTransLine(Rq.Id, lineno++, calcRq));
            }

            return doc.ToString();
        }

        private XElement MobileTrans(OneList rq)
        {
            XElement root =
                    new XElement("MobileTransaction",
                        new XElement("Id", rq.Id),
                        new XElement("StoreId", rq.StoreId),
                        new XElement("MemberContactNo", string.Empty),
                        new XElement("MemberCardNo", rq.CardId)
                    );
            return root;
        }

        private XElement MobileTransLine(string id, int lineNumber, OneListItem rq)
        {
            XElement root =
                new XElement("MobileTransactionLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(lineNumber)),
                    new XElement("LineType", "0"),
                    new XElement("Barcode", rq.BarcodeId),
                    new XElement("Number", rq.ItemId),
                    new XElement("VariantCode", rq.VariantId),
                    new XElement("UomId", rq.UnitOfMeasureId),
                    new XElement("Quantity", rq.Quantity),
                    new XElement("ExternalId", "0")
                );
            return root;
        }

        public string BasketPostSaleRequestXML(Order Rq)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>WI_NC_CREATE_ORDER</Request_ID>
              <ItemId_TypeItem No</ItemId_Type>
              <Request_Body>
                <WI_Basket_Header_Buffer>
                  <Contact_No.>MO000001</Contact_No.>
                  <Currency_Code/>
                  <Web_Transaction_GUID>A5055BB7-3894-4438-AA9B-85FDC3804003</Web_Transaction_GUID>
                  <Loyalty_Card_No.>125</Loyalty_Card_No.>
                  <Basket_Price>125</Basket_Price>
                  <Full_Name>NC_Full_Name</Full_Name>
                  <Ship-to_Address>NC_Ship-to Address</Ship-to_Address>
                  <Ship-to_Address_2>NC_Ship-to Address 2</Ship-to_Address_2>
                  <Ship-to_City>NC_Ship-to City</Ship-to_City>
                  <Ship-to_Post_Code>NC_SHIP-TO POST CODE</Ship-to_Post_Code>
                  <Ship-to_County>NC_S-Cou</Ship-to_County>
                  <Ship-to_Country_Region_Code>NC_S_C/R</Ship-to_Country_Region_Code>
                  <Bill-to_Address>NC_Bill-to Address</Bill-to_Address>
                  <Bill-to_Address_2>NC_Bill-to Address 2</Bill-to_Address_2>
                  <Bill-to_City>NC_Bill-to City</Bill-to_City>
                  <Bill-to_Post_Code>NC_BILL-TO POST CODE</Bill-to_Post_Code>
                  <Bill-to_County>NC_B-Cou</Bill-to_County>
                  <Bill-to_Country_Region_Code>NC_B_C/R</Bill-to_Country_Region_Code>
                  <Phone_No.>NC_Phone_No</Phone_No.>
                  <Mobile_Phone_No.>NC_Mobile_Phone_No</Mobile_Phone_No.>
                  <E-Mail>NC_E-Mail</E-Mail>
                  <Shipping_Agent_Code>NC_ACODE</Shipping_Agent_Code>
                  <Shipment_Method_Code/>
                  <Price_Include_VAT>true</Price_Include_VAT>
                  <WINCOrderId>1</WINCOrderId>
                  <Source_Type>1</Source_Type>
                </WI_Basket_Header_Buffer>
                <WI_Basket_Line_Buffer>
                  <Id>A5055BB7-3894-4438-AA9B-85FDC3804003</Id>
                  <Line_No.>10000</Line_No.>
                  <Item_No.>1</Item_No.>
                  <Variant_Code/>
                  <Quantity>1</Quantity>
                  <Price>10</Price>
                  <Unit_of_Measure_Code>NC_ACODE</Unit_of_Measure_Code> 
                  <Discount_Amount>1</Discount_Amount>
                  <Discount_Percent>0</Discount_Percent>
                  <Net_Amount>10</Net_Amount>
                  <TAX_Amount>0</TAX_Amount>
                  <Amount>0</Amount>
                  <TAX_Product_Code></TAX_Product_Code>
                </WI_Basket_Line_Buffer>
                <WI_Basket_Discount_Buffer>
                  <Id>A5055BB7-3894-4438-AA9B-85FDC3804003</Id>
                  <Line_No.>10000</Line_No.>
                  <No.>1</No.>
                  <Discount_Type>0</Discount_Type>
                  <Offer_No.></Offer_No.>
                  <Periodic_Disc._Type>0</Periodic_Disc._Type>
                  <Periodic_Disc._Group></Periodic_Disc._Group>
                  <Description></Description>
                  <Discount_Percent>0</Discount_Percent>
                  <Discount_Amount>0</Discount_Amount>
                </WI_Basket_Discount_Buffer>
                <WI_Basket_Payment_Buffer>
                  <Id>A5055BB7-3894-4438-AA9B-85FDC3804003</Id>
                  <Line_No.>10000</Line_No.>
                  <Tender_Type></Tender_Type>
                  <Currency_Code></Currency_Code>
                  <Amount>0</Amount>
                  <Card_Or_Cust._No.></Card_Or_Cust._No.>
                  <Card_Type></Card_Type>
                  <EFT_Card_Number></EFT_Card_Number>
                  <EFT_Card_Name>0</EFT_Card_Name>
                  <EFT_Auth._Code>0</EFT_Auth._Code>
                  <EFT_Message></EFT_Message>
                </WI_Basket_Payment_Buffer>    
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", CreateOrderRequestId),
                    new XElement("Request_Body",
                        new XElement("ItemId_Type", "ItemNo") //ProductId,ItemNo  
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");

            XElement mtransRoot = BasketPostSaleHeader(Rq);
            body.Add(mtransRoot);
            foreach (OrderLine rq in Rq.OrderLines)
            {
                body.Add(BasketPostSaleLine(Rq.Id, rq));
            }
            foreach (OrderDiscountLine rq in Rq.OrderDiscountLines)
            {
                body.Add(BasketPostSaleDiscLine(Rq.Id, rq));
            }
            foreach (OrderPayment tenderLine in Rq.OrderPayments)
            {
                body.Add(TransPaymentLine(Rq.Id, tenderLine));
            }

            return doc.ToString();
        }

        private XElement BasketPostSaleHeader(Order header)
        {
            Address shippingAddress = header.ShipToAddress;
            Address billingAddress = header.ContactAddress;
            if (shippingAddress == null)
                shippingAddress = new Address();
            if (billingAddress == null)
                billingAddress = new Address();

            XElement root =
                new XElement("WI_Basket_Header_Buffer",
                    new XElement("Contact_No.", header.ContactId),
                    new XElement("Currency_Code", string.Empty),
                    new XElement("CurrencyFactor", "1"),   //new in LS Nav 9.00.03
                    new XElement("Web_Transaction_GUID", header.Id),
                    new XElement("Loyalty_Card_No.", header.CardId),
                    new XElement("Full_Name", header.ContactName),
                    new XElement("Ship-to_Address", shippingAddress.Address1),
                    new XElement("Ship-to_Address_2", shippingAddress.Address2),
                    new XElement("Ship-to_City", shippingAddress.City),
                    new XElement("Ship-to_Post_Code", shippingAddress.PostCode),
                    new XElement("Ship-to_County", shippingAddress.StateProvinceRegion),
                    new XElement("Ship-to_Country_Region_Code", shippingAddress.Country),
                    new XElement("Bill-to_Address", billingAddress.Address1),
                    new XElement("Bill-to_Address_2", billingAddress.Address2),
                    new XElement("Bill-to_City", billingAddress.City),
                    new XElement("Bill-to_Post_Code", billingAddress.PostCode),
                    new XElement("Bill-to_County", billingAddress.StateProvinceRegion),
                    new XElement("Bill-to_Country_Region_Code", billingAddress.Country),
                    new XElement("Phone_No.", header.ContactAddress.PhoneNumber),
                    new XElement("Mobile_Phone_No.", header.ContactAddress.CellPhoneNumber),
                    new XElement("E-Mail", header.Email),
                    new XElement("Shipping_Agent_Code", ""),  //empty
                    new XElement("Shipment_Method_Code", (int)header.ShippingStatus), // ISP (instore pickup), esp
                    new XElement("Price_Include_VAT", 0),
                    new XElement("Basket_Price", ConvertTo.SafeStringDecimal(header.TotalAmount)),
                    new XElement("WINCOrderId", string.Empty),  //WINCOrderId is orderId in NOP
                    new XElement("Source_Type", "1")  //="1" 0,1
                );
            return root;
        }

        private XElement BasketPostSaleLine(string id, OrderLine line)
        {
            //if CouponCode is not empty then set the linetype = 6  (coupon) and coupon code into barcode
            //string barcode = (string.IsNullOrWhiteSpace(line.c) ? "" : line.CouponCode);
            //string lineType = (string.IsNullOrWhiteSpace(line.CouponCode) ? "0" : "6");

            XElement root =
                new XElement("WI_Basket_Line_Buffer",
                    new XElement("Id", id),
                    new XElement("Line_No.", LineNumberToNav(line.LineNumber)),
                    new XElement("LineType", "0"), //new in LS Nav 9.00.03   //LineType=0 is item, 6 is coupon
                    new XElement("Barcode", string.Empty),   //new in LS Nav 9.00.03  used for coupon code
                    new XElement("Item_No.", line.ItemId),
                    new XElement("Variant_Code", line.VariantId),
                    new XElement("Quantity", line.Quantity),
                    new XElement("Price", ConvertTo.SafeStringDecimal(line.Price)),
                    new XElement("Unit_of_Measure_Code", line.UomId),
                    new XElement("Discount_Amount", ConvertTo.SafeStringDecimal(line.DiscountAmount)),
                    new XElement("Discount_Percent", ConvertTo.SafeStringDecimal(line.DiscountPercent)),
                    new XElement("Net_Amount", ConvertTo.SafeStringDecimal(line.NetAmount)),
                    new XElement("TAX_Amount", ConvertTo.SafeStringDecimal(line.TaxAmount)),
                    new XElement("TAX_Product_Code", string.Empty),
                    new XElement("Amount", ConvertTo.SafeStringDecimal(line.Amount))
                );

            return root;
        }

        private XElement BasketPostSaleDiscLine(string id, OrderDiscountLine line)
        {
            XElement root =
                new XElement("WI_Basket_Discount_Buffer",
                    new XElement("Id", id),
                    new XElement("Line_No.", LineNumberToNav(line.LineNumber)),
                    new XElement("No.", line.No), //Entry number, if more than one discount pr Line No.
                    new XElement("Discount_Type", Convert.ToInt32(line.DiscountType).ToString()),
                    new XElement("Offer_No.", line.OfferNumber),
                    new XElement("Periodic_Disc._Type", Convert.ToInt32(line.PeriodicDiscType).ToString()), //DiscOffer=2
                    new XElement("Periodic_Disc._Group", line.PeriodicDiscGroup),
                    new XElement("Description", line.Description),
                    new XElement("Discount_Percent", line.DiscountPercent),
                    new XElement("Discount_Amount", ConvertTo.SafeStringDecimal(line.DiscountAmount))
                );
            return root;
        }

        private XElement TransPaymentLine(string id, OrderPayment line)
        {
            XElement root =
                new XElement("WI_Basket_Payment_Buffer",
                    new XElement("Id", id),
                    new XElement("Line_No.", LineNumberToNav(line.LineNumber)),
                    new XElement("Tender_Type", line.TenderType.ToString()),
                    new XElement("Currency_Code", line.CurrencyCode),
                    new XElement("CurrencyFactor", "1"),   //new in LS Nav 9.00.03
                    new XElement("Amount", ConvertTo.SafeStringDecimal(line.Amount)),
                    new XElement("Card_Or_Cust._No.", line.CardNumber),
                    new XElement("Card_Type", line.CardType),
                    new XElement("EFT_Card_Number", string.Empty),
                    new XElement("EFT_Card_Name", string.Empty),
                    new XElement("EFT_Auth._Code", string.Empty),
                    new XElement("EFT_Message", string.Empty)
                );
            return root;
        }

        public Order BasketCalcToOrderResponseXML(string responseXml, int navVersion)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Response>
                <Request_ID>WI_NC_CALCULATE_BASKET</Request_ID>
                <Response_Code>0000</Response_Code>
                <Response_Text></Response_Text>
                <Response_Body>
                    <MobileTransaction>
                        <Id>{A5055BB7-3894-4438-AA9B-85FDC3804F02}</Id>
                        <StoreId>S0013</StoreId>
                        <TerminalId>P0040</TerminalId>
                        <StaffId>1301</StaffId>
                        <TransactionType>2</TransactionType>
                        <EntryStatus>0</EntryStatus>
                        <ReceiptNo>00000P0040000000720</ReceiptNo>
                        <TransactionNo>0</TransactionNo>
                        <TransDate>2014-03-20T16:32:42.093Z</TransDate>
                        <CurrencyCode />
                        <CurrencyFactor>0</CurrencyFactor>
                        <BusinessTAXCode>NATIONAL</BusinessTAXCode>
                        <PriceGroupCode />
                        <CustomerId />
                        <CustDiscGroup />
                        <MemberCardNo />
                        <MemberPriceGroupCode />
                        <MemberContactNo>MO000008</MemberContactNo>
                        <ManualTotalDiscPercent>0</ManualTotalDiscPercent>
                        <ManualTotalDiscAmount>0</ManualTotalDiscAmount>
                    </MobileTransaction>
                    <MobileTransactionLine>
                        <Id>{A5055BB7-3894-4438-AA9B-85FDC3804F02}</Id>
                        <StoreId>S0013</StoreId>
                        <TerminalId>P0040</TerminalId>
                        <LineNo>10000</LineNo>
                        <EntryStatus>0</EntryStatus>
                        <TransactionNo>0</TransactionNo>
                        <LineType>0</LineType>
                        <Number>10000</Number>
                        <Barcode />
                        <CurrencyCode />
                        <CurrencyFactor>0</CurrencyFactor>
                        <VariantCode />
                        <UomId>BOTTLE</UomId>
                        <NetPrice>0.96</NetPrice>
                        <Price>1.2</Price>
                        <Quantity>1</Quantity>
                        <DiscountAmount>0.24</DiscountAmount>
                        <DiscountPercent>20</DiscountPercent>
                        <NetAmount>0.768</NetAmount>
                        <TAXAmount>0.192</TAXAmount>
                        <TAXProductCode>C</TAXProductCode>
                        <TAXBusinessCode>NATIONAL</TAXBusinessCode>
                        <ManualPrice>0</ManualPrice>
                        <CardOrCustNo />
                        <ManualDiscountPercent>0</ManualDiscountPercent>
                        <ManualDiscountAmount>0</ManualDiscountAmount>
                        <DiscInfoLine>0</DiscInfoLine>
                        <TotalDiscInfoLine>0</TotalDiscInfoLine>
                        <Item_Description />
                        <Variant_Description />
                        <Uom_Description />
                        <Tender_Description />
                        <TransDate />
                        <ExternalId>0</ExternalId>
                    </MobileTransactionLine>
                    <MobileTransDiscountLine>
                        <Id>{A5055BB7-3894-4438-AA9B-85FDC3804F02}</Id>
                        <LineNo>10000</LineNo>
                        <No>10000</No>
                        <DiscountType>0</DiscountType>
                        <OfferNo>P1059</OfferNo>
                        <PeriodicDiscType>3</PeriodicDiscType>
                        <PeriodicDiscGroup>P1059</PeriodicDiscGroup>
                        <Description>Test Item discount</Description>
                        <DiscountPercent>20</DiscountPercent>
                        <DiscountAmount>0.24</DiscountAmount>
                    </MobileTransDiscountLine>
                </Response_Body>
            </Response>

             */
            #endregion

            Order rs = new Order();
            decimal totalDiscountAmt = 0M;
            decimal totalNetAmount = 0M;
            decimal totalTaxAmount = 0M;

            XDocument doc = XDocument.Parse(responseXml);
            XElement mobileTrans = doc.Element("Response").Element("Response_Body").Element("MobileTransaction");
            if (mobileTrans.Element("Id") == null)
                throw new XmlException("Id node not found in response XML");
            rs.Id = mobileTrans.Element("Id").Value;
            rs.Id = rs.Id.Replace("{", "").Replace("}", ""); //strip out the curly brackets

            if (mobileTrans.Element("StoreId") == null)
                throw new XmlException("StoreId node not found in response XML");
            rs.StoreId = mobileTrans.Element("StoreId").Value;

            if (mobileTrans.Element("TerminalId") == null)
                throw new XmlException("TerminalId node not found in response XML");
            rs.TransTerminal = mobileTrans.Element("TerminalId").Value;

            if (mobileTrans.Element("ReceiptNo") == null)
                throw new XmlException("ReceiptNo node not found in response XML");
            rs.ReceiptNo = mobileTrans.Element("ReceiptNo").Value;

            if (mobileTrans.Element("TransactionNo") == null)
                throw new XmlException("TransactionNo node not found in response XML");
            rs.TransId = mobileTrans.Element("TransactionNo").Value;

            if (mobileTrans.Element("CustomerId") == null)
                throw new XmlException("CustomerId node not found in response XML");
            rs.ContactId = mobileTrans.Element("CustomerId").Value;

            if (mobileTrans.Element("MemberCardNo") == null)
                throw new XmlException("MemberCardNo node not found in response XML");
            rs.CardId = mobileTrans.Element("MemberCardNo").Value;

            if (mobileTrans.Element("MemberContactNo") == null)
                throw new XmlException("MemberContactNo node not found in response XML");
            rs.ContactId = mobileTrans.Element("MemberContactNo").Value;

            //now loop through the lines
            IEnumerable<XElement> mobileTransLines = doc.Element("Response").Element("Response_Body").Descendants("MobileTransactionLine");
            foreach (XElement mobileTransLine in mobileTransLines)
            {
                OrderLine rsl = new OrderLine();

                //Transaction Id
                if (mobileTransLine.Element("Id") == null)
                    throw new XmlException("Id node not found in response XML");
                string id = mobileTransLine.Element("Id").Value;
                id = id.Replace("{", "").Replace("}", ""); //strip out the curly brackets

                if (mobileTransLine.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response XML");
                rsl.LineNumber = LineNumberFromNav(Convert.ToInt32(mobileTransLine.Element("LineNo").Value));

                if (mobileTransLine.Element("LineType") == null)
                    throw new XmlException("LineType node not found in response XML");
                rsl.LineType = (LineType)Convert.ToInt32(mobileTransLine.Element("LineType").Value);

                //Item No. or Tender Type
                if (mobileTransLine.Element("Number") == null)
                    throw new XmlException("Number node not found in response XML");
                rsl.ItemId = mobileTransLine.Element("Number").Value;

                if (mobileTransLine.Element("VariantCode") == null)
                    throw new XmlException("VariantCode node not found in response XML");
                rsl.VariantId = mobileTransLine.Element("VariantCode").Value;

                if (mobileTransLine.Element("UomId") == null)
                    throw new XmlException("UomId ItemId node not found in response XML");
                rsl.UomId = mobileTransLine.Element("UomId").Value;

                if (mobileTransLine.Element("NetPrice") == null)
                    throw new XmlException("NetPrice node not found in response XML");
                rsl.NetPrice = ConvertTo.SafeDecimal(mobileTransLine.Element("NetPrice").Value);

                if (mobileTransLine.Element("Price") == null)
                    throw new XmlException("Price node not found in response XML");
                rsl.Price = ConvertTo.SafeDecimal(mobileTransLine.Element("Price").Value);

                if (mobileTransLine.Element("Quantity") == null)
                    throw new XmlException("Quantity node not found in response XML");
                rsl.Quantity = ConvertTo.SafeDecimal(mobileTransLine.Element("Quantity").Value);

                if (mobileTransLine.Element("DiscountAmount") == null)
                    throw new XmlException("DiscountAmount node not found in response XML");
                rsl.DiscountAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountAmount").Value);

                if (mobileTransLine.Element("DiscountPercent") == null)
                    throw new XmlException("DiscountPercent node not found in response XML");
                rsl.DiscountPercent = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountPercent").Value);

                if (mobileTransLine.Element("NetAmount") == null)
                    throw new XmlException("NetAmount node not found in response XML");
                rsl.NetAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("NetAmount").Value);

                if (mobileTransLine.Element("TAXAmount") == null)
                    throw new XmlException("TAXAmount node not found in response XML");
                rsl.TaxAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("TAXAmount").Value);
                rsl.Amount = rsl.NetAmount + rsl.TaxAmount;

                if (navVersion > 10)
                {
                    if (mobileTransLine.Element("ItemDescription") != null)
                        rsl.ItemDescription = mobileTransLine.Element("ItemDescription").Value;
                    if (mobileTransLine.Element("VariantDescription") != null)
                        rsl.VariantDescription = mobileTransLine.Element("VariantDescription").Value;
                }
                else
                {
                    if (mobileTransLine.Element("Item_Description") != null)
                        rsl.ItemDescription = mobileTransLine.Element("Item_Description").Value;
                    if (mobileTransLine.Element("Variant_Description") != null)
                        rsl.VariantDescription = mobileTransLine.Element("Variant_Description").Value;
                }

                // getting perdiscount totaldiscount - ignore them
                if (rsl.LineType == LineType.Item || rsl.LineType == LineType.Coupon)
                {
                    rs.OrderLines.Add(rsl);
                    totalDiscountAmt += rsl.DiscountAmount;
                    totalNetAmount += rsl.NetAmount;
                    totalTaxAmount += rsl.TaxAmount;
                }
            }

            rs.TotalAmount = totalNetAmount + totalTaxAmount;
            rs.TotalNetAmount = totalNetAmount;
            rs.TotalDiscount = totalDiscountAmt;

            //now loop through the discount lines
            IEnumerable<XElement> mobileTransDiscLines = doc.Element("Response").Element("Response_Body").Descendants("MobileTransDiscountLine");
            foreach (XElement mobileTransDisc in mobileTransDiscLines)
            {
                OrderDiscountLine rsld = new OrderDiscountLine();
                //The ID for the Transaction
                if (mobileTransDisc.Element("Id") == null)
                    throw new XmlException("Id node not found in response XML");
                string id = mobileTransDisc.Element("Id").Value;
                id = id.Replace("{", "").Replace("}", ""); //strip out the curly brackets

                //transaction line number
                if (mobileTransDisc.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response XML");
                rsld.LineNumber = Convert.ToInt32(mobileTransDisc.Element("LineNo").Value);//no divide by 10000 here!

                //discount line number
                if (mobileTransDisc.Element("No") == null)
                    throw new XmlException("No node not found in response XML");
                rsld.No = mobileTransDisc.Element("No").Value;

                //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                if (mobileTransDisc.Element("DiscountType") == null)
                    throw new XmlException("DiscountType node not found in response XML");
                rsld.DiscountType = (DiscountType)Convert.ToInt32(mobileTransDisc.Element("DiscountType").Value);

                if (mobileTransDisc.Element("OfferNo") == null)
                    throw new XmlException("OfferNo node not found in response XML");
                rsld.OfferNumber = mobileTransDisc.Element("OfferNo").Value;

                //PeriodicDiscType: Multibuy,Mix&Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount
                if (mobileTransDisc.Element("PeriodicDiscType") == null)
                    throw new XmlException("PeriodicDiscType node not found in response XML");
                rsld.PeriodicDiscType = (PeriodicDiscType)Convert.ToInt32(mobileTransDisc.Element("PeriodicDiscType").Value);

                if (mobileTransDisc.Element("PeriodicDiscGroup") == null)
                    throw new XmlException("PeriodicDiscGroup node not found in response XML");
                rsld.PeriodicDiscGroup = mobileTransDisc.Element("PeriodicDiscGroup").Value;

                if (mobileTransDisc.Element("Description") == null)
                    throw new XmlException("Description node not found in response XML");
                rsld.Description = mobileTransDisc.Element("Description").Value;

                if (mobileTransDisc.Element("DiscountPercent") == null)
                    throw new XmlException("DiscountPercent node not found in response XML");
                rsld.DiscountPercent = ConvertTo.SafeDecimal(mobileTransDisc.Element("DiscountPercent").Value);

                if (mobileTransDisc.Element("DiscountAmount") == null)
                    throw new XmlException("DiscountAmount node not found in response XML");
                rsld.DiscountAmount = ConvertTo.SafeDecimal(mobileTransDisc.Element("DiscountAmount").Value);

                rs.OrderDiscountLines.Add(rsld);
            }
            return rs;
        }
    }
}
