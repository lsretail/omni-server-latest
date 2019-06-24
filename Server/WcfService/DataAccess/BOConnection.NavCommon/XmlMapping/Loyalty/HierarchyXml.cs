using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    public class HierarchyXml : BaseXml
    {
        private const string GetHierarchyRequestId = "GET_HIERARCHY";
        private const string GetHierarchyNodeRequestId = "GET_HIERARCHY_NODE";

        public HierarchyXml()
        {
        }

        public string HierarchyRequestXML(string storeId)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>GET_HIERARCHY</Request_ID>
              <Request_Body>
                <StoreNo>S0013</StoreNo>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", GetHierarchyRequestId),
                    new XElement("Request_Body",
                        new XElement("StoreNo", storeId)
                    )
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            return doc.ToString();
        }

        public List<Hierarchy> HierarchyResponseXML(string responseXml, out List<HierarchyNode> nodes)
        {
            #region xml
            /*
            <Response>
              <Request_ID>GET_HIERARCHY</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
              </Response_Body>
            </Response>

             */
            #endregion

            List<Hierarchy> rs = new List<Hierarchy>();

            XDocument doc = XDocument.Parse(responseXml);
            var hierarchies = doc.Element("Response").Element("Response_Body").Descendants("Hierarchy");
            foreach (XElement hir in hierarchies)
            {
                Hierarchy hierarchy = new Hierarchy();

                if (hir.Element("Hierarchy_Code") == null)
                    throw new XmlException("Hierarchy_Code node not found in response xml");
                hierarchy.Id = hir.Element("Hierarchy_Code").Value;

                if (hir.Element("Description") != null)
                    hierarchy.Description = hir.Element("Description").Value;

                if (hir.Element("Type") == null)
                    throw new XmlException("Type node not found in response xml");
                hierarchy.Type = (HierarchyType)Convert.ToInt32(hir.Element("Type").Value);

                rs.Add(hierarchy);
            }

            nodes = new List<HierarchyNode>();
            var hnodes = doc.Element("Response").Element("Response_Body").Descendants("Hierarchy_Node");
            foreach (XElement nd in hnodes)
            {
                HierarchyNode node = new HierarchyNode();

                if (nd.Element("Node_ID") == null)
                    throw new XmlException("Node_ID node not found in response xml");
                node.Id = nd.Element("Node_ID").Value;

                if (nd.Element("Description") != null)
                    node.Description = nd.Element("Description").Value;

                if (nd.Element("Indentation") == null)
                    throw new XmlException("Indentation node not found in response xml");
                node.Indentation = Convert.ToInt32(nd.Element("Indentation").Value);

                if (nd.Element("Presentation_Order") == null)
                    throw new XmlException("Presentation_Order node not found in response xml");
                node.PresentationOrder = Convert.ToInt32(nd.Element("Presentation_Order").Value);

                if (nd.Element("Hierarchy_Code") == null)
                    throw new XmlException("Hierarchy_Code node not found in response xml");
                node.HierarchyCode = nd.Element("Hierarchy_Code").Value;

                if (nd.Element("Parent_Node_ID") == null)
                    throw new XmlException("Parent_Node_ID node not found in response xml");
                node.ParentNode = nd.Element("Parent_Node_ID").Value;

                nodes.Add(node);
            }

            return rs;
        }

        public string HierarchyNodeRequestXML(string hierarchyCode, string nodeId)
        {
            #region xml
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>GET_HIERARCHY_NODE</Request_ID>
              <Request_Body>
                <StoreNo>S0013</StoreNo>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", GetHierarchyNodeRequestId),
                    new XElement("Request_Body",
                        new XElement("HierarchyCode", hierarchyCode),
                        new XElement("NodeID", nodeId)
                    )
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            return doc.ToString();
        }

        public List<HierarchyLeaf> HierarchyNodeResponseXML(string responseXml)
        {
            #region xml
            /*
            <Response>
              <Request_ID>GET_HIERARCHY_NODE</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
              </Response_Body>
            </Response>

             */
            #endregion

            List<HierarchyLeaf> rs = new List<HierarchyLeaf>();

            XDocument doc = XDocument.Parse(responseXml);
            var nodes = doc.Element("Response").Element("Response_Body").Descendants("Hierarchy_Node_Link");
            foreach (XElement hir in nodes)
            {
                HierarchyLeaf node = new HierarchyLeaf();

                if (hir.Element("Hierarchy_Code") == null)
                    throw new XmlException("Hierarchy_Code node not found in response xml");
                node.Id = hir.Element("Hierarchy_Code").Value;

                if (hir.Element("Node_ID") == null)
                    throw new XmlException("Node_ID node not found in response xml");
                node.Id = hir.Element("Node_ID").Value;

                if (hir.Element("No.") == null)
                    throw new XmlException("No. node not found in response xml");
                node.Id = hir.Element("No.").Value;

                if (hir.Element("Description") != null)
                    node.Description = hir.Element("Description").Value;

                if (hir.Element("Type") == null)
                    throw new XmlException("Type node not found in response xml");
                node.Type = (HierarchyLeafType)Convert.ToInt32(hir.Element("Type").Value);

                rs.Add(node);
            }

            return rs;
        }
    }
}
