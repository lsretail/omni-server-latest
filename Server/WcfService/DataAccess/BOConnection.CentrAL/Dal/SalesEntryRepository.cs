using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.CentrAL.Dal
{
    public class SalesEntryRepository : BaseRepository
    {
        private string storefield = "Store No_";

        public SalesEntryRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            if (NavVersion > new Version("16.2.0.0"))
                storefield = "Created at Store";
        }

        public List<SalesEntry> SalesEntriesByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNrOfEntries)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Parameters.Clear();

                    string sqlwhere = string.Empty;
                    if (string.IsNullOrEmpty(storeId) == false)
                    {
                        sqlwhere += "AND [Store]=@sid ";
                        command.Parameters.AddWithValue("@sid", storeId);
                    }
                    if (date > DateTime.MinValue)
                    {
                        sqlwhere += string.Format("AND [Date]{0}@dt ", (dateGreaterThan) ? ">" : "<");
                        command.Parameters.AddWithValue("@dt", new DateTime(date.Year, date.Month, date.Day, 0, 0, 0));
                    }

                    command.CommandText = "SELECT " + ((maxNrOfEntries > 0) ? "TOP " + maxNrOfEntries : string.Empty) + "* FROM (" +
                        "SELECT mt.[Customer Order ID] AS [Document ID],co.[Created at Store] AS [StCreate],mt.[Store No_] AS [Store],(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date],co.[Created] AS [CrDate],mt.[Sale Is Return Sale] AS [RT],mt.[Refund Receipt No_] AS [Refund]," +
                        "co.[External ID],mt.[Member Card No_],mt.[Customer No_],1 AS [Posted],mt.[Receipt No_],mt.[Customer Order] AS [CAC]," +
                        "co.[Name],co.[Address],co.[Address 2],co.[City],co.[County],co.[Post Code],co.[Country_Region Code],co.[Phone No_],co.[Email],co.[House_Apartment No_],co.[Mobile Phone No_],co.[Daytime Phone No_]," +
                        "co.[Ship-to Name],co.[Ship-to Address],co.[Ship-to Address 2],co.[Ship-to City],co.[Ship-to County],co.[Ship-to Post Code],co.[Ship-to Country_Region Code],co.[Ship-to Phone No_],co.[Ship-to Email],co.[Ship-to House_Apartment No_]," +
                        "mt.[No_ of Items] AS [Quantity],mt.[No_ of Item Lines] AS [Lines],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount],st.[Name] AS [StName],mt.[Trans_ Currency] AS [StCur],mt.[POS Terminal No_] " +
                        "FROM [" + navCompanyName + "Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store No_] " +
                        "LEFT JOIN [" + navCompanyName + "Posted Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] co ON co.[Document ID]=mt.[Customer Order ID] " +
                        "UNION " +
                        "SELECT mt.[Document ID],mt.[" + storefield + "] AS [StCreate],mt.[" + storefield + "] AS [Store],mt.[Created] AS [Date],mt.[Created] AS [CrDate],0 AS [RT],'' AS [Refund]," +
                        "mt.[External ID],mt.[Member Card No_],mt.[Customer No_],0 AS [Posted],'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                        "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                        "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                        "0 AS [Quantity],0 AS [Lines],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],st.[Currency Code] AS [StCur],'' AS [POS Terminal No_] " +
                        "FROM [" + navCompanyName + "Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[" + storefield + "] " +
                        "UNION " +
                        "SELECT mt.[Document ID],mt.[" + storefield + "] AS [StCreate],mt.[" + storefield + "] AS [Store],mt.[Created] AS [Date],mt.[Created] AS [CrDate],0 AS [RT],'' AS [Refund]," +
                        "mt.[External ID],mt.[Member Card No_],mt.[Customer No_],3 AS [Posted],'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                        "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                        "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                        "0 AS [Quantity],0 AS [Lines],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],st.[Currency Code] AS [StCur],'' AS [POS Terminal No_] " +
                        "FROM [" + navCompanyName + "Posted Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[" + storefield + "] WHERE mt.CancelledOrder=1 " +
                        "UNION " +
                        "SELECT mt.[Queue Counter] AS [Document ID],mt.[Store No_] AS [StCreate],mt.[Store No_] AS [Store],(do.[Date Created]+CAST((CONVERT(time,do.[Time Created])) AS DATETIME)) AS [Date],(do.[Date Created]+CAST((CONVERT(time,do.[Time Created])) AS DATETIME)) AS [CrDate],mt.[Sale Is Return Sale] AS [RT],mt.[Original Receipt No_] AS [Refund]," +
                        "'' AS [External ID],mt.[Member Card No_],mt.[Customer No_],2 AS Posted,mt.[Receipt No_],0 AS [CAC]," +
                        "do.[Name],do.[Address],do.[Address 2],do.[City],'' AS [County],'' AS [Post Code],'' AS [Country_Region Code],do.[Phone No_],'' AS [Email],'' AS [House_Apartment No_],'' AS [Mobile Phone No_],'' AS [Daytime Phone No_]," +
                        "do.[Name] AS [Ship-to Name],do.[Address] AS [Ship-to Address],do.[Address 2] AS [Ship-to Address 2],do.[City] AS [Ship-to City],'' AS [Ship-to County],'' AS [Ship-to Post Code],'' AS [Ship-to Country_Region Code],do.[Phone No_] AS [Ship-to Phone No_],'' AS [Ship-to Email],'' AS [Ship-to House_Apartment No_]," +
                        "0 AS [Quantity],0 AS [Lines],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],mt.[Trans_ Currency Code] AS [StCur],mt.[POS Terminal No_] " +
                        "FROM [" + navCompanyName + "POS Transaction$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store No_] " +
                        "LEFT JOIN [" + navCompanyName + "Delivery Order$5ecfc871-5d82-43f1-9c54-59685e82318d] do ON do.[Order No_]=mt.[Receipt No_] " +
                        "WHERE mt.[Entry Status]=0" +
                        ") AS SalesEntries " +
                        "WHERE [Member Card No_]=@id " + sqlwhere +
                        "ORDER BY [Date] DESC";
                    command.Parameters.AddWithValue("@id", cardId);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(SalesEntryHeader(reader));
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public SalesEntry SalesEntryGetById(string entryId)
        {
            SalesEntry entry = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Member Card No_],mt.[Customer No_],mt.[No_ of Items] AS [Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount]," +
                        "(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date],co.[Created] AS [CrDate],mt.[Store No_],st.[Name] AS [StName],mt.[Trans_ Currency],mt.[POS Terminal No_]," +
                        "co.[External ID] AS [External ID],co.[Created at Store]," +
                        "co.[Name],co.[Address],co.[Address 2],co.[City],co.[County],co.[Post Code],co.[Country_Region Code],co.[Phone No_],co.[Email],co.[House_Apartment No_],co.[Mobile Phone No_],co.[Daytime Phone No_]," +
                        "co.[Ship-to Name],co.[Ship-to Address],co.[Ship-to Address 2],co.[Ship-to City],co.[Ship-to County],co.[Ship-to Post Code],co.[Ship-to Country_Region Code],co.[Ship-to Phone No_],co.[Ship-to Email],co.[Ship-to House_Apartment No_]," +
                        "mt.[Receipt No_],mt.[Customer Order ID] AS [Document ID],mt.[Customer Order] AS [CAC],mt.[Sale Is Return Sale] AS [RT],mt.[Refund Receipt No_] AS [Refund] " +
                        "FROM [" + navCompanyName + "Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store No_] " +
                        "LEFT JOIN [" + navCompanyName + "Posted Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] co ON co.[Document ID]=mt.[Customer Order ID] " +
                        "WHERE mt.[Receipt No_]=@id";

                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entry = TransactionToSalesEntry(reader);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return entry;
        }

        public List<SalesEntryId> SalesEntryGetReturnSales(string receiptNo)
        {
            List<SalesEntryId> list = new List<SalesEntryId>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Receipt No_]," +
                                          "(SELECT DISTINCT [Customer Order ID] " +
                                          "FROM [" + navCompanyName + "Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Receipt No_]=@no) AS CONo " +
                                          "FROM [" + navCompanyName + "Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Retrieved from Receipt No_]=@no";
                    command.Parameters.AddWithValue("@no", receiptNo);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new SalesEntryId()
                            {
                                ReceiptId = SQLHelper.GetString(reader["Receipt No_"]),
                                OrderId = SQLHelper.GetString(reader["CONo"])
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public List<SalesEntryId> SalesEntryGetSalesByOrderId(string orderId)
        {
            List<SalesEntryId> list = new List<SalesEntryId>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Receipt No_] " +
                                          "FROM [" + navCompanyName + "Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                                          "WHERE [Customer Order ID]=@no";
                    command.Parameters.AddWithValue("@no", orderId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new SalesEntryId()
                            {
                                ReceiptId = SQLHelper.GetString(reader["Receipt No_"]),
                                OrderId = orderId
                            });
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return list;
        }

        public SalesEntry POSTransactionGetById(string entryId)
        {
            SalesEntry entry = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Member Card No_],mt.[Customer No_],mt.[Receipt No_],mt.[Queue Counter],mt.[Store No_],mt.[POS Terminal No_],mt.[Staff ID],mt.[Customer No_]," +
                        "mt.[Sale Is Return Sale] AS [RT],mt.[Original Receipt No_] AS [Refund],do.[Phone No_],do.[Address],do.[Address 2],do.[City],do.[Name]," +
                        "(do.[Date Created]+CAST((CONVERT(time,do.[Time Created])) AS DATETIME)) AS [Date],st.[Name] AS [StName],mt.[Trans_ Currency Code] " +
                        "FROM [" + navCompanyName + "POS Transaction$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store No_] " +
                        "LEFT JOIN [" + navCompanyName + "Delivery Order$5ecfc871-5d82-43f1-9c54-59685e82318d] do ON do.[Order No_]=mt.[Receipt No_] " +
                        "WHERE mt.[Receipt No_]=@id";

                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entry = POSTransToSalesEntry(reader, true);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return entry;
        }

        public List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + ((maxNumberOfTransactions > 0) ? "TOP " + maxNumberOfTransactions : "") + " * FROM (" +
                        "SELECT DISTINCT mt.[Customer Order ID] AS [Document ID],co.[Created at Store] AS [StCreate],mt.[Store No_] AS [Store],(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date],co.[Created] AS [CrDate],mt.[Sale Is Return Sale] AS [RT],mt.[Refund Receipt No_] AS [Refund]," +
                        "co.[External ID],mt.[Member Card No_],mt.[Customer No_],1 AS [Posted],mt.[Receipt No_],mt.[Customer Order] AS [CAC]," +
                        "co.[Name],co.[Address],co.[Address 2],co.[City],co.[County],co.[Post Code],co.[Country_Region Code],co.[Phone No_],co.[Email],co.[House_Apartment No_],co.[Mobile Phone No_],co.[Daytime Phone No_]," +
                        "co.[Ship-to Name],co.[Ship-to Address],co.[Ship-to Address 2],co.[Ship-to City],co.[Ship-to County],co.[Ship-to Post Code],co.[Ship-to Country_Region Code],co.[Ship-to Phone No_],co.[Ship-to Email],co.[Ship-to House_Apartment No_]," +
                        "mt.[No_ of Items] AS [Quantity],mt.[No_ of Item Lines] AS [Lines],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount],st.[Name] AS [StName],mt.[Trans_ Currency] AS [StCur],mt.[POS Terminal No_] " +
                        "FROM [" + navCompanyName + "Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store No_] " +
                        "JOIN [" + navCompanyName + "Trans_ Sales Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] tl ON tl.[Receipt No_]=mt.[Receipt No_] " +
                        "JOIN [" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] i ON i.[No_]=tl.[Item No_] " +
                        "LEFT JOIN [" + navCompanyName + "Posted Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] co ON co.[Document ID]=mt.[Customer Order ID] " +
                        "WHERE UPPER(i.[Description]) LIKE UPPER(@search) " +
                        "UNION " +
                        "SELECT DISTINCT mt.[Document ID],mt.[" + storefield + "] AS [StCreate],mt.[" + storefield + "] AS [Store],mt.[Created] AS [Date],mt.[Created] AS [CrDate],0 AS [RT],'' AS [Refund]," +
                        "mt.[External ID],mt.[Member Card No_],mt.[Customer No_],0 AS Posted,'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                        "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                        "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                        "0 AS [Quantity],0 AS [Lines],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],st.[Currency Code] AS [StCur],'' AS [POS Terminal No_] " +
                        "FROM [" + navCompanyName + "Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Customer Order Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ol ON ol.[Document ID]=mt.[Document ID] " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]= mt.[" + storefield + "] " +
                        "WHERE UPPER(ol.[Item Description]) LIKE UPPER(@search) " +
                        "UNION " +
                        "SELECT mt.[Document ID],mt.[" + storefield + "] AS [StCreate],mt.[" + storefield + "] AS [Store],mt.[Created] AS [Date],mt.[Created] AS [CrDate],0 AS [RT],'' AS [Refund]," +
                        "mt.[External ID],mt.[Member Card No_],mt.[Customer No_],3 AS [Posted],'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                        "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                        "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                        "0 AS [Quantity],0 AS [Lines],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],st.[Currency Code] AS [StCur],'' AS [POS Terminal No_] " +
                        "FROM [" + navCompanyName + "Posted Customer Order Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[" + storefield + "] WHERE mt.CancelledOrder=1 " +
                        "UNION " +
                        "SELECT DISTINCT '' AS [Document ID],mt.[Store No_] AS [StCreate],mt.[Store No_] AS [Store],(do.[Date Created]+CAST((CONVERT(time,do.[Time Created])) AS DATETIME)) AS [Date],(do.[Date Created]+CAST((CONVERT(time,do.[Time Created])) AS DATETIME)) AS [CrDate],mt.[Sale Is Return Sale] AS [RT],mt.[Original Receipt No_] AS [Refund]," +
                        "'' AS [External ID],mt.[Member Card No_],mt.[Customer No_],2 AS Posted,mt.[Receipt No_],0 AS [CAC]," +
                        "do.[Name],do.[Address],do.[Address 2],do.[City],'' AS [County],'' AS [Post Code],'' AS [Country_Region Code],do.[Phone No_],'' AS [Email],'' AS [House_Apartment No_],'' AS [Mobile Phone No_],'' AS [Daytime Phone No_]," +
                        "do.[Name] AS [Ship-to Name],do.[Address] AS [Ship-to Address],do.[Address 2] AS [Ship-to Address 2],do.[City] AS [Ship-to City],'' AS [Ship-to County],'' AS [Ship-to Post Code],'' AS [Ship-to Country_Region Code],do.[Phone No_] AS [Ship-to Phone No_],'' AS [Ship-to Email],'' AS [Ship-to House_Apartment No_]," +
                        "0 AS [Quantity],0 AS [Lines],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],mt.[Trans_ Currency Code] AS [StCur],mt.[POS Terminal No_] " +
                        "FROM [" + navCompanyName + "POS Transaction$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                        "JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=mt.[Store No_] " +
                        "JOIN [" + navCompanyName + "POS Trans_ Line$5ecfc871-5d82-43f1-9c54-59685e82318d] tl ON tl.[Receipt No_]=mt.[Receipt No_] " +
                        "JOIN [" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] i ON i.[No_]=tl.[Number] " +
                        "LEFT JOIN [" + navCompanyName + "Delivery Order$5ecfc871-5d82-43f1-9c54-59685e82318d] do ON do.[Order No_]=mt.[Receipt No_] " +
                        "WHERE UPPER(i.[Description]) LIKE UPPER(@search) AND mt.[Entry Status]=0" +
                        ") AS SalesEntries " +
                        "WHERE [Member Card No_]=@id " +
                        "ORDER BY [Date] DESC";

                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@search", $"%{search}%");
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(SalesEntryHeader(reader));
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private void POSTransLinesGetTotals(string reciptNo, out int itemCount, out int lineCount, out decimal totalAmount, out decimal totalNetAmount, out decimal totalDiscount)
        {
            itemCount = 0;
            lineCount = 0;
            totalAmount = 0;
            totalNetAmount = 0;
            totalDiscount = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT SUM([Quantity]) AS Cnt,SUM([Discount Amount]) AS Disc,SUM([Net Amount]) AS NAmt,SUM([Amount]) AS Amt " +
                                          "FROM [" + navCompanyName + "POS Trans_ Line$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Entry Type]=0 AND [Receipt No_]=@id";
                    command.Parameters.AddWithValue("@id", reciptNo);
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

                    command.CommandText = "SELECT COUNT(*) AS Cnt " +
                                          "FROM [" + navCompanyName + "POS Trans_ Line$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Entry Type]=0 AND [Receipt No_]=@id";
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

        public List<SalesEntryLine> TransSalesEntryLinesGet(string receiptId)
        {
            List<SalesEntryLine> list = new List<SalesEntryLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                "ml.[Store No_],st.[Name],ml.[POS Terminal No_],ml.[Transaction No_],ml.[Item No_],ml.[Variant Code],ml.[Unit of Measure],ml.[Deal Modifier Line No_],ml.[Deal Header Line No_]," +
                                "ml.[Quantity],ml.[UOM Quantity],ml.[Price],ml.[Net Price],ml.[Net Amount],ml.[Discount Amount],ml.[VAT Amount],ml.[Refund Qty_],ml.[Line No_],i.[Description],ml.[Parent Line No_]," +
                                "v.[Variant Dimension 1],v.[Variant Dimension 2],v.[Variant Dimension 3],v.[Variant Dimension 4],v.[Variant Dimension 5]" +
                                " FROM [" + navCompanyName + "Trans_ Sales Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml" +
                                " JOIN [" + navCompanyName + "Item$437dbf0e-84ff-417a-965d-ed2bb9650972] i ON i.[No_]=ml.[Item No_]" +
                                " JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=ml.[Store No_]" +
                                " LEFT JOIN [" + navCompanyName + "Item Variant Registration$5ecfc871-5d82-43f1-9c54-59685e82318d] v ON v.[Item No_]=ml.[Item No_] AND v.[Variant]=ml.[Variant Code]" +
                                " WHERE ml.[Receipt No_]=@id ";

                    int transNo = 0;
                    string termNo = string.Empty;

                    command.Parameters.AddWithValue("@id", receiptId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToSalesEntryLine(reader, out int trans, out string term));
                            if (trans > 0)
                            {
                                transNo = trans;
                                termNo = term;
                            }
                        }
                        reader.Close();
                    }

                    if (transNo > 0)
                    {
                        command.CommandText = "SELECT " +
                                    "ml.[Line No_],ml.[Deal No_],ml.[Deal Header Line No_],ml.[Quantity],ml.[Amount],ml.[Price],ml.[Line Discount Amt_],o.[Description]" +
                                    " FROM [" + navCompanyName + "Trans_ Deal Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml" +
                                    " JOIN [" + navCompanyName + "Offer$5ecfc871-5d82-43f1-9c54-59685e82318d] o ON o.[No_]=ml.[Deal No_]" +
                                    " WHERE ml.[Store No_]=@sid AND ml.[POS Terminal No_]=@pid AND ml.[Transaction No_]=@tid";

                        SalesEntryLine firstline = list.First();

                        command.Parameters.AddWithValue("@sid", firstline.StoreId);
                        command.Parameters.AddWithValue("@pid", termNo);
                        command.Parameters.AddWithValue("@tid", transNo);
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SalesEntryLine l = ReaderToSalesDealLine(reader);
                                l.StoreId = firstline.StoreId;
                                l.StoreName = firstline.StoreName;
                                list.Add(l);
                            }
                            reader.Close();
                        }
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<SalesEntryLine> POSTransLinesGet(string receiptId)
        {
            List<SalesEntryLine> list = new List<SalesEntryLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                "ml.[Store No_],st.[Name],ml.[POS Terminal No_],ml.[Number],ml.[Parent Line],ml.[Variant Code],ml.[Unit of Measure]," +
                                "ml.[Quantity],ml.[Price],ml.[Net Price],ml.[Net Amount],ml.[Discount Amount],ml.[VAT Amount],ml.[Line No_],ml.[Description]," +
                                "v.[Variant Dimension 1],v.[Variant Dimension 2],v.[Variant Dimension 3],v.[Variant Dimension 4],v.[Variant Dimension 5]," +
                                "ml.[Promotion No_],ml.[Entry Type]" +
                                " FROM [" + navCompanyName + "POS Trans_ Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml" +
                                " JOIN [" + navCompanyName + "Store$5ecfc871-5d82-43f1-9c54-59685e82318d] st ON st.[No_]=ml.[Store No_]" +
                                " LEFT JOIN [" + navCompanyName + "Item Variant Registration$5ecfc871-5d82-43f1-9c54-59685e82318d] v ON v.[Item No_]=ml.[Number] AND v.[Variant]=ml.[Variant Code]" +
                                " WHERE ml.[Receipt No_]=@id AND (ml.[Entry Type]=0 OR (ml.[Entry Type]=5 AND ml.[Sales Type]!=''))";

                    command.Parameters.AddWithValue("@id", receiptId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(POSTransToSalesEntryLine(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<SalesEntryDiscountLine> DiscountLineGet(string receiptNo)
        {
            List<SalesEntryDiscountLine> list = new List<SalesEntryDiscountLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT ml.[Line No_],ml.[Offer Type],ml.[Offer No_],ml.[Discount Amount]," +
                                          "pd.[Description],pd.[Type] " +
                                          "FROM [" + navCompanyName + "Trans_ Discount Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          "LEFT JOIN [" + navCompanyName + "Periodic Discount$5ecfc871-5d82-43f1-9c54-59685e82318d] pd ON pd.[No_]=ml.[Offer No_] " +
                                          "WHERE ml.[Receipt No_]=@id";

                    command.Parameters.AddWithValue("@id", receiptNo);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToDiscountLine(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<SalesEntryPayment> TransSalesEntryPaymentGet(string receiptNo, string custOrderNo)
        {
            List<SalesEntryPayment> list = new List<SalesEntryPayment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Amount in Currency] AS Amt,ml.[Exchange Rate] AS Rate,ml.[Card or Account] AS No " +
                                "FROM [" + navCompanyName + "Trans_ Payment Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                "WHERE ml.[Receipt No_]=@id ORDER BY ml.[Line No_]";
                    command.Parameters.AddWithValue("@id", receiptNo);

                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(TransToSalesEntryPayment(reader));
                        }
                        reader.Close();
                    }

                    if (string.IsNullOrEmpty(custOrderNo) == false && list.Count == 0)
                    {
                        // did not find any finalized payments lines, so look for pre approved lines
                        command.CommandText = "SELECT " +
                                    "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Pre Approved Amount] AS Amt,ml.[Currency Factor] AS Rate,ml.[Card or Customer No_] AS No " +
                                    "FROM [" + navCompanyName + "Posted Customer Order Payment$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                    "WHERE ml.[Document ID]=@id AND [Type]=1 ORDER BY ml.[Line No_]";
                        TraceSqlCommand(command);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(TransToSalesEntryPayment(reader));
                            }
                            reader.Close();
                        }
                    }

                    connection.Close();
                }
            }
            return list;
        }

        public List<SalesEntryPayment> POSTransPaymentLinesGet(string receiptId)
        {
            List<SalesEntryPayment> list = new List<SalesEntryPayment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                "ml.[Line No_],ml.[Number],ml.[Currency Code],ml.[Amount],ml.[CurrencyFactor],mc.[Card Number]" +
                                " FROM [" + navCompanyName + "POS Trans_ Line$5ecfc871-5d82-43f1-9c54-59685e82318d] ml" +
                                " LEFT JOIN [" + navCompanyName + "POS Card Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] mc ON mc.[Receipt No_]=ml.[Receipt No_] AND mc.[Tender Type]=ml.[Number]" +
                                " WHERE ml.[Receipt No_]=@id AND ml.[Entry Type]=1";

                    command.Parameters.AddWithValue("@id", receiptId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(POSPaymentToSalesEntryPayment(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public void SalesEntryPointsGetTotal(string entryId, string custId, out decimal rewarded, out decimal used)
        {
            rewarded = 0;
            used = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    if (string.IsNullOrEmpty(custId) == false)
                    {
                    	//Awarded points are linked to Sales Invoice Id
					
                    	command.CommandText = "SELECT [No_] FROM [" + navCompanyName + "Sales Invoice Header$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Customer Order ID]=@id";
                        command.Parameters.AddWithValue("@id", custId);
                        TraceSqlCommand(command);
                        string salesId = (string)command.ExecuteScalar();

                        //Use sales invoice id to get Rewarded points for customer orders
                        if (!string.IsNullOrEmpty(salesId))
                            entryId = salesId;
                    }

                    //Get Used points with entryId (receiptId/orderId)
                    command.CommandText = "SELECT [Points] FROM [" + navCompanyName + "Member Point Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Document No_]=@id AND [Entry Type]=1"; //Entry type = 1 is redemption
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    var res = command.ExecuteScalar();
                    used = res == null ? 0 : -(decimal)res; //use '-' to convert to positive number

                    //Get rewarded points
                    command.CommandText = "SELECT [Points] FROM [" + navCompanyName + "Member Point Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] WHERE [Document No_]=@id AND [Entry Type]=0";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    res = command.ExecuteScalar();
                    rewarded = res == null ? 0 : (decimal)res;
                }
                connection.Close();
            }

            logger.Debug(config.LSKey.Key, "Get Point Balance: ReceiptNo:{0} CustOrderNo:{1} PointRew:{2} PointUse:{3}", 
                entryId, custId, rewarded, used);
        }

        private SalesEntry TransactionToSalesEntry(SqlDataReader reader)
        {
            SalesEntry entry = new SalesEntry()
            {
                Id = SQLHelper.GetString(reader["Receipt No_"]),
                IdType = DocumentIdType.Receipt,
                LineItemCount = (int)SQLHelper.GetDecimal(reader, "Quantity", false),
                TotalNetAmount = SQLHelper.GetDecimal(reader, "Net Amount", true),
                TotalAmount = SQLHelper.GetDecimal(reader, "Gross Amount", true),
                TotalDiscount = SQLHelper.GetDecimal(reader, "Discount Amount", false),
                CreateTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["CrDate"]), config.IsJson),
                DocumentRegTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                CreateAtStoreId = SQLHelper.GetString(reader["Created at Store"]),
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"]),
                Status = SalesEntryStatus.Complete,
                Posted = true,
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                StoreName = SQLHelper.GetString(reader["StName"]),
                StoreCurrency = SQLHelper.GetString(reader["Trans_ Currency"]),
                CustomerOrderNo = SQLHelper.GetString(reader["Document ID"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),
                ReturnSale = SQLHelper.GetBool(reader["RT"]),
                HasReturnSale = SQLHelper.GetString(reader["Refund"]) != string.Empty,

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

            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);
            if (string.IsNullOrEmpty(entry.CustomerOrderNo))
            {
                entry.CreateTime = entry.DocumentRegTime;
                entry.CreateAtStoreId = entry.StoreId;
            }

            SalesEntryPointsGetTotal(entry.Id, entry.CustomerOrderNo, out decimal rewarded, out decimal used);
            entry.PointsRewarded = rewarded;
            entry.PointsUsedInOrder = used;

            entry.Lines = TransSalesEntryLinesGet(entry.Id);
            entry.DiscountLines = DiscountLineGet(entry.Id);
            entry.Payments = TransSalesEntryPaymentGet(entry.Id, entry.CustomerOrderNo);
            entry.LineCount = entry.LineCount;
            return entry;
        }

        private SalesEntry POSTransToSalesEntry(SqlDataReader reader, bool includeLines)
        {
            SalesEntry entry = new SalesEntry()
            {
                Id = SQLHelper.GetString(reader["Receipt No_"]),
                CustomerOrderNo = SQLHelper.GetString(reader["Queue Counter"]),
                IdType = DocumentIdType.HospOrder,
                DocumentRegTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                Status = SalesEntryStatus.Pending,
                Posted = false,
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                StoreName = SQLHelper.GetString(reader["StName"]),
                StoreCurrency = SQLHelper.GetString(reader["Trans_ Currency Code"]),
                ReturnSale = SQLHelper.GetBool(reader["RT"]),
                HasReturnSale = SQLHelper.GetString(reader["Refund"]) != string.Empty,

                CustomerId = SQLHelper.GetString(reader["Customer No_"]),
                ContactName = SQLHelper.GetString(reader["Name"]),
                ContactAddress = new Address(),
                ShipToName = SQLHelper.GetString(reader["Name"]),
                ShipToAddress = new Address()
                {
                    Address1 = SQLHelper.GetString(reader["Address"]),
                    Address2 = SQLHelper.GetString(reader["Address 2"]),
                    City = SQLHelper.GetString(reader["City"]),
                    PhoneNumber = SQLHelper.GetString(reader["Phone No_"])
                }
            };

            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);
            POSTransLinesGetTotals(entry.Id, out int cnt, out int lcnt, out decimal amt, out decimal namt, out decimal disc);
            entry.LineItemCount = cnt;
            entry.LineCount = lcnt;
            entry.TotalAmount = amt;
            entry.TotalNetAmount = namt;
            entry.TotalDiscount = disc;

            if (includeLines)
            {
                entry.Lines = POSTransLinesGet(entry.Id);
                entry.Payments = POSTransPaymentLinesGet(entry.Id);
            }
            return entry;
        }

        private SalesEntry SalesEntryHeader(SqlDataReader reader)
        {
            SalesEntry entry = new SalesEntry
            {
                StoreId = SQLHelper.GetString(reader["Store"]),
                CreateAtStoreId = SQLHelper.GetString(reader["StCreate"]),
                CreateTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["CrDate"]), config.IsJson),
                DocumentRegTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson),
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                StoreName = SQLHelper.GetString(reader["StName"]),
                StoreCurrency = SQLHelper.GetString(reader["StCur"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"]),
                ReturnSale = SQLHelper.GetBool(reader["RT"]),
                HasReturnSale = SQLHelper.GetString(reader["Refund"]) != string.Empty,

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

            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);
            entry.ShippingStatus = (entry.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;

            OrderRepository orep = new OrderRepository(config, NavVersion);

            int cnt;
            int lcnt;
            decimal amt;
            decimal namt;
            decimal disc;
            switch (SQLHelper.GetInt32(reader["Posted"]))
            {
                case 1:
                    entry.Id = SQLHelper.GetString(reader["Receipt No_"]);
                    entry.CustomerOrderNo = SQLHelper.GetString(reader["Document ID"]);
                    entry.TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]);
                    cnt = SQLHelper.GetInt32(reader["Quantity"]);
                    lcnt = SQLHelper.GetInt32(reader["Lines"]);
                    namt = SQLHelper.GetDecimal(reader, "Net Amount", true);
                    amt = SQLHelper.GetDecimal(reader, "Gross Amount", true);
                    disc = SQLHelper.GetDecimal(reader, "Discount Amount", false);
                    entry.IdType = DocumentIdType.Receipt;
                    entry.Status = SalesEntryStatus.Complete;
                    entry.Posted = true;

                    SalesEntryPointsGetTotal(entry.Id, entry.CustomerOrderNo, out decimal rewarded, out decimal used);
                    entry.PointsRewarded = rewarded;
                    entry.PointsUsedInOrder = used;

                    if (string.IsNullOrEmpty(entry.CustomerOrderNo))
                    {
                        entry.CreateTime = entry.DocumentRegTime;
                        entry.CreateAtStoreId = entry.StoreId;
                    }
                    break;
                case 2:
                    entry.Id = SQLHelper.GetString(reader["Receipt No_"]);
                    entry.CustomerOrderNo = SQLHelper.GetString(reader["Document ID"]);
                    entry.IdType = DocumentIdType.HospOrder;
                    entry.Status = SalesEntryStatus.Processing;
                    entry.Posted = false;

                    POSTransLinesGetTotals(entry.Id, out cnt, out lcnt, out amt, out namt, out disc);
                    break;
                case 3:
                    entry.Id = SQLHelper.GetString(reader["Document ID"]);
                    entry.CustomerOrderNo = SQLHelper.GetString(reader["Document ID"]);
                    entry.IdType = DocumentIdType.Order;
                    entry.Status = SalesEntryStatus.Canceled;
                    entry.Posted = false;

                    orep.OrderLinesGetTotals(entry.Id, out cnt, out lcnt, out amt, out namt, out disc);
                    break;
                default:
                    entry.Id = SQLHelper.GetString(reader["Document ID"]);
                    entry.CustomerOrderNo = SQLHelper.GetString(reader["Document ID"]);
                    entry.IdType = DocumentIdType.Order;
                    entry.Status = SalesEntryStatus.Created;
                    entry.Posted = false;

                    orep.OrderLinesGetTotals(entry.Id, out cnt, out lcnt, out amt, out namt, out disc);
                    break;
            }

            entry.LineCount = lcnt;
            entry.LineItemCount = cnt;
            entry.TotalAmount = amt;
            entry.TotalNetAmount = namt;
            entry.TotalDiscount = disc;
            return entry;
        }

        private SalesEntryLine ReaderToSalesEntryLine(SqlDataReader reader, out int transNo, out string termNo)
        {
            transNo = 0;
            termNo = string.Empty;
            decimal uomqty = 0;

            SalesEntryLine line = new SalesEntryLine()
            {
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                ParentLine = SQLHelper.GetInt32(reader["Parent Line No_"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UomId = SQLHelper.GetString(reader["Unit of Measure"]),
                Quantity = SQLHelper.GetDecimal(reader, "Quantity", true),
                LineType = LineType.Item,
                ItemId = SQLHelper.GetString(reader["Item No_"]),
                NetPrice = SQLHelper.GetDecimal(reader, "Net Price"),
                Price = SQLHelper.GetDecimal(reader, "Price"),
                DiscountAmount = SQLHelper.GetDecimal(reader, "Discount Amount", false),
                NetAmount = SQLHelper.GetDecimal(reader, "Net Amount", true),
                TaxAmount = SQLHelper.GetDecimal(reader, "VAT Amount", true),
                ItemDescription = SQLHelper.GetString(reader["Description"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                StoreName = SQLHelper.GetString(reader["Name"])
            };

            uomqty = SQLHelper.GetDecimal(reader["UOM Quantity"], true);
            if (uomqty != 0)
                line.Quantity = uomqty;

            line.Amount = line.NetAmount + line.TaxAmount;
            if (line.ParentLine == 0)
                line.ParentLine = SQLHelper.GetInt32(reader["Deal Header Line No_"]);

            if (string.IsNullOrEmpty(line.VariantId) == false)
            {
                line.VariantDescription = SQLHelper.GetString(reader["Variant Dimension 1"]);
                string vartxt = SQLHelper.GetString(reader["Variant Dimension 2"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
                vartxt = SQLHelper.GetString(reader["Variant Dimension 3"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
                vartxt = SQLHelper.GetString(reader["Variant Dimension 4"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
                vartxt = SQLHelper.GetString(reader["Variant Dimension 5"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
            }

            ImageRepository imgrep = new ImageRepository(config, NavVersion);
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

            if (SQLHelper.GetInt32(reader["Deal Modifier Line No_"]) > 0)
            {
                transNo = SQLHelper.GetInt32(reader["Transaction No_"]);
                termNo = SQLHelper.GetString(reader["POS Terminal No_"]);
            }
            return line;
        }

        private SalesEntryLine ReaderToSalesDealLine(SqlDataReader reader)
        {
            return (new SalesEntryLine()
            {
                LineNumber = SQLHelper.GetInt32(reader["Deal Header Line No_"]),
                Quantity = SQLHelper.GetDecimal(reader, "Quantity", true),
                LineType = LineType.Deal,
                ItemId = SQLHelper.GetString(reader["Deal No_"]),
                Price = SQLHelper.GetDecimal(reader, "Price"),
                DiscountAmount = SQLHelper.GetDecimal(reader, "Line Discount Amt_", false),
                Amount = SQLHelper.GetDecimal(reader, "Amount", true),
                ItemDescription = SQLHelper.GetString(reader["Description"]),
            });
        }

        private SalesEntryLine POSTransToSalesEntryLine(SqlDataReader reader)
        {
            SalesEntryLine line = new SalesEntryLine()
            {
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                ParentLine = SQLHelper.GetInt32(reader["Parent Line"]),
                VariantId = SQLHelper.GetString(reader["Variant Code"]),
                UomId = SQLHelper.GetString(reader["Unit of Measure"]),
                Quantity = SQLHelper.GetDecimal(reader, "Quantity", false),
                LineType = (SQLHelper.GetInt32(reader["Entry Type"]) == 5) ? LineType.Deal :  LineType.Item,
                ItemId = SQLHelper.GetString(reader["Number"]),
                NetPrice = SQLHelper.GetDecimal(reader, "Net Price"),
                Price = SQLHelper.GetDecimal(reader, "Price"),
                DiscountAmount = SQLHelper.GetDecimal(reader, "Discount Amount", false),
                NetAmount = SQLHelper.GetDecimal(reader, "Net Amount", false),
                TaxAmount = SQLHelper.GetDecimal(reader, "VAT Amount", false),
                ItemDescription = SQLHelper.GetString(reader["Description"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                StoreName = SQLHelper.GetString(reader["Name"])
            };

            if (line.LineType == LineType.Deal)
                line.ItemId = SQLHelper.GetString(reader["Promotion No_"]);

            if (string.IsNullOrEmpty(line.VariantId) == false)
            {
                line.VariantDescription = SQLHelper.GetString(reader["Variant Dimension 1"]);
                string vartxt = SQLHelper.GetString(reader["Variant Dimension 2"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
                vartxt = SQLHelper.GetString(reader["Variant Dimension 3"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
                vartxt = SQLHelper.GetString(reader["Variant Dimension 4"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
                vartxt = SQLHelper.GetString(reader["Variant Dimension 5"]);
                if (string.IsNullOrEmpty(vartxt) == false)
                    line.VariantDescription += "/" + vartxt;
            }

            ImageRepository imgrep = new ImageRepository(config, NavVersion);
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

            line.Amount = line.NetAmount + line.TaxAmount;
            return line;
        }

        private SalesEntryPayment POSPaymentToSalesEntryPayment(SqlDataReader reader)
        {
            return new SalesEntryPayment()
            {
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                Amount = SQLHelper.GetDecimal(reader, "Amount", false),
                CurrencyFactor = SQLHelper.GetDecimal(reader, "CurrencyFactor", false),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"]),
                TenderType = SQLHelper.GetString(reader["Number"]),
                CardNo = SQLHelper.GetString(reader["Card Number"])
            };
        }

        private SalesEntryDiscountLine ReaderToDiscountLine(SqlDataReader reader)
        {
            SalesEntryDiscountLine line = new SalesEntryDiscountLine()
            {
                DiscountAmount = SQLHelper.GetDecimal(reader, "Discount Amount"),
                Description = SQLHelper.GetString(reader["Description"]),
                OfferNumber = SQLHelper.GetString(reader["Offer No_"]),
                PeriodicDiscGroup = SQLHelper.GetString(reader["Offer No_"]),
                DiscountType = DiscountType.PeriodicDisc,
                LineNumber = SQLHelper.GetInt32(reader["Line No_"])
            };
            line.SetDiscType((OfferDiscountType)SQLHelper.GetInt32(reader["Offer Type"]));
            return line;
        }

        private SalesEntryPayment TransToSalesEntryPayment(SqlDataReader reader)
        {
            return new SalesEntryPayment()
            {
                LineNumber = ConvertTo.SafeInt(SQLHelper.GetString(reader["Line No_"])),
                TenderType = SQLHelper.GetString(reader["Tender Type"]),
                Type = PaymentType.Payment,
                Amount = SQLHelper.GetDecimal(reader, "Amt", false),
                CurrencyFactor = SQLHelper.GetDecimal(reader, "Rate", false),
                CardNo = SQLHelper.GetString(reader["No"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"])
            };
        }

        public string FormatAmountToString(decimal amount, string culture)
        {
            return FormatAmount(amount, culture);
        }
    }
}
