using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.Dal
{
    public class OneListRepository : BaseRepository, IOneListRepository
    {
        static object lockOneList = new object();

        public OneListRepository(BOConfiguration config)
            : base(config)
        {
        }

        public List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines = false)
        {
            List<OneList> oneList = new List<OneList>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[IsDefaultList],[Description],[CardId],[CustomerId],[StoreId],[ListType]," +
                                          "[TotalAmount],[TotalNetAmount],[TotalTaxAmount],[TotalDiscAmount],[ShippingAmount],[CreateDate]" +
                                          " FROM [OneList] WHERE [CardId]=@id AND [ListType]=@type ORDER BY [CreateDate] DESC";

                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@type", (int)listType);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oneList.Add(ConvertToDataOneList(reader, includeLines));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneList;
        }

        public OneList OneListGetById(string oneListId, ListType listType, bool includeLines = true)
        {
            OneList oneList = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[IsDefaultList],[Description],[CardId],[CustomerId],[StoreId],[ListType]," +
                        "[TotalAmount],[TotalNetAmount],[TotalTaxAmount],[TotalDiscAmount],[ShippingAmount],[CreateDate]" +
                        " FROM [OneList] WHERE [Id]=@id AND [ListType]=@type";

                    command.Parameters.AddWithValue("@id", oneListId);
                    command.Parameters.AddWithValue("@type", (int)listType);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oneList = ConvertToDataOneList(reader, includeLines);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneList;
        }

        //takes the OneListSave and it overrides everything that is in DB
        public void OneListSave(OneList list, bool calculate)
        {
            bool isDefaultList = false;
            Guid guid = Guid.Empty;
            if (string.IsNullOrEmpty(list.Id))
            {
                guid = GuidHelper.NewGuid();
                list.Id = guid.ToString().ToUpper();
            }

            string description = (string.IsNullOrWhiteSpace(list.Description) ? "List " + list.CardId : list.Description);
            if (string.IsNullOrWhiteSpace(list.CardId) == false)
            {
                if (list.ListType == ListType.Basket)
                {
                    // only have one basket, delete all other baskets if any
                    List<OneList> conList = OneListGetByCardId(list.CardId, ListType.Basket);
                    foreach (OneList mylist in conList)
                    {
                        if (mylist.Id.Equals(guid))
                            continue;

                        OneListDeleteById(mylist.Id, ListType.Basket);
                    }
                }

                //check if there is a current user list or not
                if (HasDefaultList(list.CardId, list.ListType) == false)
                {
                    isDefaultList = true;  //user has no current list so force this one to be the current one
                }
            }

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction("OneList"))
                {
                    try
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            lock (lockOneList)
                            {
                                command.Transaction = trans;
                                //one list not found, create a new one
                                // Create a new onelist and use as default
                                command.CommandText = "IF EXISTS(SELECT * FROM [OneList] WHERE [Id]=@id AND [ListType]=@type) " +
                                                      "UPDATE [OneList] SET " +
                                                      "[IsDefaultList]=@f1,[Description]=@f2,[CardId]=@f3,[CustomerId]=@f4,[TotalAmount]=@f6," +
                                                      "[TotalNetAmount]=@f7,[TotalTaxAmount]=@f8,[TotalDiscAmount]=@f9,[ShippingAmount]=@f10,[LastAccessed]=@f12,[StoreId]=@f13 " +
                                                      "WHERE [Id]=@id" +
                                                      " ELSE " +
                                                      "INSERT INTO [OneList] (" +
                                                      "[Id],[IsDefaultList],[Description],[CardId],[CustomerId],[ListType],[TotalAmount]," +
                                                      "[TotalNetAmount],[TotalTaxAmount],[TotalDiscAmount],[ShippingAmount],[CreateDate],[LastAccessed],[StoreId]" +
                                                      ") VALUES (@id,@f1,@f2,@f3,@f4,@type,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13)";

                                command.Parameters.AddWithValue("@id", list.Id);
                                command.Parameters.AddWithValue("@f1", isDefaultList);
                                command.Parameters.AddWithValue("@f2", NullToString(description, 100));
                                command.Parameters.AddWithValue("@f3", NullToString(list.CardId, 500));
                                command.Parameters.AddWithValue("@f4", NullToString(list.CustomerId, 500));
                                command.Parameters.AddWithValue("@type", (int)list.ListType);
                                command.Parameters.AddWithValue("@f6", (calculate) ? list.TotalAmount : 0);
                                command.Parameters.AddWithValue("@f7", (calculate) ? list.TotalNetAmount : 0);
                                command.Parameters.AddWithValue("@f8", (calculate) ? list.TotalTaxAmount : 0);
                                command.Parameters.AddWithValue("@f9", (calculate) ? list.TotalDiscAmount : 0);
                                command.Parameters.AddWithValue("@f10", list.ShippingAmount);
                                command.Parameters.AddWithValue("@f11", DateTime.Now);
                                command.Parameters.AddWithValue("@f12", DateTime.Now);
                                command.Parameters.AddWithValue("@f13", NullToString(list.StoreId, 50));
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }

                            OneListItemSave(list.Id, list.Items, connection, trans, calculate);
                            OneListPublishedOfferSave(list.Id, list.PublishedOffers, connection, trans);
                            trans.Commit();
                        }
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public void OneListDeleteById(string oneListId, ListType listType)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction("Delete"))
                {
                    try
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = trans;
                            command.CommandText = "DELETE FROM [OneListItemDiscount] WHERE [OneListId]=@id";
                            command.Parameters.AddWithValue("@id", oneListId);
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();

                            command.CommandText = "DELETE FROM [OneListItem] WHERE [OneListId]=@id";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@id", oneListId);
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();

                            command.CommandText = "DELETE FROM [OneListOffer] WHERE [OneListId]=@id";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@id", oneListId);
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();

                            command.CommandText = "DELETE FROM [OneList] WHERE [Id]=@id AND [ListType]=@type";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@id", oneListId);
                            command.Parameters.AddWithValue("@type", (int)listType);
                            TraceSqlCommand(command);
                            command.ExecuteNonQuery();
                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public List<OneList> OneListSearch(string cardId, string search, int maxNumberOfLists, ListType listType, bool includeLines = false)
        {
            if (string.IsNullOrWhiteSpace(search))
                search = "";
            if (search.Contains("'"))
                search = search.Replace("'", "''");

            List<OneList> oneList = new List<OneList>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = 
                        "SELECT DISTINCT " + ((maxNumberOfLists > 0) ? "TOP(" + maxNumberOfLists + ")" : "") + " ol.* " +
                        "FROM [OneList] ol " +
                        "INNER JOIN [OneListItem] oli on oli.[OneListId] = ol.[Id] " +
                        "WHERE ol.[CardId]=@0 AND UPPER(oli.[ItemDescription]) LIKE UPPER('%'+@1+'%') OR ol.[Description] like UPPER('%'+@1+'%')";
                    command.Parameters.AddWithValue("@0", cardId);
                    command.Parameters.AddWithValue("@1", search);

                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oneList.Add(ConvertToDataOneList(reader, includeLines));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneList;
        }

        #region private

        private void OneListUpdateAccess(string id)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE [OneList] SET [LastAccessed]=GETDATE() WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private List<OneListItem> OneListItemsGetByOneListId(string oneListId)
        {
            List<OneListItem> oneLineList = new List<OneListItem>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[OneListId],[DisplayOrderId],[ItemId],[BarcodeId],[UomId],[VariantId]," +
                        "[Quantity],[NetPrice],[Price],[NetAmount],[TaxAmount],[DiscountAmount],[DiscountPercent],[CreateDate]" +
                        " FROM [OneListItem] WHERE [OneListId]=@id ORDER BY [DisplayOrderid] ASC";

                    command.Parameters.AddWithValue("@id", oneListId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OneListItem line = new OneListItem()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                CreateDate = SQLHelper.GetDateTime(reader["CreateDate"]),
                                Quantity = SQLHelper.GetDecimal(reader, "Quantity"),
                                DisplayOrderId = SQLHelper.GetInt32(reader["DisplayOrderId"]),
                                OneListId = SQLHelper.GetString(reader["OneListId"]),
                                BarcodeId = SQLHelper.GetString(reader["BarcodeId"]),
                                ItemId = SQLHelper.GetString(reader["ItemId"]),
                                VariantId = SQLHelper.GetString(reader["VariantId"]),
                                UnitOfMeasureId = SQLHelper.GetString(reader["UomId"]),
                                Price = SQLHelper.GetDecimal(reader, "Price"),
                                NetPrice = SQLHelper.GetDecimal(reader, "NetPrice"),
                                NetAmount = SQLHelper.GetDecimal(reader, "NetAmount"),
                                TaxAmount = SQLHelper.GetDecimal(reader, "TaxAmount"),
                                DiscountAmount = SQLHelper.GetDecimal(reader, "DiscountAmount"),
                                DiscountPercent = SQLHelper.GetDecimal(reader, "DiscountPercent")
                            };
                            line.Amount = line.NetAmount + line.TaxAmount;
                            oneLineList.Add(line);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneLineList;
        }

        private List<OneListPublishedOffer> OneListPublishedOffersetByOneListId(string oneListId)
        {
            List<OneListPublishedOffer> oneLineList = new List<OneListPublishedOffer>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [OfferId],[OfferType],[OneListId],[DisplayOrderId],[CreateDate]" +
                        " FROM [OneListOffer] WHERE [OneListId]=@id ORDER BY [DisplayOrderid] ASC";

                    command.Parameters.AddWithValue("@id", oneListId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OneListPublishedOffer line = new OneListPublishedOffer()
                            {
                                Type = (OfferDiscountType)SQLHelper.GetInt32(reader["OfferType"]),
                                Id = SQLHelper.GetString(reader["OfferId"]),
                                CreateDate = SQLHelper.GetDateTime(reader["CreateDate"]),
                                DisplayOrderId = SQLHelper.GetInt32(reader["DisplayOrderId"]),
                                OneListId = SQLHelper.GetString(reader["OneListId"]),
                            };
                            oneLineList.Add(line);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneLineList;
        }

        private List<OneListItemDiscount> OneListItemDiscountGetByOneListId(string oneListId, string oneListItemid, string itemId, string variantId)
        {
            List<OneListItemDiscount> oneLineList = new List<OneListItemDiscount>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[OneListId],[OneListItemId],[LineNumber],[No],[DiscountType],[PeriodicDiscType]," +
                        "[PeriodicDiscGroup],[Description],[DiscountAmount],[DiscountPercent],[Quantity],[OfferNumber],[CreateDate]" +
                        " FROM [OneListItemDiscount] WHERE [OneListId]=@id AND [OneListItemId]=@item";

                    command.Parameters.AddWithValue("@id", oneListId);
                    command.Parameters.AddWithValue("@item", oneListItemid);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OneListItemDiscount line = new OneListItemDiscount()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                OneListId = SQLHelper.GetString(reader["OneListId"]),
                                OneListItemId = SQLHelper.GetString(reader["OneListItemId"]),
                                LineNumber = SQLHelper.GetInt32(reader["LineNumber"]),
                                No = SQLHelper.GetString(reader["No"]),
                                Description = SQLHelper.GetString(reader["Description"]),
                                DiscountAmount = SQLHelper.GetDecimal(reader, "DiscountAmount"),
                                DiscountPercent = SQLHelper.GetDecimal(reader, "DiscountPercent"),
                                DiscountType = (DiscountType)SQLHelper.GetInt32(reader["DiscountType"]),
                                OfferNumber = SQLHelper.GetString(reader["OfferNumber"]),
                                PeriodicDiscGroup = SQLHelper.GetString(reader["PeriodicDiscGroup"]),
                                PeriodicDiscType = (PeriodicDiscType)SQLHelper.GetInt32(reader["PeriodicDiscType"]),
                                Quantity = SQLHelper.GetDecimal(reader, "Quantity"),
                                ItemId = itemId,
                                VariantId = variantId
                            };
                            oneLineList.Add(line);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneLineList;
        }

        private void OneListItemSave(string oneListId, List<OneListItem> listLines, SqlConnection db, SqlTransaction trans, bool calculate)
        {
            using (SqlCommand command = db.CreateCommand())
            {
                command.Transaction = trans;

                command.CommandText = "DELETE FROM [OneListItem] WHERE [OneListId]=@id";
                command.Parameters.AddWithValue("@id", oneListId);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();

                if (calculate)
                {
                    command.CommandText = "DELETE FROM [OneListItemDiscount] WHERE [OneListId]=@id";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id", oneListId);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }

                if (listLines == null || listLines.Count == 0)
                    return;

                //now add the line
                int displayOrderId = 0;
                command.CommandText = "INSERT INTO [OneListItem] (" +
                    "[Id],[OneListId],[DisplayOrderId],[ItemId],[ItemDescription],[BarcodeId],[UomId],[Quantity],[VariantId],[NetAmount]," +
                    "[TaxAmount],[Price],[NetPrice],[DiscountAmount],[DiscountPercent],[CreateDate]" +
                    ") VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14,@f15)";

                command.Parameters.Clear();
                command.Parameters.Add("@f0", SqlDbType.NVarChar);
                command.Parameters.Add("@f1", SqlDbType.NVarChar);
                command.Parameters.Add("@f2", SqlDbType.Int);
                command.Parameters.Add("@f3", SqlDbType.NVarChar);
                command.Parameters.Add("@f4", SqlDbType.NVarChar);
                command.Parameters.Add("@f5", SqlDbType.NVarChar);
                command.Parameters.Add("@f6", SqlDbType.NVarChar);
                command.Parameters.Add("@f7", SqlDbType.Decimal);
                command.Parameters.Add("@f8", SqlDbType.NVarChar);
                command.Parameters.Add("@f9", SqlDbType.Decimal);
                command.Parameters.Add("@f10", SqlDbType.Decimal);
                command.Parameters.Add("@f11", SqlDbType.Decimal);
                command.Parameters.Add("@f12", SqlDbType.Decimal);
                command.Parameters.Add("@f13", SqlDbType.Decimal);
                command.Parameters.Add("@f14", SqlDbType.Decimal);
                command.Parameters.Add("@f15", SqlDbType.DateTime);

                foreach (OneListItem line in listLines)
                {
                    if (line == null)
                        continue;

                    line.Id = GuidHelper.NewGuid().ToString();
                    displayOrderId++;

                    command.Parameters["@f0"].Value = line.Id;
                    command.Parameters["@f1"].Value = oneListId;
                    command.Parameters["@f2"].Value = displayOrderId;
                    command.Parameters["@f3"].Value = line.ItemId;
                    command.Parameters["@f4"].Value = (line.Item != null) ? NullToString(line.Item.Description, 50) : string.Empty;
                    command.Parameters["@f5"].Value = NullToString(line.BarcodeId, 20);
                    command.Parameters["@f6"].Value = NullToString(line.UnitOfMeasureId, 10);
                    command.Parameters["@f7"].Value = line.Quantity;
                    command.Parameters["@f8"].Value = NullToString(line.VariantId, 20);
                    command.Parameters["@f9"].Value = line.NetAmount;
                    command.Parameters["@f10"].Value = line.TaxAmount;
                    command.Parameters["@f11"].Value = line.Price;
                    command.Parameters["@f12"].Value = line.NetPrice;
                    command.Parameters["@f13"].Value = line.DiscountAmount;
                    command.Parameters["@f14"].Value = line.DiscountPercent;
                    command.Parameters["@f15"].Value = DateTime.Now;
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();

                    if (calculate && (line.OnelistItemDiscounts != null && line.OnelistItemDiscounts.Count > 0))
                    {
                        using (SqlCommand command2 = db.CreateCommand())
                        {
                            command2.Transaction = trans;

                            command2.CommandText = "INSERT INTO [OneListItemDiscount] (" +
                                "[Id],[OneListId],[OneListItemId],[No],[Description],[LineNumber],[OfferNumber],[Quantity]," +
                                "[DiscountAmount],[DiscountPercent],[DiscountType],[PeriodicDiscType],[PeriodicDiscGroup]" +
                                ") VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12)";

                            command2.Parameters.Add("@f0", SqlDbType.NVarChar);
                            command2.Parameters.Add("@f1", SqlDbType.NVarChar);
                            command2.Parameters.Add("@f2", SqlDbType.NVarChar);
                            command2.Parameters.Add("@f3", SqlDbType.NVarChar);
                            command2.Parameters.Add("@f4", SqlDbType.NVarChar);
                            command2.Parameters.Add("@f5", SqlDbType.Int);
                            command2.Parameters.Add("@f6", SqlDbType.NVarChar);
                            command2.Parameters.Add("@f7", SqlDbType.Decimal);
                            command2.Parameters.Add("@f8", SqlDbType.Decimal);
                            command2.Parameters.Add("@f9", SqlDbType.Decimal);
                            command2.Parameters.Add("@f10", SqlDbType.Int);
                            command2.Parameters.Add("@f11", SqlDbType.Int);
                            command2.Parameters.Add("@f12", SqlDbType.NVarChar);

                            foreach (OneListItemDiscount dline in line.OnelistItemDiscounts)
                            {
                                command2.Parameters["@f0"].Value = GuidHelper.NewGuid().ToString();
                                command2.Parameters["@f1"].Value = oneListId;
                                command2.Parameters["@f2"].Value = line.Id;
                                command2.Parameters["@f3"].Value = dline.No;
                                command2.Parameters["@f4"].Value = NullToString(dline.Description, 20);
                                command2.Parameters["@f5"].Value = dline.LineNumber;
                                command2.Parameters["@f6"].Value = NullToString(dline.OfferNumber, 20);
                                command2.Parameters["@f7"].Value = dline.Quantity;
                                command2.Parameters["@f8"].Value = dline.DiscountAmount;
                                command2.Parameters["@f9"].Value = dline.DiscountPercent;
                                command2.Parameters["@f10"].Value = dline.DiscountType;
                                command2.Parameters["@f11"].Value = dline.PeriodicDiscType;
                                command2.Parameters["@f12"].Value = NullToString(dline.PeriodicDiscGroup, 20);
                                command2.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        private void OneListPublishedOfferSave(string oneListId, List<OneListPublishedOffer> listLines, SqlConnection db, SqlTransaction trans)
        {
            //Using the OneListOffer for the PublishedOffers, will never be used at same time, offer and publishedoffers
            if (listLines == null || listLines.Count == 0)
                return;

            using (SqlCommand command = db.CreateCommand())
            {
                command.Transaction = trans;

                command.CommandText = "DELETE FROM [OneListOffer] WHERE [OneListId]=@id";
                command.Parameters.AddWithValue("@id", oneListId);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO [OneListOffer] " +
                                      "([OfferId],[OfferType],[OneListId],[DisplayOrderId],[CreateDate]) VALUES (@f0,@f1,@f2,@f3,@f4)";

                command.Parameters.Add("@f0", SqlDbType.NVarChar);
                command.Parameters.Add("@f1", SqlDbType.Int);
                command.Parameters.Add("@f2", SqlDbType.NVarChar);
                command.Parameters.Add("@f3", SqlDbType.Int);
                command.Parameters.Add("@f4", SqlDbType.DateTime);

                int displayOrderId = 0;
                foreach (OneListPublishedOffer line in listLines)
                {
                    displayOrderId++;

                    command.Parameters["@f0"].Value = line.Id;
                    command.Parameters["@f1"].Value = line.Type;
                    command.Parameters["@f2"].Value = oneListId;
                    command.Parameters["@f3"].Value = displayOrderId;
                    command.Parameters["@f4"].Value = DateTime.Now;
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
            }
        }

        private OneList ConvertToDataOneList(SqlDataReader reader, bool includeLines = true)
        {
            OneList oneList = new OneList()
            {
                Id = SQLHelper.GetString(reader["Id"]),
                CreateDate = SQLHelper.GetDateTime(reader["CreateDate"]),
                Description = SQLHelper.GetString(reader["Description"]),
                IsDefaultList = SQLHelper.GetBool(reader["IsDefaultList"]),
                CardId = SQLHelper.GetString(reader["CardId"]),
                CustomerId = SQLHelper.GetString(reader["CustomerId"]),
                StoreId = SQLHelper.GetString(reader["StoreId"]),
                ListType = (ListType)SQLHelper.GetInt32(reader["ListType"]),
                TotalAmount = SQLHelper.GetDecimal(reader, "TotalAmount"),
                TotalNetAmount = SQLHelper.GetDecimal(reader, "TotalNetAmount"),
                TotalTaxAmount = SQLHelper.GetDecimal(reader, "TotalTaxAmount"),
                TotalDiscAmount = SQLHelper.GetDecimal(reader, "TotalDiscAmount"),
                ShippingAmount = SQLHelper.GetDecimal(reader, "ShippingAmount")
            };

            if (includeLines)
            {
                oneList.Items = OneListItemsGetByOneListId(oneList.Id);
                foreach (OneListItem item in oneList.Items)
                {
                    item.OnelistItemDiscounts = OneListItemDiscountGetByOneListId(oneList.Id, item.Id.ToString(), item.ItemId, item.VariantId);
                }
                oneList.PublishedOffers = OneListPublishedOffersetByOneListId(oneList.Id);
            }

            OneListUpdateAccess(oneList.Id);
            return oneList;
        }

        private bool HasDefaultList(string contactId, ListType listType)
        {
            return base.DoesRecordExist("[OneList]", "[CardId]=@0 AND [ListType]=@1 AND [IsDefaultList]=1",
                contactId, (int)listType);
        }

        #endregion private
    }
}
