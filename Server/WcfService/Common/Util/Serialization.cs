using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LSOmni.Common.Util
{
    public static class Serialization
    {
        public static T DeserializeFromXml<T>(string xml)
        {
            T result;
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (TextReader tr = new StringReader(xml))
            {
                result = (T)ser.Deserialize(tr);
            }
            return result;
        }

        public static string SerializeToXml(object value)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(value.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                serializer.Serialize(ms, value, namespaces);
                ms.Position = 0;
                serializer = null;
                xmlDoc.Load(ms);
                return xmlDoc.InnerXml.Replace("<?xml version=\"1.0\"?>", "");
            }
        }

        public static bool TestXmlSerialize(Type classname, object classdata)
        {
            try
            {
                string xml;
                using (MemoryStream ms = new MemoryStream())
                {
                    XmlSerializer ser = new XmlSerializer(classname);
                    ser.Serialize(ms, classdata);
                    xml = Encoding.UTF8.GetString(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Use this function to test Json Serialize if error occures
        /// Sample: TestJsonSerialize(typeof(MobileMenu), mobileMenu);
        /// </summary>
        /// <param name="classname"></param>
        /// <param name="classdata"></param>
        public static bool TestJsonSerialize(Type classname, object classdata)
        {
            try
            {
                string json;
                using (MemoryStream ms = new MemoryStream())
                {
                    var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(classname);
                    ser.WriteObject(ms, classdata);
                    json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
