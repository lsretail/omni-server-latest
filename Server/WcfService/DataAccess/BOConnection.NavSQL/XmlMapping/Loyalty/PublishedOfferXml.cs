using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.XmlMapping.Loyalty
{
    public class PublishedOfferXml : BaseXml
    {
        private const string ItemsRequestId = "LOAD_PUBLISHED_OFFER_ITEMS";
        private const string OffersRequestId = "LOAD_PUBOFFERS_AND_PERSCOUPONS";
        private const string MemberDirMarkInfoRequestId = "LOAD_MEMBER_DIR_MARK_INFO"; //LS Nav 9.00.05, replaces  LOAD_PUBOFFERS_AND_PERSCOUPONS

        public PublishedOfferXml()
        {
        }

        public string PublishedOfferItemsRequestXML(string pubOfferId, int numberOfItems)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>LOAD_PUBLISHED_OFFER_ITEMS</Request_ID>
              <Request_Body>
                <No>PUB0007</No>
                <MaxNoOfRows>10</MaxNoOfRows>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", ItemsRequestId),
                    new XElement("Request_Body",
                        new XElement("No", pubOfferId),
                        new XElement("MaxNoOfRows", numberOfItems.ToString())
                    )
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);

            return doc.ToString();
        }

        public List<LoyItem> PublishedOfferItemsResponseXML(string responseXml)
        {
            #region xml
            /*
            <Response>
              <Request_ID>LOAD_PUBLISHED_OFFER_ITEMS</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Offer_Item_Buffer>
                  <Item_No.>54400</Item_No.>
                  <Item_Description>Trophy - Small</Item_Description>
                  <Image_ID>54400</Image_ID>
                  <Total_No._Of_Entries>2</Total_No._Of_Entries>
                </Offer_Item_Buffer>
                <Offer_Item_Buffer>
                  <Item_No.>54410</Item_No.>
                  <Item_Description>Trophy - Big</Item_Description>
                  <Image_ID>54410</Image_ID>
                  <Total_No._Of_Entries>2</Total_No._Of_Entries>
                </Offer_Item_Buffer>
              </Response_Body>
            </Response>

             */
            #endregion

            List<LoyItem> rs = new List<LoyItem>();

            XDocument doc = XDocument.Parse(responseXml);
            var itemBufferLines = doc.Element("Response").Element("Response_Body").Descendants("Offer_Item_Buffer");
            foreach (XElement line in itemBufferLines)
            {
                LoyItem item = new LoyItem();
                if (line.Element("Item_No.") == null)
                    throw new XmlException("Item_No. node not found in response xml");
                item.Id = line.Element("Item_No.").Value;

                if (line.Element("Item_Description") == null)
                    throw new XmlException("Item_Description node not found in response xml");
                item.Description = line.Element("Item_Description").Value;

                if (line.Element("Image_ID") == null)
                    throw new XmlException("Image_ID node not found in response xml");
                item.Images = new List<ImageView>();
                item.Images.Add(new ImageView() { Id = line.Element("Image_ID").Value });

                rs.Add(item);
            }
            return rs;
        }

        public string PublishedOfferRequestXML(string cardId, string itemId, string storeId = "")
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>LOAD_PUBOFFERS_AND_PERSCOUPONS</Request_ID>
              <Request_Body>
                <CardID>10021</CardID>
                <ItemNo>54400</ItemNo>
                <StoreNo>S0013</StoreNo>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", OffersRequestId),
                    new XElement("Request_Body",
                        new XElement("CardID", cardId),
                        new XElement("ItemNo", itemId),
                        new XElement("StoreNo", storeId)
                    )
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);

            return doc.ToString();
        }

        public List<PublishedOffer> PublishedOfferResponseXML(string responseXml)
        {
            #region xml
            /*
            <Response>
              <Request_ID>LOAD_PUBOFFERS_AND_PERSCOUPONS</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Published_Offer>
                  <No.>PUB0007</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">9</Discount_Type>
                  <Discount_No.>COUP0110</Discount_No.>
                  <Description>Special Trophy offer - 25% off</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                  <Primary_Text>Special Trophy offer - 25% off</Primary_Text>
                  <Secondary_Text>Great deal on Trophies, both large and small sizes</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-TROPHY</ImageIDIntUse>
                  <Ending_Date>2019-12-31</Ending_Date>
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0008</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">9</Discount_Type>
                  <Discount_No.>COUP0111</Discount_No.>
                  <Description>Dining table(6pers) 50 off </Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                  <Primary_Text>Dining table(6pers) - Amount 50 off</Primary_Text>
                  <Secondary_Text>A great deal on this 6 person dining table in Cronus stores</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-TABLE</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0033</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">4</Discount_Type>
                  <Discount_No.>P1053</Discount_No.>
                  <Description>10 off when you buy 3 T shirts</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">1</Offer_Category>
                  <Primary_Text>T shirts offer - buy 3 and get discount</Primary_Text>
                  <Secondary_Text>Valid for our Cronus Club members
            Buy 3 Micky Mouse T shirts and get great discounts</Secondary_Text>
                  <ImageIDIntUse>41010</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0035</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">7</Discount_Type>
                  <Discount_No.>P1050</Discount_No.>
                  <Description>Item Point Offer Dress Coat</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                  <Primary_Text>Dress Coat - use points for discount</Primary_Text>
                  <Secondary_Text>Special offer for CRONUS club members</Secondary_Text>
                  <ImageIDIntUse>40055</ImageIDIntUse>
                  <Ending_Date>2019-12-31</Ending_Date>
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0036</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">7</Discount_Type>
                  <Discount_No.>P1041</Discount_No.>
                  <Description>Golf Trolley offer</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                  <Primary_Text>Golf Trolley offer</Primary_Text>
                  <Secondary_Text>For only 25 points you get a discount on golf trolleys</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-GOLFTROLLEY</ImageIDIntUse>
                  <Ending_Date>2019-12-31</Ending_Date>
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0037</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                  <Discount_No.>S10018</Discount_No.>
                  <Description>Parma Ham and Melon</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                  <Primary_Text>Parma Ham and Melon</Primary_Text>
                  <Secondary_Text>Very good deal this month. We use superior quality Ham with cantaloupe Melon - together they make a perfect starter or can be taken as a light course on its own</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-PARMAHAM</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0038</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                  <Discount_No.>S10017</Discount_No.>
                  <Description>Chicken, Salad and Fries</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                  <Primary_Text>Chicken, Salad and Fries</Primary_Text>
                  <Secondary_Text>For only 9,50. Grilled Chicken marinated with our own recipe rich with a tasty flavor. Served with fresh salad, steak fries and our classic barbeque sauce</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-CHICKENS</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0039</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                  <Discount_No.>S10011</Discount_No.>
                  <Description>Coffee and Cake</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                  <Primary_Text>Coffee and Cake</Primary_Text>
                  <Secondary_Text>This week’s special.  Our favorite coffee and with a slice of cake. Look at the cake's selections of the day, always fresh and delicious</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-COFFEECA</ImageIDIntUse>
                  <Ending_Date>2019-12-31</Ending_Date>
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0040</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                  <Discount_No.>S10010</Discount_No.>
                  <Description>Coffee-Croiss/Muff</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                  <Primary_Text>Coffee-Croiss/Muff</Primary_Text>
                  <Secondary_Text>Buy coffee and croissant or muffin for a very special price this week. An irresistible offer for those who love coffee and something delicious with it.</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-COFFEECM</ImageIDIntUse>
                  <Ending_Date>2019-12-31</Ending_Date>
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0042</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">3</Discount_Type>
                  <Discount_No.>P1056</Discount_No.>
                  <Description>Burger and Diary -25% discount</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                  <Primary_Text>Burger and Diary -25% discount</Primary_Text>
                  <Secondary_Text>When you buy our standard delicious burger and our special appointment diary you get 25 % discount.
            This offer is valid in all our stores for the next 2 months</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-BURGERDIARY</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0045</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">3</Discount_Type>
                  <Discount_No.>P1054</Discount_No.>
                  <Description>Shoe offer - Both types get %</Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">1</Offer_Category>
                  <Primary_Text>Shoe offer - Both types get %</Primary_Text>
                  <Secondary_Text>Receive 12 % discount when you buy one pair of leather shoes and sneakers</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-SHOES</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer>
                  <No.>PUB0046</No.>
                  <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">3</Discount_Type>
                  <Discount_No.>P1055</Discount_No.>
                  <Description>Magazine and Coffee - Special </Description>
                  <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                  <Primary_Text>Magazine and Coffee - Special discount</Primary_Text>
                  <Secondary_Text>Special offer of the month. Buy one magazine and a cup of coffee and receive 15% in discount</Secondary_Text>
                  <ImageIDIntUse>PUBOFF-MAGAZINECOF2</ImageIDIntUse>
                  <Ending_Date />
                </Published_Offer>
                <Published_Offer_Detail_Line>
                  <Offer_No.>PUB0007</Offer_No.>
                  <Line_No.>10000</Line_No.>
                  <Description>25% off on small trophies</Description>
                  <ImageIDIntUse>54400</ImageIDIntUse>
                </Published_Offer_Detail_Line>
                <Published_Offer_Detail_Line>
                  <Offer_No.>PUB0007</Offer_No.>
                  <Line_No.>20000</Line_No.>
                  <Description>25% off on large trophies</Description>
                  <ImageIDIntUse>54410</ImageIDIntUse>
                </Published_Offer_Detail_Line>
                <Published_Offer_Detail_Line>
                  <Offer_No.>PUB0008</Offer_No.>
                  <Line_No.>10000</Line_No.>
                  <Description>Dining Table for 6 persons</Description>
                  <ImageIDIntUse>65000</ImageIDIntUse>
                </Published_Offer_Detail_Line>
              </Response_Body>
            </Response>

             */
            #endregion

            List<PublishedOffer> rs = new List<PublishedOffer>();

            XDocument doc = XDocument.Parse(responseXml);
            var itemBufferLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer");
            foreach (XElement line in itemBufferLines)
            {

                if (line.Element("No.") == null)
                    throw new XmlException("No. node not found in response xml");
                PublishedOffer po = new PublishedOffer(line.Element("No.").Value);

                if (line.Element("Primary_Text") == null)
                    throw new XmlException("Primary_Text node not found in response xml");
                po.Description = line.Element("Primary_Text").Value;

                if (line.Element("Secondary_Text") == null)
                    throw new XmlException("Secondary_Text node not found in response xml");
                po.Details = line.Element("Secondary_Text").Value;

                if (line.Element("ImageIDIntUse") == null)
                    throw new XmlException("ImageIDIntUse node not found in response xml");
                po.Images = new List<ImageView>();
                po.Images.Add(new ImageView() { Id = line.Element("ImageIDIntUse").Value });

                if (line.Element("Ending_Date") == null)
                    throw new XmlException("Ending_Date node not found in response xml");
                if (string.IsNullOrWhiteSpace(line.Element("Ending_Date").Value))
                    po.ExpirationDate = ConvertTo.SafeDateTime(line.Element("Ending_Date").Value);
                else
                    po.ExpirationDate = DateTime.Now.AddYears(10); //expires in the future

                if (line.Element("Discount_No.") == null)
                    throw new XmlException("Discount_No. node not found in response xml");
                po.OfferId = line.Element("Discount_No.").Value;

                if (line.Element("Discount_Type") == null)
                    throw new XmlException("Discount_Type node not found in response xml");
                po.Code = (OfferDiscountType)Convert.ToInt32(line.Element("Discount_Type").Value);

                if (line.Element("Offer_Category") == null)
                    throw new XmlException("Offer_Category node not found in response xml");
                po.Type = (OfferType)Convert.ToInt32(line.Element("Offer_Category").Value);

                po.OfferDetails = GetOfferDetails(doc, po.Id);
                rs.Add(po);
            }
            return rs;
        }

        private List<OfferDetails> GetOfferDetails(XDocument doc, string id)
        {
            List<OfferDetails> list = new List<OfferDetails>();
            var detailsLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Detail_Line");
            foreach (XElement line in detailsLines)
            {
                if (line.Element("Offer_No.") == null)
                    throw new XmlException("Offer_No. node not found in response xml");
                if (id == line.Element("Offer_No.").Value)
                {
                    OfferDetails d = new OfferDetails();
                    if (line.Element("Description") == null)
                        throw new XmlException("Descriptionnode not found in response xml");
                    d.Description = line.Element("Description").Value;

                    if (line.Element("ImageIDIntUse") == null)
                        throw new XmlException("ImageIDIntUse node not found in response xml");
                    d.Image = new ImageView(line.Element("ImageIDIntUse").Value);

                    list.Add(d);
                }
            }

            return list;
        }

        public string PublishedOfferMemberRequestXML(string cardId, string itemId, string storeId)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>LOAD_MEMBER_DIR_MARK_INFO</Request_ID>
              <Request_Body>
                <CardID>10021</CardID>
                <ItemNo>54400</ItemNo>
                <StoreNo>S0013</StoreNo>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", MemberDirMarkInfoRequestId),
                    new XElement("Request_Body",
                        new XElement("CardID", cardId),
                        new XElement("ItemNo", itemId),
                        new XElement("StoreNo", storeId)
                    )
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);

            return doc.ToString();
        }

        public List<PublishedOffer> PublishedOfferMemberResponseXML(string responseXml)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Response>
                <Request_ID>LOAD_MEMBER_DIR_MARK_INFO</Request_ID>
                <Response_Code>0000</Response_Code>
                <Response_Text></Response_Text>
                <Response_Body>
                    <Published_Offer>
                        <No.>PUB0036</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">7</Discount_Type>
                        <Discount_No.>P1041</Discount_No.>
                        <Description>Golf Trolley offer</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                        <Primary_Text>Golf Trolley offer</Primary_Text>
                        <Secondary_Text>For only 25 points you get a discount on golf trolleys</Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0037</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                        <Discount_No.>S10018</Discount_No.>
                        <Description>Parma Ham and Melon</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                        <Primary_Text>Parma Ham and Melon</Primary_Text>
                        <Secondary_Text>Very good deal this month. We use superior quality Ham with cantaloupe Melon - together they make a perfect starter or can be taken as a light course on its own</Secondary_Text>
                        <Ending_Date />
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0038</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                        <Discount_No.>S10017</Discount_No.>
                        <Description>Chicken, Salad and Fries</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                        <Primary_Text>Chicken, Salad and Fries</Primary_Text>
                        <Secondary_Text>For only 9,50. Grilled Chicken marinated with our own recipe rich with a tasty flavor. Served with fresh salad, steak fries and our classic barbeque sauce</Secondary_Text>
                        <Ending_Date />
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0039</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                        <Discount_No.>S10011</Discount_No.>
                        <Description>Coffee and Cake</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                        <Primary_Text>Coffee and Cake</Primary_Text>
                        <Secondary_Text>This week’s special.  Our favorite coffee and with a slice of cake. Look at the cake's selections of the day, always fresh and delicious</Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0040</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">1</Discount_Type>
                        <Discount_No.>S10010</Discount_No.>
                        <Description>Coffee-Croiss/Muff</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                        <Primary_Text>Coffee-Croiss/Muff</Primary_Text>
                        <Secondary_Text>Buy coffee and croissant or muffin for a very special price this week. An irresistible offer for those who love coffee and something delicious with it.</Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0042</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">3</Discount_Type>
                        <Discount_No.>P1056</Discount_No.>
                        <Description>Burger and Diary -25% discount</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                        <Primary_Text>Burger and Diary -25% discount</Primary_Text>
                        <Secondary_Text>When you buy our standard delicious burger and our special appointment diary you get 25 % discount.
                        This offer is valid in all our stores for the next 2 months</Secondary_Text>
                        <Ending_Date />
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0046</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">3</Discount_Type>
                        <Discount_No.>P1055</Discount_No.>
                        <Description>Magazine and Coffee - Special </Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">0</Offer_Category>
                        <Primary_Text>Magazine and Coffee - Special discount</Primary_Text>
                        <Secondary_Text>Special offer of the month. Buy one magazine and a cup of coffee and receive 15% in discount</Secondary_Text>
                        <Ending_Date />
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0047</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">9</Discount_Type>
                        <Discount_No.>COUP0119</Discount_No.>
                        <Description> 20% off Leather Backpack</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                        <Primary_Text>Leather Backpack -  20% off</Primary_Text>
                        <Secondary_Text>Receive 20% discount when you shop this fabulous bag.
                            This bag is genuine leather, designed by our team.
                        </Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0048</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">9</Discount_Type>
                        <Discount_No.>COUP0120</Discount_No.>
                        <Description>Shoe Offer - 20 off per pair</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                        <Primary_Text>Shoe Offer - Fabulous discount</Primary_Text>
                        <Secondary_Text>When you buy two pairs of these Leather Boots you get 20 off each pair. This is a fantastic offer for these high quality shoes. </Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0049</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">4</Discount_Type>
                        <Discount_No.>P1060</Discount_No.>
                        <Description>Denim on denim discount 15%</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">3</Offer_Category>
                        <Primary_Text>Denim on denim discount 15%</Primary_Text>
                        <Secondary_Text>In all our shops we have the Linda's jeans and shirt on a special offer. 15% of each item. Do not miss this special offer.</Secondary_Text>
                        <Ending_Date />
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0050</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">3</Discount_Type>
                        <Discount_No.>P1062</Discount_No.>
                        <Description>Davi-s line offer - mix</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">3</Offer_Category>
                        <Primary_Text>Davi-s line offer - mix</Primary_Text>
                        <Secondary_Text>Receive 10% discount and a complimentary belt when you buy the following items from the Davi-s line: jacket, jeans and moccasins.</Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer>
                        <No.>PUB0051</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">7</Discount_Type>
                        <Discount_No.>P1063</Discount_No.>
                        <Description>Bag and gloves</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                        <Primary_Text>Bag and gloves</Primary_Text>
                        <Secondary_Text>Use your points to lower your payment of either buying the Linda bag and/or Leather gloves.</Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer_Images>
                        <KeyValue>PUB0036</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-GOLFTROLLEY</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0037</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-PARMAHAM</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0038</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-CHICKENS</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0039</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-COFFEECA</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0040</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-COFFEECM</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0042</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-BURGERDIARY</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0046</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-MAGAZINECOF2</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0047</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-UR-CBAG-OM</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0048</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-UR-CSHOES-OM</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0049</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-UR-DENIM-OM</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0050</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-UR-DAVI-OM</Image_Id>
                    </Published_Offer_Images>
                    <Published_Offer_Images>
                        <KeyValue>PUB0051</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-UR-SPECIAL-OM</Image_Id>
                    </Published_Offer_Images>
                    <Member_Notification>
                        <No.>MN000001</No.>
                        <Contact_No. />
                        <Primary_Text>Remember our regular offers</Primary_Text>
                        <Secondary_Text>Cronus Club Members receive exra discounts</Secondary_Text>
                        <When_Display Options="Always,Once">0</When_Display>
                        <Valid_From_Date>2013-01-01</Valid_From_Date>
                        <Valid_To_Date />
                        <Member_Attribute />
                        <Member_Attribute_Value />
                        <Web_Link />
                        <E-Mail_Disclaimer />
                        <Personalized_E-Mail>false</Personalized_E-Mail>
                        <HTML_E-Mail />
                        <Send_HTML>false</Send_HTML>
                    </Member_Notification>
                    <Member_Notification>
                        <No.>MN000002</No.>
                        <Contact_No. />
                        <Primary_Text>Call our Help Desk for assistance</Primary_Text>
                        <Secondary_Text>Our help desk phone number is (212) 555-9999</Secondary_Text>
                        <When_Display Options="Always,Once">0</When_Display>
                        <Valid_From_Date>2013-01-01</Valid_From_Date>
                        <Valid_To_Date />
                        <Member_Attribute />
                        <Member_Attribute_Value />
                        <Web_Link />
                        <E-Mail_Disclaimer />
                        <Personalized_E-Mail>false</Personalized_E-Mail>
                        <HTML_E-Mail />
                        <Send_HTML>false</Send_HTML>
                    </Member_Notification>
                    <Member_Notification_Images>
                        <KeyValue>MN000001</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>OFFERS</Image_Id>
                    </Member_Notification_Images>
                    <Member_Notification_Images>
                        <KeyValue>MN000002</KeyValue>
                        <Display_Order>1</Display_Order>
                        <Image_Id>HELPDESK</Image_Id>
                    </Member_Notification_Images>
                    <Member_Notification_Images>
                        <KeyValue>MN000002</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>HELPDESK2</Image_Id>
                    </Member_Notification_Images>
                </Response_Body>
            </Response>
              */
            #endregion

            List<PublishedOffer> rs = new List<PublishedOffer>();

            XDocument doc = XDocument.Parse(responseXml);
            var itemBufferLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer");
            foreach (XElement line in itemBufferLines)
            {

                if (line.Element("No.") == null)
                    throw new XmlException("No. node not found in response xml");
                PublishedOffer po = new PublishedOffer(line.Element("No.").Value);

                if (line.Element("Primary_Text") == null)
                    throw new XmlException("Primary_Text node not found in response xml");
                po.Description = line.Element("Primary_Text").Value;

                if (line.Element("Secondary_Text") == null)
                    throw new XmlException("Secondary_Text node not found in response xml");
                po.Details = line.Element("Secondary_Text").Value;

                string imgId = GetOfferMemberImageId(doc, po.Id);
                po.Images = new List<ImageView>();
                if (string.IsNullOrWhiteSpace(imgId) == false)
                    po.Images.Add(new ImageView() { Id = imgId });

                if (line.Element("Ending_Date") == null)
                    throw new XmlException("Ending_Date node not found in response xml");
                if (string.IsNullOrWhiteSpace(line.Element("Ending_Date").Value))
                    po.ExpirationDate = ConvertTo.SafeDateTime(line.Element("Ending_Date").Value);
                else
                    po.ExpirationDate = DateTime.Now.AddYears(10); //expires in the future

                if (line.Element("Discount_No.") == null)
                    throw new XmlException("Discount_No. node not found in response xml");
                po.OfferId = line.Element("Discount_No.").Value;

                if (line.Element("Discount_Type") == null)
                    throw new XmlException("Discount_Type node not found in response xml");
                po.Code = (OfferDiscountType)Convert.ToInt32(line.Element("Discount_Type").Value);

                if (line.Element("Offer_Category") == null)
                    throw new XmlException("Offer_Category node not found in response xml");
                po.Type = (OfferType)Convert.ToInt32(line.Element("Offer_Category").Value);

                po.OfferDetails = GetOfferMemberDetails(doc, po.Id);
                rs.Add(po);
            }
            return rs;
        }

        public List<Notification> NotificationMemberResponseXML(string responseXml)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Response>
                <Request_ID>LOAD_MEMBER_DIR_MARK_INFO</Request_ID>
                <Response_Code>0000</Response_Code>
                <Response_Text></Response_Text>
                <Response_Body>
                    <Published_Offer>
                        <No.>PUB0036</No.>
                        <Discount_Type Options="Promotion,Deal,Multibuy,Mix&amp;Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount,Coupon">7</Discount_Type>
                        <Discount_No.>P1041</Discount_No.>
                        <Description>Golf Trolley offer</Description>
                        <Offer_Category Options="General,Special Member,Points and Coupons,Club and Scheme">2</Offer_Category>
                        <Primary_Text>Golf Trolley offer</Primary_Text>
                        <Secondary_Text>For only 25 points you get a discount on golf trolleys</Secondary_Text>
                        <Ending_Date>2019-12-31</Ending_Date>
                    </Published_Offer>
                    <Published_Offer_Images>
                        <KeyValue>PUB0040</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>PUBOFF-COFFEECM</Image_Id>
                    </Published_Offer_Images>
                    <Member_Notification>
                        <No.>MN000001</No.>
                        <Contact_No. />
                        <Primary_Text>Remember our regular offers</Primary_Text>
                        <Secondary_Text>Cronus Club Members receive exra discounts</Secondary_Text>
                        <When_Display Options="Always,Once">0</When_Display>
                        <Valid_From_Date>2013-01-01</Valid_From_Date>
                        <Valid_To_Date />
                        <Member_Attribute />
                        <Member_Attribute_Value />
                        <Web_Link />
                        <E-Mail_Disclaimer />
                        <Personalized_E-Mail>false</Personalized_E-Mail>
                        <HTML_E-Mail />
                        <Send_HTML>false</Send_HTML>
                    </Member_Notification>
                    <Member_Notification>
                        <No.>MN000002</No.>
                        <Contact_No. />
                        <Primary_Text>Call our Help Desk for assistance</Primary_Text>
                        <Secondary_Text>Our help desk phone number is (212) 555-9999</Secondary_Text>
                        <When_Display Options="Always,Once">0</When_Display>
                        <Valid_From_Date>2013-01-01</Valid_From_Date>
                        <Valid_To_Date />
                        <Member_Attribute />
                        <Member_Attribute_Value />
                        <Web_Link />
                        <E-Mail_Disclaimer />
                        <Personalized_E-Mail>false</Personalized_E-Mail>
                        <HTML_E-Mail />
                        <Send_HTML>false</Send_HTML>
                    </Member_Notification>
                    <Member_Notification_Images>
                        <KeyValue>MN000001</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>OFFERS</Image_Id>
                    </Member_Notification_Images>
                    <Member_Notification_Images>
                        <KeyValue>MN000002</KeyValue>
                        <Display_Order>1</Display_Order>
                        <Image_Id>HELPDESK</Image_Id>
                    </Member_Notification_Images>
                    <Member_Notification_Images>
                        <KeyValue>MN000002</KeyValue>
                        <Display_Order>0</Display_Order>
                        <Image_Id>HELPDESK2</Image_Id>
                    </Member_Notification_Images>
                </Response_Body>
            </Response>
              */
            #endregion

            List<Notification> rs = new List<Notification>();

            XDocument doc = XDocument.Parse(responseXml);
            var itemBufferLines = doc.Element("Response").Element("Response_Body").Descendants("Member_Notification");
            foreach (XElement line in itemBufferLines)
            {

                if (line.Element("No.") == null)
                    throw new XmlException("No. node not found in response xml");
                Notification po = new Notification(line.Element("No.").Value);

                if (line.Element("Contact_No.") == null)
                    throw new XmlException("Contact_No. node not found in response xml");
                po.ContactId = line.Element("Contact_No.").Value;

                if (line.Element("Primary_Text") == null)
                    throw new XmlException("Primary_Text node not found in response xml");
                po.Description = line.Element("Primary_Text").Value;

                if (line.Element("Secondary_Text") == null)
                    throw new XmlException("Secondary_Text node not found in response xml");
                po.Details = line.Element("Secondary_Text").Value;

                if (line.Element("Valid_To_Date") == null)
                    throw new XmlException("Valid_To_Date node not found in response xml");
                if (line.Element("Valid_To_Date").Value.Length > 8)
                    po.ExpiryDate = ConvertTo.SafeDateTime(line.Element("Valid_To_Date").Value);
                else
                    po.ExpiryDate = DateTime.Now.AddMonths(6);

                if (line.Element("Valid_From_Date") == null)
                    throw new XmlException("Valid_From_Date node not found in response xml");
                if (line.Element("Valid_From_Date").Value.Length > 8)
                    po.Created = ConvertTo.SafeDateTime(line.Element("Valid_From_Date").Value); //using created to store valid from for now
                else
                    po.Created = DateTime.Now.AddMonths(-1);

                po.Status = NotificationStatus.New;
                po.QRText = "";
                //po.NotificationType = NotificationType.BO;
                po.NotificationTextType = NotificationTextType.Plain;

                string imgId = GetOfferNotificationImageId(doc, po.Id);
                po.Images = new List<ImageView>();
                if (string.IsNullOrWhiteSpace(imgId) == false)
                    po.Images.Add(new ImageView() { Id = imgId });

                rs.Add(po);
            }
            return rs;
        }

        private List<OfferDetails> GetOfferMemberDetails(XDocument doc, string id)
        {
            List<OfferDetails> list = new List<OfferDetails>();
            var detailsLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Detail_Line");
            foreach (XElement line in detailsLines)
            {
                if (line.Element("Offer_No.") == null)
                    throw new XmlException("Offer_No. node not found in response xml");
                if (id == line.Element("Offer_No.").Value)
                {
                    OfferDetails d = new OfferDetails();
                    if (line.Element("Description") == null)
                        throw new XmlException("Descriptionnode not found in response xml");
                    d.Description = line.Element("Description").Value;

                    string imgId = GetOfferDetailsMemberImageId(doc, id);
                    if (string.IsNullOrWhiteSpace(imgId) == false)
                        d.Image = new ImageView(imgId);

                    list.Add(d);
                }
            }

            return list;
        }

        private string GetOfferMemberImageId(XDocument doc, string id)
        {
            //TODO now only taking one images,, 
            string imgId = "";
            var detailsLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Images");
            foreach (XElement line in detailsLines)
            {
                if (line.Element("KeyValue") == null)
                    throw new XmlException("KeyValue node not found in response xml");
                if (id.ToUpper() == line.Element("KeyValue").Value.ToUpper())
                {
                    if (line.Element("Image_Id") == null)
                        throw new XmlException("Image_Id not found in response xml");
                    imgId = line.Element("Image_Id").Value;

                    //Display_Order
                    break; //need to return a list of images based on Display_Order
                }
            }
            return imgId;
        }

        private string GetOfferDetailsMemberImageId(XDocument doc, string id)
        {
            //TODO now only taking one images,, 
            string imgId = "";
            var detailsLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Detail_Line_Images");
            foreach (XElement line in detailsLines)
            {
                if (line.Element("KeyValue") == null)
                    throw new XmlException("KeyValue node not found in response xml");
                if (id.ToUpper() == line.Element("KeyValue").Value.ToUpper())
                {
                    if (line.Element("Image_Id") == null)
                        throw new XmlException("Image_Id not found in response xml");
                    imgId = line.Element("Image_Id").Value;


                    //Display_Order
                    break; //need to return a list of images based on Display_Order
                }

            }

            return imgId;
        }

        private string GetOfferNotificationImageId(XDocument doc, string id)
        {
            //TODO now only taking one images,, 
            string imgId = "";
            var detailsLines = doc.Element("Response").Element("Response_Body").Descendants("Member_Notification_Images");
            foreach (XElement line in detailsLines)
            {
                if (line.Element("KeyValue") == null)
                    throw new XmlException("KeyValue node not found in response xml");
                if (id.ToUpper() == line.Element("KeyValue").Value.ToUpper())
                {
                    if (line.Element("Image_Id") == null)
                        throw new XmlException("Image_Id not found in response xml");
                    imgId = line.Element("Image_Id").Value;

                    //Display_Order
                    break; //need to return a list of images based on Display_Order
                }
            }
            return imgId;
        }
    }
}
