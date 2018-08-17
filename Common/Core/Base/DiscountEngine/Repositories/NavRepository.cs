using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.DiscountEngine.Interfaces;
using LSRetail.Omni.DiscountEngine.Utils;

namespace LSRetail.Omni.DiscountEngine.Repositories
{
    /// <summary>
    /// Implements all repository interfaces for NAV 
    /// </summary>
    public class NavRepository : IDiscountRepository, IPriceRepository
    {
        private string connectionString = string.Empty;
        private string navCompanyName = string.Empty;
        
        public NavRepository(string SqlConnectionString)
        {
            connectionString = SqlConnectionString;

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            navCompanyName = "";
            if (builder.ContainsKey("NAVCompanyName"))
            {
                navCompanyName = builder["NAVCompanyName"] as string; //get the 
                navCompanyName = navCompanyName.Trim();
                //NAV company name must end with a $
                if (navCompanyName.EndsWith("$") == false)
                    navCompanyName += "$";
            }
            builder.Remove("NAVCompanyName");
            connectionString = builder.ConnectionString;
        }
        public List<ProactiveDiscount> DiscountsGetByStoreAndItem(string storeId, string itemId)
        {
            string sqlcolumns = "p.[No_], p.[Type],p.[Priority],p.[Description],p.[Pop-up Line 1],p.[Pop-up Line 2],mt.[Store No_],mt.[Item No_]," +
                                "mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code],mt.[Discount _],mt.[Minimum Quantity],mt.[Unit of Measure Code]";

            string sqlfrom = " FROM [" + navCompanyName + "WI Discounts] mt" +
                             " INNER JOIN [" + navCompanyName + "Periodic Discount] p on p.No_ = mt.[Offer No_]";

            string sqlMMcolumns = "p.[No_],p.[Type],p.[Priority],p.[Description],p.[Pop-up Line 1],p.[Pop-up Line 2],p.[Discount _ Value] as [Discount _]," +
                                  "mt.[Store No_],mt.[Item No_],mt.[Variant Code],mt.[Customer Disc_ Group],mt.[Loyalty Scheme Code], 0 as [Minimum Quantity]" +
                                  ",'' as [Unit of Measure Code]";

            string sqlMMfrom = " FROM [" + navCompanyName + "WI Mix & Match Offer] mt" +
                               " INNER JOIN [" + navCompanyName + "Periodic Discount] p on p.No_ = mt.[Offer No_]";

            List<ProactiveDiscount> list = new List<ProactiveDiscount>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcolumns + sqlfrom + " WHERE mt.[Store No_]=@sid AND mt.[Item No_]=@iid";
                    command.Parameters.AddWithValue("@sid", storeId);
                    command.Parameters.AddWithValue("@iid", itemId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToDiscount(reader));
                        }
                        reader.Close();
                    }
                    command.CommandText = "SELECT " + sqlMMcolumns + sqlMMfrom + " WHERE mt.[Store No_]=@sid AND mt.[Item No_]=@iid";
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

