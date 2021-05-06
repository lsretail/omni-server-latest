using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class OrderRepository : BaseRepository
    {
        public OrderRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
        }

        public SalesEntry OrderGetById(string id, bool includeLines, bool external)
        {
            SalesEntry order = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    string stId = (NavVersion > new Version("16.2")) ? "mt.[Store No_],mt.[Created at Store]" : "mt.[Store No_]";

                    command.Parameters.Clear();
                    command.CommandText = "SELECT * FROM (" +
                        "SELECT mt.[Document ID]," + stId + ",mt.[External ID],mt.[Created] AS Date,mt.[Source Type]," +
                        "mt.[Member Card No_],mt.[Customer No_],mt.[Name] AS Name,mt.[Address],mt.[Address 2]," +
                        "mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_]," +
                        "mt.[Mobile Phone No_],mt.[Daytime Phone No_],mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2]," +
                        "mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_]," +
                        "mt.[Ship-to Email],mt.[Ship-to House_Apartment No_],mt.[Click and Collect Order], mt.[Shipping Agent Code]," +
                        "mt.[Shipping Agent Service Code], 0 AS Posted," +
                        "(SELECT COUNT(*) FROM [" + navCompanyName + "Customer Order Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] cop WHERE cop.[Document ID]=mt.[Document ID]) AS CoPay " +
                        "FROM [" + navCompanyName + "Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "UNION " +
                        "SELECT mt.[Document ID]," + stId + ",mt.[External ID],mt.[Created] AS Date,mt.[Source Type]," +
                        "mt.[Member Card No_],mt.[Customer No_],mt.[Name] AS Name,mt.[Address],mt.[Address 2]," +
                        "mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_]," +
                        "mt.[Mobile Phone No_],mt.[Daytime Phone No_],mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2]," +
                        "mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_]," +
                        "mt.[Ship-to Email],mt.[Ship-to House_Apartment No_],mt.[Click and Collect Order], mt.[Shipping Agent Code]," +
                        "mt.[Shipping Agent Service Code],1 AS Posted," +
                        "(SELECT COUNT(*) FROM [" + navCompanyName + "Posted Customer Order Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] cop WHERE cop.[Document ID]=mt.[Document ID]) AS CoPay " +
                        "FROM [" + navCompanyName + "Posted Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt) AS Orders " +
                        "WHERE [" + ((external) ? "External ID" : "Document ID") + "]=@id";

                    command.Parameters.AddWithValue("@id", id.ToUpper());
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToSalesEntry(reader, includeLines);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return order;
        }

        private List<SalesEntryLine> OrderLinesGet(string id)
        {
            string select = "SELECT ml.[Number],ml.[Variant Code],ml.[Unit of Measure Code],ml.[Line No_],ml.[Line Type]," +
                            "ml.[Net Price],ml.[Price],ml.[Quantity],ml.[Discount Amount],ml.[Discount Percent]," +
                            "ml.[Net Amount],ml.[Vat Amount],ml.[Amount],ml.[Item Description],ml.[Variant Description]" +
                            ",ml.[Document ID]";

            if (NavVersion > new Version("16.2.0.0"))
                select += ",ml.[External ID],ml.[Click and Collect Line],ml.[Store No_]";

            List<SalesEntryLine> list = new List<SalesEntryLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM ( " + select + " FROM [" + navCompanyName + "Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                            "UNION " + select + " FROM [" + navCompanyName + "Posted Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          ") AS OrderLines WHERE [Document ID]=@id" +
                                          " ORDER BY [Line No_]";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToOrderLine(reader));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        private void OrderLinesGetTotals(string orderId, out int itemCount, out decimal totalAmount, out decimal totalNetAmount, out decimal totalDiscount)
        {
            itemCount = 0;
            totalAmount = 0;
            totalNetAmount = 0;
            totalDiscount = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string select = "SELECT [Document ID], SUM([Quantity]) AS Cnt, SUM([Discount Amount]) AS Disc, SUM([Net Amount]) AS NAmt, SUM([Amount]) AS Amt";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select +
                                            " FROM [" + navCompanyName + "Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] GROUP BY [Document ID] " +
                                            "UNION " + select +
                                            " FROM [" + navCompanyName + "Posted Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] GROUP BY [Document ID] " +
                                            ") AS OrderTotals WHERE [Document ID]=@id";
                    command.Parameters.AddWithValue("@id", orderId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemCount = SQLHelper.GetInt32(reader["Cnt"]);
                            totalAmount = SQLHelper.GetDecimal(reader, "Amt");
                            totalNetAmount = SQLHelper.GetDecimal(reader, "NAmt");
                            totalDiscount = SQLHelper.GetDecimal(reader, "Disc");
                        }
                    }
                }
                connection.Close();
            }
        }

        private List<SalesEntryPayment> OrderPayGet(string id)
        {
            List<SalesEntryPayment> list = new List<SalesEntryPayment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string select = "SELECT ml.[Store No_],ml.[Line No_],ml.[Pre Approved Amount],ml.[Tender Type]," +
                                "ml.[Card Type],ml.[Currency Code],ml.[Currency Factor],ml.[Pre Approved Valid Date]," +
                                "ml.[Card or Customer No_],ml.[Document ID]";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select +
                                          " FROM [" + navCompanyName + "Customer Order Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          " UNION " + select + " FROM [" + navCompanyName + "Posted Customer Order Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          ") AS OrderTotal WHERE [Document ID]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToOrderPay(reader));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        private List<SalesEntryDiscountLine> OrderDiscGet(string id)
        {
            List<SalesEntryDiscountLine> list = new List<SalesEntryDiscountLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string select = "SELECT ml.[Line No_],ml.[Entry No_],ml.[Discount Type],ml.[Offer No_]," +
                                "ml.[Periodic Disc_ Type],ml.[Periodic Disc_ Group],ml.[Description],ml.[Discount Percent],ml.[Discount Amount]" +
                                ",ml.[Document ID]";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select + " FROM [" + navCompanyName + "Customer Order Discount Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          "UNION " + select + " FROM [" + navCompanyName + "Posted CO Discount Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
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
            return list;
        }

        private SalesEntry ReaderToSalesEntry(SqlDataReader reader, bool includeLines)
        {
            SalesEntry entry = new SalesEntry
            {
                Id = SQLHelper.GetString(reader["Document ID"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                DocumentRegTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson),
                IdType = DocumentIdType.Order,
                Status = SalesEntryStatus.Created,
                Posted = SQLHelper.GetBool(reader["Posted"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["Click And Collect Order"]),

                ShippingAgentCode = SQLHelper.GetString(reader["Shipping Agent Code"]),
                ShippingAgentServiceCode = SQLHelper.GetString(reader["Shipping Agent Service Code"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),

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
                    StateProvinceRegion = SQLHelper.GetString(reader["County"]),
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
                    StateProvinceRegion = SQLHelper.GetString(reader["Ship-to County"]),
                    PostCode = SQLHelper.GetString(reader["Ship-to Post Code"]),
                    Country = SQLHelper.GetString(reader["Ship-to Country_Region Code"]),
                    PhoneNumber = SQLHelper.GetString(reader["Ship-to Phone No_"])
                }
            };

            if (NavVersion > new Version("16.2"))
                entry.StoreId = SQLHelper.GetString(reader["Created at Store"]);

            int copay = SQLHelper.GetInt32(reader["CoPay"]);
            entry.PaymentStatus = (copay > 0) ? PaymentStatus.PreApproved : PaymentStatus.Approved;
            entry.ShippingStatus = (entry.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;
            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);
            entry.CustomerOrderNo = entry.Id;

            OrderLinesGetTotals(entry.Id, out int cnt, out decimal amt, out decimal namt, out decimal disc);
            entry.LineItemCount = cnt;
            entry.TotalAmount = amt;
            entry.TotalNetAmount = namt;
            entry.TotalDiscount = disc;

            if (entry.Posted)
            {
                entry.Status = SalesEntryStatus.Complete;
                SalesEntryRepository srepo = new SalesEntryRepository(config, NavVersion);
                srepo.SalesEntryPointsGetTotal(entry.Id, entry.CustomerOrderNo, out decimal rewarded, out decimal used);
                entry.PointsRewarded = rewarded;
                entry.PointsUsedInOrder = used;
            }

            if (includeLines)
            {
                entry.Lines = OrderLinesGet(entry.Id);
                entry.Payments = OrderPayGet(entry.Id);
                entry.DiscountLines = OrderDiscGet(entry.Id);

                ImageRepository imgrep = new ImageRepository(config, NavVersion);
                List<SalesEntryLine> list = new List<SalesEntryLine>();
                foreach (SalesEntryLine line in entry.Lines)
                {
                    if (NavVersion > new Version("16.2"))
                    {
                        if (line.ClickAndCollectLine && entry.ClickAndCollectOrder == false)
                            entry.ClickAndCollectOrder = true;
                    }
                    else
                    {
                        line.ClickAndCollectLine = entry.ClickAndCollectOrder;
                        line.StoreId = entry.StoreId;
                    }

                    SalesEntryLine exline = list.Find(l => l.Id.Equals(line.Id) && l.ItemId.Equals(line.ItemId) && l.VariantId.Equals(line.VariantId) && l.UomId.Equals(line.UomId));
                    if (exline == null)
                    {
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

                        list.Add(line);
                        continue;
                    }

                    SalesEntryDiscountLine dline = entry.DiscountLines.Find(l => l.LineNumber >= line.LineNumber && l.LineNumber < line.LineNumber + 10000);
                    if (dline != null)
                    {
                        // update discount line number to match existing record, as we will sum up the orderlines
                        dline.LineNumber = exline.LineNumber + dline.LineNumber / 100;
                    }

                    exline.Amount += line.Amount;
                    exline.NetAmount += line.NetAmount;
                    exline.DiscountAmount += line.DiscountAmount;
                    exline.TaxAmount += line.TaxAmount;
                    exline.Quantity += line.Quantity;
                }
                entry.Lines = list;
            }

            return entry;
        }

        private SalesEntryLine ReaderToOrderLine(SqlDataReader reader)
        {
            SalesEntryLine line = new SalesEntryLine()
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
                VariantDescription = SQLHelper.GetString(reader["Variant Description"])
            };

            if (NavVersion > new Version("16.2.0.0"))
            {
                line.ExternalId = SQLHelper.GetString(reader["External ID"]);
                line.StoreId = SQLHelper.GetString(reader["Store No_"]);
                line.ClickAndCollectLine = SQLHelper.GetBool(reader["Click and Collect Line"]);
            }
            return line;
        }

        private SalesEntryPayment ReaderToOrderPay(SqlDataReader reader)
        {
            return new SalesEntryPayment()
            {
                Amount = SQLHelper.GetDecimal(reader, "Pre Approved Amount"),
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                TenderType = SQLHelper.GetString(reader["Tender Type"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                CurrencyFactor = SQLHelper.GetDecimal(reader, "Currency Factor"),
                CardNo = SQLHelper.GetString(reader["Card or Customer No_"])
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
