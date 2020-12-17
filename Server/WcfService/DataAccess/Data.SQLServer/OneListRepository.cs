using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        private string sqlcol;
        private string sqlfrom;

        public OneListRepository(BOConfiguration config)
            : base(config)
        {
            sqlcol = "mt.[Id],mt.[ExternalType],mt.[Description],mt.[StoreId],mt.[ListType],mt.[IsHospitality]," +
                     "mt.[TotalAmount],mt.[TotalNetAmount],mt.[TotalTaxAmount],mt.[TotalDiscAmount]," +
                     "mt.[ShippingAmount],mt.[CreateDate] ";

            sqlfrom = "FROM [OneListLink] oll INNER JOIN [OneList] AS mt ON mt.[Id]=oll.[OneListId] ";
        }

        public List<OneList> OneListGetByCardId(string cardId, bool includeLines)
        {
            List<OneList> oneList = new List<OneList>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcol + sqlfrom + "WHERE oll.[CardId]=@id ORDER BY mt.[CreateDate] DESC";

                    command.Parameters.AddWithValue("@id", cardId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oneList.Add(ReaderToOneList(reader, includeLines));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneList;
        }

        public List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines)
        {
            List<OneList> oneList = new List<OneList>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcol + sqlfrom + "WHERE oll.[CardId]=@id AND mt.[ListType]=@type ORDER BY mt.[CreateDate] DESC";

                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@type", (int)listType);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oneList.Add(ReaderToOneList(reader, includeLines));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneList;
        }

        public OneList OneListGetById(string oneListId, bool includeLines)
        {
            OneList oneList = null;
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlcol + " FROM [OneList] mt WHERE mt.[Id]=@id";

                    command.Parameters.AddWithValue("@id", oneListId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oneList = ReaderToOneList(reader, includeLines);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneList;
        }

        //takes the OneListSave and it overrides everything that is in DB
        public void OneListSave(OneList list, string contactName, bool calculate)
        {
            if (string.IsNullOrEmpty(list.Id))
            {
                list.Id = GuidHelper.NewGuid().ToString().ToUpper();
            }

            string description = (string.IsNullOrWhiteSpace(list.Description) ? list.ListType.ToString() + ": " + list.CardId : list.Description);
            if (string.IsNullOrWhiteSpace(list.CardId) == false)
            {
                if (list.ListType == ListType.Basket)
                {
                    // only have one basket per card, delete all other baskets if any
                    List<OneList> conList = OneListGetByCardId(list.CardId, ListType.Basket, false);
                    foreach (OneList mylist in conList)
                    {
                        if (mylist.Id.Equals(list.Id))
                            continue;

                        OneListDeleteById(mylist.Id);
                    }
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
                                command.CommandText = "IF EXISTS(SELECT * FROM [OneList] WHERE [Id]=@id) " +
                                                      "UPDATE [OneList] SET " +
                                                      "[ExternalType]=@f1,[Description]=@f2,[TotalAmount]=@f6," +
                                                      "[TotalNetAmount]=@f7,[TotalTaxAmount]=@f8,[TotalDiscAmount]=@f9,[ShippingAmount]=@f10,[PointAmount]=@f11,[LastAccessed]=@f13,[StoreId]=@f14 " +
                                                      "WHERE [Id]=@id" +
                                                      " ELSE " +
                                                      "INSERT INTO [OneList] (" +
                                                      "[Id],[ExternalType],[Description],[ListType],[IsHospitality],[TotalAmount]," +
                                                      "[TotalNetAmount],[TotalTaxAmount],[TotalDiscAmount],[ShippingAmount],[PointAmount],[CreateDate],[LastAccessed],[StoreId]" +
                                                      ") VALUES (@id,@f1,@f2,@type,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14)";

                                command.Parameters.AddWithValue("@id", list.Id);
                                command.Parameters.AddWithValue("@f1", list.ExternalType);
                                command.Parameters.AddWithValue("@f2", NullToString(description, 100));
                                command.Parameters.AddWithValue("@type", (int)list.ListType);
                                command.Parameters.AddWithValue("@f5", list.IsHospitality);
                                command.Parameters.AddWithValue("@f6", (calculate) ? list.TotalAmount : 0);
                                command.Parameters.AddWithValue("@f7", (calculate) ? list.TotalNetAmount : 0);
                                command.Parameters.AddWithValue("@f8", (calculate) ? list.TotalTaxAmount : 0);
                                command.Parameters.AddWithValue("@f9", (calculate) ? list.TotalDiscAmount : 0);
                                command.Parameters.AddWithValue("@f10", list.ShippingAmount);
                                command.Parameters.AddWithValue("@f11", list.PointAmount);
                                command.Parameters.AddWithValue("@f12", DateTime.Now);
                                command.Parameters.AddWithValue("@f13", DateTime.Now);
                                command.Parameters.AddWithValue("@f14", NullToString(list.StoreId, 50));
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();
                            }

                            OneListItemSave(list.Id, list.Items, connection, trans, calculate);
                            OneListPublishedOfferSave(list.Id, list.PublishedOffers, connection, trans);
                            OneListLinkSave(list.Id, list.CardId, contactName, connection, trans);
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

        public void OneListDeleteById(string oneListId)
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
                            lock (lockOneList)
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

                                command.CommandText = "DELETE FROM [OneListSubLine] WHERE [OneListId]=@id";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@id", oneListId);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText = "DELETE FROM [OneListLink] WHERE [OneListId]=@id";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@id", oneListId);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                command.CommandText = "DELETE FROM [OneList] WHERE [Id]=@id";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@id", oneListId);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                trans.Commit();
                            }
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

        public void OneListLinking(string oneListId, string cardId, string name, LinkStatus status)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    lock (lockOneList)
                    {
                        if (status == LinkStatus.Remove)
                        {
                            command.CommandText = "DELETE FROM [OneListLink] WHERE [OneListId]=@id AND [CardId]=@card";
                            command.Parameters.AddWithValue("@id", oneListId);
                            command.Parameters.AddWithValue("@card", cardId);
                        }
                        else
                        {
                            command.CommandText = "IF EXISTS(SELECT * FROM [OneListLink] WHERE [OneListId]=@id AND [CardId]=@card) " +
                                                  "UPDATE [OneListLink] SET [Status]=@stat WHERE [OneListId]=@id AND [CardId]=@card " +
                                                  "ELSE " +
                                                  "INSERT INTO [OneListLink] " +
                                                  "([CardId],[OneListId],[Name],[Status]) VALUES (@card,@id,@name,@stat)";

                            command.Parameters.AddWithValue("@card", cardId);
                            command.Parameters.AddWithValue("@id", oneListId);
                            command.Parameters.AddWithValue("@name", name);
                            command.Parameters.AddWithValue("@stat", (int)status);
                        }

                        TraceSqlCommand(command);
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
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
                    command.CommandText = "SELECT DISTINCT " + ((maxNumberOfLists > 0) ? "TOP(" + maxNumberOfLists + ") " : "") +
                                          sqlcol + sqlfrom +
                                          " INNER JOIN [OneListItem] oli ON oli.[OneListId]=mt.[Id]" +
                                          " WHERE oll.[CardId]=@0 AND UPPER(oli.[ItemDescription]) LIKE UPPER('%'+@1+'%') OR mt.[Description] LIKE UPPER('%'+@1+'%')";
                    command.Parameters.AddWithValue("@0", cardId);
                    command.Parameters.AddWithValue("@1", search);

                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oneList.Add(ReaderToOneList(reader, includeLines));
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

        private ObservableCollection<OneListItem> OneListItemsGetByOneListId(string oneListId)
        {
            ObservableCollection<OneListItem> oneLineList = new ObservableCollection<OneListItem>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[OneListId],[DisplayOrderId],[ItemId],[UomId],[VariantId]," +
                        "[ItemDescription],[VariantDescription],[ImageId],[BarcodeId],[Location],[IsADeal]," +
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
                                Quantity = SQLHelper.GetDecimal(reader["Quantity"]),
                                LineNumber = SQLHelper.GetInt32(reader["DisplayOrderId"]),
                                OneListId = SQLHelper.GetString(reader["OneListId"]),
                                BarcodeId = SQLHelper.GetString(reader["BarcodeId"]),
                                ItemId = SQLHelper.GetString(reader["ItemId"]),
                                ItemDescription = SQLHelper.GetString(reader["ItemDescription"]),
                                Location = SQLHelper.GetString(reader["Location"]),
                                VariantId = SQLHelper.GetString(reader["VariantId"]),
                                VariantDescription = SQLHelper.GetString(reader["VariantDescription"]),
                                UnitOfMeasureId = SQLHelper.GetString(reader["UomId"]),
                                Image = new ImageView(SQLHelper.GetString(reader["ImageId"])),
                                Price = SQLHelper.GetDecimal(reader["Price"]),
                                NetPrice = SQLHelper.GetDecimal(reader["NetPrice"]),
                                NetAmount = SQLHelper.GetDecimal(reader["NetAmount"]),
                                TaxAmount = SQLHelper.GetDecimal(reader["TaxAmount"]),
                                DiscountAmount = SQLHelper.GetDecimal(reader["DiscountAmount"]),
                                DiscountPercent = SQLHelper.GetDecimal(reader["DiscountPercent"]),
                                IsADeal = SQLHelper.GetBool(reader["IsADeal"])
                            };
                            line.Amount = line.NetAmount + line.TaxAmount;
                            line.OnelistSubLines = OneListSubLineGetByItemId(line.OneListId, line.Id);
                            oneLineList.Add(line);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneLineList;
        }

        private List<OneListItemSubLine> OneListSubLineGetByItemId(string oneListId, string itemId)
        {
            List<OneListItemSubLine> oneLineList = new List<OneListItemSubLine>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Id],[OneListId],[OneListItemId],[LineNumber],[Type],[ItemId],[ItemDescription],[UomId]," +
                        "[VariantId],[VariantDescription],[Quantity],[DealModLineId],[DealLineId]," +
                        "[ModifierGroupCode],[ModifierSubCode],[ParentSubLineId]" +
                        " FROM [OneListSubLine] WHERE [OneListId]=@id AND [OneListItemId]=@item";

                    command.Parameters.AddWithValue("@id", oneListId);
                    command.Parameters.AddWithValue("@item", itemId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oneLineList.Add(new OneListItemSubLine()
                            {
                                Id = SQLHelper.GetString(reader["Id"]),
                                OneListId = SQLHelper.GetString(reader["OneListId"]),
                                OneListItemId = SQLHelper.GetString(reader["OneListItemId"]),
                                LineNumber = SQLHelper.GetInt32(reader["LineNumber"]),
                                Quantity = SQLHelper.GetDecimal(reader["Quantity"]),
                                ItemId = SQLHelper.GetString(reader["ItemId"]),
                                Description = SQLHelper.GetString(reader["ItemDescription"]),
                                Type = (SubLineType)SQLHelper.GetInt32(reader["Type"]),
                                VariantId = SQLHelper.GetString(reader["VariantId"]),
                                VariantDescription = SQLHelper.GetString(reader["VariantDescription"]),
                                Uom = SQLHelper.GetString(reader["UomId"]),
                                DealLineId = SQLHelper.GetInt32(reader["DealLineId"]),
                                DealModLineId = SQLHelper.GetInt32(reader["DealModLineId"]),
                                ModifierGroupCode = SQLHelper.GetString(reader["ModifierGroupCode"]),
                                ModifierSubCode = SQLHelper.GetString(reader["ModifierSubCode"]),
                                ParentSubLineId = SQLHelper.GetInt32(reader["ParentSubLineId"])
                            });
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
                            oneLineList.Add(new OneListPublishedOffer()
                            {
                                Type = (OfferDiscountType)SQLHelper.GetInt32(reader["OfferType"]),
                                Id = SQLHelper.GetString(reader["OfferId"]),
                                CreateDate = SQLHelper.GetDateTime(reader["CreateDate"]),
                                LineNumber = SQLHelper.GetInt32(reader["DisplayOrderId"]),
                                OneListId = SQLHelper.GetString(reader["OneListId"]),
                            });
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
                            oneLineList.Add(new OneListItemDiscount()
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
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return oneLineList;
        }

        private List<OneListLink> OneListLinkGetById(string oneListId)
        {
            List<OneListLink> list = new List<OneListLink>();
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [CardId],[Status],[Name],[Owner] FROM [OneListLink] WHERE [OneListId]=@id";

                    command.Parameters.AddWithValue("@id", oneListId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new OneListLink()
                            {
                                CardId = SQLHelper.GetString(reader["CardId"]),
                                Status = (LinkStatus)SQLHelper.GetInt32(reader["Status"]),
                                Owner = SQLHelper.GetBool(reader["Owner"]),
                                Name = SQLHelper.GetString(reader["Name"])
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        private void OneListItemSave(string oneListId, ObservableCollection<OneListItem> listLines, SqlConnection db, SqlTransaction trans, bool calculate)
        {
            using (SqlCommand command = db.CreateCommand())
            {
                command.Transaction = trans;

                command.CommandText = "DELETE FROM [OneListItem] WHERE [OneListId]=@id";
                command.Parameters.AddWithValue("@id", oneListId);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();

                command.CommandText = "DELETE FROM [OneListSubLine] WHERE [OneListId]=@id";
                command.Parameters.Clear();
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
                command.CommandText = "INSERT INTO [OneListItem] (" +
                    "[Id],[OneListId],[DisplayOrderId],[ItemId],[ItemDescription],[BarcodeId],[UomId],[VariantId],[VariantDescription],[ImageId]," +
                    "[Quantity],[NetAmount],[TaxAmount],[Price],[NetPrice],[DiscountAmount],[DiscountPercent],[CreateDate],[Location],[IsADeal]" +
                    ") VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14,@f15,@f16,@f17,@f18,@f19)";

                command.Parameters.Clear();
                command.Parameters.Add("@f0", SqlDbType.NVarChar);
                command.Parameters.Add("@f1", SqlDbType.NVarChar);
                command.Parameters.Add("@f2", SqlDbType.Int);
                command.Parameters.Add("@f3", SqlDbType.NVarChar);
                command.Parameters.Add("@f4", SqlDbType.NVarChar);
                command.Parameters.Add("@f5", SqlDbType.NVarChar);
                command.Parameters.Add("@f6", SqlDbType.NVarChar);
                command.Parameters.Add("@f7", SqlDbType.NVarChar);
                command.Parameters.Add("@f8", SqlDbType.NVarChar);
                command.Parameters.Add("@f9", SqlDbType.NVarChar);
                command.Parameters.Add("@f10", SqlDbType.Decimal);
                command.Parameters.Add("@f11", SqlDbType.Decimal);
                command.Parameters.Add("@f12", SqlDbType.Decimal);
                command.Parameters.Add("@f13", SqlDbType.Decimal);
                command.Parameters.Add("@f14", SqlDbType.Decimal);
                command.Parameters.Add("@f15", SqlDbType.Decimal);
                command.Parameters.Add("@f16", SqlDbType.Decimal);
                command.Parameters.Add("@f17", SqlDbType.DateTime);
                command.Parameters.Add("@f18", SqlDbType.NVarChar);
                command.Parameters.Add("@f19", SqlDbType.TinyInt);

                foreach (OneListItem line in listLines)
                {
                    if (line == null)
                        continue;

                    line.Id = GuidHelper.NewGuid().ToString().ToUpper();

                    command.Parameters["@f0"].Value = line.Id;
                    command.Parameters["@f1"].Value = oneListId;
                    command.Parameters["@f2"].Value = line.LineNumber;
                    command.Parameters["@f3"].Value = line.ItemId;
                    command.Parameters["@f4"].Value = NullToString(line.ItemDescription, 50);
                    command.Parameters["@f5"].Value = NullToString(line.BarcodeId, 20);
                    command.Parameters["@f6"].Value = NullToString(line.UnitOfMeasureId, 10);
                    command.Parameters["@f7"].Value = NullToString(line.VariantId, 20);
                    command.Parameters["@f8"].Value = NullToString(line.VariantDescription, 50);
                    command.Parameters["@f9"].Value = NullToString(line.Image?.Id, 20);
                    command.Parameters["@f10"].Value = line.Quantity;
                    command.Parameters["@f11"].Value = line.NetAmount;
                    command.Parameters["@f12"].Value = line.TaxAmount;
                    command.Parameters["@f13"].Value = line.Price;
                    command.Parameters["@f14"].Value = line.NetPrice;
                    command.Parameters["@f15"].Value = line.DiscountAmount;
                    command.Parameters["@f16"].Value = line.DiscountPercent;
                    command.Parameters["@f17"].Value = DateTime.Now;
                    command.Parameters["@f18"].Value = line.Location;
                    command.Parameters["@f19"].Value = line.IsADeal;
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
                                command2.Parameters["@f0"].Value = GuidHelper.NewGuid().ToString().ToUpper();
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

                    if (line.OnelistSubLines != null && line.OnelistSubLines.Count > 0)
                    {
                        using (SqlCommand command3 = db.CreateCommand())
                        {
                            command3.Transaction = trans;

                            command3.CommandText = "INSERT INTO [OneListSubLine] (" +
                                "[Id],[OneListId],[OneListItemId],[Type],[ItemId],[ItemDescription],[UomId]," +
                                "[VariantId],[VariantDescription],[Quantity],[DealLineId],[DealModLineId]," +
                                "[ModifierGroupCode],[ModifierSubCode],[ParentSubLineId],[LineNumber]" +
                                ") VALUES (@f0,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14,@f15)";

                            command3.Parameters.Add("@f0", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f1", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f2", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f3", SqlDbType.Int);
                            command3.Parameters.Add("@f4", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f5", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f6", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f7", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f8", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f9", SqlDbType.Decimal);
                            command3.Parameters.Add("@f10", SqlDbType.Int);
                            command3.Parameters.Add("@f11", SqlDbType.Int);
                            command3.Parameters.Add("@f12", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f13", SqlDbType.NVarChar);
                            command3.Parameters.Add("@f14", SqlDbType.Int);
                            command3.Parameters.Add("@f15", SqlDbType.Int);

                            foreach (OneListItemSubLine sline in line.OnelistSubLines)
                            {
                                command3.Parameters["@f0"].Value = GuidHelper.NewGuid().ToString().ToUpper();
                                command3.Parameters["@f1"].Value = oneListId;
                                command3.Parameters["@f2"].Value = line.Id;
                                command3.Parameters["@f3"].Value = (int)sline.Type;
                                command3.Parameters["@f4"].Value = sline.ItemId;
                                command3.Parameters["@f5"].Value = NullToString(sline.Description, 50);
                                command3.Parameters["@f6"].Value = NullToString(sline.Uom, 10);
                                command3.Parameters["@f7"].Value = NullToString(sline.VariantId, 20);
                                command3.Parameters["@f8"].Value = NullToString(sline.VariantDescription, 50);
                                command3.Parameters["@f9"].Value = sline.Quantity;
                                command3.Parameters["@f10"].Value = sline.DealLineId;
                                command3.Parameters["@f11"].Value = sline.DealModLineId;
                                command3.Parameters["@f12"].Value = NullToString(sline.ModifierGroupCode, 20);
                                command3.Parameters["@f13"].Value = NullToString(sline.ModifierSubCode, 20);
                                command3.Parameters["@f14"].Value = sline.ParentSubLineId;
                                command3.Parameters["@f15"].Value = sline.LineNumber;
                                command3.ExecuteNonQuery();
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

        private void OneListLinkSave(string oneListId, string cardId, string contactName, SqlConnection db, SqlTransaction trans)
        {
            if (string.IsNullOrEmpty(cardId))
                return;

            using (SqlCommand command = db.CreateCommand())
            {
                command.Transaction = trans;

                command.CommandText = "SELECT 1 FROM [OneListLink] WHERE [OneListId]=@id";
                command.Parameters.AddWithValue("@id", oneListId);

                TraceSqlCommand(command);
                var exists = command.ExecuteScalar();

                command.CommandText = "IF NOT EXISTS(SELECT * FROM [OneListLink] WHERE [OneListId]=@id AND [CardId]=@card) " +
                                      "INSERT INTO [OneListLink] " +
                                      "([CardId],[OneListId],[Name],[Status],[Owner]) VALUES (@card,@id,@name,@stat,@own)";

                command.Parameters.AddWithValue("@card", cardId);
                command.Parameters.AddWithValue("@stat", (int)LinkStatus.Active);
                command.Parameters.AddWithValue("@name", contactName);
                command.Parameters.AddWithValue("@own", (exists == null) ? 1 : 0);
                TraceSqlCommand(command);
                command.ExecuteNonQuery();
            }
        }

        private OneList ReaderToOneList(SqlDataReader reader, bool includeLines)
        {
            OneList oneList = new OneList()
            {
                Id = SQLHelper.GetString(reader["Id"]),
                CreateDate = SQLHelper.GetDateTime(reader["CreateDate"]),
                Description = SQLHelper.GetString(reader["Description"]),
                ExternalType = SQLHelper.GetInt32(reader["ExternalType"]),
                StoreId = SQLHelper.GetString(reader["StoreId"]),
                ListType = (ListType)SQLHelper.GetInt32(reader["ListType"]),
                IsHospitality = SQLHelper.GetBool(reader["IsHospitality"]),
                TotalAmount = SQLHelper.GetDecimal(reader, "TotalAmount"),
                TotalNetAmount = SQLHelper.GetDecimal(reader, "TotalNetAmount"),
                TotalTaxAmount = SQLHelper.GetDecimal(reader, "TotalTaxAmount"),
                TotalDiscAmount = SQLHelper.GetDecimal(reader, "TotalDiscAmount"),
                ShippingAmount = SQLHelper.GetDecimal(reader, "ShippingAmount"),
                PointAmount = SQLHelper.GetDecimal(reader, "PointAmount")
            };

            oneList.CardLinks = OneListLinkGetById(oneList.Id);
            OneListLink link = oneList.CardLinks.Find(c => c.Owner == true);
            oneList.CardId = (link != null) ? link.CardId : string.Empty;

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

        #endregion private
    }
}
