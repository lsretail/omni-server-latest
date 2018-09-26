using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.Dal
{
    public class ImageRepository : BaseRepository, IImageRepository
    {
        static object statusLock = new object();

        //private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public ImageRepository() : base()
        {
        }

        public void SaveImage(ImageView iv)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    lock (statusLock)
                    {
                        try
                        {
                            DeleteFromImages(iv.Id, connection, trans);
                            InsertIntoImages(iv, connection, trans);
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
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

        public void SaveImageLink(ImageView iv, string tableName, string recordId, string keyValue, string imgId, int displayOrder)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    lock (statusLock)
                    {
                        try
                        {
                            DeleteFromImages(iv.Id, connection, trans);
                            InsertIntoImages(iv, connection, trans);

                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;

                                command.CommandText = "DELETE FROM [ImageLink] WHERE [TableName]=@f0 AND [KeyValue]=@f1 AND [ImageId]=@f2";
                                command.Parameters.AddWithValue("@f0", tableName);
                                command.Parameters.AddWithValue("@f1", keyValue);
                                command.Parameters.AddWithValue("@f2", imgId);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText =
                                    "INSERT INTO [ImageLink] ([TableName],[RecordId],[KeyValue],[ImageId],[DisplayOrder],[CreatedDate]) " +
                                    "VALUES (@f0, @f1, @f2, @f3, @f4, @f5)";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@f0", tableName);
                                command.Parameters.AddWithValue("@f1", recordId);
                                command.Parameters.AddWithValue("@f2", keyValue);
                                command.Parameters.AddWithValue("@f3", imgId);
                                command.Parameters.AddWithValue("@f4", displayOrder);
                                command.Parameters.AddWithValue("@f5", DateTime.Now);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
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

        public List<ImageView> ItemImagesByItemId(string itemId)
        {
            List<ImageView> imageList = new List<ImageView>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT il.[ImageId],ic.[Width],ic.[Height],ic.[AvgColor] " +
                                          "FROM [ImageLink] il INNER JOIN [Images] i ON il.[ImageId]=i.[Id] " +
                                          "LEFT OUTER JOIN [ImagesCache] ic ON ic.[Id]=i.[Id] " + 
                                          "WHERE [TableName]='Item' AND [KeyValue]=@id ORDER BY [DisplayOrder] ";

                    command.Parameters.AddWithValue("@id", itemId);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ImageView imageView = new ImageView(SQLHelper.GetString(reader["ImageId"]));
                            imageView.AvgColor = SQLHelper.GetString(reader["AvgColor"]);

                            int w = SQLHelper.GetInt32(reader["Width"]);
                            int h = SQLHelper.GetInt32(reader["Height"]);
                            imageView.ImgSize = new ImageSize(w, h);
                            imageList.Add(imageView);
                        }
                    }
                    connection.Close();
                }
            }
            return imageList;
        }

        public List<ImageView> NotificationImagesById(string notificationId)
        {
            List<ImageView> imageList = new List<ImageView>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT il.[ImageId],ic.[Width],ic.[Height],ic.[AvgColor],i.[Image] " +
                                          "FROM [ImageLink] il INNER JOIN [Images] i ON il.[ImageId]=i.[Id] " +
                                          "LEFT OUTER JOIN [ImagesCache] ic ON ic.[Id]=i.[Id] " +
                                          "WHERE [TableName]='Member Notification' AND [KeyValue]=@id " +
                                          "ORDER BY [DisplayOrder]";

                    command.Parameters.AddWithValue("@id", notificationId);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ImageView imageView = new ImageView(SQLHelper.GetString(reader["ImageId"]));
                            imageView.AvgColor = SQLHelper.GetString(reader["AvgColor"]);
                            imageView.ImgBytes = SQLHelper.GetByteArray(reader["Image"]);

                            int w = SQLHelper.GetInt32(reader["Width"]);
                            int h = SQLHelper.GetInt32(reader["Height"]);
                            imageView.ImgSize = new ImageSize(w, h);
                            imageList.Add(imageView);
                        }
                    }
                    connection.Close();
                }
            }
            return imageList;
        }

        private void DeleteFromImages(string id, SqlConnection connection, SqlTransaction trans)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Transaction = trans;
                command.CommandText = "DELETE FROM [Images] WHERE [Id]=@id";
                command.Parameters.AddWithValue("@id", id);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();
            }
        }

        private void InsertIntoImages(ImageView iv, SqlConnection connection, SqlTransaction trans)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Transaction = trans;
                command.CommandText =
                    "INSERT INTO [Images] ([Id],[Image],[Type],[Location],[LastDateModified]) " +
                    "VALUES (@f0, @f1, @f2, @f3, @f4)";

                command.Parameters.AddWithValue("@f0", iv.Id);
                command.Parameters.AddWithValue("@f1", iv.ImgBytes);
                command.Parameters.AddWithValue("@f2", (int)iv.LocationType);
                command.Parameters.AddWithValue("@f3", iv.Location);
                command.Parameters.AddWithValue("@f4", DateTime.Now);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();
            }
        }
    }
}
 