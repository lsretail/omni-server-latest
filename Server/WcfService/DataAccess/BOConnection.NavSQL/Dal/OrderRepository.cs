using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class OrderRepository : BaseRepository
    {
        private string sqlOrderColumn = string.Empty;
        private string sqlOrderFrom = string.Empty;

        public OrderRepository(Version navVersion) : base(navVersion)
        {
            sqlOrderColumn = "mt.[Document Id],mt.[Store No_],mt.[Web Trans_ GUID],mt.[Document DateTime],mt.[Source Type]," +
                             "mt.[Member Card No_],mt.[Member Contact No_],mt.[Member Contact Name],mt.[Collect Location],mt.[Sales Order No_]," +
                             "mt.[Full Name] ,mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country Region Code]," +
                             "mt.[Phone No_],mt.[Email],mt.[House Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                             "mt.[Ship To Full Name],mt.[Ship To Address],mt.[Ship To Address 2],mt.[Ship To City],mt.[Ship To County],mt.[Ship To Post Code]," +
                             "mt.[Ship To Phone No_],mt.[Ship To Email],mt.[Ship To House Apartment No_],mt.[Ship To Country Region Code]," +
                             "mt.[Click And Collect Order],mt.[Anonymous Order],mt.[Shipping Agent Code],mt.[Shipping Agent Service Code]," +
                             "(SELECT COUNT(*) FROM [" + navCompanyName + "Customer Order Payment] cop WHERE cop.[Document Id]=mt.[Document Id]) AS CoPay";

            sqlOrderFrom = " FROM [" + navCompanyName + "Customer Order Header] mt";
        }

        public Order OrderGetById(string id, bool includeLines)
        {
            Order order = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    // Look in UnPosted order
                    command.Parameters.Clear();
                    command.CommandText = "SELECT " + sqlOrderColumn + sqlOrderFrom + " WHERE [Document Id]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToOrder(reader, includeLines, false);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return order;
        }

        public Order OrderGetByWebId(string id, bool includeLines)
        {
            Order order = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Parameters.Clear();
                    command.CommandText = "SELECT " + sqlOrderColumn + sqlOrderFrom + " WHERE [Web Trans_ GUID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToOrder(reader, includeLines, false);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return order;
        }

        public Order OrderOldGetById(string id, string webId, bool includeLines)
        {
            Order order = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Order No_],[Store No_],[Web Transaction GUID],[Trans_ Date],[Source Type],[Member Card No_]," +
                                          "[Full Name],[Address],[Address 2],[City],[County],[Post Code],[Phone No_]," +
                                          "[Email],[Country_Region Code],[House_Apartment No_],[Mobile Phone No_],[Daytime Phone No_]," +
                                          "[Ship-to First Name],[Ship-to Last Name],[Ship-to Address 1],[Ship-to Address 2],[Ship-to City]," +
                                          "[Ship-to County],[Ship-to Post Code],[Ship-to Phone No_],[Ship-to Country_Region Code],[Ship-to House_Apartment No_]," +
                                          "[Order Collect],[Receipt No_],[POS Terminal No_],[Transaction No_] " +
                                          "FROM [" + navCompanyName + "Transaction Order Header]";

                    if (string.IsNullOrEmpty(webId))
                    {
                        command.CommandText += " WHERE [Order No_]=@id";
                    }
                    else
                    {
                        command.CommandText += " WHERE [Web Transaction GUID]=@id";
                        id = webId;
                    }

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToOrderOld(reader, includeLines);
                        }
                        reader.Close();
                    }

                    if (order != null)
                    {
                        connection.Close();
                        return order;       // found in special orders
                    }

                    // check click and collect
                    command.Parameters.Clear();
                    command.CommandText = "SELECT mt.[Document Id],mt.[Store No_],mt.[Source Type],mt.[Collect Location],mt.[Web Trans_ GUID]," +
                                          "mt.[Document DateTime],mt.[Member Card No_],mt.[Member Contact No_],mt.[Member Contact Name] " +
                                          sqlOrderFrom + (string.IsNullOrEmpty(webId) ? " WHERE [Document Id]=@id" : " WHERE [Web Trans_ GUID]=@id");
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = ReaderToOrder(reader, includeLines, true);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return order;
        }

        private List<OrderLine> OrderOldLinesGet(string storeId, string termId, string id)
        {
            List<OrderLine> list = new List<OrderLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Line No_],[Item No_],[Variant Code],[Unit of Measure],[Price],[Net Price],[Quantity],[Discount Amount],[Net Amount],[VAT Amount] " +
                                          "FROM [" + navCompanyName + "Transaction Order Entry] WHERE [Store No_]=@sid AND [POS Terminal No_]=@tid AND [Transaction No_]=@id";

                    command.Parameters.AddWithValue("@sid", storeId);
                    command.Parameters.AddWithValue("@tid", termId);
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToOrderOldLine(reader));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        public List<Order> OrderHistoryByCardId(string cardId, bool includeLines)
        {
            List<Order> list = new List<Order>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Parameters.Clear();
                    command.CommandText = "SELECT " + sqlOrderColumn + sqlOrderFrom + " WHERE [Member Card No_]=@id ORDER BY [Document Id] DESC";
                    command.Parameters.AddWithValue("@id", cardId);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToOrder(reader, includeLines, false));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        private List<OrderLine> OrderLinesGet(string id)
        {
            List<OrderLine> list = new List<OrderLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT ml.[Number],ml.[Variant Code],ml.[Unit of Measure Code]," +
                                            "MIN(ml.[Line No_]) AS LineNr,MAX(ml.[Line Type]) AS LineType,MAX(ml.[Net Price]) AS NetPrice,MAX(ml.[Price]) AS Price," +
                                            "SUM(ml.[Quantity]) AS Quantity,SUM(ml.[Discount Amount]) AS DiscAmt,MAX(ml.[Discount Percent]) AS DiscPer," +
                                            "SUM(ml.[Net Amount]) AS NetAmt,SUM(ml.[Vat Amount]) AS VatAmt,SUM(ml.[Amount]) AS Amt," +
                                            "MAX(ml.[Item Description]) AS ItemDesc,MAX(ml.[Variant Description]) AS VarDesc" +
                                            " FROM [" + navCompanyName + "Customer Order Line] ml WHERE ml.[Document Id]=@id" +
                                            " GROUP BY ml.Number, ml.[Variant Code], ml.[Unit of Measure Code]";
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
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT SUM([Quantity]) AS Cnt,SUM([Discount Amount]) AS Disc,SUM([Net Amount]) AS NAmt,SUM([Amount]) AS Amt" +
                                            " FROM [" + navCompanyName + "Customer Order Line] ml WHERE [Document Id]=@id";
                    command.Parameters.AddWithValue("@id", orderId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemCount = SQLHelper.GetInt32(reader["Cnt"]);
                            totalAmount = SQLHelper.GetDecimal(reader["Amt"]);
                            totalNetAmount = SQLHelper.GetDecimal(reader["NAmt"]);
                            totalDiscount = SQLHelper.GetDecimal(reader["Disc"]);
                        }
                    }
                }
                connection.Close();
            }
        }

        private List<OrderPayment> OrderPayGet(string id)
        {
            List<OrderPayment> list = new List<OrderPayment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT ml.[Document Id],ml.[Store No_],ml.[Line No_],ml.[Pre Approved Amount],ml.[Finalised Amount],ml.[Tender Type]," +
                                          "ml.[Card Type],ml.[Currency Code],ml.[Currency Factor],ml.[Authorisation Code],ml.[Pre Approved Valid Date],ml.[Card or Customer number]" +
                                          " FROM [" + navCompanyName + "Customer Order Payment]" +
                                          " ml WHERE [Document Id]=@id";

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

        private List<OrderDiscountLine> OrderDiscGet(string id)
        {
            List<OrderDiscountLine> list = new List<OrderDiscountLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT ml.[Document Id],ml.[Line No_],ml.[Entry No_],ml.[Discount Type],ml.[Offer No_]," +
                                          "ml.[Periodic Disc_ Type],ml.[Periodic Disc_ Group],ml.[Description],ml.[Discount Percent],ml.[Discount Amount]" +
                                          " FROM [" + navCompanyName + "Customer Order Discount Line]" + 
                                          " ml WHERE [Document Id]=@id";

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

        private Order ReaderToOrder(SqlDataReader reader, bool includeLines, bool oldNav)
        {
            Order order = new Order
            {
                DocumentId = SQLHelper.GetString(reader["Document Id"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                Id = SQLHelper.GetString(reader["Web Trans_ GUID"]),
                DocumentRegTime = SQLHelper.GetDateTime(reader["Document DateTime"]),
                SourceType = (SourceType)SQLHelper.GetInt32(reader["Source Type"]),
                CollectLocation = SQLHelper.GetString(reader["Collect Location"]),
                ContactName = SQLHelper.GetString(reader["Member Contact Name"]),
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                ContactId = SQLHelper.GetString(reader["Member Contact No_"]),

                Posted = false,
                OrderStatus = OrderStatus.Created
            };

            if (oldNav)
            {
                order.ClickAndCollectOrder = true;
                order.AnonymousOrder = true;
                if (includeLines)
                {
                    order.OrderLines = OrderLinesGet(order.DocumentId);
                }
                return order;
            }

            order.ClickAndCollectOrder = SQLHelper.GetBool(reader["Click And Collect Order"]);
            order.AnonymousOrder = SQLHelper.GetBool(reader["Anonymous Order"]);

            order.PhoneNumber = SQLHelper.GetString(reader["Phone No_"]);
            order.Email = SQLHelper.GetString(reader["Email"]);
            order.MobileNumber = SQLHelper.GetString(reader["Mobile Phone No_"]);
            order.DayPhoneNumber = SQLHelper.GetString(reader["Daytime Phone No_"]);

            order.ShipToName = SQLHelper.GetString(reader["Ship To Full Name"]);
            order.ShipToPhoneNumber = SQLHelper.GetString(reader["Ship To Phone No_"]);
            order.ShipToEmail = SQLHelper.GetString(reader["Ship To Email"]);

            order.ShippingAgentCode = SQLHelper.GetString(reader["Shipping Agent Code"]);
            order.ShippingAgentServiceCode = SQLHelper.GetString(reader["Shipping Agent Service Code"]);

            order.ContactAddress = new Address()
            {
                Address1 = SQLHelper.GetString(reader["Address"]),
                Address2 = SQLHelper.GetString(reader["Address 2"]),
                HouseNo = SQLHelper.GetString(reader["House Apartment No_"]),
                City = SQLHelper.GetString(reader["City"]),
                StateProvinceRegion = SQLHelper.GetString(reader["County"]),
                PostCode = SQLHelper.GetString(reader["Post Code"]),
                Country = SQLHelper.GetString(reader["Country Region Code"])
            };

            order.ShipToAddress = new Address()
            {
                Address1 = SQLHelper.GetString(reader["Ship To Address"]),
                Address2 = SQLHelper.GetString(reader["Ship To Address 2"]),
                HouseNo = SQLHelper.GetString(reader["Ship To House Apartment No_"]),
                City = SQLHelper.GetString(reader["Ship To City"]),
                StateProvinceRegion = SQLHelper.GetString(reader["Ship To County"]),
                PostCode = SQLHelper.GetString(reader["Ship To Post Code"]),
                Country = SQLHelper.GetString(reader["Ship To Country Region Code"])
            };

            int copay = SQLHelper.GetInt32(reader["CoPay"]);
            order.PaymentStatus = (copay > 0) ? PaymentStatus.PreApproved : PaymentStatus.Approved;
            order.ShippingStatus = (order.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;

            OrderLinesGetTotals(order.DocumentId, out int cnt, out decimal amt, out decimal namt, out decimal disc);
            order.LineItemCount = cnt;
            order.TotalAmount = amt;
            order.TotalNetAmount = namt;
            order.TotalDiscount = disc;

            if (includeLines)
            {
                order.OrderLines = OrderLinesGet(order.DocumentId);
                order.OrderPayments = OrderPayGet(order.DocumentId);
                order.OrderDiscountLines = OrderDiscGet(order.DocumentId);
            }
            return order;
        }

        private OrderLine ReaderToOrderLine(SqlDataReader reader)
        {
            return new OrderLine()
            {
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UomId = SQLHelper.GetString(reader["Unit of Measure Code"]),
                Quantity = SQLHelper.GetDecimal(reader["Quantity"]),
                LineNumber = SQLHelper.GetInt32(reader["LineNr"]),
                LineType = (LineType)SQLHelper.GetInt32(reader["LineType"]),
                ItemId = SQLHelper.GetString(reader["Number"]),
                NetPrice = SQLHelper.GetDecimal(reader["NetPrice"]),
                Price = SQLHelper.GetDecimal(reader["Price"]),
                DiscountAmount = SQLHelper.GetDecimal(reader["DiscAmt"]),
                DiscountPercent = SQLHelper.GetDecimal(reader["DiscPer"]),
                NetAmount = SQLHelper.GetDecimal(reader["NetAmt"]),
                TaxAmount = SQLHelper.GetDecimal(reader["VatAmt"]),
                Amount = SQLHelper.GetDecimal(reader["Amt"]),
                ItemDescription = SQLHelper.GetString(reader["ItemDesc"]),
                VariantDescription = SQLHelper.GetString(reader["VarDesc"])
            };
        }

        private OrderPayment ReaderToOrderPay(SqlDataReader reader)
        {
            return new OrderPayment()
            {
                OrderId = SQLHelper.GetString(reader["Document Id"]),
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                TenderType = SQLHelper.GetString(reader["Tender Type"]),
                AuthorisationCode = SQLHelper.GetString(reader["Authorisation Code"]),
                CardNumber = SQLHelper.GetString(reader["Card or Customer number"]),
                CardType = SQLHelper.GetString(reader["Card Type"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                CurrencyFactor = SQLHelper.GetDecimal(reader["Currency Factor"]),
                FinalizedAmount = SQLHelper.GetDecimal(reader["Finalised Amount"]),
                PreApprovedAmount = SQLHelper.GetDecimal(reader["Pre Approved Amount"]),
                PreApprovedValidDate = SQLHelper.GetDateTime(reader["Pre Approved Valid Date"])
            };
        }

        private OrderDiscountLine ReaderToOrderDisc(SqlDataReader reader)
        {
            return new OrderDiscountLine()
            {
                OrderId = SQLHelper.GetString(reader["Document Id"]),
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                Description = SQLHelper.GetString(reader["Description"]),
                No = SQLHelper.GetString(reader["Entry No_"]),
                OfferNumber = SQLHelper.GetString(reader["Offer No_"]),
                DiscountAmount = SQLHelper.GetDecimal(reader["Discount Amount"]),
                DiscountPercent = SQLHelper.GetDecimal(reader["Discount Percent"]),
                DiscountType = (DiscountType)SQLHelper.GetInt32(reader["Discount Type"]),
                PeriodicDiscGroup = SQLHelper.GetString(reader["Periodic Disc_ Group"]),
                PeriodicDiscType = (PeriodicDiscType)SQLHelper.GetInt32(reader["Periodic Disc_ Type"])
            };
        }
		
        private Order ReaderToOrderOld(SqlDataReader reader, bool includeLines)
        {
            Order order = new Order();
            order.Posted = true;

            order.DocumentId = SQLHelper.GetString(reader["Order No_"]);
            order.StoreId = SQLHelper.GetString(reader["Store No_"]);
            order.Id = SQLHelper.GetString(reader["Web Transaction GUID"]);
            order.DocumentRegTime = SQLHelper.GetDateTime(reader["Trans_ Date"]);
            order.SourceType = (SourceType)SQLHelper.GetInt32(reader["Source Type"]);
            order.ClickAndCollectOrder = SQLHelper.GetBool(reader["Order Collect"]);
            order.AnonymousOrder = false;

            order.ReceiptNo = SQLHelper.GetString(reader["Receipt No_"]);
            order.TransStore = SQLHelper.GetString(reader["Store No_"]);
            order.TransTerminal = SQLHelper.GetString(reader["POS Terminal No_"]);
            order.TransId = SQLHelper.GetString(reader["Transaction No_"]);
            order.CardId = SQLHelper.GetString(reader["Member Card No_"]);
            order.ContactName = SQLHelper.GetString(reader["Full Name"]);

            order.ContactAddress = new Address();
            order.ContactAddress.Address1 = SQLHelper.GetString(reader["Address"]);
            order.ContactAddress.Address2 = SQLHelper.GetString(reader["Address 2"]);
            order.ContactAddress.HouseNo = SQLHelper.GetString(reader["House_Apartment No_"]);
            order.ContactAddress.City = SQLHelper.GetString(reader["City"]);
            order.ContactAddress.StateProvinceRegion = SQLHelper.GetString(reader["County"]);
            order.ContactAddress.PostCode = SQLHelper.GetString(reader["Post Code"]);
            order.ContactAddress.Country = SQLHelper.GetString(reader["Country_Region Code"]);
            order.PhoneNumber = SQLHelper.GetString(reader["Phone No_"]);
            order.Email = SQLHelper.GetString(reader["Email"]);
            order.MobileNumber = SQLHelper.GetString(reader["Mobile Phone No_"]);
            order.DayPhoneNumber = SQLHelper.GetString(reader["Daytime Phone No_"]);

            order.ShipToName = SQLHelper.GetString(reader["Ship-to First Name"]) + " " + SQLHelper.GetString(reader["Ship-to Last Name"]);
            order.ShipToAddress = new Address();
            order.ShipToAddress.Address1 = SQLHelper.GetString(reader["Ship-to Address 1"]);
            order.ShipToAddress.Address2 = SQLHelper.GetString(reader["Ship-to Address 2"]);
            order.ShipToAddress.HouseNo = SQLHelper.GetString(reader["Ship-to House_Apartment No_"]);
            order.ShipToAddress.City = SQLHelper.GetString(reader["Ship-to City"]);
            order.ShipToAddress.StateProvinceRegion = SQLHelper.GetString(reader["Ship-to County"]);
            order.ShipToAddress.PostCode = SQLHelper.GetString(reader["Ship-to Post Code"]);
            order.ShipToAddress.Country = SQLHelper.GetString(reader["Ship-to Country_Region Code"]);
            order.ShipToPhoneNumber = SQLHelper.GetString(reader["Ship-to Phone No_"]);

            if (includeLines)
            {
                order.OrderLines = OrderOldLinesGet(order.StoreId, order.TransTerminal, order.TransId);
            }
            return order;
        }

        private OrderLine ReaderToOrderOldLine(SqlDataReader reader)
        {
            OrderLine line = new OrderLine();
            line.LineNumber = SQLHelper.GetInt32(reader["Line No_"]);
            line.VariantId = SQLHelper.GetString(reader["Variant Code"]);
            line.UomId = SQLHelper.GetString(reader["Unit of Measure"]);
            line.Quantity = SQLHelper.GetDecimal(reader["Quantity"]);

            line.LineType = LineType.Item;
            line.ItemId = SQLHelper.GetString(reader["Item No_"]);
            line.NetPrice = SQLHelper.GetDecimal(reader["Net Price"]);
            line.Price = SQLHelper.GetDecimal(reader["Price"]);
            line.DiscountAmount = SQLHelper.GetDecimal(reader["Discount Amount"]);
            line.NetAmount = SQLHelper.GetDecimal(reader["Net Amount"]);
            line.TaxAmount = SQLHelper.GetDecimal(reader["VAT Amount"]);
            line.Amount = line.NetAmount + line.TaxAmount;
            return line;
        }
    }
}
