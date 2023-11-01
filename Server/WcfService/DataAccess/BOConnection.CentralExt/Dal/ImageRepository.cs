using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentralExt.Dal
{
    public class ImageRepository : BaseRepository
    {
        private string sqlimgfrom = string.Empty;

        const int IMAGE_TABLEID = 99009063;
        const int LINK_TABLEID = 99009064;

        public ImageRepository(BOConfiguration config, Version version) : base(config, version)
        {
            sqlimgfrom = " FROM [" + navCompanyName + "LSC Retail Image$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";
        }

        public ImageView ImageGetById(string id, bool includeBlob)
        {
            ImageView view = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Code] AS [ImgID],tm.[ID],0 as [Display Order],mt.[Type],mt.[Image Location],mt.[Last Date Modified]" +
                                          ((includeBlob) ? ",tm.[Content],tm.[Height],tm.[Width]" : string.Empty) +
                                          " FROM [" + navCompanyName + "LSC Retail Image$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "LEFT JOIN [Tenant Media Set] tms ON tms.[ID]=mt.[Image Mediaset] AND tms.[Company Name]=@cmp " +
                                          "LEFT JOIN [Tenant Media] tm ON tm.[ID]=tms.[Media ID] " +
                                          "WHERE mt.[Code]=@id";
                    command.Parameters.AddWithValue("@id", id.ToUpper());
                    command.Parameters.AddWithValue("@cmp", navOrgCompanyName);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            view = ReaderToImage(reader, includeBlob);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return view;
        }

        public ImageView ImageMediaGetById(string id)
        {
            ImageView view = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [ID],[Description],[Content],[Height],[Width] FROM [Tenant Media] WHERE [ID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            view = new ImageView()
                            {
                                ImgSize = new ImageSize(SQLHelper.GetInt32(reader["Width"]), SQLHelper.GetInt32(reader["Height"])),
                                MediaId = SQLHelper.GetGuid(reader["ID"]),
                                Id = SQLHelper.GetString(reader["Description"]),
                                ImgBytes = ImageConverter.NAVUnCompressImage(reader["Content"] as byte[])
                            };
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return view;
        }

        public List<ImageView> ImageGetByKey(string tableName, string key1, string key2, string key3, int imgCount, bool includeBlob)
        {
            try
            {
                List<ImageView> list = new List<ImageView>();
                string sqlcnt = string.Empty;
                if (imgCount > 0)
                    sqlcnt = " TOP(" + imgCount.ToString() + ") ";

                string sql = "SELECT " + sqlcnt + "il.[Image Id] AS [ImgID],tm.[ID],il.[Display Order],mt.[Type],mt.[Image Location],mt.[Last Date Modified]" +
                            ((includeBlob) ? ",tm.[Content],tm.[Height],tm.[Width]" : string.Empty) +
                             sqlimgfrom + " JOIN [" + navCompanyName + "LSC Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] il ON mt.[Code]=il.[Image Id]" +
                             " LEFT JOIN [Tenant Media Set] tms ON tms.[ID]=mt.[Image Mediaset] AND tms.[Company Name]='" + navOrgCompanyName + "'" +
                             " LEFT JOIN [Tenant Media] tm ON tm.[ID]=tms.[Media ID]" +
                             " WHERE il.[KeyValue]=@key AND il.[TableName]=@table " +
                             " ORDER BY il.[Display Order]";

                string keyvalue = key1;
                if (string.IsNullOrWhiteSpace(key2) == false)
                    keyvalue += "," + key2;
                if (string.IsNullOrWhiteSpace(key3) == false)
                    keyvalue += "," + key3;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@key", keyvalue);
                        command.Parameters.AddWithValue("@table", tableName);
                        TraceSqlCommand(command);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(ReaderToImage(reader, includeBlob));
                            }
                            reader.Close();
                        }
                        connection.Close();
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private ImageView ReaderToImage(SqlDataReader reader, bool includeblob)
        {
            ImageView view = new ImageView()
            {
                Id = SQLHelper.GetString(reader["ImgID"]),
                MediaId = SQLHelper.GetGuid(reader["ID"]),
                DisplayOrder = SQLHelper.GetInt32(reader["Display Order"]),
                Location = SQLHelper.GetString(reader["Image Location"]),
                LocationType = (LocationType)SQLHelper.GetInt32(reader["Type"]),
                ModifiedTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Last Date Modified"]), config.IsJson)
            };

            if (includeblob)
            {
                view.ImgBytes = ImageConverter.NAVUnCompressImage(reader["Content"] as byte[]);
                if (view.ImgBytes != null && view.ImgBytes.Length > 0)
                {
                    view.ImgSize = new ImageSize(SQLHelper.GetInt32(reader["Width"]), SQLHelper.GetInt32(reader["Height"]));
                    view.LocationType = LocationType.Image;
                }
                else if (string.IsNullOrEmpty(view.Location) == false)
                {
                    view.LocationType = LocationType.Url;
                }
            }
            return view;
        }

        public List<ReplImage> ReplEcommImage(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Retail Image$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*)" + sqlimgfrom + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(IMAGE_TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, IMAGE_TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplImage> list = new List<ReplImage>();

            // get records
            sql = GetSQL(fullReplication, batchSize) +
                    "mt.[Code],tm.[ID],mt.[Type],mt.[Image Location],mt.[Description],tm.[Content],tm.[Height],tm.[Width]" +
                    "FROM [" + navCompanyName + "LSC Retail Image$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                    "LEFT JOIN [Tenant Media Set] tms ON tms.[ID]=mt.[Image Mediaset] AND tms.[Company Name]=@cmp " +
                    "LEFT JOIN [Tenant Media] tm ON tm.[ID]=tms.[Media ID] " +
                    GetWhereStatement(fullReplication, keys, true);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    if (fullReplication)
                    {
                        JscActions act = new JscActions(lastKey);
                        SetWhereValues(command, act, keys, true, true);
                        command.Parameters.AddWithValue("@cmp", navOrgCompanyName);
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int cnt = 0;
                            while (reader.Read())
                            {
                                list.Add(ReaderToMediaImage(reader, out lastKey));
                                cnt++;
                            }
                            reader.Close();
                            recordsRemaining -= cnt;
                        }
                        if (recordsRemaining <= 0)
                            lastKey = maxKey;   // this should be the highest PreAction id;
                    }
                    else
                    {
                        bool first = true;
                        foreach (JscActions act in actions)
                        {
                            if (act.Type == DDStatementType.Delete)
                            {
                                list.Add(new ReplImage()
                                {
                                    Id = act.ParamValue,
                                    IsDeleted = true
                                });
                                continue;
                            }

                            if (SetWhereValues(command, act, keys, first) == false)
                                continue;

                            if (first)
                                command.Parameters.AddWithValue("@cmp", navOrgCompanyName);

                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToMediaImage(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }

        private ReplImage ReaderToMediaImage(SqlDataReader reader, out string timestamp)
        {
            ReplImage img = new ReplImage()
            {
                Id = SQLHelper.GetString(reader["Code"]),
                Location = SQLHelper.GetString(reader["Image Location"]),
                LocationType = LocationType.Image,
                Description = SQLHelper.GetString(reader["Description"]),
                MediaId = SQLHelper.GetString(reader["ID"])
            };

            byte[] imgbyte = SQLHelper.GetByteArray(reader["Content"]);
            if (imgbyte == null)
            {
                img.Image64 = string.Empty;
                img.Size = new ImageSize();
                if (string.IsNullOrEmpty(img.Location) == false)
                    img.LocationType = LocationType.Url;
            }
            else
            {
                img.Image64 = Convert.ToBase64String(imgbyte);
                img.Size = new ImageSize(SQLHelper.GetInt32(reader["Width"]), SQLHelper.GetInt32(reader["Height"]));
            }

            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);
            return img;
        }

        public List<ReplImageLink> ReplEcommImageLink(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<JscKey> keys = GetPrimaryKeys("LSC Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d");

            // get records remaining
            string sql = string.Empty;
            if (fullReplication)
            {
                sql = "SELECT COUNT(*) FROM [" + navCompanyName + "LSC Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" + GetWhereStatement(true, keys, false);
            }
            recordsRemaining = GetRecordCount(LINK_TABLEID, lastKey, sql, keys, ref maxKey);

            List<JscActions> actions = LoadActions(fullReplication, LINK_TABLEID, batchSize, ref lastKey, ref recordsRemaining);
            List<ReplImageLink> list = new List<ReplImageLink>();

            // get records
            sql = GetSQL(fullReplication, batchSize) + 
                "mt.[Image Id],mt.[Display Order],mt.[TableName],mt.[KeyValue],mt.[Description]" +
                ((LSCVersion >= new Version("22.2")) ? ",mt.[Image Description]" : "") +
                " FROM [" + navCompanyName + "LSC Retail Image Link$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" + 
                GetWhereStatement(fullReplication, keys, true);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = sql;

                    if (fullReplication)
                    {
                        JscActions act = new JscActions(lastKey);
                        SetWhereValues(command, act, keys, true, true);
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int cnt = 0;
                            while (reader.Read())
                            {
                                list.Add(ReaderToImageLink(reader, out lastKey));
                                cnt++;
                            }
                            reader.Close();
                            recordsRemaining -= cnt;
                        }
                        if (recordsRemaining <= 0)
                            lastKey = maxKey;   // this should be the highest PreAction id;
                    }
                    else
                    {
                        bool first = true;
                        foreach (JscActions act in actions)
                        {
                            if (act.Type == DDStatementType.Delete)
                            {
                                string[] par = act.ParamValue.Split(';');
                                if (par.Length < 2 || par.Length != keys.Count)
                                    continue;

                                string[] recid = par[0].Split(':');

                                list.Add(new ReplImageLink()
                                {
                                    TableName = recid[0].Trim(),
                                    KeyValue = recid[1].Trim(),
                                    ImageId = par[1],
                                    IsDeleted = true
                                });
                                continue;
                            }

                            if (SetWhereValues(command, act, keys, first) == false)
                                continue;

                            TraceSqlCommand(command);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(ReaderToImageLink(reader, out string ts));
                                }
                                reader.Close();
                            }
                            first = false;
                        }
                    }
                    connection.Close();
                }
            }

            // just in case something goes too far
            if (recordsRemaining < 0)
                recordsRemaining = 0;

            return list;
        }

        private ReplImageLink ReaderToImageLink(SqlDataReader reader, out string timestamp)
        {
            timestamp = ByteArrayToString(reader["timestamp"] as byte[]);

            ReplImageLink link = new ReplImageLink()
            {
                DisplayOrder = SQLHelper.GetInt32(reader["Display Order"]),
                ImageId = SQLHelper.GetString(reader["Image Id"]),
                KeyValue = SQLHelper.GetString(reader["KeyValue"]),
                TableName = SQLHelper.GetString(reader["TableName"]),
                Description = SQLHelper.GetString(reader["Description"])
            };

            if (LSCVersion >= new Version("22.2"))
                link.ImageDescription = SQLHelper.GetString(reader["Image Description"]);

            return link;
        }
    }
}
 