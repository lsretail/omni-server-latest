using System;
using System.IO;
using System.Runtime.Serialization.Json;
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

		/// <summary>
		/// Serializes an existing object of type {T} to XML.
		/// </summary>
		/// <typeparam name="T">Type of the object.</typeparam>
		/// <param name="value">object to be serialized.</param>
		/// <param name="prettyPrint">If true then the resulting XML will be formatted for pretty print.</param>
		/// <returns></returns>
		/// <remarks>Because the type of the object that needs to be serialized is known, the method will not throw <exception cref="NullReferenceException" /> if the <paramref name="value"/> is null</remarks>
		public static string ToXml<T>(T value, bool prettyPrint)
		{
			string result = string.Empty;

			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T), string.Empty);

				XmlSerializerNamespaces xmlNamespaces = new XmlSerializerNamespaces();
				xmlNamespaces.Add(string.Empty, string.Empty);

				XmlWriterSettings xmlSettings = new XmlWriterSettings()
				{
					Encoding = Encoding.Unicode,
					OmitXmlDeclaration = true,
					ConformanceLevel = prettyPrint ? ConformanceLevel.Document : ConformanceLevel.Fragment,
					Indent = true
				};

				using (MemoryStream ms = new MemoryStream())
				{
					using (XmlWriter xw = XmlWriter.Create(ms, xmlSettings))
					{
						xw.WriteWhitespace(string.Empty);
						serializer.Serialize(xw, value, xmlNamespaces);
					}

					ms.Position = 0;
					using (StreamReader sr = new StreamReader(ms))
					{
						result = sr.ReadToEnd();
					}
				}

				result = RemoveNode("Password", result);
			}
			catch (Exception)
            {
				return "Error Logging data for object: " + value.ToString();
            }
			return result;
		}

		/// <summary>
		/// Serializes an existing object of type {T} to XML using the <see cref="System.Runtime.Serialization.DataContractSerializer"/>.
		/// </summary>
		/// <typeparam name="T">Type of the object.</typeparam>
		/// <param name="value">object to be serialized.</param>
		/// <param name="prettyPrint">If true then the resulting XML will be formatted for pretty print.</param>
		/// <returns></returns>
		/// <remarks>
		/// Because the type of the object that needs to be serialized is known, the method will not throw <exception cref="NullReferenceException" /> if the <paramref name="value"/> is null.
		/// The DataContractSerializer is 10% faster than the classic XmlSerializer and can serialize objects with read-only properties like <see cref="LSRetail.Omni.Domain.DataModel.Pos.Transactions.PaymentLine"/>.
		/// </remarks>
		public static string ToDataContract<T>(T value, bool prettyPrint)
		{
			string contract = string.Empty;

			System.Runtime.Serialization.DataContractSerializerSettings settings = new System.Runtime.Serialization.DataContractSerializerSettings
			{
				SerializeReadOnlyTypes = true
			};

			XmlWriterSettings xmlSettings = new XmlWriterSettings()
			{
				Encoding = Encoding.Unicode,
				OmitXmlDeclaration = true,
				ConformanceLevel = prettyPrint ? ConformanceLevel.Document : ConformanceLevel.Fragment,
				Indent = true,
				CheckCharacters = false
			};

			using (MemoryStream ms = new MemoryStream())
			{
				using (XmlWriter xw = XmlWriter.Create(ms, xmlSettings))
				{
					var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T), settings);

					xw.WriteWhitespace(string.Empty);
					serializer.WriteObject(xw, value);
				};

				ms.Position = 0;
				using (StreamReader sr = new StreamReader(ms, Encoding.Unicode))
				{
					contract = sr.ReadToEnd();
				}
			}

			return contract;
		}

		public static string RemoveNode(string nodeName, string xml)
		{
			try
			{
				//remove a node from XML,, nodeName=Password removes value between <Password>
				string regex = "<" + nodeName + ">.*?</" + nodeName + ">"; //strip out 
				return System.Text.RegularExpressions.Regex.Replace(xml, regex, "<" + nodeName + ">XXX</" + nodeName + ">");
			}
			catch
			{
				return xml;
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
        /// Use this function to test Json Serialize if error occurs
        /// </summary>
        /// <param name="classname"></param>
        /// <param name="classdata"></param>
        /// <example>
        /// <code>
        /// TestJsonSerialize(typeof(MobileMenu), mobileMenu);
        /// </code>
        /// </example>
        public static void TestJsonSerialize(Type classname, object classdata)
        {
            string json;
            using (MemoryStream ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(classname);
                ser.WriteObject(ms, classdata);
                json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
            }
        }

		public static T Deserialize<T>(string json)
		{
			T obj = Activator.CreateInstance<T>();
			MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
			obj = (T)serializer.ReadObject(ms);
			ms.Close();
			return obj;
		}
	}
}
