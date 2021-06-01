using System;
using System.Xml;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Loyalty
{
    //Navision back office connection
    public class NavXml : BaseXml
    {
        //Request IDs
        private const string LoginChangeRequestId = "MM_LOGIN_CHANGE";

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
    }
}
