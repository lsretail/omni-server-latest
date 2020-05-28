using System;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    public class MemberCardXml : BaseXml
    {
        //Request IDs
        private const string MemberCardRequestId = "GET_MEMBER_CARD";

        public MemberCardXml()
        {
        }

        public string MemberCardRequestXML(string cardId)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?> <Request>
              <Request_ID>GET_MEMBER_CARD</Request_ID>
              <Request_Body>
                <Card_No>10021</Card_No>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", MemberCardRequestId),
                    new XElement("Request_Body",
                        new XElement("Card_No", cardId)
                    )
                );

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            return doc.ToString();
        }

        public long MemberCardPointsResponse(string responseXml)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Response>
	            <Request_ID>GET_MEMBER_CARD</Request_ID>
	            <Response_Code>0000</Response_Code>
	            <Response_Text></Response_Text>
	            <Response_Body>
                    <Total_Remaining_Points>1234</Total_Remaining_Points>
		            <Membership_Card>
			            <Card_No.>10021</Card_No.>
			            <Status></Status>
			            <Linked_to_Account></Linked_to_Account>
			            <Club_Code></Club_Code>
			            <Scheme_Code></Scheme_Code>
			            <Account_No.></Account_No.>
			            <Contact_No.></Contact_No.>
                        <First_Date_Used></First_Date_Used>
                        <Last_Valid_Date></Last_Valid_Date>
                        <Reason_Blocked></Reason_Blocked>
                        <Date_Blocked></Date_Blocked>
                        <Blocked_by></Blocked_by>
		            </Membership_Card>
	            </Response_Body>
            </Response>

             */
            #endregion

            XDocument doc = XDocument.Parse(responseXml);
            XElement remainingPts = doc.Element("Response").Element("Response_Body").Element("Total_Remaining_Points");
            if (remainingPts == null)
                throw new XmlException("Total_Remaining_Points node not found in response XML");
            return Convert.ToInt64(Math.Floor(ConvertTo.SafeDecimal(remainingPts.Value)));
        }
    }
}
