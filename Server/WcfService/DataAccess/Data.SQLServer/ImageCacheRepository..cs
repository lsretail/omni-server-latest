using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Dal
{
    public class ImageCache : BaseRepository, IImageCacheRepository
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static object statusLock = new object();

        public ImageCache() : base()
        {
        }

        public void SaveCache(ImageView imgView, string description, ImageSize orgImgSize)
        {
            if (imgView == null)
                return;

            //return true if the image exists in cache 
            if (base.DoesRecordExist("[ImagesCache]", "[Id]=@0 AND [Width]=@1 AND [Height]=@2 AND [AvgColor]=@3",
                imgView.Id, orgImgSize.Width, orgImgSize.Height, imgView.AvgColor) == false)
            {
                SaveImageCache(imgView, description, orgImgSize);
            }
            SaveImageSizeCache(imgView);//save with original size
        }

        public void SaveImageCache(ImageView imgView, string description, ImageSize orgImgSize)
        {
            string sqlIns = "INSERT INTO [ImagesCache] ([Id],[AvgColor],[Format],[Description],[Width],[Height]) VALUES (@Id,@AvgColor,@Format,@Description,@Width,@Height)";
            string sqlDel = "DELETE FROM [ImagesCache] WHERE [Id]=@Id";

            if (string.IsNullOrWhiteSpace(description))
                description = "";

            if (description.Contains("'")) description = description.Replace("'", "''");

            List<ImageView> list = new List<ImageView>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                lock (statusLock)
                {
                    using (SqlTransaction dbTrans = connection.BeginTransaction())
                    {
                        try
                        {
                            //delete before insert
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.CommandText = sqlDel;
                                command.CommandTimeout = 60 * 1000;
                                command.Transaction = dbTrans;
                                command.Parameters.AddWithValue("@Id", imgView.Id);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText = sqlIns;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@Id", imgView.Id);
                                command.Parameters.AddWithValue("@Width", orgImgSize.Width);
                                command.Parameters.AddWithValue("@Height", orgImgSize.Height);
                                command.Parameters.AddWithValue("@AvgColor", imgView.AvgColor);
                                command.Parameters.AddWithValue("@Format", imgView.Format);
                                command.Parameters.AddWithValue("@Description", description);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }
                            dbTrans.Commit();
                        }
                        catch (Exception)
                        {
                            dbTrans.Rollback();
                            throw;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        public void SaveImageSizeCache(ImageView imgView)
        {
            string sqlIns = "INSERT INTO [ImagesSizeCache] ([ImageId],[Width],[Height],[Base64],[URL],[Format]) VALUES (@ImageId,@Width,@Height,@Base64,@URL,@Format)";
            string sqlDel = "DELETE FROM [ImagesSizeCache] WHERE [ImageId]=@ImageId AND [Width]=@Width AND [Height]=@Height";

            if (imgView.Location.Contains("'")) imgView.Location = imgView.Location.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(imgView.Id))
                imgView.Id = GuidHelper.NewGuidString();

            List<ImageView> list = new List<ImageView>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                lock (statusLock)
                {
                    using (SqlTransaction dbTrans = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.CommandText = sqlDel;
                                command.CommandTimeout = 60 * 1000;
                                command.Transaction = dbTrans;
                                command.Parameters.AddWithValue("@ImageId", imgView.Id);
                                command.Parameters.AddWithValue("@Width", imgView.ImgSize.Width);
                                command.Parameters.AddWithValue("@Height", imgView.ImgSize.Height);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText = sqlIns;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@ImageId", imgView.Id);
                                command.Parameters.AddWithValue("@Width", imgView.ImgSize.Width);
                                command.Parameters.AddWithValue("@Height", imgView.ImgSize.Height);
                                command.Parameters.AddWithValue("@Base64", imgView.Image);
                                command.Parameters.AddWithValue("@URL", imgView.Location);
                                command.Parameters.AddWithValue("@Format", imgView.Format);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }
                            dbTrans.Commit();
                        }
                        catch
                        {
                            dbTrans.Rollback();
                            throw;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }

        public ImageView ImageCacheGetById(string id)
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
                    command.CommandText = "SELECT [Id],[Width],[Height],[AvgColor],[Format],[Description] FROM [ImagesCache] WHERE Id=@id";
                    command.Parameters.AddWithValue("@id", id);

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

        //get images that are related to each other via the ImageLink table
        public List<ImageView> ImagesCacheGetById(string id)
        {
            List<ImageView> list = new List<ImageView>();
            if (string.IsNullOrWhiteSpace(id))
                return list;

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT ic.[Id],ic.[Width],ic.[Height],ic.[AvgColor],ic.[Format],ic.[Description],il.[DisplayOrder] " +
                                          "FROM [ImageLink] il INNER JOIN [ImagesCache] ic ON ic.[Id]=il.ImageId " +
                                          "WHERE il.[KeyValue]=@id ORDER BY il.[DisplayOrder]";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ImageView iview = ReaderToImageCache(reader);
                            list.Add(iview);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public ImageView ImageSizeCacheGetById(string id, ImageSize imgSize)
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
                    command.CommandText = "SELECT ims.[ImageId],ims.[Width],ims.[Height],ims.[Base64],i.[AvgColor],i.[Format] " +
                                          "FROM [ImagesCache] i INNER JOIN [ImagesSizeCache] ims ON i.[Id]=ims.[ImageId] " +
                                          "WHERE [ImageId]=@id AND ims.[Width]=@wid AND ims.[Height]=@hei";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@wid", imgSize.Width);
                    command.Parameters.AddWithValue("@hei", imgSize.Height);

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            iview = ReaderToImageSizeCache(reader);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return iview;
        }

        public CacheState Validate(string id, ImageSize imageSize)
        {
            id = SQLHelper.CheckForSQLInjection(id);
            string sql = string.Format(@"SELECT [LastModifiedDate],[CreatedDate] FROM [ImagesSizeCache] WHERE [ImageId]='{0}' AND [Height]={1} AND [Width]={2}",
                id, imageSize.Height, imageSize.Width);
            return base.Validate(sql, CacheImageDurationInMinutes);
        }

        private ImageView ReaderToImageCache(SqlDataReader reader)
        {
            ImageView img = new ImageView();
            img.Id = SQLHelper.GetString(reader["Id"]);
            img.AvgColor = SQLHelper.GetString(reader["AvgColor"]);
            img.Format = SQLHelper.GetString(reader["Format"]);
            img.Location = SQLHelper.GetString(reader["Description"]);
            img.LocationType = LocationType.Image;

            int w = SQLHelper.GetInt32(reader["Width"]);
            int h = SQLHelper.GetInt32(reader["Height"]);
            img.ImgSize = new ImageSize(w, h); ;
            return img;
        }

        private ImageView ReaderToImageSizeCache(SqlDataReader reader)
        {
            ImageView img = new ImageView();
            img.Id = SQLHelper.GetString(reader["ImageId"]);
            img.AvgColor = SQLHelper.GetString(reader["AvgColor"]);
            img.Format = SQLHelper.GetString(reader["Format"]);
            img.Image = SQLHelper.GetString(reader["Base64"]);
            img.Location = "";
            img.LocationType = LocationType.Image;

            int w = SQLHelper.GetInt32(reader["Width"]);
            int h = SQLHelper.GetInt32(reader["Height"]);
            img.ImgSize = new ImageSize(w, h); ;
            return img;
        }
    }
}
