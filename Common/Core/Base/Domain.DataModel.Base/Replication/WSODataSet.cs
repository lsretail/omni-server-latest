using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplODataSet : IDisposable
    {
        public ReplODataSet()
        {
            LastKey = string.Empty;
            Status = string.Empty;
            ErrorText = string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DataSet != null)
                    DataSet.Dispose();
            }
        }

        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string ErrorText { get; set; }
        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public int LastEntryNo { get; set; }
        [DataMember]
        public bool EndOfTable { get; set; }
        [DataMember]
        public ReplODataSetTable DataSet { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplODataSetTable : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataSetTable()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DataSetUpd != null)
                    DataSetUpd.Dispose();
                if (DataSetDel != null)
                    DataSetDel.Dispose();
            }
        }

        [DataMember]
        public ReplODataSetTableData DataSetUpd { get; set; }
        [DataMember]
        public ReplODataSetTableData DataSetDel { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplODataSetTableData : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataSetTableData()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DynDataSet != null)
                    DynDataSet.Dispose();
            }
        }

        [DataMember]
        public ReplODataSetRecRef DynDataSet { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplODataSetRecRef : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataSetRecRef()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DataSetRows != null)
                    DataSetRows.Clear();
                if (DataSetFields != null)
                    DataSetFields.Clear();
            }
        }

        [DataMember]
        public int DataSetID { get; set; }
        [DataMember]
        public string DataSetName { get; set; }
        [DataMember]
        public List<ReplODataSetField> DataSetFields { get; set; }
        [DataMember]
        public List<ReplODataRecord> DataSetRows { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplODataSetField : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataSetField()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public int FieldIndex { get; set; }
        [DataMember]
        public string FieldDataType { get; set; }
        [DataMember]
        public string FieldName { get; set; }
    }
}
