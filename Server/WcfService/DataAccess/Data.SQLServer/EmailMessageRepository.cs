using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class EmailMessageRepository : BaseRepository, IEmailMessageRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const int maxRetryCounter = 3;

        public int MaxRetryCounter()
        {
            return maxRetryCounter;
        }

        public string Save(EmailMessage email)
        {
            if (email == null)
                throw new ApplicationException("Save() email can not be null");

            string theGuid = email.Guid.Trim();
            if (ValidateGuid(theGuid) == false)
            {
                theGuid = GuidHelper.NewGuidString();
                email.Guid = theGuid;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlTransaction trans = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;

                                command.CommandText = "DELETE FROM [EmailMessage] WHERE [Guid]=@guid ";
                                command.Parameters.AddWithValue("@guid", new Guid(email.Guid));
                                command.ExecuteNonQuery();

                                command.CommandText =
                                    "INSERT INTO [EmailMessage] ([Guid],[EmailTo],[EmailCc],[EmailFrom],[EmailStatus],[EmailType]," +
                                    "[Subject],[Body],[Attachments],[Error],[RetryCounter],[ExternalId],[DateCreated],[DateLastModified]) " +
                                    "VALUES (@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14)";

                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@f1", new Guid(email.Guid));
                                command.Parameters.AddWithValue("@f2", email.EmailTo);
                                command.Parameters.AddWithValue("@f3", email.EmailCc);
                                command.Parameters.AddWithValue("@f4", email.EmailFrom);
                                command.Parameters.AddWithValue("@f5", email.EmailStatus);
                                command.Parameters.AddWithValue("@f6", email.EmailType);
                                command.Parameters.AddWithValue("@f7", email.Subject);
                                command.Parameters.AddWithValue("@f8", email.Body);
                                command.Parameters.AddWithValue("@f9", email.Attachments);
                                command.Parameters.AddWithValue("@f10", email.Error);
                                command.Parameters.AddWithValue("@f11", email.RetryCounter);
                                command.Parameters.AddWithValue("@f12", email.ExternalId);
                                command.Parameters.AddWithValue("@f13", email.DateCreated);
                                command.Parameters.AddWithValue("@f14", email.DateLastModified);
                                command.ExecuteNonQuery();
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "email: " + email.ToString());
                throw;
            }
            return theGuid;
        }

        public void UpdateStatus(string guid, EmailStatus emailStatus, string errorMessage)
        {
            //if guid not found in db and not sent in then create a new one
            bool doesEmailExist = DoesEmailMessageExist(guid);
            if (doesEmailExist == false)
                throw new LSOmniServiceException(StatusCode.OrderQueueIdNotFound, "email does not exists Id: " + guid);

            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    Guid theGuid = new Guid(guid);
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "UPDATE [EmailMessage] SET " +
                                              "[EmailStatus]=@f0,[Error]=@f1,[RetryCounter]=[RetryCounter]+1 " +
                                              "WHERE [Guid]=@f2";

                        command.Parameters.AddWithValue("@f0", (Int32) emailStatus);
                        command.Parameters.AddWithValue("@f1", errorMessage);
                        command.Parameters.AddWithValue("@f2", theGuid);
                        TraceSqlCommand(command);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "guid: " + guid);
                throw;
            }
        }

        // Always return by Id desc, oldest first
        public List<EmailMessage> EmailMessageSearch(DateTime? dateFrom, DateTime? dateTo, EmailStatus? status)
        {
            List<EmailMessage> list = new List<EmailMessage>();
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    //must start at 0
                    string sql = "SELECT * FROM [EmailMessage] WHERE 1=1 ";

                    if (status != null)
                    {
                        sql += string.Format(" and [EmailStatus] = {0} ", (int)status);
                    }
                    if (dateFrom != null)
                    {
                        sql += string.Format(" and [DateCreated] >= '{0}' ", dateFrom.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                    }
                    if (dateTo != null)
                    {
                        sql += string.Format(" and [DateCreated] <= '{0}'  ", dateTo.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                    }
                    sql += string.Format(" and [RetryCounter] <= {0}  ", maxRetryCounter);
                    sql += " order by [DateCreated] asc";
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;

                        TraceSqlCommand(command);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(readerToEmailMessage(reader));
                            }
                        }
                    }
                    connection.Close();
                    return list;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "search: ", ex);
                throw;
            }
        }

        public EmailMessage EmailMessageGetById(string guid)
        {
            EmailMessage email = null;
            if (ValidateGuid(guid) == false)
                return email;
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM [EmailMessage] WHERE [Guid]=@guid";

                        command.Parameters.AddWithValue("@guid", guid);
                        TraceSqlCommand(command);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                email = readerToEmailMessage(reader);
                            }
                        }
                    }
                    connection.Close();
                }
                return email;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Guid: " + guid, ex);
                throw;
            }
        }

        private bool DoesEmailMessageExist(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return false;

            return base.DoesRecordExist("[EmailMessage]", "[Guid]=@0", guid);
        }

        private EmailMessage readerToEmailMessage(SqlDataReader reader)
        {
            EmailMessage email = new EmailMessage();
            email.Guid = SQLHelper.GetGuid(reader["Guid"]).ToString();
            email.Body = SQLHelper.GetString(reader["Body"]);
            email.DateCreated = SQLHelper.GetDateTime(reader["DateCreated"]);
            email.DateLastModified = SQLHelper.GetDateTime(reader["DateLastModified"]);
            email.EmailCc = SQLHelper.GetString(reader["EmailCc"]);
            email.EmailFrom = SQLHelper.GetString(reader["EmailFrom"]);
            email.EmailTo = SQLHelper.GetString(reader["EmailTo"]);
            email.EmailStatus = CastTo<EmailStatus>(SQLHelper.GetInt32(reader["EmailStatus"]));
            email.EmailType = CastTo<EmailType>(SQLHelper.GetInt32(reader["EmailType"]));
            email.Error = SQLHelper.GetString(reader["Error"]);
            email.ExternalId = SQLHelper.GetString(reader["ExternalId"]);
            email.Subject = SQLHelper.GetString(reader["Subject"]);
            email.Attachments = SQLHelper.GetString(reader["Attachments"]);
            email.RetryCounter = SQLHelper.GetInt32(reader["RetryCounter"]);
            return email;
        }

        private bool ValidateGuid(string theGuid)
        {
            return Validation.IsValidGuid(theGuid);
        }
    }
}
