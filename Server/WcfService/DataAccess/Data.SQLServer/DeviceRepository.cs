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

        public bool DoesDeviceIdExist(string deviceId)
        {
            //return true if the id exists
            return base.DoesRecordExist("[Device]", "[Id]=@0", deviceId);
        }

        public bool SpgUnlockRodDevice(string storeId, string cardId)
        {
            bool ret = false;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "IF EXISTS (SELECT * FROM [RodDeviceUnlock] WHERE [StoreId]=@sId AND [CardId]=@cId) " +
                                        "UPDATE [RodDeviceUnlock] SET [Regtime]=CURRENT_TIMESTAMP WHERE [StoreId]=@sId AND [CardId]=@cId " +
                                        "ELSE " +
                                        "INSERT INTO [RodDeviceUnlock] ([StoreId],[CardId],[Regtime]) VALUES (@sId,@cId,CURRENT_TIMESTAMP)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@sId", storeId);
                    command.Parameters.AddWithValue("@cId", cardId);
                    TraceSqlCommand(command);
                    connection.Open();
                    ret = command.ExecuteNonQuery() > 0;
                }
                connection.Close();
            }
            return ret;
        }

        public string SpgUnlockRodDeviceCheck(string storeId)
        {
            string cardId = string.Empty;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [CardId] FROM [RodDeviceUnlock] WHERE [StoreId]=@sId ORDER BY [Regtime]";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@sId", storeId);
                    TraceSqlCommand(command);
                    connection.Open();
                    cardId = (string)command.ExecuteScalar();

                    if (string.IsNullOrEmpty(cardId) == false)
                    {
                        command.CommandText = "DELETE FROM [RodDeviceUnlock] WHERE [StoreId]=@sId AND [CardId]=@cId";
                        command.Parameters.AddWithValue("@cId", cardId);
                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            return cardId;
        }
    }
}
