using System;
using System.Data.SqlClient;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    public class ResetPasswordRepository : BaseRepository, IResetPasswordRepository
    {
        public ResetPasswordRepository(BOConfiguration config) : base(config)
        {
        }

        public void ResetPasswordSave(string resetCode, string contactId, string email)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "IF EXISTS(SELECT * FROM [ResetPassword] WHERE [ResetCode]=@code AND [Enabled]=1) " +
                        "UPDATE [ResetPassword] SET [ContactId]=@f1,[Email]=@f2 WHERE [ResetCode]=@code" +
                        " ELSE " +
                        "INSERT INTO [ResetPassword] ([ResetCode],[ContactId],[Email],[Enabled],[Created]) VALUES (@code,@f1,@f2,@f3,@f4)";

                    command.Parameters.AddWithValue("@f1", contactId);
                    command.Parameters.AddWithValue("@f2", email);
                    command.Parameters.AddWithValue("@f3", true);
                    command.Parameters.AddWithValue("@f4", DateTime.Now);
                    command.Parameters.AddWithValue("@code", resetCode);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public DateTime ResetPasswordGetDateById(string id)
        {
            DateTime dtOut = DateTime.MinValue;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "SELECT [Created] FROM [ResetPassword] WHERE [ResetCode]=@code";
                    command.Parameters.AddWithValue("@code", id);
                    TraceSqlCommand(command);
                    object date = command.ExecuteScalar();
                    if (date != null)
                    {
                        dtOut = (DateTime)date;
                    }
                }
                connection.Close();
            }
            return dtOut;
        }

        public void ResetPasswordDelete(string resetCode)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = "UPDATE [ResetPassword] SET [Enabled]=0 WHERE [ResetCode]=@code";
                    command.Parameters.AddWithValue("@code", resetCode);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public bool ResetPasswordExists(string resetCode, string contactId)
        {
            bool doesExist = base.DoesRecordExist("[ResetPassword]", "[ResetCode]=@0 AND [ContactId]=@1 AND [Enabled]=1", 
                NullToString(resetCode, 100), NullToString(contactId, 20));

            //check if the resetcode and contactId match
            if (doesExist == false)
                return false;
            else
                return true;
        }
    }
}
        