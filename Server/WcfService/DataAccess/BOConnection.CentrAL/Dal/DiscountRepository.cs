using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    /// <summary>
    /// Implements all repository interfaces for NAV 
    /// </summary>
    public class DiscountRepository : BaseRepository
    {
        public DiscountRepository(BOConfiguration config) : base(config)
        {
        }

        public List<ProactiveDiscount> DiscountsGetByStoreAndItem(string storeId, List<string> itemIds)
        {
            string sqlcolumns = "p.[No_], p.[Type],p.[Priority],p.[Description],p.[Pop-up Line 1],p.[Pop-up Line 2],p.[Pop-up Line 3],p.[Validation Period ID],mt.[Store No_],mt.[Item No_]," +
                                "mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code],mt.[Discount _],mt.[Minimum Quantity],mt.[Unit of Measure Code]";

            string sqlfrom = " FROM [" + navCompanyName + "WI Discounts$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                             " INNER JOIN [" + navCompanyName + "Periodic Discount$5ecfc871-5d82-43f1-9c54-59685e82318d] p ON p.[No_]=mt.[Offer No_]";

            string sqlMMcolumns = "p.[No_],p.[Type],p.[Priority],p.[Description],p.[Pop-up Line 1],p.[Pop-up Line 2],p.[Pop-up Line 3],p.[Validation Period ID],p.[Discount _ Value] AS [Discount _]," +
                                  "mt.[Store No_],mt.[Item No_],mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code], 0 AS [Minimum Quantity]," +
                                  "'' AS [Unit of Measure Code]";

            string sqlMMfrom = " FROM [" + navCompanyName + "WI Mix & Match Offer$5ecfc871-5d82-43f1-9c54-59685e82318d] mt" +
                               " INNER JOIN [" + navCompanyName + "Periodic Discount$5ecfc871-5d82-43f1-9c54-59685e82318d] p ON p.[No_]=mt.[Offer No_]";

            List<ProactiveDiscount> list = new List<ProactiveDiscount>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sql = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Store No_]=@sid AND mt.[Item No_] IN ({0})";
                    command.Parameters.AddWithValue("@sid", storeId);

                    var idParameterList = new List<string>();
                    var index = 0;
                    foreach (string item in itemIds)
                    {
                        var paramName = "@idParam" + index;
                        command.Parameters.AddWithValue(paramName, item);
                        idParameterList.Add(paramName);
                        index++;
                    }
                    command.CommandText = String.Format(sql, string.Join(",", idParameterList));
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToDiscount(reader));
                        }
                        reader.Close();
                    }

                    sql = "SELECT " + sqlMMcolumns + sqlMMfrom + " WHERE mt.[Store No_]=@sid AND mt.[Item No_] IN ({0})";
                    command.CommandText = String.Format(sql, string.Join(",", idParameterList));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToDiscount(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public void LoadDiscountDetails(ProactiveDiscount disc, string storeId, string loyaltySchemeCode)
        {
            if (disc.Type == ProactiveDiscountType.MixMatch)
            {
                disc.ItemIds = GetItemIdsByMixAndMatchOffer(storeId, disc.Id, loyaltySchemeCode);
                disc.ItemIds.Remove(disc.ItemId);
                disc.BenefitItemIds = GetBenefitItemIds(disc.Id);
            }
            else if (disc.Type == ProactiveDiscountType.DiscOffer)
            {
                disc.Price = PriceGetByItem(storeId, disc.ItemId, disc.VariantId);
                disc.PriceWithDiscount = disc.Price * (1 - disc.Percentage / 100);
            }
        }

        public DiscountValidation GetDiscountValidationByOfferId(string periodId)
        {
            DiscountValidation discval = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [" + navCompanyName + "Validation Period$5ecfc871-5d82-43f1-9c54-59685e82318d] mt WHERE [ID]=@id";
                    command.Parameters.AddWithValue("@id", periodId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            discval = ReaderToDiscountValidation(reader);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return discval;
        }

        private List<string> GetItemIdsByMixAndMatchOffer(string storeId, string offerId, string scheme)
        {
            List<string> ids = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Item No_] FROM [" + navCompanyName + "WI Mix & Match Offer$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Offer No_]=@id AND [Store No_]=@sid AND ([Loyalty Scheme Code]=@sc OR [Loyalty Scheme Code]='')";
                    command.Parameters.AddWithValue("@id", offerId);
                    command.Parameters.AddWithValue("@sid", storeId);
                    command.Parameters.AddWithValue("@sc", scheme);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(SQLHelper.GetString(reader["Item No_"]));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return ids;
        }

        private List<string> GetBenefitItemIds(string offerId)
        {
            List<string> ids = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [No_] FROM [" + navCompanyName + "Periodic Discount Benefits$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Offer No_]=@id";
                    command.Parameters.AddWithValue("@id", offerId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(SQLHelper.GetString(reader["No_"]));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return ids;
        }

        private DiscountValidation ReaderToDiscountValidation(SqlDataReader reader)
        {
            return new DiscountValidation()
            {
                Id = SQLHelper.GetString(reader["ID"]),
                Description = SQLHelper.GetString(reader["Description"]),
                StartDate = SQLHelper.GetDateTime(reader["Starting Date"]),
                EndDate = SQLHelper.GetDateTime(reader["Ending Date"]),
                StartTime = SQLHelper.GetDateTime(reader["Starting Time"]),
                EndTime = SQLHelper.GetDateTime(reader["Ending Time"]),
                MondayStart = SQLHelper.GetDateTime(reader["Monday Starting Time"]),
                MondayEnd = SQLHelper.GetDateTime(reader["Monday Ending Time"]),
                TuesdayStart = SQLHelper.GetDateTime(reader["Tuesday Starting Time"]),
                TuesdayEnd = SQLHelper.GetDateTime(reader["Tuesday Ending Time"]),
                WednesdayStart = SQLHelper.GetDateTime(reader["Wednesday Starting Time"]),
                WednesdayEnd = SQLHelper.GetDateTime(reader["Wednesday Ending Time"]),
                ThursdayStart = SQLHelper.GetDateTime(reader["Thursday Starting Time"]),
                ThursdayEnd = SQLHelper.GetDateTime(reader["Thursday Ending Time"]),
                FridayStart = SQLHelper.GetDateTime(reader["Friday Starting Time"]),
                FridayEnd = SQLHelper.GetDateTime(reader["Friday Ending Time"]),
                SaturdayStart = SQLHelper.GetDateTime(reader["Saturday Starting Time"]),
                SaturdayEnd = SQLHelper.GetDateTime(reader["Saturday Ending Time"]),
                SundayStart = SQLHelper.GetDateTime(reader["Sunday Starting Time"]),
                SundayEnd = SQLHelper.GetDateTime(reader["Sunday Ending Time"]),
                TimeWithinBounds = SQLHelper.GetBool(reader["Time within Bounds"]),
                EndAfterMidnight = SQLHelper.GetBool(reader["Ending Time After Midnight"]),
                MondayWithinBounds = SQLHelper.GetBool(reader["Mon_ Time within Bounds"]),
                MondayEndAfterMidnight = SQLHelper.GetBool(reader["Mon_ End_ Time After Midnight"]),
                TuesdayWithinBounds = SQLHelper.GetBool(reader["Tue_ Time within Bounds"]),
                TuesdayEndAfterMidnight = SQLHelper.GetBool(reader["Tue_ End_ Time After Midnight"]),
                WednesdayWithinBounds = SQLHelper.GetBool(reader["Wed_ Time within Bounds"]),
                WednesdayEndAfterMidnight = SQLHelper.GetBool(reader["Wed_ End_ Time After Midnight"]),
                ThursdayWithinBounds = SQLHelper.GetBool(reader["Thu_ Time within Bounds"]),
                ThursdayEndAfterMidnight = SQLHelper.GetBool(reader["Thu_ End_ Time After Midnight"]),
                FridayWithinBounds = SQLHelper.GetBool(reader["Fri_ Time within Bounds"]),
                FridayEndAfterMidnight = SQLHelper.GetBool(reader["Fri_ End_ Time After Midnight"]),
                SaturdayWithinBounds = SQLHelper.GetBool(reader["Sat_ Time within Bounds"]),
                SaturdayEndAfterMidnight = SQLHelper.GetBool(reader["Sat_ End_ Time After Midnight"]),
                SundayWithinBounds = SQLHelper.GetBool(reader["Sun_ Time within Bounds"]),
                SundayEndAfterMidnight = SQLHelper.GetBool(reader["Sun_ End_ Time After Midnight"])
            };
        }

        private ProactiveDiscount ReaderToDiscount(SqlDataReader reader)
        {
            ProactiveDiscount disc = new ProactiveDiscount()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Percentage = SQLHelper.GetDecimal(reader["Discount _"]),
                Priority = SQLHelper.GetInt32(reader["Priority"]),
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                LoyaltySchemeCode = SQLHelper.GetString(reader["Loyalty Scheme Code"]),
                MinimumQuantity = SQLHelper.GetDecimal(reader["Minimum Quantity"]),
                Description = SQLHelper.GetString(reader["Description"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UnitOfMeasureId = SQLHelper.GetString(reader["Unit of Measure Code"]),
                PopUpLine1 = SQLHelper.GetString(reader["Pop-up Line 1"]),
                PopUpLine2 = SQLHelper.GetString(reader["Pop-up Line 2"]),
                PopUpLine3 = SQLHelper.GetString(reader["Pop-up Line 3"]),
                PeriodId = SQLHelper.GetString(reader["Validation Period ID"])
            };

            disc.SetProactiveDiscountType(SQLHelper.GetInt32(reader["Type"]));
            return disc;
        }

        private decimal PriceGetByItem(string storeId, string itemId, string variantId)
        {
            string sqlMcolumns = "mt.[Unit Price] ";
            string sqlMfrom = " FROM [" + navCompanyName + "WI Price$5ecfc871-5d82-43f1-9c54-59685e82318d] mt";

            decimal price = 0m;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlMcolumns + sqlMfrom + " WHERE mt.[Item No_]=@iid ";
                    command.Parameters.AddWithValue("@iid", itemId);

                    if (string.IsNullOrWhiteSpace(storeId) == false)
                    {
                        command.CommandText += " AND mt.[Store No_]=@Sid";
                        command.Parameters.AddWithValue("@Sid", storeId);
                    }

                    if (string.IsNullOrWhiteSpace(variantId) == false)
                    {
                        command.CommandText += " AND mt.[Variant Code]=@Vid";
                        command.Parameters.AddWithValue("@Vid", variantId);
                    }

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            price = SQLHelper.GetDecimal(reader["Unit Price"]);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return price;
        }
    }
}
