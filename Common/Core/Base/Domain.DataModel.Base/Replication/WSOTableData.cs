using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplOTableData : IDisposable
    {
        public ReplOTableData()
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
                if (TableData != null)
                    TableData.Dispose();
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
        public ReplOTableDataTable TableData { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplOTableDataTable : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplOTableDataTable()
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
                if (TableDataUpd != null)
                    TableDataUpd.Dispose();
                if (TableDataDel != null)
                    TableDataDel.Dispose();
            }
        }

        [DataMember]
        public ReplOTableDataTableData TableDataUpd { get; set; }
        [DataMember]
        public ReplOTableDataTableData TableDataDel { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplOTableDataTableData : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplOTableDataTableData()
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
                if (RecRefJson != null)
                    RecRefJson.Dispose();
            }
        }

        [DataMember]
        public ReplOTableDataRecRef RecRefJson { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplOTableDataRecRef : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplOTableDataRecRef()
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
                if (Records != null)
                    Records.Clear();
                if (RecordFields != null)
                    RecordFields.Clear();
            }
        }

        [DataMember]
        public int TableId { get; set; }
        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public List<ReplODataRecordField> RecordFields { get; set; }
        [DataMember]
        public List<ReplODataRecord> Records { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplODataRecord : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataRecord()
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
                if (Fields != null)
                    Fields.Clear();
            }
        }

        [DataMember]
        public List<ReplODataField> Fields { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplODataRecordField : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataRecordField()
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
        public int FieldNo { get; set; }
        [DataMember]
        public string FieldName { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public class ReplODataField : IDisposable
    {
        /// <summary>
        /// Vendor or Manufacturer
        /// </summary>
        public ReplODataField()
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
        public string FieldValue { get; set; }
    }
}
