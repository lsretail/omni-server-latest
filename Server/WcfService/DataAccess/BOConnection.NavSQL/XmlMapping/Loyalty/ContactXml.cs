using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.NavSQL.XmlMapping.Loyalty
{
    // 
    public class ContactXml : BaseXml
    {
        //Request IDs
        private const string RequestId = "GET_MEMBER_EXT_INFO";

        public ContactXml() 
        {
        }

        public string RequestXML(string cardId,int numberOfTransReturned)
        {
            #region xml
            /*
            <Request>
            <Request_ID>GET_MEMBER_EXT_INFO</Request_ID>
            <Request_Body>
	            <Card_No>10001</Card_No>
                <Number_Of_Trans>5</Number_Of_Trans>
            </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", RequestId),
                    new XElement("Request_Body",
                        new XElement("Card_No", cardId),
                        new XElement("Number_Of_Trans", numberOfTransReturned.ToString())
                    )
                );
             ;
             XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
             doc.Add(root);
             return doc.ToString();
        }
        
        public MemberContact ResponseXML(string responseXml)
        {
            #region xml
            /*
                <Response>
                  <Request_ID>GET_MEMBER_EXT_INFO</Request_ID>
                  <Response_Code>0000</Response_Code>
                  <Response_Text></Response_Text>
                  <Response_Body>
                    <Membership_Card>
                      <Card_No.>10027</Card_No.>
                      <Status Options="Free,Allocated,Active,Blocked">2</Status>
                      <Linked_to_Account>true</Linked_to_Account>
                      <Club_Code>CRONUS</Club_Code>
                      <Scheme_Code>CR1-BRONZE</Scheme_Code>
                      <Account_No.>MA000012</Account_No.>
                      <Contact_No.>MO000014</Contact_No.>
                      <First_Date_Used>2014-04-11</First_Date_Used>
                      <Last_Valid_Date>2015-04-11</Last_Valid_Date>
                      <Reason_Blocked />
                      <Date_Blocked />
                      <Blocked_by />
                      <Date_Created>2014-04-11</Date_Created>
                    </Membership_Card>
                    <Member_Account>
                      <No.>MA000012</No.>
                      <Status Options="Unassigned,Active,Closed">1</Status>
                      <Account_Type Options="Private,Family,Company">0</Account_Type>
                      <Description>fannar magnusson</Description>
                      <Linked_To_Customer_No. />
                      <Date_Activated>2014-04-11</Date_Activated>
                      <Activated_By>LSRETAIL\KASSI</Activated_By>
                      <Club_Code>CRONUS</Club_Code>
                      <Scheme_Code>CR1-BRONZE</Scheme_Code>
                      <Price_Group />
                      <Cust._Disc._Group />
                      <No._Series>R-MEM-ACCO</No._Series>
                      <Expiration_Period_Type Options="Current Week,Next Week,Current Month,Next Month">0</Expiration_Period_Type>
                      <Sales_Current_Year>0</Sales_Current_Year>
                      <Last_Sales_Date />
                      <Blocked>false</Blocked>
                      <Reason_Blocked />
                      <Date_Blocked />
                      <Blocked_By />
                      <Created_Date>2014-04-11</Created_Date>
                      <Created_By>LSRETAIL\KASSI</Created_By>
                      <Balance>200</Balance>
                      <Unprocessed_Points>100</Unprocessed_Points>
                    </Member_Account>
                    <Member_Contact>
                      <Account_No.>MA000012</Account_No.>
                      <Club_Code>CRONUS</Club_Code>
                      <Scheme_Code>CR1-BRONZE</Scheme_Code>
                      <Contact_Type Options="Active,Information Only">0</Contact_Type>
                      <Contact_No.>MO000014</Contact_No.>
                      <Main_Contact>true</Main_Contact>
                      <Name>fannar magnusson</Name>
                      <Search_Name>FANNAR MAGNUSSON</Search_Name>
                      <Name_2 />
                      <Address />
                      <Address_2 />
                      <City />
                      <Post_Code />
                      <E-Mail>tttt@ttt.tt</E-Mail>
                      <Home_Page />
                      <Phone_No.>333444</Phone_No.>
                      <Mobile_Phone_No.>333444555</Mobile_Phone_No.>
                      <Territory_Code />
                      <Picture />
                      <County />
                      <Country />
                      <Gender Options=" ,Male,Female">0</Gender>
                      <Date_of_Birth />
                      <Birthday />
                      <Marital_Status Options=" ,Single,Married,Divorced,Widowed">0</Marital_Status>
                      <Blocked>false</Blocked>
                      <Reason_Blocked />
                      <Date_Blocked />
                      <Blocked_by />
                      <Created_Date>2014-04-11</Created_Date>
                      <Created_by>LSRETAIL\KASSI</Created_by>
                      <No._Series>R-MEM-CONT</No._Series>
                      <External_ID />
                      <External_System />
                      <First_Name>fannar</First_Name>
                      <Middle_Name />
                      <Surname>magnusson</Surname>
                      <Salutation_Code />
                      <Search_E-Mail>TTTT@TTT.TT</Search_E-Mail>
                    </Member_Contact>
                    <Member_Attribute_List>
                      <Type Options="Attribute,Discount Limitation,Action">0</Type>
                      <Code>BIRTHDAY</Code>
                      <Description>Birthday</Description>
                      <Status Options="Solved,Pending,Blocked">0</Status>
                      <Value>No       </Value>
                      <Action_Type Options="Message,Text On Receipt">0</Action_Type>
                      <Limitation_Type Options="None,Discount Amount,No. of Times Triggered">0</Limitation_Type>
                    </Member_Attribute_List>
                    <Published_Offer>
                      <No.>PUB0007</No.>
                      <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">9</Discount_Type>
                      <Discount_No.>COUP0110</Discount_No.>
                      <Description>Special Trophy offer - 25% off</Description>
                      <Status Options="Disabled,Enabled">1</Status>
                      <Offer_Category Options="General,Special Member,Points and Coupons">2</Offer_Category>
                      <Primary_Text>Special Trophy offer - 25% off</Primary_Text>
                      <Secondary_Text>Great deal on Trophies, both large and small sizes</Secondary_Text>
                      <Picture />
                      <Coupon_Code />
                      <Coupon_Qty_Needed>0</Coupon_Qty_Needed>
                      <Points_Needed>0</Points_Needed>
                      <Validation_Period_ID>8</Validation_Period_ID>
                      <Member_Type Options="Scheme,Club">1</Member_Type>
                      <Member_Value>CRONUS</Member_Value>
                      <Member_Attribute />
                      <Member_Attribute_Value />
                      <No._Series>R-PUBOFFER</No._Series>
                      <Web_Link />
                      <Disclaimer />
                      <Personal_E-Mail>false</Personal_E-Mail>
                      <HTML_E-Mail />
                      <Send_HTML>false</Send_HTML>
                    </Published_Offer>
                    <Published_Offer_Detail_Line>
                      <Offer_No.>PUB0007</Offer_No.>
                      <Line_No.>10000</Line_No.>
                      <Description>25% off on small trophies</Description>
                      <Picture />
                    </Published_Offer_Detail_Line>
                    <Published_Offer_Detail_Line>
                      <Offer_No.>PUB0007</Offer_No.>
                      <Line_No.>20000</Line_No.>
                      <Description>25% off on large trophies</Description>
                      <Picture />
                    </Published_Offer_Detail_Line>
                  </Response_Body>
                </Response>
             * */
            #endregion

            MemberContact contact = new MemberContact();
            contact.Account = new Account();
            contact.Card = new Card();
            contact.Addresses = new List<Address>();
            Address address = new Address();
            Card card = new Card();
            Account account = new Account();
            Club club = new Club();

            contact.PublishedOffers = new List<PublishedOffer>();
            contact.Transactions = new List<LoyTransaction>();

            XDocument doc = XDocument.Parse(responseXml);
            XElement contactEl = doc.Element("Response").Element("Response_Body").Element("Member_Contact");
            XElement acctEl = doc.Element("Response").Element("Response_Body").Element("Member_Account");
            XElement cardEl = doc.Element("Response").Element("Response_Body").Element("Membership_Card");


            if (contactEl.Element("Contact_No.") == null)
                throw new XmlException("Contact_No. node not found in response xml");
            contact.Id = contactEl.Element("Contact_No.").Value;

            if (contactEl.Element("E-Mail") == null)
                throw new XmlException("E-Mail node not found in response xml");
            contact.Email = contactEl.Element("E-Mail").Value;

            if (contactEl.Element("First_Name") == null)
                throw new XmlException("First_Name node not found in response xml");
            contact.FirstName = contactEl.Element("First_Name").Value;

            if (contactEl.Element("Surname") == null)
                throw new XmlException("Surname node not found in response xml");
            contact.LastName = contactEl.Element("Surname").Value;

            if (contactEl.Element("Middle_Name") == null)
                throw new XmlException("Middle_Name node not found in response xml");
            contact.MiddleName = contactEl.Element("Middle_Name").Value;

            if (contactEl.Element("Gender") == null)
                throw new XmlException("Gender node not found in response xml");
            if (!string.IsNullOrWhiteSpace(contactEl.Element("Gender").Value))
                contact.Gender = (Gender)ConvertTo.SafeInt(contactEl.Element("Gender").Value);

            if (contactEl.Element("Marital_Status") == null)
                throw new XmlException("Marital_Status node not found in response xml");
            if (!string.IsNullOrWhiteSpace(contactEl.Element("Marital_Status").Value))
                contact.MaritalStatus = (MaritalStatus)ConvertTo.SafeInt(contactEl.Element("Marital_Status").Value);

            if (contactEl.Element("Phone_No.") == null)
                throw new XmlException("Phone_No. node not found in response xml");
            contact.Phone = contactEl.Element("Phone_No.").Value;

            if (contactEl.Element("Mobile_Phone_No.") == null)
                throw new XmlException("Mobile_Phone_No. node not found in response xml");
            contact.MobilePhone = contactEl.Element("Mobile_Phone_No.").Value;

            if (contactEl.Element("Date_of_Birth") == null)
                throw new XmlException("Date_of_Birth node not found in response xml");
            if (!string.IsNullOrWhiteSpace(contactEl.Element("Date_of_Birth").Value))
                contact.BirthDay = ConvertTo.SafeDateTime(contactEl.Element("Date_of_Birth").Value);

            // Address 
            address.Type = AddressType.Residential;
            if (contactEl.Element("Address") == null)
                throw new XmlException("Address node not found in response xml");
            address.Address1 = contactEl.Element("Address").Value;

            if (contactEl.Element("Address_2") == null)
                throw new XmlException("Address_2 node not found in response xml");
            address.Address2 = contactEl.Element("Address_2").Value;

            if (contactEl.Element("City") == null)
                throw new XmlException("City node not found in response xml");
            address.City = contactEl.Element("City").Value;

            if (contactEl.Element("Post_Code") == null)
                throw new XmlException("Post_Code node not found in response xml");
            address.PostCode = contactEl.Element("Post_Code").Value;

            if (contactEl.Element("Territory_Code") == null)
                throw new XmlException("Territory_Code node not found in response xml");
            address.StateProvinceRegion = contactEl.Element("Territory_Code").Value;

            if (contactEl.Element("Country") != null)
                address.Country = contactEl.Element("Country").Value;

            // Account data
            if (acctEl.Element("No.") == null)
                throw new XmlException("No. node not found in response xml");
            account.Id = acctEl.Element("No.").Value;

            if (acctEl.Element("Scheme_Code") == null)
                throw new XmlException("Scheme_Code node not found in response xml");
            account.Scheme =  new Scheme(acctEl.Element("Scheme_Code").Value);

            if (acctEl.Element("Club_Code") == null)
                throw new XmlException("Club_Code node not found in response xml");
            account.Scheme.Club = new Club(acctEl.Element("Club_Code").Value);

            //NOTE total remaining points is the sum of Balance and Unprocessed_Points
            if (acctEl.Element("Balance") == null)
                throw new XmlException("Balance node not found in response xml");
            if (!string.IsNullOrWhiteSpace(acctEl.Element("Balance").Value))
                account.PointBalance = Convert.ToInt64(Math.Floor(ConvertTo.SafeDecimal(acctEl.Element("Balance").Value)));

            if (acctEl.Element("Unprocessed_Points") == null)
                throw new XmlException("Unprocessed_Points node not found in response xml");
            if (!string.IsNullOrWhiteSpace(acctEl.Element("Unprocessed_Points").Value))
                account.PointBalance += Convert.ToInt64(Math.Floor(ConvertTo.SafeDecimal(acctEl.Element("Unprocessed_Points").Value)));//added 

            // Card data
            if (cardEl.Element("Card_No.") == null)
                throw new XmlException("Card_No. node not found in response xml");
            card.Id = cardEl.Element("Card_No.").Value;

            if (cardEl.Element("Blocked_by") == null)
                throw new XmlException("Blocked_by node not found in response xml");
            card.BlockedBy = cardEl.Element("Blocked_by").Value;

            if (cardEl.Element("Reason_Blocked") == null)
                throw new XmlException("Reason_Blocked node not found in response xml");
            card.BlockedReason = cardEl.Element("Reason_Blocked").Value;

            if (cardEl.Element("Date_Blocked") == null)
                throw new XmlException("Date_Blocked node not found in response xml");
            if (!string.IsNullOrWhiteSpace(cardEl.Element("Date_Blocked").Value))
                card.DateBlocked = ConvertTo.SafeDateTime(cardEl.Element("Date_Blocked").Value);

            if (cardEl.Element("Linked_to_Account") == null)
                throw new XmlException("Linked_to_Account node not found in response xml");
            if (!string.IsNullOrWhiteSpace(cardEl.Element("Linked_to_Account").Value))
                card.LinkedToAccount = ToBool(cardEl.Element("Linked_to_Account").Value);

            if (cardEl.Element("Status") == null)
                throw new XmlException("Status node not found in response xml");
            if (!string.IsNullOrWhiteSpace(cardEl.Element("Status").Value))
                card.Status = (CardStatus)ConvertTo.SafeInt(cardEl.Element("Status").Value);
            contact.Card = card;
            contact.Account = account;

            //now loop thru the lines
            var attrList = doc.Element("Response").Element("Response_Body").Descendants("Member_Attribute_List");
            foreach (XElement attr in attrList)
            {
                Profile profile = new Profile();
                if (attr.Element("Type") == null)
                    throw new XmlException("Type node not found in response xml");
                string type = attr.Element("Type").Value; //Attribute,Discount Limitation,Action
                //only take the attributes, ignore the others
                if (type != "0")
                    continue;
                // 
                if (attr.Element("Code") == null)
                    throw new XmlException("Code node not found in response xml");
                profile.Id = attr.Element("Code").Value;

                if (attr.Element("Description") == null)
                    throw new XmlException("Description node not found in response xml");
                profile.Description = attr.Element("Description").Value;

                if (attr.Element("Value") == null)
                    throw new XmlException("Value node not found in response xml");
                profile.DefaultValue = attr.Element("Value").Value;

                contact.Profiles.Add(profile);
            }
            //now loop thru the published offer lines
            var pubOffer = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer");
            foreach (XElement pubOff in pubOffer)
            {
                if (pubOff.Element("Status") == null)
                    throw new XmlException("StatusNo. not found in response xml");
                string status = pubOff.Element("Status").Value;
                //skip disabled status
                if (status == "0")
                    continue;

                OfferDiscountType dtype = OfferDiscountType.Coupon;
                
                if (pubOff.Element("No.") == null)
                    throw new XmlException("No. not found in response xml");
                string offerNumber = pubOff.Element("No.").Value;

                if (pubOff.Element("Discount_Type") == null)
                    throw new XmlException("Discount_Type not found in response xml");
                dtype = (OfferDiscountType)ConvertTo.SafeInt(pubOff.Element("Discount_Type").Value);

                //if (pubOff.Element("Discount_No.") == null)
                //    throw new XmlException("Discount_No. not found in response xml");
                //string id = pubOff.Element("Discount_No.").Value;

                if (pubOff.Element("Primary_Text") == null)
                    throw new XmlException("Primary_Text not found in response xml");
                string primaryText = pubOff.Element("Primary_Text").Value;

                if (pubOff.Element("Secondary_Text") == null)
                    throw new XmlException("Secondary_Text not found in response xml");
                string secondaryText = pubOff.Element("Secondary_Text").Value;

                if (dtype == OfferDiscountType.Coupon)
                {
                    /*
                    Coupon coupon = new Coupon();
                    coupon.Id = offerNumber;
                    coupon.PrimaryText = primaryText;
                    coupon.SecondaryText = secondaryText;

                    //coupon.ExpiryDate  TODO   Validation_Period_ID

                    //now loop thru the published offer detail lines
                    var pubOfferLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Detail_Line");
                    foreach (XElement pubOffLine in pubOfferLines)
                    {
                        if (pubOffLine.Element("Offer_No.") == null)
                            throw new XmlException("Offer_No. not found in response xml");
                        string offNo = pubOffLine.Element("Offer_No.").Value;
                        if (pubOffLine.Element("Description") == null)
                            throw new XmlException("Description not found in response xml");
                        string desc = pubOffLine.Element("Description").Value;

                        if (offerNumber == offNo)
                        {
                            if (coupon.Details == null)
                                coupon.Details = new List<CouponDetails>();
                            CouponDetails details = new CouponDetails();
                            details.CouponId = coupon.Id;
                            details.Description = desc;
                            coupon.Details.Add(details);
                        }
                    }
                    contact.Coupons.Add(coupon);
                    */
                }
                else
                {
                    //else this is an offer
                    PublishedOffer offer = new PublishedOffer();
                    offer.Id = offerNumber;
                    offer.Description = primaryText;
                    offer.Details = secondaryText;

                    if (pubOff.Element("Offer_Category") == null)
                        throw new XmlException("Offer_Category not found in response xml");
                    offer.Type = (OfferType)ConvertTo.SafeInt(pubOff.Element("Offer_Category").Value);

                    //offer.ExpiryDate  TODO   Validation_Period_ID

                    //now loop thru the published offer detail lines
                    var pubOfferLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Detail_Line");
                    foreach (XElement pubOffLine in pubOfferLines)
                    {
                        if (pubOffLine.Element("Offer_No.") == null)
                            throw new XmlException("Offer_No. not found in response xml");
                        string offNo = pubOffLine.Element("Offer_No.").Value;
                        if (pubOffLine.Element("Description") == null)
                            throw new XmlException("Description not found in response xml");
                        string desc = pubOffLine.Element("Description").Value;

                        if (offerNumber == offNo)
                        {
                            if (offer.OfferDetails == null)
                                offer.OfferDetails = new List<OfferDetails>();
                            OfferDetails details = new OfferDetails();
                            details.OfferId = offer.Id;
                            details.Description = desc;
                            offer.OfferDetails.Add(details);
                        }
                    }
                    contact.PublishedOffers.Add(offer);
                }

            }
            //group by transactionIds 
            var transList = doc.Element("Response").Element("Response_Body").Descendants("MobileTransaction");
            foreach (XElement trs in transList)
            {
                LoyTransaction trans = new LoyTransaction();
                //the guid Id
                if (trs.Element("Id") == null)
                    throw new XmlException("Id node not found in response xml");
                string guidId= trs.Element("Id").Value;

                if (trs.Element("StoreId") == null)
                    throw new XmlException("StoreId node not found in response xml");
                trans.Store = new Store(trs.Element("StoreId").Value);

                if (trs.Element("TerminalId") == null)
                    throw new XmlException("TerminalId node not found in response xml");
                trans.Terminal = trs.Element("TerminalId").Value;

                if (trs.Element("StaffId") == null)
                    throw new XmlException("StaffId node not found in response xml");
                trans.Staff = trs.Element("StaffId").Value;

                if (trs.Element("ReceiptNo") == null)
                    throw new XmlException("ReceiptNo node not found in response xml");
                trans.ReceiptNumber = trs.Element("ReceiptNo").Value;

                if (trs.Element("TransactionNo") == null)
                    throw new XmlException("TransactionNo node not found in response xml");
                trans.Id = trs.Element("TransactionNo").Value;

                if (trs.Element("TransDate") == null)
                    throw new XmlException("TransDate node not found in response xml");
                trans.Date = ConvertTo.SafeDateTime(trs.Element("TransDate").Value);

                decimal vatAmount = 0M;
                decimal netAmount = 0M;
                decimal discountAmount = 0M;

                //now loop thru the published offer detail lines
                var transLines = doc.Element("Response").Element("Response_Body").Descendants("MobileTransactionLine");
                foreach (XElement trsLine in transLines)
                {
                    //the guid Id
                    if (trsLine.Element("Id") == null)
                        throw new XmlException("Id node not found in response xml");
                    string lineGuidId = trsLine.Element("Id").Value;
                    if (guidId != lineGuidId)
                        continue;

                    if (trsLine.Element("LineType") == null)
                        throw new XmlException("LineType node not found in response xml");
                    LineType lineType = (LineType)ConvertTo.SafeInt(trsLine.Element("LineType").Value);
                    if (lineType == LineType.Payment)
                    {
                        LoyTenderLine tl = new LoyTenderLine();
                        if (trsLine.Element("StoreId") == null)
                            throw new XmlException("StoreId node not found in response xml");
                        tl.StoreId = trsLine.Element("StoreId").Value;

                        if (trsLine.Element("TerminalId") == null)
                            throw new XmlException("TerminalId node not found in response xml");
                        tl.TerminalId = trsLine.Element("TerminalId").Value;

                        if (trsLine.Element("LineNo") == null)
                            throw new XmlException("LineNo node not found in response xml");
                        tl.Id = trsLine.Element("LineNo").Value;

                        if (trsLine.Element("NetAmount") == null)
                            throw new XmlException("NetAmount node not found in response xml");
                        tl.Amt = Math.Abs(ConvertTo.SafeDecimal(trsLine.Element("NetAmount").Value));

                        if (trsLine.Element("TransactionNo") == null)
                            throw new XmlException("TransactionNo node not found in response xml");
                        tl.TransactionId = trsLine.Element("TransactionNo").Value;

                        if (trsLine.Element("Number") == null)
                            throw new XmlException("Number node not found in response xml");
                        tl.Type = trsLine.Element("Number").Value;

                        //TODO remove this text mapping to tender type
                        if (trsLine.Element("Tender_Description") == null)
                        {
                            if (trsLine.Element("TenderDescription") == null)
                                throw new XmlException("Tender_Description or TenderDescription node not found in response xml");
                            tl.Description = trsLine.Element("TenderDescription").Value;
                        }
                        else
                            tl.Description = trsLine.Element("Tender_Description").Value;

                        trans.TenderLines.Add(tl);
                    }
                    else
                    {
                        LoySaleLine sl = new LoySaleLine();
                        if (trsLine.Element("StoreId") == null)
                            throw new XmlException("StoreId node not found in response xml");
                        sl.StoreId = trsLine.Element("StoreId").Value;

                        if (trsLine.Element("TerminalId") == null)
                            throw new XmlException("TerminalId node not found in response xml");
                        sl.TerminalId = trsLine.Element("TerminalId").Value;

                        if (trsLine.Element("LineNo") == null)
                            throw new XmlException("LineNo node not found in response xml");
                        sl.Id = trsLine.Element("LineNo").Value;

                        if (trsLine.Element("TransactionNo") == null)
                            throw new XmlException("TransactionNo node not found in response xml");
                        sl.TransactionId = trsLine.Element("TransactionNo").Value;

                        if (trsLine.Element("Number") == null)
                            throw new XmlException("Number node not found in response xml");
                        sl.Item = new LoyItem(trsLine.Element("Number").Value);

                        if (trsLine.Element("VariantCode") == null)
                            throw new XmlException("VariantCode node not found in response xml");
                        sl.VariantReg = new VariantRegistration(trsLine.Element("VariantCode").Value);

                        if (trsLine.Element("UomId") == null)
                            throw new XmlException("UomId node not found in response xml");
                        sl.Uom = new UnitOfMeasure(trsLine.Element("UomId").Value, sl.Item.Id);

                        if (trsLine.Element("Item_Description") == null)
                        {
                            if (trsLine.Element("ItemDescription") == null)
                                throw new XmlException("Item_Description or ItemDescription node not found in response xml");
                            sl.Item.Description = trsLine.Element("ItemDescription").Value;
                        }
                        else
                            sl.Item.Description = trsLine.Element("Item_Description").Value;

                        if (trsLine.Element("Quantity") == null)
                            throw new XmlException("Quantity node not found in response xml");
                        sl.Quantity = Math.Abs(ConvertTo.SafeInt(trsLine.Element("Quantity").Value));

                        if (trsLine.Element("DiscountAmount") == null)
                            throw new XmlException("DiscountAmount node not found in response xml");
                        sl.DiscountAmt = Math.Abs(ConvertTo.SafeDecimal(trsLine.Element("DiscountAmount").Value));

                        if (trsLine.Element("NetAmount") == null)
                            throw new XmlException("NetAmount node not found in response xml");
                        sl.NetAmt = Math.Abs(ConvertTo.SafeDecimal(trsLine.Element("NetAmount").Value));

                        if (trsLine.Element("TAXAmount") == null)
                            throw new XmlException("TAXAmount node not found in response xml");
                        sl.VatAmt = Math.Abs(ConvertTo.SafeDecimal(trsLine.Element("TAXAmount").Value));

                        sl.Amount = sl.NetAmount + sl.VatAmount;
                        netAmount += sl.NetAmt;
                        vatAmount += sl.VatAmt;
                        discountAmount += sl.DiscountAmt;

                        trans.SaleLines.Add(sl);
                    }
                }
                trans.Amt = netAmount + vatAmount;
                trans.NetAmt = netAmount;
                trans.VatAmt = vatAmount;
                trans.DiscountAmt = discountAmount;
                contact.Transactions.Add(trans);
            }

            contact.Addresses.Add(address);
            return contact;
        }
    }
}
