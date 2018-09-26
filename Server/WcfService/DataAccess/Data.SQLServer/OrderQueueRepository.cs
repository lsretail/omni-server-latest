using System;
using System.Data.SqlClient;
using System.Collections.Generic;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;

namespace LSOmni.DataAccess.Dal
{
    public class OrderQueueRepository : BaseRepository, IOrderQueueRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static object statusLock = new object();

        public string Save(OrderQueue order)
        {
            if (order == null)
                throw new ApplicationException("Save() order can not be null");

            if (Validation.IsValidGuid(order.Id) == false)
                order.Id = GuidHelper.NewGuidString();

            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlTransaction trans = db.BeginTransaction())
                {
                    lock (statusLock)
                    {
                        try
                        {
                            using (SqlCommand command = db.CreateCommand())
                            {
                                command.Transaction = trans;

                                // delete previous data
                                command.CommandText = "DELETE FROM [OrderQueue] WHERE [Guid]=@id";
                                command.Parameters.AddWithValue("@id", order.Id);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText = "INSERT INTO [OrderQueue] (" +
                                                "[Guid],[OrderStatus],[OrderType],[OrderXml],[DateCreated],[DateLastModified]," +
                                                "[Description],[PhoneNumber],[Email],[SearchKey],[ContactId]," +
                                                "[DeviceId],[StoreId],[TerminalId],[StatusChange]" +
                                                ") VALUES (@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14,@f15)";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@f1", order.Id);
                                command.Parameters.AddWithValue("@f2", (int)order.OrderQueueStatus);
                                command.Parameters.AddWithValue("@f3", (int)order.OrderQueueType);
                                command.Parameters.AddWithValue("@f4", NullToString(order.OrderXml));
                                command.Parameters.AddWithValue("@f5", (order.DateCreated == DateTime.MinValue) ? MinDate : order.DateCreated);
                                command.Parameters.AddWithValue("@f6", (order.DateLastModified == DateTime.MinValue) ? MinDate : order.DateLastModified);
                                command.Parameters.AddWithValue("@f7", NullToString(order.Description));
                                command.Parameters.AddWithValue("@f8", NullToString(order.PhoneNumber, 20));
                                command.Parameters.AddWithValue("@f9", NullToString(order.Email, 250));
                                command.Parameters.AddWithValue("@f10", NullToString(order.SearchKey, 100));
                                command.Parameters.AddWithValue("@f11", NullToString(order.ContactId, 50));
                                command.Parameters.AddWithValue("@f12", NullToString(order.DeviceId, 100));
                                command.Parameters.AddWithValue("@f13", NullToString(order.StoreId, 50));
                                command.Parameters.AddWithValue("@f14", NullToString(order.TerminalId, 50));
                                command.Parameters.AddWithValue("@f15", NullToString(order.StatusChange));
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                trans.Commit();
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
            }
            return order.Id.Replace("-", "");   //strip out the guild so client doesn't see it
        }

        public void UpdateStatus(string guid, OrderQueueStatus status)
        {
            guid = GuidHelper.GuidWithDash(guid);
            if (base.DoesRecordExist("[OrderQueue]", "[Guid]=@0", guid) == false)
                throw new LSOmniServiceException(StatusCode.OrderQueueIdNotFound, "OrderQueue does not exists Id: " + guid);

            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                lock (statusLock)
                {
                    using (SqlCommand command = db.CreateCommand())
                    {
                        command.CommandText = "UPDATE [OrderQueue] SET " +
                            "[OrderStatus]=@f1,[StatusChange]=@f2 WHERE [Guid]=@id";

                        command.Parameters.AddWithValue("@f1", (int)status);
                        command.Parameters.AddWithValue("@f2", string.Format("StatusChange - {0}", status));
                        command.Parameters.AddWithValue("@id", guid);
                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                }
                db.Close();
            }
        }

        // Always return by Id desc, oldest first
        public List<OrderQueue> OrderSearch(OrderSearchRequest request)
        {
            List<OrderQueue> orders = new List<OrderQueue>();
            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlCommand command = db.CreateCommand())
                {

                    string sql = "SELECT ";
                    if (request.MaxOrders > 0)
                        sql += string.Format("TOP ({0}) ", request.MaxOrders);

                    sql += "[Guid],[Id],[OrderStatus],[OrderType],[OrderXml],[DateCreated]," +
                           "[DateLastModified],[Description],[PhoneNumber],[Email],[SearchKey]," +
                           "[ContactId],[DeviceId],[StoreId],[TerminalId],[StatusChange] FROM [OrderQueue]";

                    List<string> sqlwhere = new List<string>();

                    sqlwhere.Add("[OrderType]=@f1");
                    command.Parameters.AddWithValue("@f1", (int)request.OrderType);

                    if (request.OrderStatusFilter != OrderQueueStatusFilterType.None)
                    {
                        sqlwhere.Add("[OrderStatus]=@f2");
                        command.Parameters.AddWithValue("@f2", (int)request.OrderStatusFilter);
                    }
                    if (request.DateFrom != null && request.DateTo != null)
                    {
                        sqlwhere.Add("([DateCreated]>=@f3 AND [DateCreated]<=@f4)");
                        command.Parameters.AddWithValue("@f3", request.DateFrom);
                        command.Parameters.AddWithValue("@f4", request.DateTo);
                    }
                    if (string.IsNullOrWhiteSpace(request.StoreId) == false)
                    {
                        sqlwhere.Add("[StoreId]=@f5");
                        command.Parameters.AddWithValue("@f5", request.StoreId);
                    }
                    if (string.IsNullOrWhiteSpace(request.ContactId) == false)
                    {
                        sqlwhere.Add("[ContactId]=@f6");
                        command.Parameters.AddWithValue("@f6", request.ContactId);
                    }
                    if (string.IsNullOrWhiteSpace(request.SearchKey) == false)
                    {
                        sqlwhere.Add("[SearchKey]=@f7");
                        command.Parameters.AddWithValue("@f7", request.SearchKey);
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
                            OrderQueue order = ReaderToOrderQueue(reader);
                            orders.Add(order);
                        }
                    }
                }
                db.Close();
            }
            return orders;
        }

        public OrderQueue OrderGetById(string guid)
        {
            OrderQueue order = null;
            if (Validation.IsValidGuid(guid) == false)
                return order;
            guid = GuidHelper.GuidWithDash(guid);

            using (SqlConnection db = new SqlConnection(sqlConnectionString))
            {
                db.Open();
                using (SqlCommand command = db.CreateCommand())
                {
                    command.CommandText = "SELECT [Guid],[Id],[OrderStatus],[OrderType],[OrderXml],[DateCreated]," +
                                          "[DateLastModified],[Description],[PhoneNumber],[Email],[SearchKey]," +
                                          "[ContactId],[DeviceId],[StoreId],[TerminalId],[StatusChange] " +
                                          "FROM [OrderQueue] WHERE [Guid]=@id";

                    command.Parameters.AddWithValue("@id", guid);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToOrderQueue(reader);
                        }
                    }
                }
                db.Close();
            }
            return order;
        }

        private OrderQueue ReaderToOrderQueue(SqlDataReader reader)
        {
            OrderQueue order = new OrderQueue();
            order.QueueId = SQLHelper.GetInt64(reader["Id"]);
            order.Id = SQLHelper.GetString(reader["Guid"]);
            order.Id = order.Id.Replace("-", ""); //strip out the guild so client doesn't see it

            order.OrderXml = SQLHelper.GetString(reader["OrderXml"]);
            order.Description = SQLHelper.GetString(reader["Description"]);
            order.ContactId = SQLHelper.GetString(reader["ContactId"]);
            order.StoreId = SQLHelper.GetString(reader["StoreId"]);
            order.SearchKey = SQLHelper.GetString(reader["SearchKey"]);
            order.Email = SQLHelper.GetString(reader["Email"]);
            order.PhoneNumber = SQLHelper.GetString(reader["PhoneNumber"]);
            order.DeviceId = SQLHelper.GetString(reader["DeviceId"]);
            order.TerminalId = SQLHelper.GetString(reader["TerminalId"]);
            order.OrderQueueStatus = (OrderQueueStatus)SQLHelper.GetInt32(reader["OrderStatus"]);
            order.OrderQueueType = (OrderQueueType)SQLHelper.GetInt32(reader["OrderType"]);

            order.DateCreated = SQLHelper.GetDateTime(reader["DateCreated"]);
            order.DateLastModified = SQLHelper.GetDateTime(reader["DateLastModified"]);
            return order;
        }
    }
}
