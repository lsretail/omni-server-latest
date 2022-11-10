using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class StaffPermissionRepository : BaseRepository
    {
        // Key: ID
        const int TABLEID = 99001518;

        private string sqlcolumns = string.Empty;
        private string sqlfrom = string.Empty;

        public StaffPermissionRepository(BOConfiguration config) : base(config)
        {
            sqlcolumns = "st.[ID]," +
                         "st.[Manager Privileges] AS FLD_A1,st.[Void Transaction] AS FLD_A2,st.[Void Line] AS FLD_A3,st.[Return in Transaction] AS FLD_A4,st.[Suspend Transaction] AS FLD_A5," +
                         "st.[Add Payment] AS FLD_A6,st.[Price Override] AS FLD_A7,st.[Max_ Discount to Give _] AS FLD_A8,st.[Max_ Total Discount _] AS FLD_A9," +
                         "st.[XZY-Report Printing] AS FLD_A10,st.[Tender Declaration] AS FLD_A11,st.[Floating Declaration] AS FLD_A12," +
                         "st.[Create Customers] AS FLD_A13,st.[Update Customers] AS FLD_A14,st.[View Sales History] AS FLD_A15,st.[Customer Comments] AS FLD_A16," +
                         "spg.[Manager Privileges] AS FLD_B1,spg.[Void Transaction] AS FLD_B2,spg.[Void Line] AS FLD_B3,spg.[Return in Transaction] AS FLD_B4,spg.[Suspend Transaction] AS FLD_B5," +
                         "spg.[Add Payment] AS FLD_B6,spg.[Price Override] AS FLD_B7,spg.[Max_ Discount to Give _] AS FLD_B8,spg.[Max_ Total Discount _] AS FLD_B9," +
                         "spg.[XZY-Report Printing] AS FLD_B10,spg.[Tender Declaration] AS FLD_B11,spg.[Floating Declaration] AS FLD_B12," +
                         "spg.[Create Customers] AS FLD_B13,spg.[Update Customers] AS FLD_B14,spg.[View Sales History] AS FLD_B15,spg.[Customer Comments] AS FLD_B16";

            sqlfrom = " FROM [" + navCompanyName + "Staff] st" +
                      " LEFT JOIN [" + navCompanyName + "Staff Permission Group] spg ON spg.[Code]=st.[Permission Group]";
        }

        public List<ReplStaffPermission> ReplicateStaffPermission(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            List<ReplStaffPermission> list = new List<ReplStaffPermission>();

            // get valid staff list
            StaffRepository repo = new StaffRepository(config);
            List<ReplStaff> staffs = repo.ReplicateStaff(storeId, batchSize, fullReplication, ref lastKey, ref maxKey, ref recordsRemaining);
            if (staffs.Count == 0)
                return list;

            // get permission values for Staff
            string stIds = string.Empty;
            foreach (ReplStaff staff in staffs)
            {
                if (stIds == string.Empty)
                {
                    stIds = string.Format("'{0}'", staff.Id);
                }
                else
                {
                    stIds += string.Format(",'{0}'", staff.Id);
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + string.Format(" WHERE st.[ID] IN ({0})", stIds);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.AddRange(ReaderToPermission(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private string GetOptionValue(object value1, object value2, int emptyValue)
        {
            // staff value, should never be NULL
            int val1 = emptyValue;
            if (value1 != null && value1 != DBNull.Value)
            {
                val1 = SQLHelper.GetInt32(value1);
            }

            if (val1 != emptyValue)
            {
                // return staff value, as it overrides group value
                return val1.ToString();
            }

            // use permission group value
            if (value2 == null || value2 == DBNull.Value)
                return emptyValue.ToString();

            return SQLHelper.GetString(value2);
        }

        private string GetDecimalValue(object value1, object value2)
        {
            // staff value, should never be NULL
            decimal val1 = 0.0M;
            if (value1 != null && value1 != DBNull.Value)
            {
                val1 = SQLHelper.GetDecimal(value1);
            }

            if (val1 > 0.0M)
            {
                // return staff value, as it overrides group value
                return ConvertTo.SafeStringDecimal(val1);
            }

            // use permission group value
            if (value2 == null || value2 == DBNull.Value)
                return "0";

            return ConvertTo.SafeStringDecimal(SQLHelper.GetDecimal(value2));
        }

        private List<ReplStaffPermission> ReaderToPermission(SqlDataReader reader)
        {
            string staffID = SQLHelper.GetString(reader["ID"]);

            List<ReplStaffPermission> list = new List<ReplStaffPermission>()
            {
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.ManagerPrivileges,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A1"], reader["FLD_B1"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.VoidTransaction,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A2"], reader["FLD_B2"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.VoidLine,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A3"], reader["FLD_B3"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.ReturnInTransaction,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A4"], reader["FLD_B4"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.SuspendTransaction,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A5"], reader["FLD_B5"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.AddPayment,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A6"], reader["FLD_B6"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.PriceOverRide,
                    StaffId = staffID,
                    Type = PermissionType.List,
                    Value = GetOptionValue(reader["FLD_A7"], reader["FLD_B7"], 4)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.MaxDiscountToGivePercent,
                    StaffId = staffID,
                    Type = PermissionType.Decimal,
                    Value = GetDecimalValue(reader["FLD_A8"], reader["FLD_B8"])
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.MaxTotalDiscountPercent,
                    StaffId = staffID,
                    Type = PermissionType.Decimal,
                    Value = GetDecimalValue(reader["FLD_A9"], reader["FLD_B9"])
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.XZYReport,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A10"], reader["FLD_B10"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.TenderDeclaration,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A11"], reader["FLD_B11"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.FloatingDeclaration,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A12"], reader["FLD_B12"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.CreateCustomer,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A13"], reader["FLD_B13"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.UpdateCustomer,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A14"], reader["FLD_B14"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.ViewSalesHistory,
                    StaffId = staffID,
                    Type = PermissionType.Boolean,
                    Value = GetOptionValue(reader["FLD_A15"], reader["FLD_B15"], 2)
                },
                new ReplStaffPermission()
                {
                    Id = PermissionEntry.CustomerComments,
                    StaffId = staffID,
                    Type = PermissionType.List,
                    Value = GetOptionValue(reader["FLD_A16"], reader["FLD_B16"], 3)
                },
            };
            return list;
        }
    }
}
