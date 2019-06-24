using System;
using System.Data.SqlClient;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class DeviceRepository : BaseRepository, IDeviceRepository
    {
        public DeviceRepository(BOConfiguration config)
            : base(config)
        {

        }

        public void DeviceSave(string deviceId, string contactId, string securityToken)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "IF EXISTS (SELECT * FROM [DeviceSecurity] WHERE [DeviceId]=@dId AND [ContactId]=@cId) " +
                                        "UPDATE [DeviceSecurity] SET [SecurityToken]=@tok WHERE [DeviceId]=@dId AND [ContactId]=@cId " +
                                        "ELSE " +
                                        "INSERT INTO [DeviceSecurity] ([SecurityToken],[DeviceId],[ContactId]) VALUES (@tok,@dId,@cId)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@dId", deviceId);
                    command.Parameters.AddWithValue("@cId", contactId);
                    command.Parameters.AddWithValue("@tok", securityToken);
                    TraceSqlCommand(command);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void Logout(string userName, string deviceId)
        {
            RemoveSecurityToken(deviceId, userName);
        }

        public bool DoesDeviceIdExist(string deviceId)
        {
            //return true if the id exists
            return base.DoesRecordExist("[Device]", "[Id]=@0", deviceId);
        }

        private void RemoveSecurityToken(string deviceId, string contactId)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [DeviceSecurity] WHERE [DeviceId]=@f0 AND [ContactId]=@f1";
                    command.Parameters.AddWithValue("@f0", deviceId);
                    command.Parameters.AddWithValue("@f1", contactId);
                    connection.Open();
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
