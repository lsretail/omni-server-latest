using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping.Loyalty
{
    // 
    public class AdvertisementXml : BaseXml
    {
        private XDocument doc;

        public AdvertisementXml(string xml)
        {
            doc = XDocument.Parse(xml, LoadOptions.None);
        }

        public List<Advertisement> ParseXml(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = string.Empty;
            }
            List<Advertisement> ads = new List<Advertisement>();
            ValidateXmlDoc();
            int i = 1;
            IEnumerable<XElement> elAds = doc.Element("Advertisements").Descendants("Advertisement")
                .Where(x => x.Attribute("Id")?.Value == id || !x.HasAttributes);
          
            
            foreach (XElement elAd in elAds)
            {
                Advertisement ad = new Advertisement();
                ad.Id = this.GetValue(elAd, "Id");
                ad.Description = this.GetValue(elAd, "Description");
                ad.ExpirationDate = GetExpirationDate(this.GetValue(elAd, "ExpirationDate"));
                ad.AdType = (AdvertisementType)Convert.ToInt32(this.GetValue(elAd, "AdType"));
                ad.AdValue = this.GetValue(elAd, "AdValue");
                string imageId = this.GetValue(elAd, "ImageId");
                ad.ImageView = new ImageView(imageId);
                ad.RV = i;
                i++;
                ads.Add(ad);
            }
            return ads;
        }

        private DateTime GetExpirationDate(string expDate)
        {
            DateTime dateOut = DateTime.Now.AddYears(1);
            string format = "yyyy-MM-dd"; // "yyyyMMddHHmmss"; 2015-07-01
            if (string.IsNullOrEmpty(expDate) == false)
            {
                if (DateTime.TryParseExact(expDate, format, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime dateTime))
                    dateOut = dateTime.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            return dateOut;
        }
        /*
        <?xml version="1.0" encoding="utf-8"?> 
        <Advertisements Version="">
            <Advertisement>
              <Id>home</Id>
                <Description>20% off</Description>
                <ExpirationDate>2014-06-01</ExpirationDate>
                <ImageId>40020</ImageId>
                <AdType enum="none=0,itemid=1,url=2,MenuNodeId=3">1</AdType>
                <AdValue>40020</AdValue> 
            </Advertisement>
            <Advertisement>
              <Id>home</Id>
                <Description>20% off</Description>
                <ExpirationDate>2014-06-01</ExpirationDate>
                <ImageId>40030</ImageId>
                <AdType enum="none=0,itemid=1,url=2,MenuNodeId=3">1</AdType>
                <AdValue>40030</AdValue>
     
            </Advertisement>
        </Advertisements>
        */

        private string GetValue(XElement elIn, string name, string defaultValue = "")
        {
            //string text = (string)elIn.Element(name) ?? ""; //if name not found returns blank. But I want a good error msg

            if (elIn.Element(name) == null)
                throw new XmlException(name + " node not found in XML " + elIn.ToString());
            string val = elIn.Element(name).Value;
            if (string.IsNullOrWhiteSpace(defaultValue) == false && string.IsNullOrWhiteSpace(val))
                val = defaultValue;
            return val;
        }
		
        private void ValidateXmlDoc()
        {
            //minimum XML validated
            XElement elAd = doc.Element("Advertisements").Element("Advertisement");
            if (elAd == null)
                throw new XmlException("Advertisements.Advertisement node not found in XML");
            if (elAd.Element("Id") == null)
                throw new XmlException("Advertisement Id node not found in XML");
            if (elAd.Element("Description") == null)
                throw new XmlException("Advertisement Description node not found in XML");
            if (elAd.Element("ExpirationDate") == null)
                throw new XmlException("Advertisement ExpirationDate node not found in XML");
            if (elAd.Element("AdType") == null)
                throw new XmlException("Advertisement AdType MenuNode node not found in XML");
            if (elAd.Element("AdValue") == null)
                throw new XmlException("Advertisement AdValue MenuNode node not found in XML");
        }
    }
}
