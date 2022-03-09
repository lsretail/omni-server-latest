using System;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using Newtonsoft.Json;

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

            lastKey = (string.IsNullOrEmpty(result.LastKey)) ? result.LastEntryNo.ToString() : result.LastKey;
            recordsRemaining = (result.EndOfTable) ? 0 : 1;
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

            lastKey = (string.IsNullOrEmpty(result.LastKey)) ? result.LastEntryNo.ToString() : result.LastKey;
            recordsRemaining = (result.EndOfTable) ? 0 : 1;
            return result;
        }
    }
}
