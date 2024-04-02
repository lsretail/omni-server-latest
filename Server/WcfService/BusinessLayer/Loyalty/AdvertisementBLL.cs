using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.Common.Util;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using System.Linq;

namespace LSOmni.BLL.Loyalty
{
    public class AdvertisementBLL : BaseLoyBLL
    {
        public AdvertisementBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
        }

        public virtual List<Advertisement> AdvertisementsGetById(string id, string contactId, Statistics stat)
        {
            //call local db or BO web service. 
            List<Advertisement> ads = new List<Advertisement>();

            try
            {
                //not exist in cache so get them from BO
                ads = AdvertisementsGetById(id, stat);
            }
            catch (LSOmniServiceException ex)
            {
                //ignore the error and return what is in the cache, if anything
                logger.Error(config.LSKey.Key, ex, "Something failed when calling BackOffice but continue...");
                throw;
            }
            return ads;
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

        private List<Advertisement> AdvertisementsGetById(string id, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            List<Advertisement> ads = new List<Advertisement>();
            string fullFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Xml", "navdata_ads.xml");
            if (File.Exists(fullFileName) == false)
                return ads;

            string xml = File.ReadAllText(fullFileName);
            ads = ParseXml(xml, id);
            logger.StatisticEndSub(ref stat, index);
            return ads;
        }

        private List<Advertisement> ParseXml(string xml, string id)
        {
            XDocument doc = XDocument.Parse(xml, LoadOptions.None);

            if (string.IsNullOrEmpty(id))
            {
                id = string.Empty;
            }
            List<Advertisement> ads = new List<Advertisement>();
            ValidateXmlDoc(doc);
            int i = 1;
            IEnumerable<XElement> elAds = doc.Element("Advertisements").Descendants("Advertisement")
                .Where(x => x.Attribute("Id")?.Value == id || !x.HasAttributes);

            foreach (XElement elAd in elAds)
            {
                Advertisement ad = new Advertisement();
                ad.Id = XMLHelper.GetXMLValue(elAd, "Id");
                ad.Description = XMLHelper.GetXMLValue(elAd, "Description");
                ad.ExpirationDate = GetExpirationDate(XMLHelper.GetXMLValue(elAd, "ExpirationDate"));
                ad.AdType = (AdvertisementType)Convert.ToInt32(XMLHelper.GetXMLValue(elAd, "AdType"));
                ad.AdValue = XMLHelper.GetXMLValue(elAd, "AdValue");
                string imageId = XMLHelper.GetXMLValue(elAd, "ImageId");
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

        private void ValidateXmlDoc(XDocument doc)
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
