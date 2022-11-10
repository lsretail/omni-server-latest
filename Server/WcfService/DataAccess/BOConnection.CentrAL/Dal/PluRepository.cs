using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSRetail.Omni.Domain.DataModel.Pos.Replication;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class PluRepository : BaseRepository
    {
        public PluRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
        }

        public List<ReplPlu> ReplicatePlu(string storeId, int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            string sql = "SELECT ml.[Parameter],ml.[Command],bp.[Parameter Value]" +
                     " FROM dbo.[" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st" +
                     " LEFT JOIN [" + navCompanyName + "POS Menu Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml ON ml.[Menu ID]=st.[PLU Menu ID] AND ml.[Profile ID]=st.[PLU Menu Profile]" +
                     " LEFT JOIN [" + navCompanyName + "POS Button Parameters$5ecfc871-5d82-43f1-9c54-59685e82318d] bp ON ml.[Profile ID]=bp.[POS Menu Profile ID] AND" +
                     " ml.[Menu ID]=bp.[Menu ID] AND ml.[Key No_]=bp.[Key No_] AND ml.[Command]=bp.[POS Command] AND bp.[Parameter Value]>''" +
                     " WHERE st.[No_]='" + storeId + "' AND ml.[Parameter]>''";

            List<ReplPlu> list = new List<ReplPlu>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int pid = 1;
                        int pindex = 1;
                        string itemsql;
                        while (reader.Read())
                        {
                            if (NavVersion.Major > 16)
                            {
                                itemsql = "SELECT it.[No_],it.[Description],it.[Search Description],i.[Code],tm.[Content] AS Blob" +
                                                " FROM dbo.[" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] it" +
                                                " LEFT JOIN [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il ON it.[No_]=il.[KeyValue] AND il.[TableName]='Item' AND il.[Display Order]=0 AND [Image Id]<>''" +
                                                " LEFT JOIN [" + navCompanyName + "Retail Image$5ecfc871-5d82-43f1-9c54-59685e82318d] i ON i.[Code]= il.[Image Id]" +
                                                " LEFT JOIN [Tenant Media Set] tms ON tms.[ID]=i.[Image Mediaset] AND tms.[Company Name]='" + navCompanyName.Substring(0, navCompanyName.Length - 1) + "'" +
                                                " LEFT JOIN [Tenant Media] tm ON tm.[ID]=tms.[Media ID]";
                            }
                            else
                            {
                                itemsql = "SELECT it.[No_],it.[Description],it.[Search Description],i.[Code],i.[Image Blob] AS Blob" +
                                                " FROM dbo.[" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] it" +
                                                " LEFT JOIN [" + navCompanyName + "Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il ON it.[No_]=il.[KeyValue] AND il.[TableName]='Item' AND il.[Display Order]=0 AND [Image Id]<>''" +
                                                " LEFT JOIN [" + navCompanyName + "Retail Image$5ecfc871-5d82-43f1-9c54-59685e82318d] i ON i.[Code]= il.[Image Id] and i.[Type]=1";
                            }

                            string cmd = SQLHelper.GetString(reader["Command"]);
                            string par = SQLHelper.GetString(reader["Parameter"]);
                            if (cmd == "DYNLOOKUP")
                            {
                                bool addand = false;
                                itemsql += " WHERE";
                                if (par == "PRODUCT")
                                {
                                    itemsql += " it.[Product Group Code]='" + SQLHelper.GetString(reader["Parameter Value"]) + "'";
                                    addand = true;
                                }
                                if (par == "CATEGORY")
                                {
                                    if (addand)
                                        itemsql += " AND";

                                    itemsql += " it.[Item Category Code]='" + SQLHelper.GetString(reader["Parameter Value"]) + "'";
                                }
                            }
                            else if (cmd == "PLU_K")
                            {
                                itemsql += " WHERE it.[No_]='" + par + "'";
                            }
                            else
                            {
                                continue;
                            }

                            // TODO: do NAV 10 version of the Image lookup

                            using (SqlCommand itemcmd = connection.CreateCommand())
                            {
                                itemcmd.CommandText = itemsql;
                                using (SqlDataReader itemreader = itemcmd.ExecuteReader())
                                {
                                    while (itemreader.Read())
                                    {
                                        ReplPlu val = ReaderToPlu(itemreader, storeId);
                                        val.PageId = pid;
                                        val.PageIndex = pindex++;
                                        list.Add(val);
                                        if (pindex == 10)
                                        {
                                            pindex = 1;
                                            pid++;
                                        }
                                    }
                                    itemreader.Close();
                                }
                            }
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

        private ReplPlu ReaderToPlu(SqlDataReader reader, string storeid)
        {
            return new ReplPlu()
            {
                StoreId = storeid,
                ItemId = SQLHelper.GetString(reader["No_"]),
                Descritpion = SQLHelper.GetString(reader["Description"]),
                ImageId = SQLHelper.GetString(reader["Code"]),
                ItemImage = SQLHelper.GetByteArray(reader["Blob"])
            };
        }
    }
}