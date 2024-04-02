using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class StaffStoreLinkRepository : BaseRepository
    {
        // Key: Staff ID, Store No.
        const int TABLEID = 99001633;

        public StaffStoreLinkRepository(BOConfiguration config) : base(config)
        {
        }

        public List<ReplStaffStoreLink> ReplicateStaffStoreLink(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<ReplStaffStoreLink> list = new List<ReplStaffStoreLink>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Staff ID],mt.[Default Sales Type]" + 
                                         " FROM [" + navCompanyName + "LSC STAFF Store Link$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                                         " WHERE mt.[Store No_]=@id";
                    command.Parameters.AddWithValue("@id", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToStaffStoreLink(reader, storeId));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }

            // we always replicate everything here
            maxKey = string.Empty;
            lastKey = string.Empty;
            recordsRemaining = 0;

            return list;
        }

        private ReplStaffStoreLink ReaderToStaffStoreLink(SqlDataReader reader, string storeId)
        {
            return new ReplStaffStoreLink()
            {
                StaffId = SQLHelper.GetString(reader["Staff ID"]),
                DefaultHospType = SQLHelper.GetString(reader["Default Sales Type"]),
                StoreId = storeId
            };
        }
    }
}
 