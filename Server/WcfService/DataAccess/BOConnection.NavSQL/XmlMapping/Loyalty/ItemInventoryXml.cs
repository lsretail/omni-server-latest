using System.Xml;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSOmni.Common.Util;

namespace LSOmni.DataAccess.BOConnection.NavSQL.XmlMapping.Loyalty
{
    public class ItemInventoryXml : BaseXml
    {
        //Request IDs
        private const string ItemsInStockRequestId = "MM_MOBILE_GET_ITEMS_IN_STOCK";

        public ItemInventoryXml()
        {
        }

        public string ItemsInStockRequestXML(string itemId, string variantId, int arrivingInStockInDays, List<string> locationIds)
        {
            /*
            <?xml version="1.0" encoding="utf-8" standalone="no"?>
            <Request>
              <Request_ID>MM_MOBILE_GET_ITEMS_IN_STOCK</Request_ID>
              <Request_Body>
                <Item_No>10000</Item_No>
                <Variant_Code></Variant_Code>
                <Stock_Days></Stock_Days>
                  <WS_Inventory_Buffer>
                    <Store_No>S0001</Store_No>
                 </WS_Inventory_Buffer>	
                 <WS_Inventory_Buffer>	
                   <Store_No>S0002</Store_No>
                  </WS_Inventory_Buffer>	
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
            requestId.InnerText = ItemsInStockRequestId;
            request.AppendChild(requestId);

            // Create the Request Body element
            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            // Create the item ID element
            XmlElement itemIdElement = document.CreateElement("Item_No");
            itemIdElement.InnerText = itemId;
            requestBody.AppendChild(itemIdElement);

            // Create the variant code element
            XmlElement variantCodeElement = document.CreateElement("Variant_Code");
            variantCodeElement.InnerText = variantId;
            requestBody.AppendChild(variantCodeElement);

            // Create the variant code element
            XmlElement stockDaysElement = document.CreateElement("Stock_Days");
            stockDaysElement.InnerText = arrivingInStockInDays.ToString();
            requestBody.AppendChild(stockDaysElement);

            //must send an empty store, will get errorcode 1410 back
            if (locationIds.Count == 0)
            {
                // Create the inventoryBuffer element
                XmlElement inventoryBufferBody = document.CreateElement("WS_Inventory_Buffer");
                requestBody.AppendChild(inventoryBufferBody);

                XmlElement storeNoElement = document.CreateElement("Store_No");
                storeNoElement.InnerText = "";
                inventoryBufferBody.AppendChild(storeNoElement);
            }
            else
            {
                foreach (string storeId in locationIds)
                {
                    // Create the inventoryBuffer element
                    XmlElement inventoryBufferBody = document.CreateElement("WS_Inventory_Buffer");
                    requestBody.AppendChild(inventoryBufferBody);

                    // Create the variant code element
                    XmlElement storeNoElement = document.CreateElement("Store_No");
                    storeNoElement.InnerText = storeId.ToUpper();
                    inventoryBufferBody.AppendChild(storeNoElement);
                }
            }
            return document.OuterXml;
        }

        public List<InventoryResponse> ItemsInStockResponseXML(string responseXml, string itemId, string variantId)
        {
            /*
                <?xml version="1.0" encoding="utf-8" standalone="no"?>
                <Response>
                    <Request_ID>MM_MOBILE_GET_ITEMS_IN_STOCK</Request_ID>
                    <Response_Code>0000</Response_Code>
                    <Response_Text></Response_Text>
                    <Response_Body>
                        <Response_Line>
                            <Store_No>S0001</Store_No>
                            <Base_Unit_of_Measure>BOTTLE</Base_Unit_of_Measure>
                            <Inventory>99</Inventory>
                            <Qty._Sold_not_Posted>5</Qty._Sold_not_Posted>
                            <Actual_Inventory>94</Actual_Inventory>
                            <Expected_Stock>0</Expected_Stock>
                            <Reorder_Point>100</Reorder_Point>
                        </Response_Line>
                        <Response_Line>
                            <Store_No>S0001</Store_No>
                            <Base_Unit_of_Measure>BOTTLE</Base_Unit_of_Measure>
                            <Inventory>98</Inventory>
                            <Qty._Sold_not_Posted>9</Qty._Sold_not_Posted>
                            <Actual_Inventory>89</Actual_Inventory>
                            <Expected_Stock>29</Expected_Stock>
                            <Reorder_Point>100</Reorder_Point>
                        </Response_Line>
                    </Response_Body>
                </Response>
            */

            List<InventoryResponse> inventoryList = new List<InventoryResponse>();

            // Create the xml document containe
            XmlDocument document = new XmlDocument();
            document.LoadXml(responseXml);

            XmlNodeList nodelist = document.SelectNodes("//Response_Line");
            foreach (XmlNode nodeLoop in nodelist)
            {
                InventoryResponse inventory = new InventoryResponse();
                inventory.ItemId = itemId;
                inventory.VariantId = variantId;

                XmlNode node = nodeLoop.SelectSingleNode("Store_No");
                if (node == null)
                    throw new XmlException("Store_No node not found in response xml");
                inventory.StoreId = node.InnerText;

                node = nodeLoop.SelectSingleNode("Base_Unit_of_Measure");
                if (node == null)
                    throw new XmlException("Base_Unit_of_Measure node not found in response xml");
                inventory.BaseUnitOfMeasure = node.InnerText;

                decimal temp = 0L;
                node = nodeLoop.SelectSingleNode("Inventory");
                if (node == null)
                    throw new XmlException("Inventory node not found in response xml");
                if (string.IsNullOrWhiteSpace(node.InnerText) == false)
                    temp = ConvertTo.SafeDecimal(node.InnerText);
                inventory.QtyInventory = temp;

                temp = 0L;
                node = nodeLoop.SelectSingleNode("Qty._Sold_not_Posted");
                if (node == null)
                    throw new XmlException("Qty._Sold_not_Posted node not found in response xml");
                if (string.IsNullOrWhiteSpace(node.InnerText) == false)
                    temp = ConvertTo.SafeDecimal(node.InnerText);
                inventory.QtySoldNotPosted = temp;

                temp = 0L;
                node = nodeLoop.SelectSingleNode("Actual_Inventory");
                if (node == null)
                    throw new XmlException("Actual_Inventory node not found in response xml");
                if (string.IsNullOrWhiteSpace(node.InnerText) == false)
                    temp = ConvertTo.SafeDecimal(node.InnerText);
                inventory.QtyActualInventory = temp;

                temp = 0L;
                node = nodeLoop.SelectSingleNode("Expected_Stock");
                if (node == null)
                {
                    inventory.QtyExpectedStock = temp;
                    //throw new XmlException("Expected_Stock node not found in response xml");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(node.InnerText) == false)
                        temp = ConvertTo.SafeDecimal(node.InnerText);
                    inventory.QtyExpectedStock = temp;
                }

                temp = 0L;
                node = nodeLoop.SelectSingleNode("Reorder_Point");
                if (node != null)
                {
                    if (string.IsNullOrWhiteSpace(node.InnerText) == false)
                        temp = ConvertTo.SafeDecimal(node.InnerText);
                    inventory.ReorderPoint = temp;
                }

                inventoryList.Add(inventory);
            }
            return inventoryList;
        }
    }
}
