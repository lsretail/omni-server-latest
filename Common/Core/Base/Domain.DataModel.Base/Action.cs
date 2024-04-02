
/* Unmerged change from project 'Domain.DataModel.Base (netstandard2.0)'
Before:
using System;
After:
using System;
using LSRetail;
using LSRetail.Omni;
using LSRetail.Omni.Domain;
using LSRetail.Omni.Domain.DataModel;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Base;
*/
using System;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    /// <summary>
    /// Action Records to be replicated
    /// </summary>
    public class JscActions
    {
        public long id;
        public DDStatementType Type;
        public string ParamValue = string.Empty;
        public int TableId = 0;

        public JscActions()
        {
            Type = DDStatementType.Invalid;
            ParamValue = string.Empty;
        }

        public JscActions(string param)
        {
            Type = DDStatementType.Invalid;
            ParamValue = param;
        }
    }

    public class JscKey
    {
        public string FieldName = string.Empty;
        public string FieldType = string.Empty;
    }

    /// <summary>
    /// Action Types for Replication Actions
    /// </summary>
    public enum DDStatementType
    {
        Invalid = -1,
        Insert = 0,
        Update = 1,
        Delete = 2,
        Insert_Update = 3
    }
}
