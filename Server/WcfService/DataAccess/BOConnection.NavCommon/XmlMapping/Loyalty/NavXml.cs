using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    //Navision back office connection
    public class NavXml : BaseXml
    {
        //Request IDs
        private const string ChangePwdRequestId = "MM_MOBILE_PWD_CHANGE";
        private const string ResetPwdRequestId = "MM_MOBILE_PWD_RESET";
        private const string LoginChangeRequestId = "MM_LOGIN_CHANGE";
        private const string LoginRequestId = "MM_MOBILE_LOGON";
        private const string CreateDeviceAndLinkToUserRequestId = "MM_CREATE_LOGIN_LINKS";//"MM_MOBILE_CREATE_DEVICE_USER";
        private const string ContactCreateRequestId = "MM_MOBILE_CONTACT_CREATE";
        private const string ContactUpdateRequestId = "MM_MOBILE_CONTACT_UPDATE";
        private const string ContactAddCardRequestId = "MM_CARD_TO_CONTACT";
        private const string ProfilesGetRequestId = "MM_MOBILE_GET_PROFILES";
        private const string VersionTestConnectionRequestId = "TEST_CONNECTION";
        private const string MemberCardRequestId = "GET_MEMBER_CARD";

        public NavXml()
        {
        }

        public string ContactAddCardRequestXML(string contactId, string accountId, string cardId)
        {
            /*
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
			<Request_ID>MM_CARD_TO_CONTACT</Request_ID>
			<Request_Body>
			 <AccountID>jane</AccountID>
			 <ContactID>jane.1</ContactID>
			 <CardID>jane</CardID>
			</Request_Body>
			</Request>
				 */

            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            ////Create the Request ID element
            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = ContactAddCardRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement accountIdElement = document.CreateElement("AccountID");
            accountIdElement.InnerText = accountId;
            requestBody.AppendChild(accountIdElement);

            XmlElement contactIdElement = document.CreateElement("ContactID");
            contactIdElement.InnerText = contactId;
            requestBody.AppendChild(contactIdElement);

            XmlElement cardIdElement = document.CreateElement("CardID");
            cardIdElement.InnerText = cardId;
            requestBody.AppendChild(cardIdElement);

            return document.OuterXml;
        }

        public double ContactAddCardResponseXml(string responseXml)
        {
            /*
		    <?xml version="1.0" encoding="utf-8" standalone="no"?>
		    <Response>
		      <Response_Code>0000</Response_Code>
		      <Request_ID>MM_CARD_TO_CONTACT</Request_ID>
		      <Response_Text></Response_Text>
		      <Request_Body>
			    <Total_Remaining_Points>13</Total_Remaining_Points>
		      </Request_Body>
		    </Response>
			    PointBalance changed to 
		    */

            // Create the xml document container
            XmlDocument document = new XmlDocument();
            document.LoadXml(responseXml);

            XmlNode node = document.SelectSingleNode("//Total_Remaining_Points");
            if (node == null)
                throw new XmlException("Total_Remaining_Points node not found in response XML");
            double balance = 0L;
            if (string.IsNullOrWhiteSpace(node.InnerText) == false)
                balance = ConvertTo.SafeDouble(node.InnerText);

            return balance;
        }

        public string ContactCreateResponseXML(string responseXml)
        {
            /*
			<?xml version="1.0" standalone="no"?>
			<Response><Request_ID>MM_MOBILE_CONTACT_CREATE</Request_ID>
			 <Response_Code>0000</Response_Code>
			 <Response_Text></Response_Text>
			 <Response_Body>
				<ContactID>MO000012</ContactID>
				<AccountID>MA000010</AccountID>
				<SchemeID>CR1-BRONZE</SchemeID>
				<CardID></CardID>
				<Total_Remaining_Points>0</Total_Remaining_Points>
				<ClubID>1</ClubID>
			 </Response_Body>
			</Response>
			 */

            // Create the xml document containe
            XmlDocument document = new XmlDocument();
            document.LoadXml(responseXml);

            XmlNode node = document.SelectSingleNode("//CardID");
            if (node == null)
                throw new XmlException("CardID node not found in response XML");
            return node.InnerText;
        }

        public string ContactUpdateRequestXML(MemberContact contact, string accountId)
        {
            /*
			<Request>
			  <Request_ID>MM_MOBILE_CONTACT_UPDATE</Request_ID>
			  <Request_Body>
				<ContactID>MO000012</ContactID>
				<Email>JIJ@lsretail.com</Email>
				<FirstName>Johann</FirstName>
				<LastName>Johannsson</LastName>
				<MiddleName>Ingi</MiddleName>
				<Gender>M</Gender>
				<Phone>615464</Phone>
				<Address1>Skulagotu 100</Address1>
				<Address2>Apt 3</Address2>
				<City>Reykjavik</City>
				<PostCode>230</PostCode>
				<StateProvinceRegion></StateProvinceRegion>
				<Country>IS</Country>
				<AccountID>MA000010</AccountID>
				<ExternalID></ExternalID>
				<ExternalSystem></ExternalSystem>
			  </Request_Body>
			</Request>
			 */
            // Create the XML document contain
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            // Create the Request element
            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            // Create the Request ID element
            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = ContactUpdateRequestId;
            request.AppendChild(requestId);

            // Create the Request Body element
            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            // Create the user ID element
            XmlElement userNameElement = document.CreateElement("ContactID");
            userNameElement.InnerText = contact.Id;
            requestBody.AppendChild(userNameElement);

            XmlElement emailElement = document.CreateElement("Email");
            emailElement.InnerText = contact.Email;
            requestBody.AppendChild(emailElement);

            XmlElement firstNameElement = document.CreateElement("FirstName");
            firstNameElement.InnerText = contact.FirstName;
            requestBody.AppendChild(firstNameElement);

            XmlElement lastNameElement = document.CreateElement("LastName");
            lastNameElement.InnerText = contact.LastName;
            requestBody.AppendChild(lastNameElement);

            XmlElement middleNameElement = document.CreateElement("MiddleName");
            middleNameElement.InnerText = contact.MiddleName;
            requestBody.AppendChild(middleNameElement);

            XmlElement genderElement = document.CreateElement("Gender");
            switch (contact.Gender)
            {
                case Gender.Female:
                    genderElement.InnerText = "1";
                    break;
                case Gender.Male:
                    genderElement.InnerText = "0";
                    break;
                default:
                    genderElement.InnerText = string.Empty;
                    break;
            }
            requestBody.AppendChild(genderElement);

            Address address = new Address(); //defaults to empty strings
            if (contact.Addresses.Count > 0)
                address = contact.Addresses[0];

            XmlElement addr1Element = document.CreateElement("Address1");
            addr1Element.InnerText = address.Address1;
            requestBody.AppendChild(addr1Element);

            XmlElement addr2Element = document.CreateElement("Address2");
            addr2Element.InnerText = address.Address2;
            requestBody.AppendChild(addr2Element);

            XmlElement cityElement = document.CreateElement("City");
            cityElement.InnerText = address.City;
            requestBody.AppendChild(cityElement);

            XmlElement postCodeElement = document.CreateElement("PostCode");
            postCodeElement.InnerText = address.PostCode;
            requestBody.AppendChild(postCodeElement);

            XmlElement stateRegionElement = document.CreateElement("StateProvinceRegion");
            stateRegionElement.InnerText = address.StateProvinceRegion;
            requestBody.AppendChild(stateRegionElement);

            XmlElement countryElement = document.CreateElement("Country");
            countryElement.InnerText = address.Country;
            requestBody.AppendChild(countryElement);

            XmlElement phoneElement = document.CreateElement("Phone");
            phoneElement.InnerText = address.PhoneNumber;
            requestBody.AppendChild(phoneElement);

            XmlElement mphoneElement = document.CreateElement("MobilePhoneNo");
            mphoneElement.InnerText = address.CellPhoneNumber;
            requestBody.AppendChild(mphoneElement);


            XmlElement accountIDElement = document.CreateElement("AccountID");
            accountIDElement.InnerText = accountId;
            requestBody.AppendChild(accountIDElement);

            XmlElement externalIDElement = document.CreateElement("ExternalID");
            externalIDElement.InnerText = "";
            requestBody.AppendChild(externalIDElement);

            XmlElement externalSystemElement = document.CreateElement("ExternalSystem");
            externalSystemElement.InnerText = "";
            requestBody.AppendChild(externalSystemElement);

            XmlElement dobElement = document.CreateElement("DateOfBirth");
            dobElement.InnerText = ToNAVDate(contact.BirthDay);
            requestBody.AppendChild(dobElement);

            return document.OuterXml;
        }

        public string ContactCreateRequestXML(MemberContact contact)
        {
            /*
		<Request>
		  <Request_ID>MM_MOBILE_CONTACT_CREATE</Request_ID>
		  <Request_Body>
			<LoginID>JIJ</LoginID>
			<Password>myPassword1</Password>
			<Email>JIJ@lsretail.com</Email>
			<FirstName>Johann</FirstName>
			<LastName>Johannsson</LastName>
			<MiddleName>Ingi</MiddleName>
			<Gender>M</Gender>
			<Phone>615464</Phone>
			<Address1>Skulagotu 100</Address1>
			<Address2>Apt 3</Address2>
			<City>Reykjavik</City>
			<PostCode>230</PostCode>
			<StateProvinceRegion></StateProvinceRegion>
			<Country>IS</Country>
			<ClubID></ClubID>
			<SchemeID></SchemeID>
			<AccountID></AccountID>
			 * ContactID var bætt inn og error ef ég sendi það ekki
			<DeviceID>JIJ-DEVICE</DeviceID>
			<DeviceFriendlyName>My Samsung III</DeviceFriendlyName>
			<ExternalID></ExternalID>
			<ExternalSystem></ExternalSystem>
		  </Request_Body>
		</Request>

			 */
            // Create the XML document contain
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = ContactCreateRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement userNameElement = document.CreateElement("LoginID");
            userNameElement.InnerText = contact.UserName;
            requestBody.AppendChild(userNameElement);

            XmlElement passwordElement = document.CreateElement("Password");
            passwordElement.InnerText = contact.Password;
            requestBody.AppendChild(passwordElement);

            XmlElement emailElement = document.CreateElement("Email");
            emailElement.InnerText = contact.Email;
            requestBody.AppendChild(emailElement);

            XmlElement firstNameElement = document.CreateElement("FirstName");
            firstNameElement.InnerText = contact.FirstName;
            requestBody.AppendChild(firstNameElement);

            XmlElement lastNameElement = document.CreateElement("LastName");
            lastNameElement.InnerText = contact.LastName;
            requestBody.AppendChild(lastNameElement);

            XmlElement middleNameElement = document.CreateElement("MiddleName");
            middleNameElement.InnerText = contact.MiddleName;
            requestBody.AppendChild(middleNameElement);

            XmlElement genderElement = document.CreateElement("Gender");
            switch (contact.Gender)
            {
                case Gender.Female:
                    genderElement.InnerText = "1";
                    break;
                case Gender.Male:
                    genderElement.InnerText = "0";
                    break;
                default:
                    genderElement.InnerText = string.Empty;
                    break;
            }
            requestBody.AppendChild(genderElement);

            Address address = new Address(); //defaults to empty strings
            if (contact.Addresses != null && contact.Addresses.Count > 0)
                address = contact.Addresses[0];

            XmlElement addr1Element = document.CreateElement("Address1");
            addr1Element.InnerText = address.Address1;
            requestBody.AppendChild(addr1Element);

            XmlElement addr2Element = document.CreateElement("Address2");
            addr2Element.InnerText = address.Address2;
            requestBody.AppendChild(addr2Element);

            XmlElement cityElement = document.CreateElement("City");
            cityElement.InnerText = address.City;
            requestBody.AppendChild(cityElement);

            XmlElement postCodeElement = document.CreateElement("PostCode");
            postCodeElement.InnerText = address.PostCode;
            requestBody.AppendChild(postCodeElement);

            XmlElement stateRegionElement = document.CreateElement("StateProvinceRegion");
            stateRegionElement.InnerText = address.StateProvinceRegion;
            requestBody.AppendChild(stateRegionElement);

            XmlElement countryElement = document.CreateElement("Country");
            countryElement.InnerText = address.Country;
            requestBody.AppendChild(countryElement);

            XmlElement phoneElement = document.CreateElement("Phone");
            phoneElement.InnerText = address.PhoneNumber;
            requestBody.AppendChild(phoneElement);

            XmlElement mphoneElement = document.CreateElement("MobilePhoneNo");
            mphoneElement.InnerText = address.CellPhoneNumber;
            requestBody.AppendChild(mphoneElement);


            XmlElement clubIIDElement = document.CreateElement("ClubID");
            clubIIDElement.InnerText = contact.Account?.Scheme?.Club?.Id;
            requestBody.AppendChild(clubIIDElement);

            XmlElement schemeIDElement = document.CreateElement("SchemeID");
            schemeIDElement.InnerText = string.Empty;
            requestBody.AppendChild(schemeIDElement);

            XmlElement accountIDElement = document.CreateElement("AccountID");
            accountIDElement.InnerText = contact.Account?.Id;
            requestBody.AppendChild(accountIDElement);

            XmlElement contactIDElement = document.CreateElement("ContactID");
            contactIDElement.InnerText = ""; //contactID never sent
            requestBody.AppendChild(contactIDElement);

            XmlElement deviceIdElement = document.CreateElement("DeviceID");
            deviceIdElement.InnerText = contact.LoggedOnToDevice.Id;
            requestBody.AppendChild(deviceIdElement);

            XmlElement deviceNameElement = document.CreateElement("DeviceFriendlyName");
            deviceNameElement.InnerText = contact.LoggedOnToDevice.DeviceFriendlyName;
            requestBody.AppendChild(deviceNameElement);

            XmlElement externalIDElement = document.CreateElement("ExternalID");
            externalIDElement.InnerText = "";
            requestBody.AppendChild(externalIDElement);

            XmlElement externalSystemElement = document.CreateElement("ExternalSystem");
            externalSystemElement.InnerText = "";
            requestBody.AppendChild(externalSystemElement);

            XmlElement dobElement = document.CreateElement("DateOfBirth");
            dobElement.InnerText = ToNAVDate(contact.BirthDay);
            requestBody.AppendChild(dobElement);

            return document.OuterXml;
        }

        public string CreateDeviceAndLinkToUser(string userName, string deviceId, string deviceFriendlyName, string cardId)
        {
            //blank for cardId means it only links user to device but not to card

            /*was MM_MOBILE_CREATE_DEVICE_USER
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
			  <Request_ID>MM_CREATE_LOGIN_LINKS</Request_ID> 
			  <Request_Body>
				<LoginID>tom</LoginID>
				<DeviceID>tomdevice</DeviceID>
				<DeviceFriendlyName>mytomdevice</DeviceFriendlyName>
				<CardID>10021</CardID>
			  </Request_Body>
			</Request>
			 */
            // Create the xml document containe
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = CreateDeviceAndLinkToUserRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement userNameElement = document.CreateElement("LoginID");
            userNameElement.InnerText = userName;
            requestBody.AppendChild(userNameElement);

            XmlElement deviceIdElement = document.CreateElement("DeviceID");
            deviceIdElement.InnerText = deviceId;
            requestBody.AppendChild(deviceIdElement);

            XmlElement deviceNameElement = document.CreateElement("DeviceFriendlyName");
            deviceNameElement.InnerText = deviceFriendlyName;
            requestBody.AppendChild(deviceNameElement);

            XmlElement cardIdElement = document.CreateElement("CardID");
            cardIdElement.InnerText = cardId; //blank for cardID means it only links user to device but not to card
            requestBody.AppendChild(cardIdElement);

            return document.OuterXml;
        }

        public string LoginRequestXML(string userName, string pwd, string cardId)
        {
            /*
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
			  <Request_ID>MM_MOBILE_LOGON</Request_ID>
			  <Request_Body>
				<LoginID>JIJXX</LoginID>
				<Password>GunnarH</Password>
				<CardID>10021</CardID>
			  </Request_Body>
			</Request>
			 */
            // Create the XML document contain
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = LoginRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement userNameElement = document.CreateElement("LoginID");
            userNameElement.InnerText = userName;
            requestBody.AppendChild(userNameElement);

            XmlElement pwdElement = document.CreateElement("Password");
            pwdElement.InnerText = pwd;
            requestBody.AppendChild(pwdElement);

            XmlElement cardElement = document.CreateElement("CardID");
            cardElement.InnerText = cardId;
            requestBody.AppendChild(cardElement);

            XmlElement devElement = document.CreateElement("DeviceID");
            devElement.InnerText = string.Empty;
            requestBody.AppendChild(devElement);

            return document.OuterXml;
        }

        public string ResetPasswordRequestXML(string userName, string newPwd)
        {
            /*
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
			  <Request_ID>MM_MOBILE_PWD_RESET</Request_ID>
			  <Request_Body>
				<LoginID>GunnarH</LoginID>
				<Password>HrannuG</Password>
			  </Request_Body>
			</Request>
			 */
            // Create the XML document contain
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = ResetPwdRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement userNameElement = document.CreateElement("LoginID");
            userNameElement.InnerText = userName;
            requestBody.AppendChild(userNameElement);

            XmlElement pwdElement = document.CreateElement("Password");
            pwdElement.InnerText = newPwd;
            requestBody.AppendChild(pwdElement);

            return document.OuterXml;
        }

        public string ChangePasswordRequestXML(string userName, string newPwd, string oldPwd)
        {
            /*
			 <?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
			  <Request_ID>MM_MOBILE_PWD_CHANGE</Request_ID>
			  <Request_Body>
				<LoginID>GunnarH</LoginID>
				<NewPassword>HrannuG</NewPassword>
				<OldPassword>HrannuG</OldPassword>
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
            requestId.InnerText = ChangePwdRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement loginIdElement = document.CreateElement("LoginID");
            loginIdElement.InnerText = userName;
            requestBody.AppendChild(loginIdElement);

            XmlElement newPwdElement = document.CreateElement("NewPassword");
            newPwdElement.InnerText = newPwd;
            requestBody.AppendChild(newPwdElement);

            XmlElement oldPwdElement = document.CreateElement("OldPassword");
            oldPwdElement.InnerText = oldPwd;
            requestBody.AppendChild(oldPwdElement);

            return document.OuterXml;
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

        public string ProfilesRequestXML(string profileId)
        {
            /*
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
				<Request_ID>MM_MOBILE_GET_PROFILES</Request_ID>
				<Request_Body>
				<Profile_ID>All</Profile_ID>   
				</Request_Body>
			</Request>
			 */
            // Create the XML document
            XmlDocument document = new XmlDocument();

            // Create the XML Declaration, and append it to XML document
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            // Create the Request element
            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            // Create the Request ID element
            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = ProfilesGetRequestId;
            request.AppendChild(requestId);

            // Create the Request Body element
            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            // Create the profile ID element
            XmlElement profileElement = document.CreateElement("Profile_ID");
            profileElement.InnerText = profileId;
            requestBody.AppendChild(profileElement);

            return document.OuterXml;
        }

        public List<Profile> ProfileResponseXML(string responseXml)
        {
            /*
                <?xml version="1.0" encoding="utf-8" standalone="no"?>
                <Response>
                  <Request_ID>MM_MOBILE_GET_PROFILES</Request_ID>
                  <Response_Code>0000</Response_Code>
                  <Response_Text></Response_Text>
                  <Response_Body>
                    <Member_Attribute_Setup>
                      <Code>GOLF</Code>
                      <Description>Golf</Description>
                      <Attribute_Type Options="Text,Integer,Decimal,Date,Boolean">4</Attribute_Type>
                      <Default_Value />
                      <Mandatory>true</Mandatory>
                    </Member_Attribute_Setup>
                    <Member_Attribute_Setup>
                      <Code>LINDALINE</Code>
                      <Description>Likes Linda Line</Description>
                      <Attribute_Type Options="Text,Integer,Decimal,Date,Boolean">4</Attribute_Type>
                      <Default_Value />
                      <Mandatory>true</Mandatory>
                    </Member_Attribute_Setup>
                    <Member_Attribute_Setup>
                      <Code>TIMLINE</Code>
                      <Description>Likes Tim Line</Description>
                      <Attribute_Type Options="Text,Integer,Decimal,Date,Boolean">4</Attribute_Type>
                      <Default_Value />
                      <Mandatory>true</Mandatory>
                    </Member_Attribute_Setup>
                  </Response_Body>
                </Response>
            */

            List<Profile> profileList = new List<Profile>();

            // Create the XML document
            XmlDocument document = new XmlDocument();
            document.LoadXml(responseXml);

            XmlNodeList nodelist = document.SelectNodes("//Member_Attribute_Setup");
            foreach (XmlNode nodeLoop in nodelist)
            {
                Profile profile = new Profile();
                //profile..ItemId = itemId;

                XmlNode node = nodeLoop.SelectSingleNode("Code");
                if (node == null)
                    throw new XmlException("Code node not found in response XML");
                profile.Id = node.InnerText;

                node = nodeLoop.SelectSingleNode("Description");
                if (node == null)
                    throw new XmlException("Description node not found in response XML");
                profile.Description = node.InnerText;

                int temp = 0;
                node = nodeLoop.SelectSingleNode("Attribute_Type");
                if (node == null)
                    throw new XmlException("Attribute_Type node not found in response XML");
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                    temp = Convert.ToInt32(node.InnerText);
                profile.DataType = (ProfileDataType)temp;

                node = nodeLoop.SelectSingleNode("Default_Value");
                if (node == null)
                    throw new XmlException("Default_Value node not found in response XML");
                profile.DefaultValue = node.InnerText;

                node = nodeLoop.SelectSingleNode("Mandatory");
                if (node == null)
                    throw new XmlException("Mandatory node not found in response XML");
                bool bTemp = false;
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                    bTemp = ConvertTo.SafeBoolean(node.InnerText);
                profile.Mandatory = bTemp;

                profileList.Add(profile);
            }
            return profileList;
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
