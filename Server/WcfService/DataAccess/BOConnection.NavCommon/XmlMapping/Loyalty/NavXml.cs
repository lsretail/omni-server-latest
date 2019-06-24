using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    //Navision back office connection
    public class NavXml : BaseXml
    {
        //Request IDs
        private const string ChangePwdRequestId = "MM_MOBILE_PWD_CHANGE";
        private const string ResetPwdRequestId = "MM_MOBILE_PWD_RESET";
        private const string LoginRequestId = "MM_MOBILE_LOGON";
        private const string CreateDeviceAndLinkToUserRequestId = "MM_CREATE_LOGIN_LINKS";//"MM_MOBILE_CREATE_DEVICE_USER";
        private const string ContactCreateRequestId = "MM_MOBILE_CONTACT_CREATE";
        private const string ContactUpdateRequestId = "MM_MOBILE_CONTACT_UPDATE";
        private const string ContactAddCardRequestId = "MM_CARD_TO_CONTACT";
        private const string ProfilesGetRequestId = "MM_MOBILE_GET_PROFILES";
        private const string TransactionGetByRequestId = "WEB_POS_PRINT_EXT";
        private const string VersionTestConnectionRequestId = "TEST_CONNECTION";

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
                throw new XmlException("Total_Remaining_Points node not found in response xml");
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
                throw new XmlException("CardID node not found in response xml");
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
            // Create the xml document containe
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

            XmlElement phoneElement = document.CreateElement("Phone");
            phoneElement.InnerText = contact.Phone;
            requestBody.AppendChild(phoneElement);

            XmlElement mphoneElement = document.CreateElement("MobilePhoneNo");
            mphoneElement.InnerText = contact.MobilePhone;
            requestBody.AppendChild(mphoneElement);

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
            // Create the xml document containe
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

            XmlElement phoneElement = document.CreateElement("Phone");
            phoneElement.InnerText = contact.Phone;
            requestBody.AppendChild(phoneElement);

            XmlElement mphoneElement = document.CreateElement("MobilePhoneNo");
            mphoneElement.InnerText = contact.MobilePhone;
            requestBody.AppendChild(mphoneElement);

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
            // Create the xml document containe
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
            // Create the xml document containe
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
            // Create the xml document containe
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
            // Create the xml document containe
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

            // Create the xml document containe
            XmlDocument document = new XmlDocument();
            document.LoadXml(responseXml);

            XmlNodeList nodelist = document.SelectNodes("//Member_Attribute_Setup");
            foreach (XmlNode nodeLoop in nodelist)
            {
                Profile profile = new Profile();
                //profile..ItemId = itemId;

                XmlNode node = nodeLoop.SelectSingleNode("Code");
                if (node == null)
                    throw new XmlException("Code node not found in response xml");
                profile.Id = node.InnerText;

                node = nodeLoop.SelectSingleNode("Description");
                if (node == null)
                    throw new XmlException("Description node not found in response xml");
                profile.Description = node.InnerText;

                int temp = 0;
                node = nodeLoop.SelectSingleNode("Attribute_Type");
                if (node == null)
                    throw new XmlException("Attribute_Type node not found in response xml");
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                    temp = Convert.ToInt32(node.InnerText);
                profile.DataType = (ProfileDataType)temp;

                node = nodeLoop.SelectSingleNode("Default_Value");
                if (node == null)
                    throw new XmlException("Default_Value node not found in response xml");
                profile.DefaultValue = node.InnerText;

                node = nodeLoop.SelectSingleNode("Mandatory");
                if (node == null)
                    throw new XmlException("Mandatory node not found in response xml");
                bool bTemp = false;
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                    bTemp = ConvertTo.SafeBoolean(node.InnerText);
                profile.Mandatory = bTemp;

                profileList.Add(profile);
            }
            return profileList;
        }

        public string TransactionGetByIdRequestXML(string storeId, string terminalId, string transactionId)
        {
            /*
			<?xml version="1.0" encoding="utf-8" standalone="no"?>
			<Request>
				<Request_ID>WEB_POS_PRINT_EXT</Request_ID>
				<Request_Body>
					<Store_No>S0001</Store_No>
					<Terminal_No>P0001</Terminal_No>
					<Transaction_No>242</Transaction_No>
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
            requestId.InnerText = TransactionGetByRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement storeElement = document.CreateElement("Store_No");
            storeElement.InnerText = storeId;
            requestBody.AppendChild(storeElement);

            XmlElement terminalElement = document.CreateElement("Terminal_No");
            terminalElement.InnerText = terminalId;
            requestBody.AppendChild(terminalElement);

            XmlElement transactionIdElement = document.CreateElement("Transaction_No");
            transactionIdElement.InnerText = transactionId;
            requestBody.AppendChild(transactionIdElement);

            return document.OuterXml;
        }

        public LoyTransaction TransactionGetByIdResponseXml(string responseXml)
        {
            //NOTE the TransactionBLL does format the decimal
            #region xml

            /*
            <Request>
              <Request_ID>WEB_POS_PRINT_EXT</Request_ID>
              <Request_Body>
                <Store_No>S0004</Store_No>
                <Terminal_No>P0007</Terminal_No>
                <Transaction_No>40000206</Transaction_No>
              </Request_Body>
            </Request>
  
            <Response>
              <Request_ID>WEB_POS_PRINT_EXT</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Header_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Header_Line>LS Retail Store</Header_Line>
                </Header_Lines>
                <Receipt_info>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Store_No. />
                  <Terminal_No. />
                  <Transaction_No.>40000206</Transaction_No.>
                  <Receipt_No.>00000P0007000000030</Receipt_No.>
                  <Staff_ID>401</Staff_ID>
                  <Trans._Date>2013-10-23</Trans._Date>
                  <Trans._Time>01:49:00</Trans._Time>
                  <Description />
                  <Customer_No.>44010</Customer_No.>
                  <Customer_Name>Brad and sons Ltd</Customer_Name>
                  <Contact_No. />
                  <Contact_Name />
                  <Account_Text />
                  <Account_No. />
                  <VAT_Registration_No. />
                  <Table_No.>0</Table_No.>
                  <Rounding_Amount>0</Rounding_Amount>
                  <Order_No. />
                  <Deposit>0</Deposit>
                  <Remaining_Amount>0</Remaining_Amount>
                  <Non_Refundable>0</Non_Refundable>
                </Receipt_info>
                <Info_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Print_Info>** COPY **</Print_Info>
                </Info_Lines>
                <Item_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Line_No.>10000</Line_No.>
                  <Item_No.>41030</Item_No.>
                  <Item_Description>Boys Sweater</Item_Description>
                  <Variant_Code>062</Variant_Code>
                  <UOM_ID>PCS</UOM_ID>
                  <UOM_Description />
                  <Quantity>1</Quantity>
                  <Discount_Amount>0</Discount_Amount>
                  <Net_Amount>0</Net_Amount>
                  <VAT_Amount>0</VAT_Amount>
                  <VAT_Code>C</VAT_Code>
                  <VAT__>0</VAT__>
                  <Amount>45</Amount>
                  <Barcode />
                  <Price>45</Price>
                  <Station />
                  <Sourcing_Status />
                </Item_Lines>
                <Item_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>2</Section_Line_No.>
                  <Line_No.>20000</Line_No.>
                  <Item_No.>42030</Item_No.>
                  <Item_Description>Girls Sweater</Item_Description>
                  <Variant_Code>007</Variant_Code>
                  <UOM_ID>PCS</UOM_ID>
                  <UOM_Description />
                  <Quantity>1</Quantity>
                  <Discount_Amount>0</Discount_Amount>
                  <Net_Amount>0</Net_Amount>
                  <VAT_Amount>0</VAT_Amount>
                  <VAT_Code>C</VAT_Code>
                  <VAT__>0</VAT__>
                  <Amount>35</Amount>
                  <Barcode />
                  <Price>35</Price>
                  <Station />
                  <Sourcing_Status />
                </Item_Lines>
                <Item_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>3</Section_Line_No.>
                  <Line_No.>30000</Line_No.>
                  <Item_No.>42030</Item_No.>
                  <Item_Description>Girls Sweater</Item_Description>
                  <Variant_Code>008</Variant_Code>
                  <UOM_ID>PCS</UOM_ID>
                  <UOM_Description />
                  <Quantity>1</Quantity>
                  <Discount_Amount>0</Discount_Amount>
                  <Net_Amount>0</Net_Amount>
                  <VAT_Amount>0</VAT_Amount>
                  <VAT_Code>C</VAT_Code>
                  <VAT__>0</VAT__>
                  <Amount>35</Amount>
                  <Barcode />
                  <Price>35</Price>
                  <Station />
                  <Sourcing_Status />
                </Item_Lines>
                <Item_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>4</Section_Line_No.>
                  <Line_No.>40000</Line_No.>
                  <Item_No.>41030</Item_No.>
                  <Item_Description>Boys Sweater</Item_Description>
                  <Variant_Code>075</Variant_Code>
                  <UOM_ID>PCS</UOM_ID>
                  <UOM_Description />
                  <Quantity>1</Quantity>
                  <Discount_Amount>0</Discount_Amount>
                  <Net_Amount>0</Net_Amount>
                  <VAT_Amount>0</VAT_Amount>
                  <VAT_Code>C</VAT_Code>
                  <VAT__>0</VAT__>
                  <Amount>45</Amount>
                  <Barcode />
                  <Price>45</Price>
                  <Station />
                  <Sourcing_Status />
                </Item_Lines>
                <Tender_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Tender_Type>4</Tender_Type>
                  <Tender_Description>Customer Account</Tender_Description>
                  <Currency_Code />
                  <Amount_In_Currency>0</Amount_In_Currency>
                  <Amount_In_Tender>-160</Amount_In_Tender>
                  <Tender_Unit_Value>0</Tender_Unit_Value>
                  <Exchange_Rate>0</Exchange_Rate>
                  <Detail_Text>Account No. 44010</Detail_Text>
                </Tender_Lines>
                <Member_Point_Info>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Member_Info>Member Account MA000006.</Member_Info>
                  <Issued_Points>160</Issued_Points>
                  <Used_Points>0</Used_Points>
                  <Point_Balance>11634</Point_Balance>
                </Member_Point_Info>
                <Total_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Total_Text>Total </Total_Text>
                  <Total_Amount>160</Total_Amount>
                </Total_Lines>
                <Tax_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <VAT_Code>C</VAT_Code>
                  <VAT__>25</VAT__>
                  <Net_Amount>128</Net_Amount>
                  <VAT_Amount>32</VAT_Amount>
                  <Amount>160</Amount>
                </Tax_Lines>
                <Footer_Lines>
                  <Print_ID>100</Print_ID>
                  <Section_Line_No.>1</Section_Line_No.>
                  <Footer_Line>Welcome again</Footer_Line>
                </Footer_Lines>
              </Response_Body>
            </Response>
			  */

            #endregion

            LoyTransaction transaction = new LoyTransaction();

            XmlDocument document = new XmlDocument();
            document.LoadXml(responseXml);

            #region Header

            List<LoyTransactionHeader> headerLinesList = new List<LoyTransactionHeader>();
            XmlNodeList headerLines = document.SelectNodes("//Header_Lines");
            if (headerLines != null)
            {
                foreach (XmlNode transactionHeader in headerLines)
                {
                    LoyTransactionHeader tranHeader = new LoyTransactionHeader();

                    XmlNode headernode = transactionHeader.SelectSingleNode("Header_Line");
                    if (headernode == null)
                        throw new XmlException("Header Line node not found in response xml");
                    tranHeader.HeaderDescription = headernode.InnerText;
                    headerLinesList.Add(tranHeader);
                }
            }

            #endregion

            #region Footer

            List<LoyTransactionFooter> footerLinesList = new List<LoyTransactionFooter>();
            XmlNodeList footerLines = document.SelectNodes("//Footer_Lines");
            if (footerLines != null)
            {
                foreach (XmlNode transactionFooter in footerLines)
                {
                    LoyTransactionFooter footer = new LoyTransactionFooter();

                    XmlNode headernode = transactionFooter.SelectSingleNode("Footer_Line");
                    if (headernode == null)
                        throw new XmlException("Header Line node not found in response xml");
                    footer.FooterDescription = headernode.InnerText;
                    footerLinesList.Add(footer);
                }
            }

            #endregion

            XmlNode node = document.SelectSingleNode("//Transaction_No.");
            if (node == null)
                throw new XmlException(" transaction no node not found in response xml");
            transaction.Id = node.InnerText;

            node = document.SelectSingleNode("//Receipt_No.");
            if (node == null)
                throw new XmlException("Receipt_No. node not found ");
            transaction.ReceiptNumber = node.InnerText;

            node = document.SelectSingleNode("//Staff_ID");
            if (node == null)
                throw new XmlException("Staff_ID node not found ");
            transaction.Staff = node.InnerText;

            XmlNode time = document.SelectSingleNode("//Trans._Time ");
            XmlNode date = document.SelectSingleNode("//Trans._Date ");
            if (date != null && time != null)
            {
                string xmldateTime = date.InnerText + " " + time.InnerText;
                DateTime.TryParse(xmldateTime, out DateTime dateTime);
                transaction.Date = dateTime;
            }

            List<LoySaleLine> saleLines = new List<LoySaleLine>();
            XmlNodeList itemLines = document.SelectNodes("//Item_Lines");
            foreach (XmlNode itemLine in itemLines)
            {
                LoyItem item = new LoyItem();
                LoySaleLine sl = new LoySaleLine();
                Price price = new Price();

                string lineno = itemLine.SelectSingleNode("Line_No.").InnerText;
                sl.Id = lineno;

                XmlNodeList disclines = document.SelectNodes("//Discount_Info");
                foreach (XmlNode discline in disclines)
                {
                    string line = discline.SelectSingleNode("Line_No.").InnerText;
                    if (line == lineno)
                    {
                        node = discline.SelectSingleNode("Detail_Text");
                        if (node != null)
                        {
                            sl.ExtraInfoLines.Add(node.InnerText);
                        }
                        node = discline.SelectSingleNode("Detail_Amount");
                        if (node == null)
                            throw new XmlException("Detail_Amount node not found");

                        sl.DiscountAmount = ConvertTo.SafeDecimalString(node.InnerText);
                        sl.DiscountAmt = ConvertTo.SafeDecimal(sl.DiscountAmount);
                    }
                }

                node = itemLine.SelectSingleNode("Item_No.");
                if (node == null)
                    throw new XmlException("Item no is empty");
                item.Id = node.InnerText;

                node = itemLine.SelectSingleNode("Item_Description");
                if (node == null)
                    throw new XmlException("ITem description node not found");
                item.Description = node.InnerText;

                node = itemLine.SelectSingleNode("Variant_Code");
                if (node == null)
                    throw new XmlException("Variant code node empty");
                sl.VariantReg = new VariantRegistration(node.InnerText);

                node = itemLine.SelectSingleNode("UOM_ID");
                if (node == null)
                    throw new XmlException("Uom ID node not found");
                sl.Uom = new UnitOfMeasure(node.InnerText, 0);

                node = itemLine.SelectSingleNode("UOM_Description");
                if (node == null)
                    throw new XmlException("Uom description node not found");
                sl.Uom.Description = node.InnerText;

                node = itemLine.SelectSingleNode("Quantity");
                if (node == null)
                    throw new XmlException("Quantity node not found");
                sl.Quantity = ConvertTo.SafeDecimal(node.InnerText);

                node = itemLine.SelectSingleNode("Net_Amount");
                if (node == null)
                    throw new XmlException("net amount node not found");
                sl.NetAmount = ConvertTo.SafeDecimalString(node.InnerText);
                sl.NetAmt = ConvertTo.SafeDecimal(sl.NetAmount);

                node = itemLine.SelectSingleNode("VAT_Amount");
                if (node == null)
                    throw new XmlException("Vat amount node not found");
                sl.VatAmount = ConvertTo.SafeDecimalString(node.InnerText);
                sl.VatAmt = ConvertTo.SafeDecimal(sl.VatAmount);

                //this is the VAT %, i.e. 25 is 25% tax
                node = itemLine.SelectSingleNode("VAT__");
                if (node == null)
                    throw new XmlException("vat  amount node not found");
                sl.VatAmount = ConvertTo.SafeDecimalString(node.InnerText);
                sl.VatAmt = ConvertTo.SafeDecimal(sl.VatAmount);
                if (string.IsNullOrWhiteSpace(sl.VatAmount) == false && sl.VatAmount.Contains("%") == false)
                    sl.VatAmount += "%";

                node = itemLine.SelectSingleNode("Amount");
                if (node == null)
                    throw new XmlException("amount node not found");
                sl.Amount = ConvertTo.SafeDecimalString(node.InnerText);
                sl.Amt = ConvertTo.SafeDecimal(sl.Amount);

                //Price
                node = itemLine.SelectSingleNode("Price");
                if (node == null)
                    throw new XmlException("price node not found");
                price.Amount = ConvertTo.SafeDecimalString(node.InnerText);
                price.Amt = ConvertTo.SafeDecimal(price.Amount);

                price.ItemId = item.Id;
                price.UomId = sl.Uom.Id;
                price.VariantId = sl.VariantReg.Id;

                sl.Uom.ItemId = item.Id;
                sl.Uom.QtyPerUom = sl.Quantity;
                sl.Item = item;
                sl.Item.Prices.Add(price);
                sl.Item.UnitOfMeasures = new List<UnitOfMeasure>();
                sl.Item.UnitOfMeasures.Add(sl.Uom);
                sl.Item.VariantsRegistration = new List<VariantRegistration>();
                sl.Item.VariantsRegistration.Add(sl.VariantReg);

                saleLines.Add(sl);
            }

            List<LoyTenderLine> tenderLineList = new List<LoyTenderLine>();
            XmlNodeList tenderLines = document.SelectNodes("//Tender_Lines");
            if (tenderLines == null)
                throw new XmlException("Tenderlines not found ");

            foreach (XmlNode tenderLine in tenderLines)
            {
                LoyTenderLine tl = new LoyTenderLine();
                node = tenderLine.SelectSingleNode("Tender_Type");
                if (node == null)
                    throw new XmlException("Tender type node not found");
                tl.Type = node.InnerText;

                node = tenderLine.SelectSingleNode("Tender_Description");
                if (node == null)
                    throw new XmlException("Tender desc node not found");
                tl.Description = node.InnerText;

                node = tenderLine.SelectSingleNode("Amount_In_Tender");
                if (node == null)
                    throw new XmlException("Amount_In_Tender  node not found");
                tl.Amount = ConvertTo.SafeDecimalString(node.InnerText);
                tl.Amt = ConvertTo.SafeDecimal(tl.Amount);

                tenderLineList.Add(tl);
            }

            decimal transactionFullAmount = 0;
            XmlNodeList totalLines = document.SelectNodes("//Total_Lines");
            foreach (XmlNode totalLine in totalLines)
            {
                node = totalLine.SelectSingleNode("Total_Amount");
                if (node == null)
                    throw new XmlException("total amount node not found");
                transactionFullAmount = ConvertTo.SafeDecimal(node.InnerText);
            }

            List<TaxLine> taxLinesList = new List<TaxLine>();
            XmlNodeList taxLines = document.SelectNodes("//Tax_Lines");
            if (taxLines != null)
            {
                foreach (XmlNode taxLine in taxLines)
                {
                    TaxLine tl = new TaxLine();

                    XmlNode selectSingleNode = taxLine.SelectSingleNode("VAT_Code");
                    if (selectSingleNode == null)
                        throw new XmlException("Vat code node not found");
                    tl.TaxCode = selectSingleNode.InnerText;

                    selectSingleNode = taxLine.SelectSingleNode("Net_Amount");
                    if (selectSingleNode == null)
                        throw new XmlException("net amt code node not found");
                    tl.NetAmt = ConvertTo.SafeDecimalString(selectSingleNode.InnerText);
                    tl.NetAmnt = ConvertTo.SafeDecimal(tl.NetAmt);

                    selectSingleNode = taxLine.SelectSingleNode("Amount");
                    if (selectSingleNode == null)
                        throw new XmlException("amount code node not found");
                    tl.Amount = ConvertTo.SafeDecimalString(selectSingleNode.InnerText);
                    tl.Amt = ConvertTo.SafeDecimal(tl.Amount);

                    selectSingleNode = taxLine.SelectSingleNode("VAT__");
                    if (selectSingleNode == null)
                        throw new XmlException("VAT__ node not found");
                    tl.TaxDesription = selectSingleNode.InnerText;
                    if (string.IsNullOrWhiteSpace(tl.TaxDesription) == false && tl.TaxDesription.Contains("%") == false)
                        tl.TaxDesription += "%";

                    selectSingleNode = taxLine.SelectSingleNode("VAT_Amount");
                    if (selectSingleNode == null)
                        throw new XmlException("VAT_Amount node not found");
                    tl.TaxAmount = ConvertTo.SafeDecimalString(selectSingleNode.InnerText);
                    tl.TaxAmt = ConvertTo.SafeDecimal(tl.TaxAmount);

                    taxLinesList.Add(tl);
                }
            }

            transaction.TaxLines = taxLinesList;
            transaction.TenderLinesCount = tenderLineList.Count;
            transaction.TenderLines = tenderLineList;
            transaction.SaleLines = saleLines;

            decimal vat = 0.0M;
            decimal net = 0.0M;
            decimal dis = 0.0M;
            decimal taxNet = 0.0M;
            decimal taxVat = 0.0M;

            foreach (LoySaleLine sline in transaction.SaleLines)
            {
                vat += sline.VatAmt;
                net += sline.NetAmt;
                dis += sline.DiscountAmt;
            }

            foreach (TaxLine txLine in transaction.TaxLines)
            {
                taxNet += txLine.NetAmnt;
                taxVat += txLine.TaxAmt;
            }

            transaction.NetAmount = ConvertTo.SafeStringDecimal(taxNet);
            transaction.VatAmount = ConvertTo.SafeStringDecimal(taxVat);
            transaction.NetAmt = taxNet;
            transaction.VatAmt = taxVat;

            transaction.SaleLinesCount = saleLines.Count;
            transaction.TransactionHeaders = headerLinesList;
            transaction.TransactionFooters = footerLinesList;

            transaction.Amt = transactionFullAmount - dis;
            transaction.Amount = ConvertTo.SafeStringDecimal(transaction.Amt);
            transaction.DiscountAmt = dis;
            transaction.DiscountAmount = ConvertTo.SafeStringDecimal(transaction.DiscountAmt);

            return transaction;
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
                throw new XmlException("LS_Retail_Version node not found in response xml");
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
