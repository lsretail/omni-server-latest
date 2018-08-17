using System;
using System.Data.SqlClient;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class ValidationRepository : BaseRepository, IValidationRepository
    {
        public ValidationRepository() : base()
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

                    if (string.IsNullOrEmpty(deviceId) == false)
                    {
                        command.CommandText = "SELECT [DeviceStatus] FROM [Device] WHERE [Id]=@id";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@id", deviceId);

                        object value = command.ExecuteScalar();
                        if (value != null && value != DBNull.Value)
                        {
                            int status = Convert.ToInt32(value);
                            if (status == 3 || status == 1)
                            {
                                statusCode = StatusCode.DeviceIsBlocked;
                            }
                        }
                    }
                    else
                    {
                        statusCode = StatusCode.UserNotLoggedIn;
                    }
                }
                connection.Close();
            }
            return statusCode;
        }

        public bool ValidateAccount(string id, string contactId)
        {
            return base.DoesRecordExist("[Contact]", "[Id]=@0 AND [AccountId]=@1 AND [ContactStatus] IS NULL", contactId, id);
        }

        public bool ValidateContact(string id, string contactId)
        {
            //if id is blank then dont event check the contactId from securitytoken
            if (string.IsNullOrWhiteSpace(id))
                return false;

            if (id.Equals(contactId, StringComparison.InvariantCultureIgnoreCase) == false)
                return false;

            return base.DoesRecordExist("[Contact]", "[Id]=@0 AND [ContactStatus] IS NULL", id);
        }

        public bool ValidateCard(string id, string contactId)
        {
            //if id is blank then dont event check the contactId from securitytoken
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return base.DoesRecordExist("[Card]", "[Id]=@0 AND [ContactId]=@1", id, contactId);
        }

        public bool ValidateContactUserName(string userName, string contactId)
        {
            return base.DoesRecordExist("[Contact] AS c INNER JOIN [Card] AS cd ON c.[Id] = cd.[ContactId] INNER JOIN [UserCard] AS uc ON cd.[Id] = uc.[CardId] " +
                                        "INNER JOIN [User] AS u ON uc.UserId = u.UserId", "c.[Id]=@0 AND u.[UserId]=@1 AND [ContactStatus] IS NULL", contactId, userName);
        }

        public bool ValidateOneList(string id, string contactId)
        {
            return base.DoesRecordExist("[OneList] s INNER JOIN [Contact] c ON s.[ContactId]=c.[Id]", "s.[Id]=@0 AND s.[ContactId]=@1 AND c.[ContactStatus] IS NULL", id, contactId);
        }
    }
}
         