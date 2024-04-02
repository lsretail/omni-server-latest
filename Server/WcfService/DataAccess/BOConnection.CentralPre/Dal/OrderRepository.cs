using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class OrderRepository : BaseRepository
    {
        public OrderRepository(BOConfiguration config, Version version) : base(config, version)
        {
        }

        public SalesEntry OrderGetById(string id, bool includeLines, bool external, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            SalesEntry order = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Parameters.Clear();
                    command.CommandText = "SELECT * FROM (" +
                        "SELECT mt.[Document ID],mt.[Created at Store],st.[Currency Code],mt.[External ID],mt.[Created] AS [Date],mt.[Source Type]," +
                        "mt.[Member Card No_],mt.[Customer No_],mt.[Name] AS [Name],mt.[Address],mt.[Address 2]," +
                        "mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Territory Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_]," +
                        "mt.[Mobile Phone No_],mt.[Daytime Phone No_],mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2]," +
                        "mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_]," +
                        "mt.[Ship-to Email],mt.[Ship-to House_Apartment No_],mt.[Click and Collect Order], mt.[Shipping Agent Code]," +
                        "mt.[Shipping Agent Service Code], 0 AS Posted,0 AS Cancelled,mt.[Requested Delivery Date] " +
                        "FROM [" + navCompanyName + "LSC Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Created at Store] " +
                        "UNION " +
                        "SELECT mt.[Document ID],mt.[Created at Store],st.[Currency Code],mt.[External ID],mt.[Created] AS [Date],mt.[Source Type]," +
                        "mt.[Member Card No_],mt.[Customer No_],mt.[Name] AS [Name],mt.[Address],mt.[Address 2]," +
                        "mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Territory Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_]," +
                        "mt.[Mobile Phone No_],mt.[Daytime Phone No_],mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2]," +
                        "mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_]," +
                        "mt.[Ship-to Email],mt.[Ship-to House_Apartment No_],mt.[Click and Collect Order], mt.[Shipping Agent Code]," +
                        "mt.[Shipping Agent Service Code],1 AS Posted,mt.[CancelledOrder] AS Cancelled,mt.[Requested Delivery Date] " +
                        "FROM [" + navCompanyName + "LSC Posted CO Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Created at Store] " +
                        ") AS Orders " +
                        "WHERE [" + ((external) ? "External ID" : "Document ID") + "]=@id";

                    command.Parameters.AddWithValue("@id", id.ToUpper());
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToSalesEntry(reader, includeLines, stat);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return order;
        }

        public OrderStatusResponse OrderStatusGet(string id)
        {
            OrderStatusResponse status = new OrderStatusResponse();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Line No_],mt.[Status Code]," +
                                          "ln.[Number] AS COItem,ln.[Variant Code] AS COVar,ln.[Unit of Measure Code] AS COUom,ln.[Quantity] AS COQty," +
                                          "pln.[Number] AS PCOItem,pln.[Variant Code] AS PCOVar,pln.[Unit of Measure Code] AS PCOUom,pln.[Quantity] AS PCOQty," +
                                          "s0.[Description] AS Desc0,s1.[Description] AS Desc1,s1.[Cancel Allowed],s1.[Modify Allowed]" +
                                          (LSCVersion >= new Version("18.2") ? ",s0.[External Code] AS Ext0,s1.[External Code] AS Ext1" : string.Empty) +
                                          " FROM [" + navCompanyName + "LSC CO Status$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                          "LEFT JOIN [" + navCompanyName + "LSC Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ln ON ln.[Document ID]=mt.[Document ID] AND ln.[Line No_]=mt.[Line No_] " +
                                          "LEFT JOIN [" + navCompanyName + "LSC Posted Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] pln ON pln.[Document ID]=mt.[Document ID] AND pln.[Line No_]=mt.[Line No_] " +
                                          "LEFT JOIN [" + navCompanyName + "LSC CO Status Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] s0 ON s0.[Code]=mt.[Status Code] " +
                                          "LEFT JOIN [" + navCompanyName + "LSC CO Line Status Setup$5ecfc871-5d82-43f1-9c54-59685e82318d] s1 ON s1.[Code]=mt.[Status Code] " +
                                          "WHERE mt.[Document ID]=@id ORDER BY mt.[Line No_]";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int lineno = SQLHelper.GetInt32(reader["Line No_"]);
                            if (lineno == 0)
                            {
                                status.DocumentNo = id;
                                status.OrderStatus = SQLHelper.GetString(reader["Status Code"]);
                                status.Description = SQLHelper.GetString(reader["Desc0"]);
                                if (LSCVersion >= new Version("18.2"))
                                    status.ExtCode = SQLHelper.GetString(reader["Ext0"]);
                            }
                            else
                            {
                                bool posted = reader["COItem"] == DBNull.Value;
                                status.Lines.Add(new OrderLineStatus()
                                {
                                    LineStatus = SQLHelper.GetString(reader["Status Code"]),
                                    ExtCode = (LSCVersion >= new Version("18.2")) ? SQLHelper.GetString(reader["Ext1"]) : string.Empty,
                                    Description = SQLHelper.GetString(reader["Desc1"]),
                                    LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                                    ItemId = SQLHelper.GetString(posted ? reader["PCOItem"] : reader["COItem"]),
                                    VariantId = SQLHelper.GetString(posted ? reader["PCOVar"] : reader["COVar"]),
                                    UnitOfMeasureId = SQLHelper.GetString(posted ? reader["PCOUom"] : reader["COUom"]),
                                    Quantity = SQLHelper.GetDecimal(posted ? reader["PCOQty"] : reader["COQty"]),
                                    AllowCancel = SQLHelper.GetBool(reader["Cancel Allowed"]),
                                    AllowModify = SQLHelper.GetBool(reader["Modify Allowed"])
                                });
                            }
                        }
                    }
                }
                connection.Close();
            }
            return status;
        }

        private List<SalesEntryLine> OrderLinesGet(string id, out string storeCurCode, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            string select = "SELECT ml.[Number],ml.[Variant Code],ml.[Unit of Measure Code],ml.[Line No_],ml.[Line Type]," +
                            "ml.[Net Price],ml.[Price],ml.[Quantity],ml.[Discount Amount],ml.[Discount Percent]," +
                            "ml.[Net Amount],ml.[Vat Amount],ml.[Amount],ml.[Item Description],ml.[Variant Description]" +
                            ",ml.[Document ID],ml.[External ID],ml.[Click and Collect Line],ml.[Store No_],st.[Name],st.[Currency Code]";

            storeCurCode = string.Empty;
            List<SalesEntryLine> list = new List<SalesEntryLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM ( " + select + " FROM [" + navCompanyName + "LSC Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          "JOIN [" + navCompanyName + "LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=ml.[Store No_] " +
                                          "UNION " + select + " FROM [" + navCompanyName + "LSC Posted Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          "JOIN [" + navCompanyName + "LSC Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=ml.[Store No_]" +
                                          ") AS OrderLines WHERE [Document ID]=@id" +
                                          " ORDER BY [Line No_]";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesEntryLine line = ReaderToOrderLine(reader);
                            if (LSCVersion >= new Version("23.0"))
                                line.ExtraInformation = OrderLinesDataEntryGet(id, line.LineNumber, false, stat);
                            list.Add(line);
                            storeCurCode = SQLHelper.GetString(reader["Currency Code"]);
                        }
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public string OrderLinesDataEntryGet(string id, int lineNo, bool hintOnly, Statistics stat)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            logger.StatisticStartSub(false, ref stat, out int index);

            string entryData = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Entry Type],[Entry Code],[PIN] " +
                                          "FROM [" + navCompanyName + "LSC POS Data Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Created by Receipt No_]=@id AND [Created by Line No_]=@line";

                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@line", lineNo);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entryData = (hintOnly) ? $"Type:{SQLHelper.GetString(reader["Entry Type"])}" : $"Code:{SQLHelper.GetString(reader["Entry Code"])} Type:{SQLHelper.GetString(reader["Entry Type"])} Pin:{SQLHelper.GetInt32(reader["PIN"])}";
                        }
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return entryData;
        }

        public void OrderLinesGetTotals(string orderId, out int itemCount, out decimal qty, out int lineCount, out decimal totalAmount, out decimal totalNetAmount, out decimal totalDiscount)
        {
            itemCount = 0;
            lineCount = 0;
            qty = 0;
            totalAmount = 0;
            totalNetAmount = 0;
            totalDiscount = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string select = "SELECT [Document ID], SUM([Quantity]) AS Cnt, SUM([Discount Amount]) AS Disc, SUM([Net Amount]) AS NAmt, SUM([Amount]) AS Amt";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select +
                                            " FROM [" + navCompanyName + "LSC Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] GROUP BY [Document ID] " +
                                            "UNION " + select +
                                            " FROM [" + navCompanyName + "LSC Posted Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] GROUP BY [Document ID] " +
                                            ") AS OrderTotals WHERE [Document ID]=@id";
                    command.Parameters.AddWithValue("@id", orderId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemCount = SQLHelper.GetInt32(reader["Cnt"]);
                            qty = SQLHelper.GetDecimal(reader["Cnt"]);
                            totalAmount = SQLHelper.GetDecimal(reader, "Amt");
                            totalNetAmount = SQLHelper.GetDecimal(reader, "NAmt");
                            totalDiscount = SQLHelper.GetDecimal(reader, "Disc");
                        }
                    }

                    select = "SELECT [Document ID],[Line No_]";
                    command.CommandText = "SELECT COUNT(*) AS [Cnt] FROM (" + select +
                                            " FROM [" + navCompanyName + "LSC Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d]" +
                                            " UNION " + select +
                                            " FROM [" + navCompanyName + "LSC Posted Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d]" +
                                            ") AS OrderTotals WHERE [Document ID]=@id";
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lineCount = SQLHelper.GetInt32(reader["Cnt"]);
                        }
                    }
                }
                connection.Close();
            }
        }

        public bool CompressCOActive(Statistics stat)
        {
            if (LSCVersion < new Version("24.1"))
                return false;

            logger.StatisticStartSub(false, ref stat, out int index);

            bool isActive = false;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Compressed Lines Active] FROM [" + navCompanyName + "LSC Customer Order Setup$5ecfc871-5d82-43f1-9c54-59685e82318d]";
                    TraceSqlCommand(command);
                    connection.Open();
                    isActive = SQLHelper.GetBool(command.ExecuteScalar());
                }
                connection.Close();
            }
            logger.Debug(config.LSKey.Key, $"CO Compress Status: {isActive}");
            logger.StatisticEndSub(ref stat, index);
            return isActive;
        }

        private List<SalesEntryPayment> OrderPayGet(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<SalesEntryPayment> list = new List<SalesEntryPayment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string select = "SELECT ml.[Store No_],ml.[Line No_],ml.[Pre Approved Amount],ml.[Tender Type],ml.[Finalized Amount],ml.[Type]," +
                                "ml.[Card Type],ml.[Currency Code],ml.[Currency Factor],ml.[Pre Approved Valid Date]," +
                                "ml.[Card or Customer No_],ml.[Document ID],ml.[Token No_],ml.[External Reference]";

                if (LSCVersion >= new Version("19.0"))
                    select += ",ml.[EFT Authorization Code]";
                else
                    select += ",ml.[Authorization Code]";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select +
                                          " FROM [" + navCompanyName + "LSC Customer Order Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          " UNION " + select + " FROM [" + navCompanyName + "LSC Posted CO Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          ") AS OrderTotal WHERE [Document ID]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesEntryPayment pay = new SalesEntryPayment()
                            {
                                Amount = SQLHelper.GetDecimal(reader, "Pre Approved Amount"),
                                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                                Type = (PaymentType)SQLHelper.GetInt32(reader["Type"]),
                                TenderType = SQLHelper.GetString(reader["Tender Type"]),
                                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                                CurrencyFactor = SQLHelper.GetDecimal(reader, "Currency Factor"),
                                CardType = SQLHelper.GetString(reader["Card Type"]),
                                CardNo = SQLHelper.GetString(reader["Card or Customer No_"]),
                                TokenNumber = SQLHelper.GetString(reader["Token No_"]),
                                ExternalReference = SQLHelper.GetString(reader["External Reference"])
                            };

                            if (LSCVersion >= new Version("19.0"))
                                pay.AuthorizationCode = SQLHelper.GetString(reader["EFT Authorization Code"]);
                            else
                                pay.AuthorizationCode = SQLHelper.GetString(reader["Authorization Code"]);

                            decimal amt = SQLHelper.GetDecimal(reader, "Finalized Amount");
                            if (amt > 0)
                            {
                                pay.Amount = amt;
                            }
                            list.Add(pay);
                        }
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        private List<SalesEntryDiscountLine> OrderDiscGet(string id, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<SalesEntryDiscountLine> list = new List<SalesEntryDiscountLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string select = "SELECT ml.[Line No_],ml.[Entry No_],ml.[Discount Type],ml.[Offer No_]," +
                                "ml.[Periodic Disc_ Type],ml.[Periodic Disc_ Group],ml.[Description],ml.[Discount Percent],ml.[Discount Amount]" +
                                ",ml.[Document ID]";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select + " FROM [" + navCompanyName + "LSC CO Discount Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          "UNION " + select + " FROM [" + navCompanyName + "LSC Posted CO Discount Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          ") AS OrderDiscounts WHERE [Document ID]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToOrderDisc(reader));
                        }
                    }
                }
                connection.Close();
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        private SalesEntry ReaderToSalesEntry(SqlDataReader reader, bool includeLines, Statistics stat)
        {
            SalesEntry entry = new SalesEntry
            {
                Id = SQLHelper.GetString(reader["Document ID"]),
                CreateAtStoreId = SQLHelper.GetString(reader["Created at Store"]),
                StoreCurrency = SQLHelper.GetString(reader["Currency Code"]),
                DocumentRegTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson),
                IdType = DocumentIdType.Order,
                Status = SalesEntryStatus.Created,
                Posted = SQLHelper.GetBool(reader["Posted"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["Click And Collect Order"]),

                ShippingAgentCode = SQLHelper.GetString(reader["Shipping Agent Code"]),
                ShippingAgentServiceCode = SQLHelper.GetString(reader["Shipping Agent Service Code"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),
                RequestedDeliveryDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Requested Delivery Date"]), config.IsJson),

                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                CustomerId = SQLHelper.GetString(reader["Customer No_"]),

                ContactName = SQLHelper.GetString(reader["Name"]),
                ContactDayTimePhoneNo = SQLHelper.GetString(reader["Daytime Phone No_"]),
                ContactEmail = SQLHelper.GetString(reader["Email"]),
                ContactAddress = new Address()
                {
                    Address1 = SQLHelper.GetString(reader["Address"]),
                    Address2 = SQLHelper.GetString(reader["Address 2"]),
                    HouseNo = SQLHelper.GetString(reader["House_Apartment No_"]),
                    City = SQLHelper.GetString(reader["City"]),
                    County = SQLHelper.GetString(reader["County"]),
                    StateProvinceRegion = SQLHelper.GetString(reader["Territory Code"]),
                    PostCode = SQLHelper.GetString(reader["Post Code"]),
                    Country = SQLHelper.GetString(reader["Country_Region Code"]),
                    PhoneNumber = SQLHelper.GetString(reader["Phone No_"]),
                    CellPhoneNumber = SQLHelper.GetString(reader["Mobile Phone No_"]),
                },

                ShipToName = SQLHelper.GetString(reader["Ship-to Name"]),
                ShipToEmail = SQLHelper.GetString(reader["Ship-to Email"]),
                ShipToAddress = new Address()
                {
                    Address1 = SQLHelper.GetString(reader["Ship-to Address"]),
                    Address2 = SQLHelper.GetString(reader["Ship-to Address 2"]),
                    HouseNo = SQLHelper.GetString(reader["Ship-to House_Apartment No_"]),
                    City = SQLHelper.GetString(reader["Ship-to City"]),
                    County = SQLHelper.GetString(reader["Ship-to County"]),
                    PostCode = SQLHelper.GetString(reader["Ship-to Post Code"]),
                    Country = SQLHelper.GetString(reader["Ship-to Country_Region Code"]),
                    PhoneNumber = SQLHelper.GetString(reader["Ship-to Phone No_"])
                }
            };

            entry.ShippingStatus = (entry.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;
            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);
            entry.CustomerOrderNo = entry.Id;
            entry.CreateTime = entry.DocumentRegTime;

            OrderLinesGetTotals(entry.Id, out int cnt, out decimal qty, out int lcnt, out decimal amt, out decimal namt, out decimal disc);
            entry.LineItemCount = cnt;
            entry.LineCount = lcnt;
            entry.Quantity = qty;
            entry.TotalAmount = amt;
            entry.TotalNetAmount = namt;
            entry.TotalDiscount = disc;

            if (entry.Posted)
            {
                entry.Status = (SQLHelper.GetBool(reader["Cancelled"])) ? SalesEntryStatus.Canceled : SalesEntryStatus.Complete;
                SalesEntryRepository srepo = new SalesEntryRepository(config, LSCVersion);
                srepo.SalesEntryPointsGetTotal(entry.Id, entry.CustomerOrderNo, out decimal rewarded, out decimal used);
                entry.PointsRewarded = rewarded;
                entry.PointsUsedInOrder = used;
            }

            if (includeLines)
            {
                entry.Lines = OrderLinesGet(entry.Id, out string storeCurCode, stat);
                entry.Payments = OrderPayGet(entry.Id, stat);
                entry.DiscountLines = OrderDiscGet(entry.Id, stat);

                if (entry.Lines != null && entry.Lines.Count > 0)
                {
                    entry.StoreId = entry.Lines[0].StoreId;
                    entry.StoreName = entry.Lines[0].StoreName;
                    entry.StoreCurrency = storeCurCode;
                }

                ImageRepository imgrep = new ImageRepository(config, LSCVersion);
                foreach (SalesEntryLine line in entry.Lines)
                {
                    if (line.ClickAndCollectLine && entry.ClickAndCollectOrder == false)
                        entry.ClickAndCollectOrder = true;

                    if (string.IsNullOrEmpty(line.VariantId))
                    {
                        List<ImageView> img = imgrep.ImageGetByKey("Item", line.ItemId, string.Empty, string.Empty, 1, false);
                        if (img != null && img.Count > 0)
                            line.ItemImageId = img[0].Id;
                    }
                    else
                    {
                        List<ImageView> img = imgrep.ImageGetByKey("Item Variant", line.ItemId, line.VariantId, string.Empty, 1, false);
                        if (img != null && img.Count > 0)
                            line.ItemImageId = img[0].Id;
                    }
                }
            }
            return entry;
        }

        private SalesEntryLine ReaderToOrderLine(SqlDataReader reader)
        {
            return new SalesEntryLine()
            {
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UomId = SQLHelper.GetString(reader["Unit of Measure Code"]),
                Quantity = SQLHelper.GetDecimal(reader, "Quantity"),
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                LineType = (LineType)SQLHelper.GetInt32(reader["Line Type"]),
                ItemId = SQLHelper.GetString(reader["Number"]),
                NetPrice = SQLHelper.GetDecimal(reader, "Net Price"),
                Price = SQLHelper.GetDecimal(reader, "Price"),
                DiscountAmount = SQLHelper.GetDecimal(reader, "Discount Amount"),
                DiscountPercent = SQLHelper.GetDecimal(reader, "Discount Percent"),
                NetAmount = SQLHelper.GetDecimal(reader, "Net Amount"),
                TaxAmount = SQLHelper.GetDecimal(reader, "Vat Amount"),
                Amount = SQLHelper.GetDecimal(reader, "Amount"),
                ItemDescription = SQLHelper.GetString(reader["Item Description"]),
                VariantDescription = SQLHelper.GetString(reader["Variant Description"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                StoreName = SQLHelper.GetString(reader["Name"]),
                ClickAndCollectLine = SQLHelper.GetBool(reader["Click and Collect Line"])
            };
        }

        private SalesEntryDiscountLine ReaderToOrderDisc(SqlDataReader reader)
        {
            return new SalesEntryDiscountLine()
            {
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                No = SQLHelper.GetString(reader["Entry No_"]),
                OfferNumber = SQLHelper.GetString(reader["Offer No_"]),
                DiscountAmount = SQLHelper.GetDecimal(reader, "Discount Amount"),
                DiscountPercent = SQLHelper.GetDecimal(reader, "Discount Percent"),
                DiscountType = (DiscountType)SQLHelper.GetInt32(reader["Discount Type"]),
                PeriodicDiscGroup = SQLHelper.GetString(reader["Periodic Disc_ Group"]),
                PeriodicDiscType = (PeriodicDiscType)SQLHelper.GetInt32(reader["Periodic Disc_ Type"])
            };
        }
    }
}
