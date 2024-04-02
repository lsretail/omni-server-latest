using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public abstract class BaseJMapping
    {
        protected bool IsJson = false;
        protected static LSLogger logger = new LSLogger();
        protected Version LSCVersion = new Version("19.0");

        public void SetKeys(bool fullRepl, ref string lastKey, out int lastEntry)
        {
            if (string.IsNullOrEmpty(lastKey))
                lastKey = "0";

            lastEntry = 0;
            if (fullRepl)
            {
                if (lastKey == "0")
                    lastKey = string.Empty;
            }
            else
            {
                lastEntry = Convert.ToInt32(lastKey);
                lastKey = string.Empty;
            }
        }

        public ReplOTableData JsonToTableData(string ret, ref string lastKey, ref int recordsRemaining)
        {
            ReplOTableData result = JsonConvert.DeserializeObject<ReplOTableData>(ret);
            if (result == null || result.TableData == null || result.TableData.TableDataUpd == null || result.TableData.TableDataUpd.RecRefJson == null)
            {
                if (result.Status == "OnError")
                    throw new LSOmniServiceException(StatusCode.NavWSError, result.ErrorText);
                return null;
            }

            lastKey = (result.LastEntryNo == 0) ? result.LastKey : result.LastEntryNo.ToString();
            recordsRemaining = (result.EndOfTable) ? 0 : 1;
            return result;
        }

        public WSODataCollection JsonToWSOData(string ret, string valueMemberCode)
        {

            JObject parsed;
            try
            {
                parsed = JObject.Parse(ret);
            }
            catch (Exception)
            {
                throw new LSOmniServiceException(StatusCode.NavWSError, "OData4 Error:" + ret);
            }

            var token = parsed.SelectToken("value");
            parsed = JObject.Parse(token.ToString());
            token = parsed.SelectToken(valueMemberCode);
            string resCode = parsed.SelectToken("ResponseCode").Value<string>();
            string resErr = parsed.SelectToken("ErrorText").Value<string>();

            WSODataCollection result = JsonConvert.DeserializeObject<WSODataCollection>(token.ToString());
            if (result == null || result.DataCollection == null)
            {
                if (resCode != "0000")
                    throw new LSOmniServiceException(StatusCode.NavWSError, resErr);
                return null;
            }
            return result;
        }

        public ReplODataSet JsonToDataSet(string ret, ref string lastKey, ref int recordsRemaining)
        {
            ReplODataSet result = JsonConvert.DeserializeObject<ReplODataSet>(ret);
            if (result == null || result.DataSet == null || result.DataSet.DataSetUpd == null || result.DataSet.DataSetUpd.DynDataSet == null)
            {
                if (result.Status == "OnError")
                    throw new LSOmniServiceException(StatusCode.NavWSError, result.ErrorText);
                return null;
            }

            lastKey = (result.LastEntryNo == 0) ? result.LastKey : result.LastEntryNo.ToString();
            recordsRemaining = (result.EndOfTable) ? 0 : 1;
            return result;
        }

        public string LoadOneValue(ReplODataSetRecRef dynDataSet, string lookupField, string lookupValue, string returnField)
        {
            string value = string.Empty;
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0 || string.IsNullOrEmpty(lookupValue))
                return value;

            ReplODataSetField lfld = dynDataSet.DataSetFields.Find(f => f.FieldName.Equals(lookupField));
            if (lfld == null)
                return value;
            ReplODataSetField rfld = dynDataSet.DataSetFields.Find(f => f.FieldName.Equals(returnField));
            if (rfld == null)
                return value;

            foreach (ReplODataRecord rec in dynDataSet.DataSetRows)
            {
                ReplODataField fld = rec.Fields.Find(f => f.FieldIndex == lfld.FieldIndex && f.FieldValue == lookupValue);
                if (fld != null)
                {
                    ReplODataField res = rec.Fields.Find(f => f.FieldIndex == rfld.FieldIndex);
                    if (res != null)
                        return res.FieldValue;
                }
            }
            return value;
        }

        public List<WSODataFlowField> LoadFlowFields(ReplODataSetRecRef dynDataSet)
        {
            List<WSODataFlowField> list = new List<WSODataFlowField>();
            if (dynDataSet == null || dynDataSet.DataSetRows.Count == 0)
                return list;

            foreach (ReplODataRecord row in dynDataSet.DataSetRows)
            {
                WSODataFlowField rec = new WSODataFlowField();
                foreach (ReplODataField col in row.Fields)
                {
                    ReplODataSetField fld = dynDataSet.DataSetFields.Find(f => f.FieldIndex == col.FieldIndex);
                    if (fld == null)
                        continue;

                    switch (fld.FieldIndex)
                    {
                        case 1: rec.TableNo = ConvertTo.SafeInt(col.FieldValue); break;
                        case 2: rec.Key = col.FieldValue; break;
                        case 3: rec.FieldNo = ConvertTo.SafeInt(col.FieldValue); break;
                        case 4: rec.FieldName = col.FieldValue; break;
                        case 5: rec.DecimalValue = ConvertTo.SafeDecimal(col.FieldValue); break;
                    }
                }
                list.Add(rec);
            }
            return list;
        }
    }
}
