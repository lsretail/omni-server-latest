using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Dal
{
    public class ImageCache : BaseRepository, IImageCacheRepository
    {
        static object statusLock = new object();

        public ImageCache(BOConfiguration config) : base(config)
        {
        }

        public void SaveImageCache(string lsKey, ImageView imgView, bool doUpdate)
        {
            if (imgView == null)
                return;

            //return true if the image exists in cache 
            if (base.DoesRecordExist("[ImagesCache]", "[Id]=@0 AND [Width]=@1 AND [Height]=@2 AND [MinSize]=@3", imgView.Id, imgView.ImgSize.Width, imgView.ImgSize.Height, imgView.ImgSize.UseMinHorVerSize))
            {
                if (doUpdate == false)
                    return;
            }

            List<ImageView> list = new List<ImageView>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                lock (statusLock)
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        if (doUpdate)
                        {
                            //delete before insert
                            command.CommandText = "DELETE FROM [ImagesCache] WHERE [LSKey]=@Key AND [Id]=@Id AND [Width]=@Width AND [Height]=@Height AND [MinSize]=@Min";
                            command.Parameters.AddWithValue("@Key", lsKey);
                            command.Parameters.AddWithValue("@Id", imgView.Id);
                            command.Parameters.AddWithValue("@Width", imgView.ImgSize.Width);
                            command.Parameters.AddWithValue("@Height", imgView.ImgSize.Height);
                            command.Parameters.AddWithValue("@Min", imgView.ImgSize.UseMinHorVerSize);
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();
                        }

                        command.CommandText = "INSERT INTO [ImagesCache] ([LSKey],[Id],[Format],[AvgColor],[Base64],[URL],[Width],[Height],[MinSize]) " +
                                              "VALUES (@Key,@Id,@Format,@Avg,@Base64,@Url,@Width,@Height,@Min)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Key", lsKey);
                        command.Parameters.AddWithValue("@Id", imgView.Id);
                        command.Parameters.AddWithValue("@Format", imgView.Format);
                        command.Parameters.AddWithValue("@Base64", imgView.Image);
                        command.Parameters.AddWithValue("@Avg", imgView.AvgColor);
                        command.Parameters.AddWithValue("@URL", imgView.Location);
                        command.Parameters.AddWithValue("@Width", imgView.ImgSize.Width);
                        command.Parameters.AddWithValue("@Height", imgView.ImgSize.Height);
                        command.Parameters.AddWithValue("@Min", imgView.ImgSize.UseMinHorVerSize);
                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        public ImageView ImageCacheGetById(string lsKey, string id, ImageSize imageSize)
        {
            //only return on image with displayoder = 0  - the main image
            ImageView iview = null;
            if (string.IsNullOrWhiteSpace(id))
                return iview;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [LSKey],[Id],[Width],[Height],[MinSize],[Format],[AvgColor],[Base64],[Url] " +
                                          "FROM [ImagesCache] WHERE [LSKey]=@key AND [Id]=@id AND [Width]=@wid AND [Height]=@hei AND [MinSize]=@min";
                    command.Parameters.AddWithValue("@key", lsKey);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@wid", imageSize.Width);
                    command.Parameters.AddWithValue("@hei", imageSize.Height);
                    command.Parameters.AddWithValue("@min", imageSize.UseMinHorVerSize);

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            iview = ReaderToImageCache(reader);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return iview;
        }

        public CacheState Validate(string lsKey, string id, ImageSize imageSize, out DateTime lastModeTime)
        {
            lastModeTime = DateTime.MinValue;

            int durationInMinutes = config.SettingsIntGetByKey(ConfigKey.Cache_Image_DurationInMinutes);
            if (durationInMinutes <= 0)
                durationInMinutes = 0; // 

            CacheState state = CacheState.NotExist;//in case nothing got returned default this to notexist
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT [LastModifiedDate] FROM [ImagesCache] " +
                                              "WHERE [LSKey]=@key AND [Id]=@id AND [Height]=@hei AND [Width]=@wid AND [MinSize]=@min";

                        command.Parameters.AddWithValue("@key", lsKey);
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@hei", imageSize.Height);
                        command.Parameters.AddWithValue("@wid", imageSize.Width);
                        command.Parameters.AddWithValue("@min", imageSize.UseMinHorVerSize);
                        TraceSqlCommand(command);

                        DateTime currDate = DateTime.MinValue;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currDate = DateTime.Now;
                                lastModeTime = SQLHelper.GetDateTime(reader["LastModifiedDate"]);
                            }
                            reader.Close();

                            if (currDate == DateTime.MinValue)
                                state = CacheState.NotExist;            // id does not exist
                            else if ((currDate - lastModeTime).TotalMinutes < durationInMinutes)
                                state = CacheState.Exists;              // id found and has not expired
                            else
                                state = CacheState.ExistsButExpired;    //id found but has expired
                        }
                    }
                    connection.Close();
                }
                return state;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "Validate failed..");
                throw;
            }
        }

        private ImageView ReaderToImageCache(SqlDataReader reader)
        {
            int w = SQLHelper.GetInt32(reader["Width"]);
            int h = SQLHelper.GetInt32(reader["Height"]);
            bool min = SQLHelper.GetBool(reader["MinSize"]);

            return new ImageView()
            {
                Id = SQLHelper.GetString(reader["Id"]),
                Format = SQLHelper.GetString(reader["Format"]),
                Image = SQLHelper.GetString(reader["Base64"]),
                Location = SQLHelper.GetString(reader["Url"]),
                AvgColor = SQLHelper.GetString(reader["AvgColor"]),
                LocationType = LocationType.Image,
                ImgSize = new ImageSize(w, h, min),
            };
        }
    }
}
