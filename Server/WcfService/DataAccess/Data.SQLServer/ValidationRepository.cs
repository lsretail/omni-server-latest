using System;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class ValidationRepository : BaseRepository, IValidationRepository
    {
        public ValidationRepository(BOConfiguration config) : base(config)
        {
        }

        public StatusCode ValidateSecurityToken(string securityToken, out string deviceId, out string contactId)
        {
            deviceId = string.Empty;
            contactId = string.Empty;
            StatusCode statusCode = StatusCode.OK;

            if (string.IsNullOrWhiteSpace(securityToken))
            {
                return StatusCode.SecurityTokenInvalid;
            }

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [DeviceId],[ContactId] FROM [DeviceSecurity] WHERE [SecurityToken]=@token";
                    command.Parameters.AddWithValue("@token", securityToken);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            deviceId = SQLHelper.GetString(reader["DeviceId"]);
                            contactId = SQLHelper.GetString(reader["ContactId"]);
                        }
                        reader.Close();
                    }

                    if (string.IsNullOrEmpty(deviceId))
                    {
                        statusCode = StatusCode.UserNotLoggedIn;
                    }
                }
                connection.Close();
            }
            return statusCode;
        }

        public bool ValidateOneList(string id, string cardId)
        {
            return base.DoesRecordExist("[OneList] s ", "s.[Id]=@0 AND s.[CardId]=@1 ", id, cardId);
        }
    }
}
         