        public DiscountValidation GetDiscountValidationByOfferId(string offerId)
        {
            DiscountValidation discval = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [" + navCompanyName +
                                          "Validation Period] mt WHERE [ID]= (Select [Validation Period ID] from [" +
                                          navCompanyName + "Periodic Discount] where [No_]=@id)";

                    command.Parameters.AddWithValue("@id", offerId);
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

        public List<string> GetItemIdsByMixAndMatchOffer(string storeId, string offerId)
        {
            List<string> ids = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Item No_] FROM [" + navCompanyName +
                                          "WI Mix & Match Offer] WHERE [Offer No_]=@id AND [Store No_]=@sid";

                    command.Parameters.AddWithValue("@id", offerId);
                    command.Parameters.AddWithValue("@sid", storeId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
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

        private DiscountValidation ReaderToDiscountValidation(SqlDataReader reader)
        {
            DiscountValidation discVal = new DiscountValidation();
            discVal.Id = SQLHelper.GetString(reader["ID"]);
            discVal.Description = SQLHelper.GetString(reader["Description"]);
            discVal.StartDate = SQLHelper.GetDateTime(reader["Starting Date"]);
            discVal.EndDate = SQLHelper.GetDateTime(reader["Ending Date"]);
            discVal.StartTime = SQLHelper.GetDateTime(reader["Starting Time"]);
            discVal.EndTime = SQLHelper.GetDateTime(reader["Ending Time"]);
            discVal.MondayStart = SQLHelper.GetDateTime(reader["Monday Starting Time"]);
            discVal.MondayEnd = SQLHelper.GetDateTime(reader["Monday Ending Time"]);
            discVal.TuesdayStart = SQLHelper.GetDateTime(reader["Tuesday Starting Time"]);
            discVal.TuesdayEnd = SQLHelper.GetDateTime(reader["Tuesday Ending Time"]);
            discVal.WednesdayStart = SQLHelper.GetDateTime(reader["Wednesday Starting Time"]);
            discVal.WednesdayEnd = SQLHelper.GetDateTime(reader["Wednesday Ending Time"]);
            discVal.ThursdayStart = SQLHelper.GetDateTime(reader["Thursday Starting Time"]);
            discVal.ThursdayEnd = SQLHelper.GetDateTime(reader["Thursday Ending Time"]);
            discVal.FridayStart = SQLHelper.GetDateTime(reader["Friday Starting Time"]);
            discVal.FridayEnd = SQLHelper.GetDateTime(reader["Friday Ending Time"]);
            discVal.SaturdayStart = SQLHelper.GetDateTime(reader["Saturday Starting Time"]);
            discVal.SaturdayEnd = SQLHelper.GetDateTime(reader["Saturday Ending Time"]);
            discVal.SundayStart = SQLHelper.GetDateTime(reader["Sunday Starting Time"]);
            discVal.SundayEnd = SQLHelper.GetDateTime(reader["Sunday Ending Time"]);
            discVal.TimeWithinBounds = SQLHelper.GetBool(reader["Time within Bounds"]);
            discVal.EndAfterMidnight = SQLHelper.GetBool(reader["Ending Time After Midnight"]);
            discVal.MondayWithinBounds = SQLHelper.GetBool(reader["Mon_ Time within Bounds"]);
            discVal.MondayEndAfterMidnight = SQLHelper.GetBool(reader["Mon_ End_ Time After Midnight"]);
            discVal.TuesdayWithinBounds = SQLHelper.GetBool(reader["Tue_ Time within Bounds"]);
            discVal.TuesdayEndAfterMidnight = SQLHelper.GetBool(reader["Tue_ End_ Time After Midnight"]);
            discVal.WednesdayWithinBounds = SQLHelper.GetBool(reader["Wed_ Time within Bounds"]);
            discVal.WednesdayEndAfterMidnight = SQLHelper.GetBool(reader["Wed_ End_ Time After Midnight"]);
            discVal.ThursdayWithinBounds = SQLHelper.GetBool(reader["Thu_ Time within Bounds"]);
            discVal.ThursdayEndAfterMidnight = SQLHelper.GetBool(reader["Thu_ End_ Time After Midnight"]);
            discVal.FridayWithinBounds = SQLHelper.GetBool(reader["Fri_ Time within Bounds"]);
            discVal.FridayEndAfterMidnight = SQLHelper.GetBool(reader["Fri_ End_ Time After Midnight"]);
            discVal.SaturdayWithinBounds = SQLHelper.GetBool(reader["Sat_ Time within Bounds"]);
            discVal.SaturdayEndAfterMidnight = SQLHelper.GetBool(reader["Sat_ End_ Time After Midnight"]);
            discVal.SundayWithinBounds = SQLHelper.GetBool(reader["Sun_ Time within Bounds"]);
            discVal.SundayEndAfterMidnight = SQLHelper.GetBool(reader["Sun_ End_ Time After Midnight"]);
            return discVal;
        }

        private ProactiveDiscount ReaderToDiscount(SqlDataReader reader)
        {
            ProactiveDiscount discount = new ProactiveDiscount();

            discount.Id = SQLHelper.GetString(reader["No_"]);
            discount.Percentage = SQLHelper.GetDecimal(reader["Discount _"]);
            discount.Priority = SQLHelper.GetInt32(reader["Priority"]);
            discount.ItemId = SQLHelper.GetString(reader["Item No_"]);
            discount.Type = ToProactiveDiscountType(SQLHelper.GetInt32(reader["Type"]));
            discount.LoyaltySchemeCode = SQLHelper.GetString(reader["Loyalty Scheme Code"]);
            discount.MinimumQuantity = SQLHelper.GetDecimal(reader["Minimum Quantity"]);
            discount.Description = SQLHelper.GetString(reader["Description"]);
            discount.VariantId = SQLHelper.GetString(reader["Variant Code"]);
            discount.UnitOfMeasureId = SQLHelper.GetString(reader["Unit of Measure Code"]);
            discount.PopUpLine1 = SQLHelper.GetString(reader["Pop-up Line 1"]);
            discount.PopUpLine2 = SQLHelper.GetString(reader["Pop-up Line 2"]);
            return discount;
        }

        private ProactiveDiscountType ToProactiveDiscountType(int type)
        {
            //NAV Types: 0 - Multibuy, 1 - Mix&Match, 2 - Disc. Offer
            switch (type)
            {
                case 0: return ProactiveDiscountType.Multibuy;
                case 1: return ProactiveDiscountType.MixMatch;
                case 2: return ProactiveDiscountType.DiscOffer;
            }
            return ProactiveDiscountType.Unknown;
        }

        public decimal PriceGetByItem(string storeId, string itemId, string variantId)
        {
            string sqlMcolumns = "mt.[Unit Price] ";
            string sqlMfrom = " FROM [" + navCompanyName + "WI Price] mt" +
                       " LEFT OUTER JOIN [" + navCompanyName + "Item Unit of Measure] u ON mt.[Item No_] = u.[Item No_] AND mt.[Unit of Measure Code] = u.[Code]";

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
