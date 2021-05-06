using System;
using System.Xml;
using System.Xml.Linq;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Loyalty
{
    //Navision back office connection
    public class NavXml : BaseXml
    {
        //Request IDs
        private const string LoginChangeRequestId = "MM_LOGIN_CHANGE";
        private const string VersionTestConnectionRequestId = "TEST_CONNECTION";

        public NavXml()
        {
        }

        public string ChangeLoginRequestXML(string oldUserName, string newUserName, string pwd)
        {
            /*
			 <?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
			  <Request_ID>MM_LOGIN_CHANGE</Request_ID>
			  <Request_Body>
				<LoginID>tom</LoginID>
				<Password>tom.1</Password>
				<NewLoginID>newtom</NewLoginID>
			  </Request_Body>
			</Request>
			 */

            // Create the XML document
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = LoginChangeRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement loginIdElement = document.CreateElement("LoginID");
            loginIdElement.InnerText = oldUserName;
            requestBody.AppendChild(loginIdElement);

            XmlElement newPwdElement = document.CreateElement("Password");
            newPwdElement.InnerText = pwd;
            requestBody.AppendChild(newPwdElement);

            XmlElement oldPwdElement = document.CreateElement("NewLoginID");
            oldPwdElement.InnerText = newUserName;
            requestBody.AppendChild(oldPwdElement);

            return document.OuterXml;
        }

        public string TestConnectionRequestXML()
        {
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?> 
            <Request>
              <Request_ID>TEST_CONNECTION</Request_ID>
              <Request_Body />
            </Request>
  
            <Response>
              <Request_ID>TEST_CONNECTION</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Application_Version>W1 7.10</Application_Version>
                <Application_Build>36281</Application_Build>
                <LS_Retail_Version>LS Nav 2013 (7.10.01)</LS_Retail_Version>
                <LS_Retail_Copyright>C 1998-2014 LS Retail ehf.</LS_Retail_Copyright>
              </Response_Body>
            </Response>

             <Response>
              <Request_ID>TEST_CONNECTION</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Application_Version>W1 8.00</Application_Version>
                <Application_Build>42222</Application_Build>
                <LS_Retail_Version>LS Nav 2015 (8.00.06)</LS_Retail_Version>
                <LS_Retail_Copyright>C 1998-2015 LS Retail ehf.</LS_Retail_Copyright>
              </Response_Body>
            </Response> 
             */
            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", VersionTestConnectionRequestId),
                    new XElement("Request_Body")
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            return doc.ToString();
        }

        public void TestConnectionResponseXML(string responseXml, ref string appVersion, ref string appBuild, ref string retailVersion, ref string retailCopyright)
        {
            /*
            <Response>
              <Request_ID>TEST_CONNECTION</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Application_Version>W1 8.00</Application_Version>  simply hardcoded in Codeunit.  ApplicationVersion()  EXIT('W1 7.10')
                <Application_Build>37874</Application_Build>
                <LS_Retail_Version>LS Nav 2015 (8.0)</LS_Retail_Version>
                <LS_Retail_Copyright>© 1998-2015 LS Retail ehf.</LS_Retail_Copyright>
              </Response_Body>
            </Response>
             * 
            NAV 7.10.01
            <Application_Version>W1 7.10</Application_Version>
            <Application_Build>36281</Application_Build>
            <LS_Retail_Version>LS Nav 2013 (7.10.01)</LS_Retail_Version>
            <LS_Retail_Copyright>© 1998-2014 LS Retail ehf.</LS_Retail_Copyright>
             */

            XDocument doc = XDocument.Parse(responseXml);
            XElement xElement = doc.Element("Response").Element("Response_Body").Element("LS_Retail_Version");
            if (xElement == null)
                throw new XmlException("LS_Retail_Version node not found in response XML");
            retailVersion = xElement.Value;

            xElement = doc.Element("Response").Element("Response_Body").Element("Application_Build");
            if (xElement != null)
                appBuild = xElement.Value;

            xElement = doc.Element("Response").Element("Response_Body").Element("Application_Version");
            if (xElement != null)
                appVersion = xElement.Value;

            xElement = doc.Element("Response").Element("Response_Body").Element("LS_Retail_Copyright");
            if (xElement != null)
                retailCopyright = xElement.Value;
        }
    }
}
