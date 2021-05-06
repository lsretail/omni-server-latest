using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Loyalty
{
    public class PublishedOfferXml : BaseXml
    {
        private const string ItemsRequestId = "LOAD_PUBLISHED_OFFER_ITEMS";

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
                    throw new XmlException("Item_No. node not found in response XML");
                item.Id = line.Element("Item_No.").Value;

                if (line.Element("Item_Description") == null)
                    throw new XmlException("Item_Description node not found in response XML");
                item.Description = line.Element("Item_Description").Value;

                if (line.Element("Image_ID") == null)
                    throw new XmlException("Image_ID node not found in response XML");
                item.Images = new List<ImageView>();
                item.Images.Add(new ImageView() { Id = line.Element("Image_ID").Value });

                rs.Add(item);
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
                    throw new XmlException("Offer_No. node not found in response XML");
                if (id == line.Element("Offer_No.").Value)
                {
                    OfferDetails d = new OfferDetails();
                    if (line.Element("Description") == null)
                        throw new XmlException("Description node not found in response XML");
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
            // TODO now only taking one images,, 
            string imgId = "";
            var detailsLines = doc.Element("Response").Element("Response_Body").Descendants("Published_Offer_Images");
            foreach (XElement line in detailsLines)
            {
                if (line.Element("KeyValue") == null)
                    throw new XmlException("KeyValue node not found in response XML");
                if (id.ToUpper() == line.Element("KeyValue").Value.ToUpper())
                {
                    if (line.Element("Image_Id") == null)
                        throw new XmlException("Image_Id not found in response XML");
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
                    throw new XmlException("KeyValue node not found in response XML");
                if (id.ToUpper() == line.Element("KeyValue").Value.ToUpper())
                {
                    if (line.Element("Image_Id") == null)
                        throw new XmlException("Image_Id not found in response XML");
                    imgId = line.Element("Image_Id").Value;


                    //Display_Order
                    break; //need to return a list of images based on Display_Order
                }
            }
            return imgId;
        }
    }
}
