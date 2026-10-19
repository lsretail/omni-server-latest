using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LSOmni.DataAccess.BOConnection.PreCommon.JMapping
{
    public abstract class BaseJMapping
    {
        protected bool IsJson = false;
        protected static LSLogger logger = new LSLogger();
        protected Version LSCVersion = new Version("27.0");

        // BC API pages serialize Option/Enum fields by their caption, EDM-encoding any character that isn't a valid
        // identifier char as _xHHHH_ (hex code point) - e.g. a blank caption of " " comes back as literal "_x0020_",
        // and "Mobile POS" as "Mobile_x0020_POS". Decodes that back to the real caption text (trimmed).
        protected static string DecodeEdmEnumCaption(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            string decoded = Regex.Replace(value, "_x([0-9A-Fa-f]{4})_", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
            return decoded.Trim();
        }

        public string SetKeys(bool fullRepl, int batchSize, ref string lastKey, out int lastEntry)
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

            JObject body = new JObject()
            {
                { "batchSize", batchSize },
                { "fullRepl", fullRepl },
                { "lastKey", lastKey },
                { "lastEntryNo", lastEntry }
            };
            return body.ToString();
        }

        public string SetKeys(string storeId, bool fullRepl, int batchSize, ref string lastKey, out int lastEntry)
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

            JObject body = new JObject()
            {
                { "storeNo", storeId },
                { "batchSize", batchSize },
                { "fullRepl", fullRepl },
                { "lastKey", lastKey },
                { "lastEntryNo", lastEntry }
            };
            return body.ToString();
        }

        public ReplOTableData JsonToTableData(string ret, ref string lastKey, ref int recordsRemaining)
        {
            ReplOTableData result = JsonConvert.DeserializeObject<ReplOTableData>(GetValue(ret));
            if (result == null || result.TableData == null || result.TableData.TableDataUpd == null || result.TableData.TableDataUpd.RecRefJson == null)
            {
                if (result?.Status == "OnError")
                    throw new LSOmniServiceException(StatusCode.NavODataReplicationError, result.ErrorText);
                return null;
            }

            lastKey = (result.LastEntryNo == 0) ? result.LastKey : result.LastEntryNo.ToString();
            recordsRemaining = (result.EndOfTable) ? 0 : 1;
            return result;
        }

        public string GetJsonValue(string ret, string valueMemberCode, out string resCode, out string resErr)
        {
            JObject parsed = JObject.Parse(GetValue(ret));
            var token = parsed.SelectToken(valueMemberCode);
            if (token == null)
                throw new LSOmniServiceException(StatusCode.NavODataError, $"{valueMemberCode} element not found in result");

            resCode = parsed.SelectToken("ResponseCode").Value<string>();
            resErr = parsed.SelectToken("ErrorText").Value<string>();
            return token.ToString();
        }

        public string GetJsonPageValue(string ret, out string resCode, out string resErr, out bool isArray)
        {
            resCode = "0000";
            resErr = string.Empty;
            isArray = false;

            if (ret.StartsWith("ERROR"))
            {
                resCode = "9900";
                resErr = ret;
                return null;
            }

            JObject parsed;
            try
            {
                parsed = JObject.Parse(ret);
            }
            catch (Exception)
            {
                throw new LSOmniServiceException(StatusCode.NavODataError, "OData4 Error:" + ret);
            }

            var err = parsed.SelectToken("error");
            if (err != null)
            {
                resCode = err.SelectToken("code").Value<string>();
                resErr = err.SelectToken("message").Value<string>();
                return null;
            }

            var value = parsed.SelectToken("value");
            if (value == null)
                return parsed.ToString();

            isArray = true;
            return value.ToString();
        }

        public WSODataCollection JsonToWSOData(string ret, string valueMemberCode, out string resCode, out string resErr)
        {
            string token = GetJsonValue(ret, valueMemberCode, out resCode, out resErr);
            WSODataCollection result = JsonConvert.DeserializeObject<WSODataCollection>(token);
            if (result == null || result.DataCollection == null)
                return null;

            return result;
        }

        public ReplODataSet JsonToDataSet(string ret, ref string lastKey, ref int recordsRemaining)
        {
            ReplODataSet result = JsonConvert.DeserializeObject<ReplODataSet>(GetValue(ret));
            if (result == null || result.DataSet == null || result.DataSet.DataSetUpd == null || result.DataSet.DataSetUpd.DynDataSet == null)
            {
                if (result.Status == "OnError")
                    throw new LSOmniServiceException(StatusCode.NavODataReplicationError, result.ErrorText);
                return null;
            }

            lastKey = (result.LastEntryNo == 0) ? result.LastKey : result.LastEntryNo.ToString();
            recordsRemaining = (result.EndOfTable) ? 0 : 1;
            return result;
        }

        private string GetValue(string ret)
        {
            JObject parsed;
            try
            {
                parsed = JObject.Parse(ret);
            }
            catch (Exception)
            {
                throw new LSOmniServiceException(StatusCode.NavODataError, "OData4 Error:" + ret);
            }

            var err = parsed.SelectToken("error");
            if (err != null)
            {
                string errCode = err.SelectToken("code")?.Value<string>();
                string errMsg = err.SelectToken("message")?.Value<string>();
                throw new LSOmniServiceException(StatusCode.NavODataError, errCode, errMsg);
            }

            var token = parsed.SelectToken("value");
            return token.ToString();
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
