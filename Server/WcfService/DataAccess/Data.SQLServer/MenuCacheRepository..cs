using System.Data;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Dal
{
    public class MenuCacheRepository : BaseRepository, IMenuCacheRepository
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MobileMenu MenuGetById(string id, string lastVersion)
        {
            MobileMenu mobileMenu = null;
            id = SQLHelper.CheckForSQLInjection(id);
            //try and find by Id, else take the blank one, finally just pick one..
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[Version],[XmlData] FROM [MenuCache] WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mobileMenu = new MobileMenu();
                            mobileMenu.Id = SQLHelper.GetString(reader["Id"]);
                            mobileMenu.Version = SQLHelper.GetString(reader["Version"]);
                            mobileMenu.xmlData = SQLHelper.GetString(reader["XmlData"]);
                            mobileMenu = DeserializeFromXml(mobileMenu.xmlData);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return mobileMenu;
        }

        public void Save(string id, string version, MobileMenu menu)
        {
            if (menu == null)
                return;

            string xml = SerializeToXml(menu);

            //need to serialize the menulist into one 
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                //del in same transaction
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM [MenuCache] WHERE [Id]=@Id";
                    command.CommandTimeout = 30 * 1000;
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO [MenuCache] ([Id],[Version],[XmlData]) VALUES (@f1,@f2,@f3)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@f1", id);
                    command.Parameters.AddWithValue("@f2", version);
                    command.Parameters.AddWithValue("@f3", xml);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public CacheState Validate(string id)
        {
            id = SQLHelper.CheckForSQLInjection(id);

            string sql = string.Format(@"SELECT [LastModifiedDate],[CreatedDate] FROM [MenuCache] WHERE [Id]='{0}'", id);
            return base.Validate(sql, CacheMenuDurationInMinutes);
        }

        //a bit faster when not using generics like in base class
        private MobileMenu DeserializeFromXml(string xml)
        {
            MobileMenu result = null;
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(MobileMenu));
            using (System.IO.TextReader tr = new System.IO.StringReader(xml))
            {
                result = (MobileMenu)ser.Deserialize(tr);
            }
            return result;
        }
    }
}
