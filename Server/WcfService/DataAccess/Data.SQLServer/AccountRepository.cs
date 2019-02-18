using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSOmni.DataAccess.Dal
{
    public class AccountRepository : BaseRepository, IAccountRepository
    {
        public AccountRepository() : base()
        {
        }

        public Account AccountGetById(string id)
        {
            Account account = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[SchemeId],[Balance] FROM [Account] WHERE [Id]=@id  ";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            account = new Account()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                PointBalance = Convert.ToInt64(SQLHelper.GetDecimal(reader, "Balance")),
                                Scheme = SchemeGetById(SQLHelper.GetString(reader["SchemeId"]))
                            };
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return account;
        }

        public Scheme SchemeGetById(string id)
        {
            List<Scheme> schemeList = new List<Scheme>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT [Id],[Description],[ClubId],[MinPointsToUpgrade],[NextSchemeBenefits] " +
                        "FROM [Scheme] WHERE [ClubId]=(SELECT [ClubId] FROM [Scheme] WHERE [Id]=@id) " +
                        "ORDER BY [UpdateSequence]";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Scheme scheme = new Scheme()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                Description = SQLHelper.GetString(reader["Description"]),
                                Perks = SQLHelper.GetString(reader["NextSchemeBenefits"]),
                                PointsNeeded = Convert.ToInt64(SQLHelper.GetString(reader["MinPointsToUpgrade"])),
                                NextScheme = null,
                                Club = ClubGetById(SQLHelper.GetString(reader["ClubId"])),
                            };
                            schemeList.Add(scheme);
                        }
                        reader.Close();
                    }
                }
                connection.Close();

                //set the next scheme, last scheme has NextScheme = null
                for (int i = 0; i < schemeList.Count - 1; i++)
                {
                    schemeList[i].PointsNeeded = Math.Abs(schemeList[i + 1].PointsNeeded);
                    schemeList[i].NextScheme = schemeList[i + 1];
                }
                return schemeList.FirstOrDefault<Scheme>();
            }
        }

        public Club ClubGetById(string id)
        {
            Club club = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[Description] FROM [Club] WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            club = new Club()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                Name = SQLHelper.GetString(reader["Description"]),
                            };
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return club;
        }
    }
}
 