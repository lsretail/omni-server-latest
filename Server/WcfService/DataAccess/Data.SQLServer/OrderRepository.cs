using System;
using System.Data.SqlClient;
using System.Collections.Generic;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Dal
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static object statusLock = new object();

        public string SaveOrderMessage(OrderMessage order)
        {
            if (order == null)
                throw new ApplicationException("SaveOrderMessage() order can not be null");

            string theGuid = order.Id.Trim();
            // dont need this strip guid like for queues
            if (Validation.IsValidGuid(theGuid) == false)
                theGuid = GuidHelper.NewGuidString();

            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlTransaction trans = db.BeginTransaction())
                {
                    try
                    {
                        lock (statusLock)
                        {
                            using (SqlCommand command = db.CreateCommand())
                            {
                                command.Transaction = trans;

                                // delete previous data
                                command.CommandText = "DELETE FROM [OrderMessage] WHERE [Guid]=@id";
                                command.Parameters.AddWithValue("@id", order.Id);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText = "INSERT INTO [OrderMessage] ([Guid],[MessageStatus],[Description],[Details],[DateCreated],[DateLastModified]" +
                                                      ") VALUES (@f1,@f2,@f3,@f4,@f5,@f6)";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@f1", order.Id);
                                command.Parameters.AddWithValue("@f2", order.OrderMessageStatus);
                                command.Parameters.AddWithValue("@f3", NullToString(order.Description));
                                command.Parameters.AddWithValue("@f4", NullToString(order.Details));
                                command.Parameters.AddWithValue("@f5", (order.DateCreated == DateTime.MinValue) ? MinDate : order.DateCreated);
                                command.Parameters.AddWithValue("@f6", (order.DateLastModified == DateTime.MinValue) ? MinDate : order.DateLastModified);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                trans.Commit();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, "Order: " + order.ToString());
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        db.Close();
                    }
                }
            }
            return theGuid;
        }

        public OrderMessage OrderMessageGetById(string guid)
        {
            OrderMessage order = null;
            if (Validation.IsValidGuid(guid) == false)
                return order;

            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlCommand command = db.CreateCommand())
                {
                    command.CommandText = "SELECT [Guid],[Id],[MessageStatus],[Description],[Details],[DateCreated],[DateLastModified] " +
                                          "FROM [OrderMessage] WHERE [Guid]=@id";

                    command.Parameters.AddWithValue("@id", guid);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToOrderMessage(reader);
                        }
                    }
                }
                db.Close();
            }
            return order;
        }

        public List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest request)
        {
            List<OrderMessage> orders = new List<OrderMessage>();
            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlCommand command = db.CreateCommand())
                {
                    string sql = "SELECT ";
                    if (request.MaxOrders > 0)
                        sql += string.Format("TOP({0}) ", request.MaxOrders);
                    sql += "[Guid],[Id],[MessageStatus],[Description],[Details],[DateCreated],[DateLastModified] " +
                           "FROM [OrderMessage]";

                    List<string> sqlwhere = new List<string>();
                    if (request.MessageStatusFilter != OrderMessageStatusFilterType.None)
                    {
                        sqlwhere.Add("[MessageStatus]=@f1");
                        command.Parameters.AddWithValue("@f1", (int)request.MessageStatusFilter);
                    }
                    if (string.IsNullOrWhiteSpace(request.Description) == false)
                    {
                        sqlwhere.Add("[Description] LIKE @f2");
                        command.Parameters.AddWithValue("@f2", string.Format("%{0}%", request.Description));
                    }
                    if (request.DateFrom != null && request.DateTo != null)
                    {
                        sqlwhere.Add("([DateCreated]>=@f3 AND [DateCreated]<=@f4)");
                        command.Parameters.AddWithValue("@f3", request.DateFrom);
                        command.Parameters.AddWithValue("@f4", request.DateTo);
                    }

                    if (sqlwhere.Count > 0)
                    {
                        sql += " WHERE ";
                        bool first = true;
                        foreach (string w in sqlwhere)
                        {
                            if (first == false)
                                sql += " AND ";
                            sql += w;
                            first = false;
                        }
                    }

                    sql += " ORDER BY [Id] ASC";

                    command.CommandText = sql;
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrderMessage order = ReaderToOrderMessage(reader);
                            orders.Add(order);
                        }
                    }
                }
                db.Close();
            }
            return orders;
        }

        public void UpdateStatus(string guid, OrderMessageStatus status)
        {
            //if guid not found in db and not sent in then create a new one
            if (base.DoesRecordExist("[OrderMessage]", "[Guid]=@0", guid) == false)
                throw new LSOmniServiceException(StatusCode.OrderQueueIdNotFound, "OrderMessage does not exists Id: " + guid);

            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                lock (statusLock)
                {
                    using (SqlCommand command = db.CreateCommand())
                    {
                        command.CommandText = "UPDATE [OrderMessage] SET " +
                            "[MessageStatus]=@f1,[DateLastModified]=@f2 WHERE [Guid]=@id";

                        command.Parameters.AddWithValue("@f1", status);
                        command.Parameters.AddWithValue("@f2", DateTime.Now);
                        command.Parameters.AddWithValue("@id", guid);
                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                }
                db.Close();
            }
        }

        public string GetNotificationIdGetByOrderMessageId(long orderMessageId)
        {
            string notificationId = "";
            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlCommand command = db.CreateCommand())
                {
                    command.CommandText = "SELECT [NotificationId] FROM [OrderMessageNotification] WHERE [OrderMessageId]=@id";

                    command.Parameters.AddWithValue("@id", orderMessageId);
                    TraceSqlCommand(command);
                    notificationId = (string)command.ExecuteScalar();
                }
                db.Close();
            }
            return notificationId;
        }

        public void OrderMessageNotificationSave(string notificationId, long orderMessageId, string contactId, string description, string details, string qrText)
        {
            //for notification table, Type should be = 1 (means contact) and TypeCode = contactId
            int typeOfContact = 1;
            string typeCodeContact = contactId;
            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlTransaction trans = db.BeginTransaction())
                {
                    try
                    {
                        lock (statusLock)
                        {
                            using (SqlCommand command = db.CreateCommand())
                            {
                                command.Transaction = trans;

                                command.CommandText = "INSERT INTO [Notification] ([Id],[Type],[TypeCode],[PrimaryText],[SecondaryText],[DisplayFrequency]," +
                                                      "[ValidFrom],[ValidTo],[Created],[CreatedBy],[LastModifiedDate],[DateLastModified],[QRText],[NotificationType],[Status]" +
                                                      ") VALUES (@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14,@f15)";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@f1", notificationId);
                                command.Parameters.AddWithValue("@f2", typeOfContact);
                                command.Parameters.AddWithValue("@f3", typeCodeContact);
                                command.Parameters.AddWithValue("@f4", NullToString(description));
                                command.Parameters.AddWithValue("@f5", NullToString(details));
                                command.Parameters.AddWithValue("@f6", 0);
                                command.Parameters.AddWithValue("@f7", DateTime.Now.AddMonths(-1));
                                command.Parameters.AddWithValue("@f8", DateTime.Now.AddDays(2));
                                command.Parameters.AddWithValue("@f9", DateTime.Now);
                                command.Parameters.AddWithValue("@f10", string.Empty);
                                command.Parameters.AddWithValue("@f11", DateTime.Now);
                                command.Parameters.AddWithValue("@f12", DateTime.Now);
                                command.Parameters.AddWithValue("@f13", NullToString(qrText));
                                command.Parameters.AddWithValue("@f14", 1); //0 = BO, 1 = OrderMessage 
                                command.Parameters.AddWithValue("@f15", 0); //Status 0 = New
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                trans.Commit();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex);
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        db.Close();
                    }
                }
            }
        }

        private OrderMessage ReaderToOrderMessage(SqlDataReader reader)
        {
            OrderMessage order = new OrderMessage();
            {
                order.OrderId = SQLHelper.GetInt64(reader["Id"]);
                order.Id = SQLHelper.GetString(reader["Guid"]);
                order.Details = SQLHelper.GetString(reader["Details"]);
                order.Description = SQLHelper.GetString(reader["Description"]);
                order.OrderMessageStatus = (OrderMessageStatus)SQLHelper.GetInt32(reader["MessageStatus"]);
                order.DateCreated = SQLHelper.GetDateTime(reader["DateCreated"]);
                order.DateLastModified = SQLHelper.GetDateTime(reader["DateLastModified"]);
            }
            return order;
        }
    }
}
