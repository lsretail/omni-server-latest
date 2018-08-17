using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.BOConnection.NavSQL.XmlMapping.Loyalty
{
    //Navision click and collect back office connection
    // 
    public class ClickCollectXml : BaseXml
    {
        //Request IDs
        private const string CheckAvailabilityRequestId = "CO_QTY_AVAILABILITY";    //check the quantity of an item available in a certain store.
        private const string CheckAvailabilityExtRequestId = "CO_QTY_AVAILABILITY_EXT";
        private const string CustomerOrderCreateRequestId = "CUSTOMER_ORDER_CREATE";
        private const string CustomerOrderCreateExtRequestId = "CUSTOMER_ORDER_CREATE_EXT";

        public ClickCollectXml()
        {
        }

        #region CheckAvailability

        public string CheckAvailabilityRequestXML(OrderAvailabilityRequest Rq)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?> <Request>
                 <Request_ID>CO_QTY_AVAILABILITY</Request_ID>
                 <Request_Body>
                      <Item_No_Type>ItemNo</Item_No_Type>
                      <Customer_Order_Header>
                     <Document_Id>{1998A029-5D7D-42D3-B22B-2C3563EEC9F2}</Document_Id>
                        <Store_No.>S0009</Store_No.>
                     <Member_Card_No.>10008</Member_Card_No.>
                        <Source_Type>0</Source_Type>
                   </Customer_Order_Header>
                   <Customer_Order_Line>
                     <Document_Id>{1998A029-5D7D-42D3-B22B-2C3563EEC9F2}</Document_Id>
                     <Line_No.>10000</Line_No.>
                     <Line_Type>0</Line_Type>
                     <Number>50000</Number>
                     <Variant_Code></Variant_Code>
                     <Unit_of_Measure_Code>PCS</Unit_of_Measure_Code>
            <Quantity>9</Quantity>
                   </Customer_Order_Line>
                 </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", CheckAvailabilityRequestId),
                    new XElement("Request_Body",
                        new XElement("Item_No_Type", "ItemNo") //ItemNo,ProductId
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");

            XElement orderHeaderRoot = OrderHeader(Rq);
            body.Add(orderHeaderRoot);
            int cnt = 1;
            foreach (OrderLineAvailability orderlineRq in Rq.OrderLineAvailabilityRequests)
            {
                orderlineRq.LineNumber = cnt++;
                body.Add(OrderLine(Rq.Id, orderlineRq));
            }
            return doc.ToString();
        }

        public string CheckAvailabilityRequestXML(OneList Rq)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?> <Request>
                 <Request_ID>CO_QTY_AVAILABILITY_EXT</Request_ID>
                 <Request_Body>
                      <Item_No_Type>ItemNo</Item_No_Type>
                      <Customer_Order_Header>
                     <Document_Id>{1998A029-5D7D-42D3-B22B-2C3563EEC9F2}</Document_Id>
                        <Store_No.>S0009</Store_No.>
                     <Member_Card_No.>10008</Member_Card_No.>
                        <Source_Type>0</Source_Type>
                   </Customer_Order_Header>
                   <Customer_Order_Line>
                     <Document_Id>{1998A029-5D7D-42D3-B22B-2C3563EEC9F2}</Document_Id>
                     <Line_No.>10000</Line_No.>
                     <Line_Type>0</Line_Type>
                     <Number>50000</Number>
                     <Variant_Code></Variant_Code>
                     <Unit_of_Measure_Code>PCS</Unit_of_Measure_Code>
            <Quantity>9</Quantity>
                   </Customer_Order_Line>
                 </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", CheckAvailabilityExtRequestId),
                    new XElement("Request_Body",
                        new XElement("Item_No_Type", "ItemNo") //ItemNo,ProductId
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");

            XElement orderHeaderRoot = OrderHeader(Rq);
            body.Add(orderHeaderRoot);
            int lineno = 1;
            foreach (OneListItem orderlineRq in Rq.Items)
            {
                body.Add(OrderLine(Rq.Id, orderlineRq, lineno++));
            }
            return doc.ToString();
        }

        private XElement OrderHeader(OrderAvailabilityRequest rq)
        {
            XElement root =
                    new XElement("Customer_Order_Header",
                        new XElement("Document_Id", rq.Id),
                        new XElement("Store_No.", rq.StoreId),
                        new XElement("Member_Card_No.", rq.CardId),
                        new XElement("Source_Type", Convert.ToInt32(rq.SourceType).ToString())
                    );
            return root;
        }

        private XElement OrderHeader(OneList rq)
        {
            XElement root =
                    new XElement("Customer_Order_Header",
                        new XElement("Document_Id", rq.Id),
                        new XElement("Store_No.", rq.StoreId),
                        new XElement("Member_Card_No.", rq.CardId),
                        new XElement("Anonymous_Order", (string.IsNullOrEmpty(rq.CardId)) ? true : false),
                        new XElement("Source_Type", (int)SourceType.LSOmni),
                        new XElement("Sourcing_Order", 0)
                    );
            return root;
        }

        private XElement OrderLine(string id, OrderLineAvailability rq)
        {
            XElement root =
                new XElement("Customer_Order_Line",
                    new XElement("Document_Id", id),
                    new XElement("Line_No.", LineNumberToNav(rq.LineNumber)),
                    new XElement("Line_Type", Convert.ToInt32(rq.LineType).ToString()),
                    new XElement("Number", rq.ItemId),
                    new XElement("Variant_Code", rq.VariantId),
                    new XElement("Unit_of_Measure_Code", rq.UomId),
                    new XElement("Quantity", rq.Quantity)
                );
            return root;
        }

        private XElement OrderLine(string id, OneListItem rq, int linenNo)
        {
            XElement root =
                new XElement("Customer_Order_Line",
                    new XElement("Document_Id", id),
                    new XElement("Line_No.", LineNumberToNav(linenNo)),
                    new XElement("Line_Type", (int)LineType.Item),
                    new XElement("Number", rq.Item?.Id),
                    new XElement("Variant_Code", rq.VariantReg?.Id),
                    new XElement("Unit_of_Measure_Code", rq.UnitOfMeasure?.Id),
                    new XElement("Quantity", rq.Quantity)
                );
            return root;
        }

        //
        public List<OrderLineAvailability> CheckAvailabilityResponseXML(string responseXml)
        {
            #region xml
            /*
            <Response>
              <Request_ID>CO_QTY_AVAILABILITY</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Customer_Order_Line>
                  <Document_Id>{1998A029-5D7D-42D3-B22B-2C3563EEC9F2}</Document_Id>
                  <Line_No.>10000</Line_No.>
                  <Line_Type>0</Line_Type>
                  <Number>50000</Number>
                  <Variant_Code />
                  <Unit_of_Measure_Code>PCS</Unit_of_Measure_Code>
                  <Quantity>9</Quantity>
                  <Available_Qty.>0</Available_Qty.>
                </Customer_Order_Line>
              </Response_Body>
            </Response>
             */
            #endregion

            List<OrderLineAvailability> rs = new List<OrderLineAvailability>();

            XDocument doc = XDocument.Parse(responseXml);
            IEnumerable<XElement> orderLines = doc.Element("Response").Element("Response_Body").Descendants("Customer_Order_Line");
            foreach (XElement line in orderLines)
            {
                OrderLineAvailability ola = new OrderLineAvailability();
                if (line.Element("Document_Id") == null)
                    throw new XmlException("Document_Id node not found in response xml");
                ola.OrderId = line.Element("Document_Id").Value;
                ola.OrderId = ola.OrderId.Replace("{", "").Replace("}", ""); //strip out the curly brackets

                if (line.Element("Line_No.") == null)
                    throw new XmlException("Line_No. node not found in response xml");
                ola.LineNumber = LineNumberFromNav(Convert.ToInt32(line.Element("Line_No.").Value));

                if (line.Element("Line_Type") == null)
                    throw new XmlException("Line_Type node not found in response xml");
                ola.LineType = (LineType)Convert.ToInt32(line.Element("Line_Type").Value);

                if (line.Element("Number") == null)
                    throw new XmlException("Number node not found in response xml");
                ola.ItemId = line.Element("Number").Value;

                if (line.Element("Variant_Code") == null)
                    throw new XmlException("Variant_Code  node not found in response xml");
                ola.VariantId = line.Element("Variant_Code").Value;

                if (line.Element("Unit_of_Measure_Code") == null)
                    throw new XmlException("Unit_of_Measure_Code  node not found in response xml");
                ola.UomId = line.Element("Unit_of_Measure_Code").Value;

                if (line.Element("Available_Qty.") == null)
                    throw new XmlException("Available_Qty.  node not found in response xml");
                ola.Quantity = Convert.ToInt32(line.Element("Available_Qty.").Value);

                rs.Add(ola);
            }
            return rs;
        }

        public OrderAvailabilityResponse CheckAvailabilityExtResponseXML(string responseXml)
        {
            #region xml
            /*
            <Response>
              <Request_ID>CO_QTY_AVAILABILITY_EXT</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Customer_Order_Line>
                  <Document_Id>{1998A029-5D7D-42D3-B22B-2C3563EEC9F2}</Document_Id>
                  <Line_No.>10000</Line_No.>
                  <Line_Type>0</Line_Type>
                  <Number>50000</Number>
                  <Variant_Code />
                  <Unit_of_Measure_Code>PCS</Unit_of_Measure_Code>
                  <Quantity>9</Quantity>
                  <Available_Qty.>0</Available_Qty.>
                </Customer_Order_Line>
              </Response_Body>
            </Response>
             */
            #endregion

            OrderAvailabilityResponse rs = new OrderAvailabilityResponse();

            XDocument doc = XDocument.Parse(responseXml);
            XElement body = doc.Element("Response").Element("Response_Body");

            if (body.Element("Preferred_Sourcing_Location") == null)
                throw new XmlException("Preferred_Sourcing_Location node not found in response xml");
            rs.PreferredSourcingLocation = body.Element("Preferred_Sourcing_Location").Value;

            IEnumerable<XElement> orderLines = doc.Element("Response").Element("Response_Body").Descendants("WS_Inventory_Buffer");
            foreach (XElement line in orderLines)
            {
                OrderLineAvailabilityResponse ola = new OrderLineAvailabilityResponse();

                if (line.Element("Item_No") == null)
                    throw new XmlException("Item_No node not found in response xml");
                ola.ItemId = line.Element("Item_No").Value;

                if (line.Element("Variant_Code") == null)
                    throw new XmlException("Variant_Code node not found in response xml");
                ola.VariantId = line.Element("Variant_Code").Value;

                if (line.Element("Location_Code") == null)
                    throw new XmlException("Location_Code node not found in response xml");
                ola.LocationCode = line.Element("Location_Code").Value;

                if (line.Element("Lead_Time__Days_") == null)
                    throw new XmlException("Lead_Time__Days_ node not found in response xml");
                ola.LeadTimeDays = Convert.ToInt32(line.Element("Lead_Time__Days_").Value);

                if (line.Element("Actual_Inventory") == null)
                    throw new XmlException("Actual_Inventory node not found in response xml");
                ola.Quantity = Convert.ToInt32(line.Element("Actual_Inventory").Value);

                rs.Lines.Add(ola);
            }
            return rs;
        }

        #endregion CheckAvailability

        #region OrderCreateRequest

        public string OrderCreateCACRequestXML(Order Rq)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?> <Request>
                <Request_ID>CUSTOMER_ORDER_CREATE</Request_ID>
                <Request_Body>
                    <Item_No_Type>ItemNo</Item_No_Type>
                    <Customer_Order_Header>
                        <Document_Id>{614D84F5-93C8-4792-8A7C-2A0FF3E606CE}</Document_Id>
                        <Store_No.>S0001</Store_No.>
                        <Member_Card_No.>10001</Member_Card_No.>
                        <Source_Type>0</Source_Type>
                    </Customer_Order_Header>
                    <Customer_Order_Line>
                        <Document_Id>{614D84F5-93C8-4792-8A7C-2A0FF3E606CE}</Document_Id>
                        <Line_No.>10000</Line_No.>
                        <Line_Type>0</Line_Type>
                    <Status>0</Status>
                        <Number>1000</Number>
                        <Variant_Code/>
                        <Unit_of_Measure_Code/>
                        <Net_Price>800</Net_Price>
                        <Price>1000</Price>
                        <Quantity>5</Quantity>
                        <Discount_Amount>1000</Discount_Amount>
                        <Discount_Percent>20</Discount_Percent>
                        <Net_Amount>3200</Net_Amount>
                        <Vat_Amount>800</Vat_Amount>
                        <Amount>4000</Amount>
                    </Customer_Order_Line>
                    <Customer_Order_Discount_Line>
                        <Document_Id>{614D84F5-93C8-4792-8A7C-2A0FF3E606CE}</Document_Id>
                        <Line_No.>10000</Line_No.>
                        <Entry_No.>1</Entry_No.>
                        <Discount_Type>4</Discount_Type>
                        <Offer_No./>
                        <Periodic_Disc._Type>0</Periodic_Disc._Type>
                        <Periodic_Disc._Group/>
                        <Description/>
                        <Discount_Percent>20</Discount_Percent>
                        <Discount_Amount>1000</Discount_Amount>
                    </Customer_Order_Discount_Line>
                </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", CustomerOrderCreateRequestId),
                    new XElement("Request_Body",
                        new XElement("Item_No_Type", "ItemNo") //ItemNo
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");
            body.Add(OrderCreateHeader(Rq));

            foreach (OrderLine orderlineRq in Rq.OrderLines)
            {
                body.Add(OrderCreateLine(Rq.Id, orderlineRq));
            }
            if (Rq.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine orderDiscountRq in Rq.OrderDiscountLines)
                {
                    body.Add(OrderCreateDiscountLine(Rq.Id, orderDiscountRq));
                }
            }
            return doc.ToString();
        }

        public string OrderCreateRequestXML(Order Rq)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?> <Request>
                <Request_ID>CUSTOMER_ORDER_CREATE</Request_ID>
                <Request_Body>
                    <Item_No_Type>ItemNo</Item_No_Type>
                    <Customer_Order_Header>
                        <Document_Id>{614D84F5-93C8-4792-8A7C-2A0FF3E606CE}</Document_Id>
                        <Store_No.>S0001</Store_No.>
                        <Member_Card_No.>10001</Member_Card_No.>
                        <Source_Type>0</Source_Type>
                    </Customer_Order_Header>
                    <Customer_Order_Line>
                        <Document_Id>{614D84F5-93C8-4792-8A7C-2A0FF3E606CE}</Document_Id>
                        <Line_No.>10000</Line_No.>
                        <Line_Type>0</Line_Type>
                    <Status>0</Status>
                        <Number>1000</Number>
                        <Variant_Code/>
                        <Unit_of_Measure_Code/>
                        <Net_Price>800</Net_Price>
                        <Price>1000</Price>
                        <Quantity>5</Quantity>
                        <Discount_Amount>1000</Discount_Amount>
                        <Discount_Percent>20</Discount_Percent>
                        <Net_Amount>3200</Net_Amount>
                        <Vat_Amount>800</Vat_Amount>
                        <Amount>4000</Amount>
                    </Customer_Order_Line>
                    <Customer_Order_Discount_Line>
                        <Document_Id>{614D84F5-93C8-4792-8A7C-2A0FF3E606CE}</Document_Id>
                        <Line_No.>10000</Line_No.>
                        <Entry_No.>1</Entry_No.>
                        <Discount_Type>4</Discount_Type>
                        <Offer_No./>
                        <Periodic_Disc._Type>0</Periodic_Disc._Type>
                        <Periodic_Disc._Group/>
                        <Description/>
                        <Discount_Percent>20</Discount_Percent>
                        <Discount_Amount>1000</Discount_Amount>
                    </Customer_Order_Discount_Line>
                </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", CustomerOrderCreateExtRequestId),
                    new XElement("Request_Body",
                        new XElement("Item_No_Type", "ItemNo") //ItemNo,ProductId
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");
            body.Add(OrderCreateHeaderExtended(Rq));

            foreach (OrderLine orderlineRq in Rq.OrderLines)
            {
                body.Add(OrderCreateLine(Rq.Id, orderlineRq));
            }
            if (Rq.OrderDiscountLines != null)
            {
                int lastlineindex = 0;
                int lastsubline = 1;
                foreach (OrderDiscountLine orderDiscountRq in Rq.OrderDiscountLines)
                {
                    int lineno = LineNumberToNav(orderDiscountRq.LineNumber);
                    if (orderDiscountRq.LineNumber == lastlineindex)
                    {
                        lineno += (lastsubline * 100);
                        lastsubline++;
                    }

                    lastlineindex = orderDiscountRq.LineNumber;
                    orderDiscountRq.LineNumber = lineno;
                    body.Add(OrderCreateDiscountLine(Rq.Id, orderDiscountRq));
                }
            }
            if (Rq.OrderPayments != null)
            {
                foreach (OrderPayment orderPay in Rq.OrderPayments)
                {
                    body.Add(OrderCreatePayment(Rq.Id, orderPay));
                }
            }
            return doc.ToString();
        }

        private XElement OrderCreateHeader(Order rq)
        {
            XElement root =
                    new XElement("Customer_Order_Header",
                        new XElement("Document_Id", rq.Id),
                        new XElement("Store_No.", rq.StoreId),
                        new XElement("Member_Card_No.", rq.CardId),
                        new XElement("Source_Type", Convert.ToInt32(rq.SourceType).ToString())
                    );
            return root;
        }

        private XElement OrderCreateHeaderExtended(Order rq)
        {
            XElement root =
                    new XElement("Customer_Order_Header",
                        new XElement("Document_Id", rq.Id),
                        new XElement("Store_No.", (rq.ClickAndCollectOrder && string.IsNullOrEmpty(rq.CollectLocation) == false) ? rq.CollectLocation : rq.StoreId),
                        new XElement("Member_Card_No.", rq.CardId),
                        new XElement("Member_Contact_No.", rq.ContactId),
                        new XElement("Member_Contact_Name", rq.ContactName),
                        new XElement("Source_Type", Convert.ToInt32(rq.SourceType)),
                        new XElement("Sourcing_Location", string.Empty),
                        new XElement("Inventory_Transfer", false),
                        new XElement("Full_Name", rq.ContactName),
                        new XElement("Address", rq.ContactAddress.Address1),
                        new XElement("Address_2", rq.ContactAddress.Address2),
                        new XElement("City", rq.ContactAddress.City),
                        new XElement("County", rq.ContactAddress.StateProvinceRegion),
                        new XElement("Post_Code", rq.ContactAddress.PostCode),
                        new XElement("Country_Region_Code", rq.ContactAddress.Country),
                        new XElement("Phone_No.", rq.PhoneNumber),
                        new XElement("Email", rq.Email),
                        new XElement("House_Apartment_No.", rq.ContactAddress.HouseNo),
                        new XElement("Mobile_Phone_No.", rq.MobileNumber),
                        new XElement("Daytime_Phone_No.", rq.DayPhoneNumber),
                        new XElement("Ship_To_Full_Name", rq.ShipToName),
                        new XElement("Ship_To_Address", rq.ShipToAddress.Address1),
                        new XElement("Ship_To_Address_2", rq.ShipToAddress.Address2),
                        new XElement("Ship_To_City", rq.ShipToAddress.City),
                        new XElement("Ship_To_County", rq.ShipToAddress.StateProvinceRegion),
                        new XElement("Ship_To_Post_Code", rq.ShipToAddress.PostCode),
                        new XElement("Ship_To_Country_Region_Code", rq.ShipToAddress.Country),
                        new XElement("Ship_To_Phone_No.", rq.ShipToPhoneNumber),
                        new XElement("Ship_To_Email", rq.ShipToEmail),
                        new XElement("Ship_To_House_Apartment_No.", rq.ShipToAddress.HouseNo),
                        new XElement("Click_And_Collect_Order", rq.ClickAndCollectOrder),
                        new XElement("Anonymous_Order", rq.AnonymousOrder),
                        new XElement("Collect_Location", string.Empty),
                        new XElement("Created_at_Store", rq.StoreId),
                        new XElement("Receipt_No.", string.Empty),
                        new XElement("Trans._Store_No.", string.Empty),
                        new XElement("Trans._Terminal_No.", string.Empty),
                        new XElement("Ship_Order", rq.ShipClickAndCollect),
                        new XElement("Shipping_Agent_Code", rq.ShippingAgentCode),
                        new XElement("Shipping_Agent_Service_Code", rq.ShippingAgentServiceCode)
                    );
            return root;
        }

        private XElement OrderCreateLine(string id, OrderLine rq)
        {
            XElement root =
                new XElement("Customer_Order_Line",
                    new XElement("Document_Id", id),
                    new XElement("Line_No.", LineNumberToNav(rq.LineNumber)),
                    new XElement("Line_Type", Convert.ToInt32(rq.LineType).ToString()),
                    new XElement("Number", rq.ItemId),
                    new XElement("Variant_Code", rq.VariantId),
                    new XElement("Unit_of_Measure_Code", rq.UomId),
                    new XElement("Net_Price", ConvertTo.SafeStringDecimal(rq.NetPrice)),
                    new XElement("Price", ConvertTo.SafeStringDecimal(rq.Price)),
                    new XElement("Quantity", ConvertTo.SafeStringDecimal(rq.Quantity)),
                    new XElement("Discount_Amount", ConvertTo.SafeStringDecimal(rq.DiscountAmount)),
                    new XElement("Discount_Percent", ConvertTo.SafeStringDecimal(rq.DiscountPercent)),
                    new XElement("Net_Amount", ConvertTo.SafeStringDecimal(rq.NetAmount)),
                    new XElement("Vat_Amount", ConvertTo.SafeStringDecimal(rq.TaxAmount)),
                    new XElement("Amount", ConvertTo.SafeStringDecimal(rq.Amount)),
                    new XElement("Sourcing_Location", string.Empty),
                    new XElement("Order_Reference", string.Empty)
                );
            return root;
        }

        private XElement OrderCreateDiscountLine(string id, OrderDiscountLine rq)
        {
            XElement root =
                new XElement("Customer_Order_Discount_Line",
                    new XElement("Document_Id", id),
                    new XElement("Line_No.", LineNumberToNav(rq.LineNumber)),
                    new XElement("Entry_No.", rq.No),  //Entry number, if more than one discount pr Line No.
                    new XElement("Discount_Type", Convert.ToInt32(rq.DiscountType).ToString()),
                    new XElement("Offer_No.", rq.OfferNumber),
                    new XElement("Periodic_Disc._Type", Convert.ToInt32(rq.PeriodicDiscType).ToString()),
                    new XElement("Periodic_Disc._Group", rq.PeriodicDiscGroup),
                    new XElement("Description", rq.Description),
                    new XElement("Discount_Percent", ConvertTo.SafeStringDecimal(rq.DiscountPercent)),
                    new XElement("Discount_Amount", ConvertTo.SafeStringDecimal(rq.DiscountAmount))
                );
            return root;
        }

        private XElement OrderCreatePayment(string id, OrderPayment rq)
        {
            XElement root =
                new XElement("Customer_Order_Payment",
                    new XElement("Document_Id", id),
                    new XElement("Line_No.", LineNumberToNav(rq.LineNumber)),
                    new XElement("Pre_Approved_Amount", rq.PreApprovedAmount),  //Entry number, if more than one discount pr Line No.
                    new XElement("Finalised_Amount", rq.FinalizedAmount),
                    new XElement("Tender_Type", rq.TenderType),
                    new XElement("Card_Type", rq.CardType),
                    new XElement("Currency_Code", rq.CurrencyCode),
                    new XElement("Currency_Factor", (rq.CurrencyFactor == 0) ? 1 : rq.CurrencyFactor),
                    new XElement("Authorisation_Code", rq.AuthorisationCode),
                    new XElement("Pre_Approved_Valid_Date", ToNAVDate(rq.PreApprovedValidDate)),
                    new XElement("Card_or_Customer_number", rq.CardNumber)
                );
            return root;
        }

        #endregion OrderCreateRequest
    }
}
