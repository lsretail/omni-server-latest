using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    public class StoreServicesXml : BaseXml
    {
        private XDocument doc;

        public StoreServicesXml(string xml)
        {
            doc = XDocument.Parse(xml, LoadOptions.None);
        }

        public List<StoreServices> ParseXml()
        {
            List<StoreServices> StoreServicesList = new List<StoreServices>();
 

            IEnumerable<XElement> elServices = doc.Element("StoreServices").Descendants("StoreService");
            foreach (XElement elSer in elServices)
            {
                StoreServices service = new StoreServices();
                service.StoreId = this.GetValue(elSer, "StoreId");
                service.Description = this.GetValue(elSer, "Description");
                service.StoreServiceType = (StoreServiceType)Convert.ToInt32(this.GetValue(elSer, "StoreServiceType"));
                StoreServicesList.Add(service);
            }
            return StoreServicesList;
        }
 
        /*
        <?xml version="1.0" encoding="utf-8"?> 
        <StoreServices Version="">
	        <StoreService>
	          <StoreId>S0001</StoreId>
		        <Description>Free Wi-Fi</Description>
		        <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">2</StoreServiceType>
	        </StoreService>
	        <StoreService>
	          <StoreId>S0001</StoreId>
		        <Description>PlayPlaces. Super-fun for the kids, stress-free for you!</Description>
		        <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">5</StoreServiceType>
	        </StoreService>
	        <StoreService>
	          <StoreId>S0001</StoreId>
		        <Description>Drive thru window</Description>
		        <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">3</StoreServiceType>
	        </StoreService>	
	        <StoreService>
	          <StoreId>S0001</StoreId>
		        <Description>Free refill</Description>
		        <StoreServiceType enum="none=0,FreeWiFi=2,DriveThruWindow=3,GiftCard=4,PlayPlace=5,FreeRefill=6">6</StoreServiceType>
	        </StoreService>		
        </StoreServices>
        */

        private string GetValue(XElement elIn, string name, string defaultValue = "")
        {
            //string text = (string)elIn.Element(name) ?? ""; //if name not found returns blank. But I want a good error msg

            if (elIn.Element(name) == null)
                throw new XmlException(name + " node not found in xml " + elIn.ToString());
            string val = elIn.Element(name).Value;
            if (string.IsNullOrWhiteSpace(defaultValue) == false && string.IsNullOrWhiteSpace(val))
                val = defaultValue;
            return val;
        } 
    }
}
