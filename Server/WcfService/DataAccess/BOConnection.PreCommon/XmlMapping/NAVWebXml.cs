using System;
using System.Data;
using System.Collections.Generic;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.PreCommon.XmlMapping
{
    public class NAVWebXml : BaseXml
    {
        private string AppId = string.Empty;
        private string AppType = string.Empty;
        private string StoreId = string.Empty;

        public NAVWebXml()
        {
        }

        public NAVWebXml(string storeId, string appId, string apptype)
        {
            this.StoreId = storeId;
            this.AppId = appId;
            this.AppType = apptype;
        }

        public string RegisterApplicationRequestXML(Version navVer)
        {
            #region xml
            /*
            <Request>
              <Request_ID>REGISTER_APPLICATION</Request_ID>
              <Request_Body>
                <Application_Type>NOP</Application_Type>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
                <Store_No>S0001</Store_No>
              </Request_Body>
            </Request>
             */
            #endregion xml

            XElement body = new XElement("Request_Body",
                                new XElement("Application_Type", this.AppType),
                                new XElement("Application_ID", this.AppId),
                                new XElement("Store_No", this.StoreId));

            switch (this.AppType.ToUpper())
            {
                case "ECOM":
                    AddEComTableData(body, navVer);
                    break;
                case "MPOS":
                    AddMPosTableData(body, navVer);
                    break;
                case "INV":
                    AddInvTableData(body, navVer);
                    break;
            }

            // Create the XML Declaration, and append it to XML document
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "REGISTER_APPLICATION"),
                        body
                    ));

            return doc.ToString();
        }

        public string StartSyncRequestXML(int batchsize)
        {
            #region xml
            /*
            <Request>
              <Request_ID>START_SYNC_CYCLE</Request_ID>
              <Request_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
              </Request_Body>
            </Request>
             */
            #endregion xml

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "START_SYNC_CYCLE"),
                        new XElement("Request_Body",
                            new XElement("Application_ID", this.AppId),
                            new XElement("Max_Records_Per_Request", batchsize)
                        )
                    ));

            return doc.ToString();
        }

        public string GetSyncStatusRequestXML()
        {
            #region xml
            /*
            <Request>
              <Request_ID>GET_SYNC_STATUS</Request_ID>
              <Request_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                       new XElement("Request_ID", "GET_SYNC_STATUS"),
                        new XElement("Request_Body",
                            new XElement("Application_ID", this.AppId)
                        )
                    ));

            return doc.ToString();
        }

        public NAVSyncCycleStatus GetSyncStatusResponseXML(string responseXml, int tableToGet, out int restorePoint)
        {
            #region xml
            /*
            <Response>
              <Request_ID>GET_SYNC_STATUS</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text/>
              <Response_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
                <Restore_Point>30</Restore_Point>
                <Web_Application_Sync_Status>
                  <Table_No.>27</Table_No.>
                  <Sync_Type Options="Full,Delta">1</Sync_Type>
                  <Sync_Cycle_Status Options="New,InProgress,Finished">0</Sync_Cycle_Status>
                  <Number_Of_Records>1</Number_Of_Records>
                  <Max_Records_Per_Request>200</Max_Records_Per_Request>
                </Web_Application_Sync_Status>
                <Web_Application_Sync_Status>
                ....
                </Web_Application_Sync_Status>
              </Response_Body>
            </Response>
            */
            #endregion

            XDocument doc = XDocument.Parse(responseXml);
            XElement body = doc.Element("Response").Element("Response_Body");

            restorePoint = XMLHelper.GetXMLInt32(body, "Restore_Point_ID");

            NAVSyncCycleStatus status = NAVSyncCycleStatus.New;
            bool found = false;
            foreach (XElement el in body.Elements("Web_Applic._Sync_Status"))
            {
                found = true;
                int tableNo = XMLHelper.GetXMLInt32(el, "Table_No.");
                if (tableNo == tableToGet)
                {
                    status = (NAVSyncCycleStatus)XMLHelper.GetXMLInt32(el, "Sync_Cycle_Status");
                    break;
                }
            }
            if (found == false)
                throw new LSOmniServiceException(StatusCode.Error, "Web_Applic._Sync_Status Not Found in GET_SYNC_STATUS result xml");
            return status;
        }

        public string RestoreSyncRequestXML(int restorePoint)
        {
            #region xml
            /*
            <Request>
              <Request_ID>RESTORE_APPLICATION_SYNC</Request_ID>
              <Request_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
                <Restore_Point>30</Restore_Point>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "RESTORE_APPLICATION_SYNC"),
                        new XElement("Request_Body",
                            new XElement("Application_ID", this.AppId),
                            new XElement("Restore_Point_ID", restorePoint)
                        )
                    ));

            return doc.ToString();
        }

        public List<XMLTableData> SyncResponseXML(string responseXml, out int restorePoint)
        {
            #region xml
            /*
            <Response>
              <Request_ID>START_SYNC_CYCLE</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text/>
              <Response_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
                <Restore_Point>30</Restore_Point>
                <Web_Application_Sync_Status>
                  <Table_No.>27</Table_No.>
                  <Sync_Type Options="Full,Delta">1</Sync_Type>
                  <Sync_Cycle_Status Options="New,InProgress,Finished">0</Sync_Cycle_Status>
                  <Number_Of_Records>1</Number_Of_Records>
                  <Max_Records_Per_Request>200</Max_Records_Per_Request>
                </Web_Application_Sync_Status>
                <Web_Application_Sync_Status>
                ....
                </Web_Application_Sync_Status>
              </Response_Body>
            </Response>
            */
            #endregion

            XDocument doc = XDocument.Parse(responseXml);
            XElement body = doc.Element("Response").Element("Response_Body");

            restorePoint = XMLHelper.GetXMLInt32(body, "Restore_Point_ID");

            List<XMLTableData> tablist = new List<XMLTableData>();
            bool found = false;
            foreach (XElement el in body.Elements("Web_Applic._Sync_Status"))
            {
                found = true;
                XMLTableData table = new XMLTableData();
                table.TableId = XMLHelper.GetXMLInt32(el, "Table_No.");
                table.TableName = XMLHelper.GetXMLValue(el, "Table_Name");
                table.SyncType = (NAVSyncType)XMLHelper.GetXMLInt32(el, "Sync_Type");
                table.SyncCycleStatus = (NAVSyncCycleStatus)XMLHelper.GetXMLInt32(el, "Sync_Cycle_Status");
                table.NumberOfRecords = XMLHelper.GetXMLInt32(el, "Number_Of_Records");
                table.MaxRecPerRequest = XMLHelper.GetXMLInt32(el, "Max_Records_Per_Request");
                tablist.Add(table);
            }
            if (found == false)
                throw new LSOmniServiceException(StatusCode.Error, "Web_Applic._Sync_Status Not Found in START_SYNC_CYCLE result xml");

            return tablist;
        }

        public string GetTableDataRequestXML(int tableNo)
        {
            #region xml
            /*
            <Request>
              <Request_ID>GET_SYNC_TABLE_DATA</Request_ID>
              <Request_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
              </Request_Body>
            </Request>
             */
            #endregion xml

            // Create the XML Declaration, and append it to XML document
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "GET_SYNC_TABLE_DATA"),
                        new XElement("Request_Body",
                            new XElement("Application_ID", this.AppId),
                            new XElement("Table_No", tableNo)
                        )
                    ));

            return doc.ToString();
        }

        public XMLTableData GetTableDataResponseXML(string responseXml, XMLTableData table, out bool endoftable, out int restorePoint)
        {
            #region xml
            /*
            <Response>
              <Request_ID>GET_SYNC_TABLE_DATA</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text/>
              <Response_Body>
                <Application_ID>{1E51C19E-128E-4611-A49B-35CB24A49CD4}</Application_ID>
                <Table_No>27</Table_No>
                <End_Of_Table>true</End_Of_Table>
                <Restore_Point_ID></Restore_Point_ID>
                <WS_Table_Field_Buffer>
                  <Field_Index>1</Field_Index>
                  <Field_Name>No.</Field_Name>
                  <Node_Name>I1</Node_Name>
                  <Data_Type>Text50</Data_Type>
                </WS_Table_Field_Buffer>
                <WS_Table_Field_Buffer>
                  ....
                </WS_Table_Field_Buffer>
                <Table_Update_Data>
                  <I1>xxx</I1>
                </Table_Update_Data>
                <Table_Update_Data>
                  <I1>xxx</I1>
                </Table_Update_Data>
              </Response_Body>
            </Response>
            */
            #endregion

            XDocument doc = XDocument.Parse(responseXml);
            XElement body = doc.Element("Response").Element("Response_Body");

            int tableno = XMLHelper.GetXMLInt32(body, "Table_No");
            endoftable = XMLHelper.GetXMLBool(body, "End_Of_Table");
            restorePoint = XMLHelper.GetXMLInt32(body, "Restore_Point_ID");

            if (table.FieldList == null)
                table.FieldList = new List<XMLFieldData>();
            else
                table.FieldList.Clear();

            int index = 0;
            foreach (XElement el in body.Elements("WS_Table_Field_Buffer"))
            {
                XMLFieldData field = new XMLFieldData()
                {
                    FieldId = XMLHelper.GetXMLInt32(el, "Field_Index"),
                    FieldName = XMLHelper.GetXMLValue(el, "Field_Name"),
                    NodeId = XMLHelper.GetXMLValue(el, "Node_Name"),
                    FieldType = XMLHelper.GetXMLValue(el, "Data_Type"),
                    Index = index++,
                    Values = new List<string>(),
                    DeleteValues = new List<string>()
                };
                table.FieldList.Add(field);
            }

            table.NumberOfValues = 0;
            foreach (XElement el in body.Elements("Table_Update_Data"))
            {
                table.NumberOfValues++;
                foreach (XMLFieldData field in table.FieldList)
                {
                    field.Values.Add(XMLHelper.GetXMLValue(el, field.NodeId, string.Empty));
                }
            }

            table.NumberOfDeleteValues = 0;
            foreach (XElement el in body.Elements("Table_Delete_Data"))
            {
                table.NumberOfDeleteValues++;
                foreach (XMLFieldData field in table.FieldList)
                {
                    field.DeleteValues.Add(XMLHelper.GetXMLValue(el, field.NodeId, string.Empty));
                }
            }

            return table;
        }

        public string GetGeneralWebRequestXML(string tablename)
        {
            #region xml
            /*
            <Request>
              <Request_ID>GET_TABLE_DATA</Request_ID>
              <Request_Body>
                <Table_Name>Retail Image</Table_Name>
                <Read_Direction>Forward</Read_Direction>
                <Max_Number_Of_Records>0</Max_Number_Of_Records>
              </Request_Body>
            </Request >
            */
            #endregion

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "GET_TABLE_DATA"),
                        new XElement("Request_Body",
                            new XElement("Table_Name", tablename),
                            new XElement("Read_Direction", "Forward"),
                            new XElement("Ignore_Extra_Fields", 1),
                            new XElement("Max_Number_Of_Records", "0")
                        )
                    ));

            return doc.ToString();
        }

        public string GetGeneralWebRequestXML(string tablename, string field, string value, int maxrows = 0)
        {
            #region xml
            /*
            <Request>
              <Request_ID>GET_TABLE_DATA</Request_ID>
              <Request_Body>
                <Table_Name>Retail Image</Table_Name>
                <Read_Direction>Forward</Read_Direction>
                <Max_Number_Of_Records>0</Max_Number_Of_Records>
                <WS_Table_Filter_Buffer>
                  <Field_Index>1</Field_Index>
                  <Field_Name>Code</Field_Name>
                  <Filter>10010</Filter>
                </WS_Table_Filter_Buffer>
              </Request_Body>
            </Request >
            */
            #endregion

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "GET_TABLE_DATA"),
                        new XElement("Request_Body",
                            new XElement("Table_Name", tablename),
                            new XElement("Read_Direction", "Forward"),
                            new XElement("Max_Number_Of_Records", maxrows),
                            new XElement("Ignore_Extra_Fields", 1),
                            new XElement("WS_Table_Filter_Buffer",
                                new XElement("Field_Index", 1),
                                new XElement("Field_Name", field),
                                new XElement("Filter", value)
                        )
                    )));

            return doc.ToString();
        }

        public string GetGeneralWebRequestXML(string tablename, string field1, string value1, string field2, string value2, int maxrows = 0)
        {
            #region xml
            /*
            <Request>
              <Request_ID>GET_TABLE_DATA</Request_ID>
              <Request_Body>
                <Table_Name>Retail Image</Table_Name>
                <Read_Direction>Forward</Read_Direction>
                <Max_Number_Of_Records>0</Max_Number_Of_Records>
                <WS_Table_Filter_Buffer>
                  <Field_Index>1</Field_Index>
                  <Field_Name>Code</Field_Name>
                  <Filter>10010</Filter>
                </WS_Table_Filter_Buffer>
              </Request_Body>
            </Request >
            */
            #endregion

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "GET_TABLE_DATA"),
                        new XElement("Request_Body",
                            new XElement("Table_Name", tablename),
                            new XElement("Read_Direction", "Forward"),
                            new XElement("Max_Number_Of_Records", maxrows),
                            new XElement("Ignore_Extra_Fields", 1),
                            new XElement("WS_Table_Filter_Buffer",
                                new XElement("Field_Index", 1),
                                new XElement("Field_Name", field1),
                                new XElement("Filter", value1)
                            ),
                            new XElement("WS_Table_Filter_Buffer",
                                new XElement("Field_Index", 2),
                                new XElement("Field_Name", field2),
                                new XElement("Filter", value2)
                            )
                    )));

            return doc.ToString();
        }

        public string GetBatchWebRequestXML(string tablename, List<string> fields, List<XMLFieldData> filter, string lastKey, int maxrows = 0)
        {
            #region xml
            /*
            <Request>
              <Request_ID>GET_TABLE_DATA</Request_ID>
              <Request_Body>
                <Table_Name>Retail Image</Table_Name>
                <Read_Direction>Forward</Read_Direction>
                <Max_Number_Of_Records>0</Max_Number_Of_Records>
                <WS_Table_Filter_Buffer>
                  <Field_Index>1</Field_Index>
                  <Field_Name>Code</Field_Name>
                  <Filter>10010</Filter>
                </WS_Table_Filter_Buffer>
              </Request_Body>
            </Request >
            */
            #endregion

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));

            XElement body = new XElement("Request_Body",
                                new XElement("Table_Name", tablename),
                                new XElement("Read_Direction", "Forward"),
                                new XElement("Max_Number_Of_Records", maxrows),
                                new XElement("Ignore_Extra_Fields", 1)
                            );

            int index = 1;
            if (fields != null)
            {
                foreach (string fld in fields)
                {
                    body.Add(new XElement("WS_Table_Field_Buffer",
                                new XElement("Field_Index", index++),
                                new XElement("Field_Name", fld)
                            ));
                }
            }

            index = 1;
            string values;
            foreach (XMLFieldData fld in filter)
            {
                values = string.Empty;
                foreach (string val in fld.Values)
                {
                    if (string.IsNullOrEmpty(values) == false)
                        values += "|";
                    values += val;
                }

                body.Add(new XElement("WS_Table_Filter_Buffer",
                            new XElement("Field_Index", index++),
                            new XElement("Field_Name", fld.FieldName),
                            new XElement("Filter", values)
                        ));
            }

            if (string.IsNullOrEmpty(lastKey) == false && lastKey != "0")
            {
                body.Add(new XElement("WS_Table_Record_Buffer",
                            new XElement("Key", lastKey)
                        ));
            }

            doc.Add(new XElement("Request",
                        new XElement("Request_ID", "GET_TABLE_DATA"),
                        body
                    ));

            return doc.ToString();
        }


        public XMLTableData GetGeneralWebResponseXML(string responseXml)
        {
            #region xml
            /*
            <Response>
              <Request_ID>GET_TABLE_DATA</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Table_Name>Retail Image</Table_Name>
                <WS_Table_Field_Buffer>
                  <Field_Index>1</Field_Index>
                  <Field_Name>Code</Field_Name>
                  <Node_Name>I1</Node_Name>
                  <Data_Type>Code20</Data_Type>
                </WS_Table_Field_Buffer>
                <WS_Table_Field_Buffer>
                  ...
                </WS_Table_Field_Buffer>
                <Table_Data>
                  <I1>10010</I1>
                  <I2>1</I2>
                  <I3 />
                </Table_Data>
              </Response_Body>
            </Response>
            */
            #endregion

            XDocument doc = XDocument.Parse(responseXml);
            XElement body = doc.Element("Response").Element("Response_Body");

            string tablename = XMLHelper.GetXMLValue(body, "Table_Name");

            XMLTableData table = new XMLTableData();
            table.FieldList = new List<XMLFieldData>();

            int index = 0;
            foreach (XElement el in body.Elements("WS_Table_Field_Buffer"))
            {
                XMLFieldData field = new XMLFieldData()
                {
                    FieldId = XMLHelper.GetXMLInt32(el, "Field_Index"),
                    FieldName = XMLHelper.GetXMLValue(el, "Field_Name"),
                    NodeId = XMLHelper.GetXMLValue(el, "Node_Name"),
                    FieldType = XMLHelper.GetXMLValue(el, "Data_Type"),
                    Index = index++,
                    Values = new List<string>(),
                    DeleteValues = new List<string>()
                };
                table.FieldList.Add(field);
            }

            table.NumberOfValues = 0;
            foreach (XElement el in body.Elements("Table_Data"))
            {
                table.NumberOfValues++;
                foreach (XMLFieldData field in table.FieldList)
                {
                    field.Values.Add(XMLHelper.GetXMLValue(el, field.NodeId, string.Empty));
                }
            }
            return table;
        }

        private void AddEComTableData(XElement body, Version navVer)
        {
            // Item
            AddTable(body, 27, true);
            AddField(body, 27, 1);
            AddField(body, 27, 3);
            AddField(body, 27, 8);
            AddField(body, 27, 18);
            AddField(body, 27, 22);
            AddField(body, 27, 31);
            AddField(body, 27, 32);
            AddField(body, 27, 41);
            AddField(body, 27, 43);
            AddField(body, 27, 44);
            AddField(body, 27, 99);
            AddField(body, 27, 5425);
            AddField(body, 27, 5426);
            AddField(body, 27, 5702);
            AddField(body, 27, (navVer > new Version("14.2")) ? 10000703 : 5704);
            AddField(body, 27, 10001401);
            AddField(body, 27, 99001463);
            AddField(body, 27, 99001480);
            AddField(body, 27, 99001490);

            // Item Category
            AddTable(body, 5722, false);
            AddField(body, 5722, 1);
            AddField(body, 5722, 3);

            // Product Group
            int pgtableid = (navVer > new Version("14.2")) ? 10000705 : 5723;
            AddTable(body, pgtableid, false);
            AddField(body, pgtableid, 1);
            AddField(body, pgtableid, 2);
            AddField(body, pgtableid, 3);

            // Hierarchy
            AddTable(body, 10000920, false);
            AddField(body, 10000920, 1);
            AddField(body, 10000920, 2);
            AddField(body, 10000920, 3);

            // Hierarchy Nodes
            AddTable(body, 10000921, false);
            AddField(body, 10000921, 1);
            AddField(body, 10000921, 2);
            AddField(body, 10000921, 3);
            AddField(body, 10000921, 4);
            AddField(body, 10000921, 8);
            AddField(body, 10000921, 9);
            AddField(body, 10000921, 10);
            AddField(body, 10000921, 12);

            // Hierarchy Nodes Link
            AddTable(body, 10000922, false);
            AddField(body, 10000922, 1);
            AddField(body, 10000922, 2);
            AddField(body, 10000922, 3);
            AddField(body, 10000922, 4);
            AddField(body, 10000922, 10);

            // Extended Variant Values
            AddTable(body, 10001413, true);
            AddField(body, 10001413, 1);
            AddField(body, 10001413, 5);
            AddField(body, 10001413, 9);
            AddField(body, 10001413, 10);
            AddField(body, 10001413, 11);
            AddField(body, 10001413, 16);

            // Item Variant Registration
            AddTable(body, 10001414, true);
            AddField(body, 10001414, 1);
            AddField(body, 10001414, 2);
            AddField(body, 10001414, 10);
            AddField(body, 10001414, 11);
            AddField(body, 10001414, 12);
            AddField(body, 10001414, 13);
            AddField(body, 10001414, 14);
            AddField(body, 10001414, 15);
            AddField(body, 10001414, 20);

            // Attribute
            AddTable(body, 10000784, false);
            AddField(body, 10000784, 1);
            AddField(body, 10000784, 5);
            AddField(body, 10000784, 20);
            AddField(body, 10000784, 21);

            // Attribute Option Value
            AddTable(body, 10000785, false);
            AddField(body, 10000785, 1);
            AddField(body, 10000785, 2);
            AddField(body, 10000785, 3);

            // Attribute Value
            AddTable(body, 10000786, false);
            AddField(body, 10000786, 1);
            AddField(body, 10000786, 2);
            AddField(body, 10000786, 3);
            AddField(body, 10000786, 4);
            AddField(body, 10000786, 5);
            AddField(body, 10000786, 6);
            AddField(body, 10000786, 7);
            AddField(body, 10000786, 8);

            // Unit of Measure
            AddTable(body, 204, false);
            AddField(body, 204, 1);
            AddField(body, 204, 2);

            // Item Unit of Measure
            AddTable(body, 5404, true);
            AddField(body, 5404, 1);
            AddField(body, 5404, 2);
            AddField(body, 5404, 3);

            // Collection Framework
            AddTable(body, 10001430, false);
            AddField(body, 10001430, 1);
            AddField(body, 10001430, 2);
            AddField(body, 10001430, 3);
            AddField(body, 10001430, 4);

            // WI Discounts
            AddTable(body, 10012862, true);
            AddField(body, 10012862, 1);
            AddField(body, 10012862, 2);
            AddField(body, 10012862, 3);
            AddField(body, 10012862, 4);
            AddField(body, 10012862, 10);
            AddField(body, 10012862, 11);
            AddField(body, 10012862, 12);
            AddField(body, 10012862, 13);
            AddField(body, 10012862, 14);
            AddField(body, 10012862, 20);
            AddField(body, 10012862, 21);
            AddField(body, 10012862, 23);

            // WI Mix & Match Offer
            AddTable(body, 10012863, true);
            AddField(body, 10012863, 1);
            AddField(body, 10012863, 2);
            AddField(body, 10012863, 3);
            AddField(body, 10012863, 10);
            AddField(body, 10012863, 11);
            AddField(body, 10012863, 12);
            AddField(body, 10012863, 13);
            AddField(body, 10012863, 20);

            // Validation Period
            AddTable(body, 99001481, false);
            AddField(body, 99001481, 1);
            AddField(body, 99001481, 2);
            AddField(body, 99001481, 10);
            AddField(body, 99001481, 11);
            AddField(body, 99001481, 12);
            AddField(body, 99001481, 13);
            AddField(body, 99001481, 14);
            AddField(body, 99001481, 15);
            AddField(body, 99001481, 16);
            AddField(body, 99001481, 17);
            AddField(body, 99001481, 18);
            AddField(body, 99001481, 19);
            AddField(body, 99001481, 20);
            AddField(body, 99001481, 21);
            AddField(body, 99001481, 22);
            AddField(body, 99001481, 23);
            AddField(body, 99001481, 24);
            AddField(body, 99001481, 25);
            AddField(body, 99001481, 26);
            AddField(body, 99001481, 27);
            AddField(body, 99001481, 28);
            AddField(body, 99001481, 29);
            AddField(body, 99001481, 30);
            AddField(body, 99001481, 31);
            AddField(body, 99001481, 32);
            AddField(body, 99001481, 33);
            AddField(body, 99001481, 34);
            AddField(body, 99001481, 35);
            AddField(body, 99001481, 40);
            AddField(body, 99001481, 41);
            AddField(body, 99001481, 42);
            AddField(body, 99001481, 43);
            AddField(body, 99001481, 44);
            AddField(body, 99001481, 45);
            AddField(body, 99001481, 46);
            AddField(body, 99001481, 47);
            AddField(body, 99001481, 81);

            // Barcodes
            AddTable(body, 99001451, false);
            AddField(body, 99001451, 1);
            AddField(body, 99001451, 10);
            AddField(body, 99001451, 25);
            AddField(body, 99001451, 110);
            AddField(body, 99001451, 200);

            // Currency
            AddTable(body, 4, false);
            AddField(body, 4, 1);
            AddField(body, 4, 10);
            AddField(body, 4, 12);
            AddField(body, 4, 13);
            AddField(body, 4, 15);
            AddField(body, 4, 99008900);
            AddField(body, 4, 99008901);

            // Currency Exchange Rate
            AddTable(body, 330, false);
            AddField(body, 330, 1);
            AddField(body, 330, 2);
            AddField(body, 330, 3);
            AddField(body, 330, 5);
            AddField(body, 330, 6);
            AddField(body, 330, 99001450);
            AddField(body, 330, 99001451);

            // Vendors
            AddTable(body, 23, false);
            AddField(body, 23, 1);
            AddField(body, 23, 2);
            AddField(body, 23, 39);
            AddField(body, 23, 54);

            // Store
            AddTable(body, 99001470, false);
            AddField(body, 99001470, 1);
            AddField(body, 99001470, 3);
            AddField(body, 99001470, 5);
            AddField(body, 99001470, 7);
            AddField(body, 99001470, 8);
            AddField(body, 99001470, 13);
            AddField(body, 99001470, 14);
            AddField(body, 99001470, 17);
            AddField(body, 99001470, 70);
            AddField(body, 99001470, 182);
            AddField(body, 99001470, 160);
            AddField(body, 99001470, 740);
            AddField(body, 99001470, 741);
            AddField(body, 99001470, 745);

            // Tender Type
            AddTable(body, 99001462, false);
            AddField(body, 99001462, 1);
            AddField(body, 99001462, 5);
            AddField(body, 99001462, 10);
            AddField(body, 99001462, 11);
            AddField(body, 99001462, 25);
            AddField(body, 99001462, 26);
            AddField(body, 99001462, 30);
            AddField(body, 99001462, 35);
            AddField(body, 99001462, 40);
            AddField(body, 99001462, 110);
            AddField(body, 99001462, 111);
            AddField(body, 99001462, 115);
            AddField(body, 99001462, 120);
            AddField(body, 99001462, 125);
            AddField(body, 99001462, 225);
            AddField(body, 99001462, 305);
            AddField(body, 99001462, 390);

            // VAT Posting Setup
            AddTable(body, 325, false);
            AddField(body, 325, 1);
            AddField(body, 325, 2);
            AddField(body, 325, 4);

            // Retail Image
            AddTable(body, 99009063, false);
            AddField(body, 99009063, 1);
            AddField(body, 99009063, 2);
            AddField(body, 99009063, 3);
            AddField(body, 99009063, 4);
            AddField(body, 99009063, 10);

            // Retail Image Link
            AddTable(body, 99009064, false);
            AddField(body, 99009064, 10);
            AddField(body, 99009064, 11);
            AddField(body, 99009064, 20);
            AddField(body, 99009064, 21);
            AddField(body, 99009064, 22);

            // Data Translation
            AddTable(body, 10000971, false);
            AddField(body, 10000971, 1);
            AddField(body, 10000971, 2);
            AddField(body, 10000971, 3);
            AddField(body, 10000971, 10);

            // Data Translation Lang Code
            AddTable(body, 10000972, false);
            AddField(body, 10000972, 1);

            // Shipping agent
            AddTable(body, 291, false);
            AddField(body, 291, 1);
            AddField(body, 291, 2);
            AddField(body, 291, 3);
            AddField(body, 291, 4);

            // Member Contact
            AddTable(body, 99009002, false);
            AddField(body, 99009002, 1);
            AddField(body, 99009002, 2);
            AddField(body, 99009002, 3);
            AddField(body, 99009002, 5);
            AddField(body, 99009002, 10);
            AddField(body, 99009002, 13);
            AddField(body, 99009002, 14);
            AddField(body, 99009002, 15);
            AddField(body, 99009002, 17);
            AddField(body, 99009002, 18);
            AddField(body, 99009002, 19);
            AddField(body, 99009002, 20);
            AddField(body, 99009002, 21);
            AddField(body, 99009002, 26);
            AddField(body, 99009002, 27);
            AddField(body, 99009002, 30);
            AddField(body, 99009002, 31);
            AddField(body, 99009002, 35);
            AddField(body, 99009002, 100);
            AddField(body, 99009002, 5054);
            AddField(body, 99009002, 5055);
            AddField(body, 99009002, 5056);

            // Country_Region
            AddTable(body, 9, false);
            AddField(body, 9, 1);
            AddField(body, 9, 2);
        }

        private void AddMPosTableData(XElement body, Version navVer)
        {
            // Item
            AddTable(body, 27, true);
            AddField(body, 27, 1);
            AddField(body, 27, 3);
            AddField(body, 27, 8);
            AddField(body, 27, 18);
            AddField(body, 27, 22);
            AddField(body, 27, 31);
            AddField(body, 27, 32);
            AddField(body, 27, 41);
            AddField(body, 27, 43);
            AddField(body, 27, 44);
            AddField(body, 27, 99);
            AddField(body, 27, 5425);
            AddField(body, 27, 5426);
            AddField(body, 27, 5702);
            AddField(body, 27, (navVer > new Version("14.2")) ? 10000703 : 5704);
            AddField(body, 27, 10001401);
            AddField(body, 27, 99001463);
            AddField(body, 27, 99001480);
            AddField(body, 27, 99001490);

            // Item Category
            AddTable(body, 5722, false);
            AddField(body, 5722, 1);
            AddField(body, 5722, 3);

            // Product Group
            int pgtableid = (navVer > new Version("14.2")) ? 10000705 : 5723;
            AddTable(body, pgtableid, false);
            AddField(body, pgtableid, 1);
            AddField(body, pgtableid, 2);
            AddField(body, pgtableid, 3);

            // Hierarchy
            AddTable(body, 10000920, false);
            AddField(body, 10000920, 1);
            AddField(body, 10000920, 2);
            AddField(body, 10000920, 3);

            // Hierarchy Nodes
            AddTable(body, 10000921, false);
            AddField(body, 10000921, 1);
            AddField(body, 10000921, 2);
            AddField(body, 10000921, 3);
            AddField(body, 10000921, 4);
            AddField(body, 10000921, 8);
            AddField(body, 10000921, 9);
            AddField(body, 10000921, 10);
            AddField(body, 10000921, 12);

            // Hierarchy Nodes Link
            AddTable(body, 10000922, false);
            AddField(body, 10000922, 1);
            AddField(body, 10000922, 2);
            AddField(body, 10000922, 3);
            AddField(body, 10000922, 4);
            AddField(body, 10000922, 10);

            // Extended Variant Values
            AddTable(body, 10001413, true);
            AddField(body, 10001413, 1);
            AddField(body, 10001413, 5);
            AddField(body, 10001413, 9);
            AddField(body, 10001413, 10);
            AddField(body, 10001413, 11);
            AddField(body, 10001413, 16);

            // Item Variant Registration
            AddTable(body, 10001414, true);
            AddField(body, 10001414, 1);
            AddField(body, 10001414, 2);
            AddField(body, 10001414, 10);
            AddField(body, 10001414, 11);
            AddField(body, 10001414, 12);
            AddField(body, 10001414, 13);
            AddField(body, 10001414, 14);
            AddField(body, 10001414, 15);
            AddField(body, 10001414, 20);

            // Unit of Measure
            AddTable(body, 204, false);
            AddField(body, 204, 1);
            AddField(body, 204, 2);

            // Item Unit of Measure
            AddTable(body, 5404, true);
            AddField(body, 5404, 1);
            AddField(body, 5404, 2);
            AddField(body, 5404, 3);

            // WI Discounts
            AddTable(body, 10012862, true);
            AddField(body, 10012862, 1);
            AddField(body, 10012862, 2);
            AddField(body, 10012862, 3);
            AddField(body, 10012862, 4);
            AddField(body, 10012862, 10);
            AddField(body, 10012862, 11);
            AddField(body, 10012862, 12);
            AddField(body, 10012862, 13);
            AddField(body, 10012862, 14);
            AddField(body, 10012862, 20);
            AddField(body, 10012862, 21);
            AddField(body, 10012862, 23);

            // Item Section Locations
            AddTable(body, 99001533, false);
            AddField(body, 99001533, 1);
            AddField(body, 99001533, 2);
            AddField(body, 99001533, 5);
            AddField(body, 99001533, 6);
            AddField(body, 99001533, 10);
            AddField(body, 99001533, 11);

            // Barcodes
            AddTable(body, 99001451, false);
            AddField(body, 99001451, 1);
            AddField(body, 99001451, 10);
            AddField(body, 99001451, 25);
            AddField(body, 99001451, 110);
            AddField(body, 99001451, 200);

            // Barcode Mask
            AddTable(body, 99001459, false);
            AddField(body, 99001459, 1);
            AddField(body, 99001459, 5);
            AddField(body, 99001459, 10);
            AddField(body, 99001459, 15);
            AddField(body, 99001459, 25);
            AddField(body, 99001459, 30);
            AddField(body, 99001459, 35);

            // Barcode Mask Segment
            AddTable(body, 99001480, false);
            AddField(body, 99001480, 10);
            AddField(body, 99001480, 20);
            AddField(body, 99001480, 25);
            AddField(body, 99001480, 30);
            AddField(body, 99001480, 35);
            AddField(body, 99001480, 40);

            // Currency
            AddTable(body, 4, false);
            AddField(body, 4, 1);
            AddField(body, 4, 10);
            AddField(body, 4, 12);
            AddField(body, 4, 13);
            AddField(body, 4, 15);
            AddField(body, 4, 99008900);
            AddField(body, 4, 99008901);

            // Currency Exchange Rate
            AddTable(body, 330, false);
            AddField(body, 330, 1);
            AddField(body, 330, 2);
            AddField(body, 330, 3);
            AddField(body, 330, 5);
            AddField(body, 330, 6);
            AddField(body, 330, 99001450);
            AddField(body, 330, 99001451);

            // Store
            AddTable(body, 99001470, false);
            AddField(body, 99001470, 1);
            AddField(body, 99001470, 3);
            AddField(body, 99001470, 5);
            AddField(body, 99001470, 7);
            AddField(body, 99001470, 8);
            AddField(body, 99001470, 13);
            AddField(body, 99001470, 14);
            AddField(body, 99001470, 17);
            AddField(body, 99001470, 70);
            AddField(body, 99001470, 182);
            AddField(body, 99001470, 160);
            AddField(body, 99001470, 740);
            AddField(body, 99001470, 741);
            AddField(body, 99001470, 745);

            // Tender Type
            AddTable(body, 99001462, false);
            AddField(body, 99001462, 1);
            AddField(body, 99001462, 5);
            AddField(body, 99001462, 10);
            AddField(body, 99001462, 11);
            AddField(body, 99001462, 25);
            AddField(body, 99001462, 26);
            AddField(body, 99001462, 30);
            AddField(body, 99001462, 35);
            AddField(body, 99001462, 40);
            AddField(body, 99001462, 110);
            AddField(body, 99001462, 111);
            AddField(body, 99001462, 115);
            AddField(body, 99001462, 120);
            AddField(body, 99001462, 125);
            AddField(body, 99001462, 225);
            AddField(body, 99001462, 305);
            AddField(body, 99001462, 390);

            // Staff
            AddTable(body, 99001461, false);
            AddField(body, 99001461, 1);
            AddField(body, 99001461, 5);
            AddField(body, 99001461, 6);
            AddField(body, 99001461, 7);
            AddField(body, 99001461, 35);
            AddField(body, 99001461, 36);
            AddField(body, 99001461, 90);
            AddField(body, 99001461, 111);
            AddField(body, 99001461, 112);
            AddField(body, 99001461, 1100);
            AddField(body, 99001461, 1101);

            // Staff Store Link
            AddTable(body, 99001633, false);
            AddField(body, 99001633, 1);
            AddField(body, 99001633, 25);

            // Customer
            AddTable(body, 18, false);
            AddField(body, 18, 1);
            AddField(body, 18, 2);
            AddField(body, 18, 5);
            AddField(body, 18, 7);
            AddField(body, 18, 9);
            AddField(body, 18, 22);
            AddField(body, 18, 35);
            AddField(body, 18, 39);
            AddField(body, 18, 82);
            AddField(body, 18, 91);
            AddField(body, 18, 92);
            AddField(body, 18, 102);
            AddField(body, 18, 103);
            AddField(body, 18, 110);
            AddField(body, 18, 10012701);

            // MobilePlu
            AddTable(body, 99009274, false);
            AddField(body, 99009274, 1);
            AddField(body, 99009274, 2);
            AddField(body, 99009274, 3);
            AddField(body, 99009274, 10);
            AddField(body, 99009274, 11);

            // VAT Posting Setup
            AddTable(body, 325, false);
            AddField(body, 325, 1);
            AddField(body, 325, 2);
            AddField(body, 325, 4);

            // Tender Type Setup
            AddTable(body, 99001466, false);
            AddField(body, 99001466, 1);
            AddField(body, 99001466, 5);
            AddField(body, 99001466, 10);

            // Tender Type Currency Setup
            AddTable(body, 99001636, false);
            AddField(body, 99001636, 1);
            AddField(body, 99001636, 2);
            AddField(body, 99001636, 3);
            AddField(body, 99001636, 4);

            // Vendors
            AddTable(body, 23, false);
            AddField(body, 23, 1);
            AddField(body, 23, 2);
            AddField(body, 23, 39);
            AddField(body, 23, 54);

            // POS Terminal
            AddTable(body, 99001471, false);
            AddField(body, 99001471, 1);
            AddField(body, 99001471, 5);
            AddField(body, 99001471, 7);
            AddField(body, 99001471, 10);
            AddField(body, 99001471, 150);
            AddField(body, 99001471, 155);
            AddField(body, 99001471, 182);
            AddField(body, 99001471, 183);
            AddField(body, 99001471, 601);
            AddField(body, 99001471, 1005);
            AddField(body, 99001471, 1007);
            AddField(body, 99001471, 1008);
            AddField(body, 99001471, 2102);
        }

        private void AddInvTableData(XElement body, Version navVer)
        {
            // Item
            AddTable(body, 27, true);
            AddField(body, 27, 1);
            AddField(body, 27, 3);
            AddField(body, 27, 8);
            AddField(body, 27, 18);
            AddField(body, 27, 22);
            AddField(body, 27, 31);
            AddField(body, 27, 32);
            AddField(body, 27, 41);
            AddField(body, 27, 43);
            AddField(body, 27, 44);
            AddField(body, 27, 99);
            AddField(body, 27, 5425);
            AddField(body, 27, 5426);
            AddField(body, 27, 5702);
            AddField(body, 27, (navVer > new Version("14.2")) ? 10000703 : 5704);
            AddField(body, 27, 10001401);
            AddField(body, 27, 99001463);
            AddField(body, 27, 99001480);
            AddField(body, 27, 99001490);

            // Extended Variant Values
            AddTable(body, 10001413, true);
            AddField(body, 10001413, 1);
            AddField(body, 10001413, 5);
            AddField(body, 10001413, 9);
            AddField(body, 10001413, 10);
            AddField(body, 10001413, 11);
            AddField(body, 10001413, 16);

            // Item Variant Registration
            AddTable(body, 10001414, true);
            AddField(body, 10001414, 1);
            AddField(body, 10001414, 2);
            AddField(body, 10001414, 10);
            AddField(body, 10001414, 11);
            AddField(body, 10001414, 12);
            AddField(body, 10001414, 13);
            AddField(body, 10001414, 14);
            AddField(body, 10001414, 15);
            AddField(body, 10001414, 20);

            // Item Unit of Measure
            AddTable(body, 5404, true);
            AddField(body, 5404, 1);
            AddField(body, 5404, 2);
            AddField(body, 5404, 3);

            // Barcodes
            AddTable(body, 99001451, false);
            AddField(body, 99001451, 1);
            AddField(body, 99001451, 10);
            AddField(body, 99001451, 25);
            AddField(body, 99001451, 110);
            AddField(body, 99001451, 200);

            // Barcode Mask
            AddTable(body, 99001459, false);
            AddField(body, 99001459, 1);
            AddField(body, 99001459, 5);
            AddField(body, 99001459, 10);
            AddField(body, 99001459, 15);
            AddField(body, 99001459, 25);
            AddField(body, 99001459, 30);
            AddField(body, 99001459, 35);

            // Barcode Mask Segment
            AddTable(body, 99001480, false);
            AddField(body, 99001480, 10);
            AddField(body, 99001480, 20);
            AddField(body, 99001480, 25);
            AddField(body, 99001480, 30);
            AddField(body, 99001480, 35);
            AddField(body, 99001480, 40);

            // GS1 DataBar Barcode Setup
            AddTable(body, 10000936, false);
            AddField(body, 10000936, 1);
            AddField(body, 10000936, 2);
            AddField(body, 10000936, 3);
            AddField(body, 10000936, 4);
            AddField(body, 10000936, 5);
            AddField(body, 10000936, 6);
            AddField(body, 10000936, 7);
            AddField(body, 10000936, 8);
            AddField(body, 10000936, 9);
            AddField(body, 10000936, 10);
            AddField(body, 10000936, 100);
            AddField(body, 10000936, 101);
            AddField(body, 10000936, 102);

            // Store
            AddTable(body, 99001470, false);
            AddField(body, 99001470, 1);
            AddField(body, 99001470, 3);
            AddField(body, 99001470, 5);
            AddField(body, 99001470, 7);
            AddField(body, 99001470, 8);
            AddField(body, 99001470, 13);
            AddField(body, 99001470, 14);
            AddField(body, 99001470, 17);
            AddField(body, 99001470, 70);
            AddField(body, 99001470, 182);
            AddField(body, 99001470, 160);
            AddField(body, 99001470, 740);
            AddField(body, 99001470, 741);
            AddField(body, 99001470, 745);

            // Staff
            AddTable(body, 99001461, false);
            AddField(body, 99001461, 1);
            AddField(body, 99001461, 5);
            AddField(body, 99001461, 6);
            AddField(body, 99001461, 7);
            AddField(body, 99001461, 35);
            AddField(body, 99001461, 36);
            AddField(body, 99001461, 90);
            AddField(body, 99001461, 111);
            AddField(body, 99001461, 112);
            AddField(body, 99001461, 1100);
            AddField(body, 99001461, 1101);

            // Customer
            AddTable(body, 18, false);
            AddField(body, 18, 1);
            AddField(body, 18, 2);
            AddField(body, 18, 5);
            AddField(body, 18, 7);
            AddField(body, 18, 9);
            AddField(body, 18, 22);
            AddField(body, 18, 35);
            AddField(body, 18, 39);
            AddField(body, 18, 82);
            AddField(body, 18, 91);
            AddField(body, 18, 92);
            AddField(body, 18, 102);
            AddField(body, 18, 103);
            AddField(body, 18, 110);
            AddField(body, 18, 10012701);

            // Vendors
            AddTable(body, 23, false);
            AddField(body, 23, 1);
            AddField(body, 23, 2);
            AddField(body, 23, 39);
            AddField(body, 23, 54);

            // Inventory Location List
            AddTable(body, 10012808, false);
            AddField(body, 10012808, 1);
            AddField(body, 10012808, 2);
            AddField(body, 10012808, 4);
            AddField(body, 10012808, 10);

            // Store Inventory Worksheet
            AddTable(body, 10001312, false);
            AddField(body, 10001312, 1);
            AddField(body, 10001312, 3);
            AddField(body, 10001312, 4);
            AddField(body, 10001312, 5);
            AddField(body, 10001312, 6);
            AddField(body, 10001312, 8);
            AddField(body, 10001312, 9);
            AddField(body, 10001312, 12);
            AddField(body, 10001312, 21);
            AddField(body, 10001312, 113);
            AddField(body, 10001312, 123);

            // Store Inventory Worksheet
            AddTable(body, 10012806, false);
            AddField(body, 10012806, 1);
            AddField(body, 10012806, 2);
            AddField(body, 10012806, 5);

        }

        private void AddTable(XElement body, int tableNo, bool distribution)
        {
            body.Add(new XElement("Web_Group_Table",
                        new XElement("Table_No.", tableNo),
                        new XElement("Active_For_Item_Distribution", distribution)));
        }

        private void AddField(XElement body, int tableNo, int fieldNo)
        { 
            body.Add(new XElement("Web_Group_Table_Fields",
                         new XElement("Table_No.", tableNo),
                         new XElement("Field_No.", fieldNo)));

        }
    }

    public enum NAVSyncType
    {
        Full,
        Delta
    }

    public enum NAVSyncCycleStatus
    {
        New,
        InProgress,
        Finished
    }

    public class XMLFieldData
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string NavName { get; set; }
        public string FieldType { get; set; }
        public SqlDbType NativeType { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }
        public byte DecPrec { get; set; }
        public byte DecScale { get; set; }
        public string NodeId { get; set; }
        public bool IsInPrimaryKey { get; set; }
        public List<string> Values { get; set; }
        public List<string> DeleteValues { get; set; }

        public string GetName()
        {
            if (string.IsNullOrEmpty(NavName))
                return FieldName;
            else
                return NavName;
        }
    }

    public class XMLTableData
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public string NavName { get; set; }
        public NAVSyncType SyncType { get; set; }
        public NAVSyncCycleStatus SyncCycleStatus { get; set; }
        public int NumberOfRecords { get; set; }
        public int MaxRecPerRequest { get; set; }
        public int NumberOfValues { get; set; }
        public int NumberOfDeleteValues { get; set; }
        public List<XMLFieldData> FieldList { get; set; }

        public string GetName()
        {
            if (string.IsNullOrEmpty(NavName))
                return TableName;
            else
                return NavName;
        }
    }
}
