using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class SalesEntryRepository : BaseRepository
    {
        private string documentId = string.Empty;
        private string externalId = string.Empty;
        private string externalIdString = string.Empty;
        private string cac = string.Empty;
        private string cac2 = string.Empty;
        private string shipto = string.Empty;
        private string shiptoPost = string.Empty;

        public SalesEntryRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            documentId = (navVersion > new Version("13.5")) ? "Document ID" : "Document Id";
            externalId = (navVersion > new Version("13.5")) ? "External ID" : "Web Trans_ GUID";
            externalIdString = (navVersion > new Version("13.5")) ? "''" : "NULL";
            cac = (navVersion > new Version("13.5")) ? "Click and Collect Order" : "ClickAndCollectOrder";
            cac2 = (navVersion > new Version("13.5")) ? "Click and Collect Order" : "Click And Collect Order";
            shipto = (navVersion > new Version("13.5")) ? "Ship-to " : "Ship To ";
            shiptoPost = (navVersion > new Version("13.5")) ? "Ship-to Full Name" : "ShipToFullName";
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
                        sqlwhere += "AND [Store No_]=@sid ";
                        command.Parameters.AddWithValue("@sid", storeId);
                    }
                    if (date > DateTime.MinValue)
                    {
                        sqlwhere += string.Format("AND [Date]{0}@dt ", (dateGreaterThan) ? ">" : "<");
                        command.Parameters.AddWithValue("@dt", date);
                    }

                    command.CommandText = "SELECT " + (maxNrOfEntries > 0 ? "TOP " + maxNrOfEntries : "") + " * FROM ";
                    if (NavVersion > new Version("14.2"))
                    {
                        command.CommandText += "(" +
                            "SELECT mt.[Customer Order ID] AS [Document ID],mt.[Store No_],(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date]," +
                            "co.[External ID],mt.[Member Card No_],1 AS [Posted],mt.[Receipt No_],mt.[Customer Order] AS [CAC]," +
                            "co.[Name],co.[Address],co.[Address 2],co.[City],co.[County],co.[Post Code],co.[Country_Region Code],co.[Phone No_],co.[Email],co.[House_Apartment No_],co.[Mobile Phone No_],co.[Daytime Phone No_]," +
                            "co.[Ship-to Name],co.[Ship-to Address],co.[Ship-to Address 2],co.[Ship-to City],co.[Ship-to County],co.[Ship-to Post Code],co.[Ship-to Country_Region Code],co.[Ship-to Phone No_],co.[Ship-to Email],co.[Ship-to House_Apartment No_]," +
                            "mt.[No_ of Items] AS [Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount],st.[Name] AS [StName],mt.[POS Terminal No_],mt.[Transaction No_] " +
                            "FROM [" + navCompanyName + "Transaction Header] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                            "LEFT JOIN [" + navCompanyName + "Posted Customer Order Header] co ON co.[Document ID]=mt.[Customer Order ID] " +
                            "UNION " +
                            "SELECT mt.[Document ID],mt.[Store No_],mt.[Created] AS [Date]," +
                            "mt.[External ID],mt.[Member Card No_],0 AS [Posted],'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                            "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                            "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                            "0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],'' AS [POS Terminal No_],'' AS [Transaction No_] " +
                            "FROM [" + navCompanyName + "Customer Order Header] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]= mt.[Store No_] " +
                            "UNION " +
                            "SELECT mt.[Document ID],mt.[Store No_],mt.[Created] AS [Date]," +
                            "mt.[External ID],mt.[Member Card No_],3 AS [Posted],'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                            "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                            "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                            "0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],'' AS [POS Terminal No_],'' AS [Transaction No_] " +
                            "FROM [" + navCompanyName + "Posted Customer Order Header] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] WHERE mt.CancelledOrder=1 " +
                            ") AS SalesEntries " +
                            "WHERE [Member Card No_]=@id " + sqlwhere +
                            "ORDER BY [Date] DESC";
                    }
                    else
                    {
                        command.CommandText += "(" +
                            "SELECT mt.[Document No_] AS [Document ID],MAX(mt.[Store No_]) AS [Store No_],MAX(mt.[Date]) AS [Date]," +
                            "MAX(mt.[Member Card No_]) AS [Member Card No_],1 AS [Posted],0 AS [PayCount]," +
                            externalIdString + " AS [External ID],1 AS [CAC],'' AS [Ship-to Name]," +
                            "MAX(mt.[Transaction No_]) AS [Transaction No_],SUM(mt.[Quantity]) AS [Quantity]," +
                            "SUM(mt.[Net Amount]) AS [Net Amount],SUM(mt.[Gross Amount]) AS [Gross Amount],SUM(mt.[Discount Amount]) AS [Discount Amount]," +
                            "MAX(st.[Name]) AS [StName],MAX(mt.[POS Terminal No_]) AS [POS Terminal No_],1 AS [Transaction] " +
                            "FROM [" + navCompanyName + "Member Sales Entry] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                            "WHERE [Transaction No_]!=0 GROUP BY [Document No_] " +
                            "UNION " +
                            "SELECT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_], 0 AS Posted," +
                            "(SELECT COUNT(*) FROM [" + navCompanyName + "Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS PayCount," +
                            "mt.[" + externalId + "],mt.[" + cac2 + "] AS [CAC],mt.[" + shipto + "Full Name] AS [Ship-to Name]," +
                            "'' AS [Transaction No_],0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                            "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [StName],'' AS [POS Terminal No_],0 AS [Transaction] " +
                            "FROM [" + navCompanyName + "Customer Order Header] mt " +
                            "UNION " +
                            "SELECT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_],0 AS Posted," +
                            "(SELECT COUNT(*) FROM [" + navCompanyName + "Posted Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS PayCount," +
                            "mt.[" + externalId + "],mt.[" + cac + "] AS [CAC],mt.[" + shiptoPost + "] AS [Ship-to Name]," +
                            "'' AS [Transaction No_],0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                            "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [StName],'' AS [POS Terminal No_],0 AS [Transaction] " +
                            "FROM [" + navCompanyName + "Posted Customer Order Header] mt " +
                            "WHERE (SELECT COUNT(*) FROM [" + navCompanyName + "Member Sales Entry] se WHERE se.[Document No_]=mt.[Receipt No_])=0" +
                            ") AS SalesEntries WHERE [Member Card No_]=@id " + sqlwhere +
                            "ORDER BY [Date] DESC";
                    }

                    command.Parameters.AddWithValue("@id", cardId);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int trans = 0;
                        while (reader.Read())
                        {
                            trans = SQLHelper.GetInt32(reader["Posted"]);
                            if (trans == 1)
                                list.Add(TransactionToSalesEntry(reader, false));
                            else
                                list.Add(OrderToSalesEntry(reader, trans));
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
                    if (NavVersion > new Version("14.2"))
                    {
                        command.CommandText = "SELECT mt.[Member Card No_],mt.[No_ of Items] AS [Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount]," +
                            "(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date],mt.[Store No_],st.[Name] AS [StName],mt.[POS Terminal No_],mt.[Transaction No_]," +
                            "co.[External ID] AS [External ID],co.[Ship-to Name],mt.[Receipt No_],mt.[Customer Order ID] AS [Document ID],mt.[Customer Order] AS [CAC] " +
                            "FROM [" + navCompanyName + "Transaction Header] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                            "LEFT JOIN [" + navCompanyName + "Posted Customer Order Header] co ON co.[Document ID]=mt.[Customer Order ID] " +
                            "WHERE [Receipt No_]=@id";
                    }
                    else
                    {
                        command.CommandText = "SELECT mt.[Transaction No_],MAX(mt.[Member Card No_]) AS [Member Card No_],SUM(mt.[Quantity]) AS [Quantity],1 AS [CAC]," +
                            "SUM(mt.[Net Amount]) AS [Net Amount],SUM(mt.[Gross Amount]) AS[Gross Amount],SUM(mt.[Discount Amount]) AS[Discount Amount],MAX(mt.[Date]) AS [Date]," +
                            "MAX(mt.[Store No_]) AS [Store No_], MAX(st.[Name]) AS [StName],MAX(mt.[POS Terminal No_]) AS[POS Terminal No_],MAX(mt.[Document No_]) AS [Document ID] " +
                            "FROM [" + navCompanyName + "Member Sales Entry] mt " +
                            "JOIN [" + navCompanyName + "Store] st on st.[No_]=mt.[Store No_] " +
                            "WHERE [Document No_]=@id " +
                            "GROUP BY [Transaction No_]";
                    }

                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entry = TransactionToSalesEntry(reader, true);
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

            SQLHelper.CheckForSQLInjection(search);

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string searchWords = string.Empty;
            foreach (string si in searchitems)
            {
                searchWords += string.Format("%{0}", si);
            }
            searchWords += "%";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + ((maxNumberOfTransactions > 0) ? "TOP " + maxNumberOfTransactions + " * FROM " : " * FROM ");
                    if (NavVersion > new Version("14.2"))
                    {
                        command.CommandText += "(" +
                            "SELECT DISTINCT mt.[Customer Order ID] AS [Document ID],mt.[Store No_],(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date]," +
                            "co.[External ID],mt.[Member Card No_],1 AS [Posted],mt.[Receipt No_],mt.[Customer Order] AS [CAC]," +
                            "co.[Name],co.[Address],co.[Address 2],co.[City],co.[County],co.[Post Code],co.[Country_Region Code],co.[Phone No_],co.[Email],co.[House_Apartment No_],co.[Mobile Phone No_],co.[Daytime Phone No_]," +
                            "co.[Ship-to Name],co.[Ship-to Address],co.[Ship-to Address 2],co.[Ship-to City],co.[Ship-to County],co.[Ship-to Post Code],co.[Ship-to Country_Region Code],co.[Ship-to Phone No_],co.[Ship-to Email],co.[Ship-to House_Apartment No_]," +
                            "mt.[No_ of Items] AS [Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount],st.[Name] AS [StName],mt.[POS Terminal No_],mt.[Transaction No_] " +
                            "FROM [" + navCompanyName + "Transaction Header] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                            "JOIN [" + navCompanyName + "Trans_ Sales Entry] tl ON tl.[Receipt No_]=mt.[Receipt No_] " +
                            "JOIN [" + navCompanyName + "Item] i ON i.[No_]=tl.[Item No_]" +
                            "LEFT JOIN [" + navCompanyName + "Posted Customer Order Header] co ON co.[Document ID]=mt.[Customer Order ID] " +
                            "WHERE UPPER(i.[Description]) LIKE UPPER(@search) " +
                            "UNION " +
                            "SELECT DISTINCT mt.[Document ID],mt.[Store No_],mt.[Created] AS [Date]," +
                            "mt.[External ID],mt.[Member Card No_],0 AS [Posted],'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC]," +
                            "mt.[Name],mt.[Address],mt.[Address 2],mt.[City],mt.[County],mt.[Post Code],mt.[Country_Region Code],mt.[Phone No_],mt.[Email],mt.[House_Apartment No_],mt.[Mobile Phone No_],mt.[Daytime Phone No_]," +
                            "mt.[Ship-to Name],mt.[Ship-to Address],mt.[Ship-to Address 2],mt.[Ship-to City],mt.[Ship-to County],mt.[Ship-to Post Code],mt.[Ship-to Country_Region Code],mt.[Ship-to Phone No_],mt.[Ship-to Email],mt.[Ship-to House_Apartment No_]," +
                            "0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name] AS [StName],'' AS [POS Terminal No_],'' AS [Transaction No_] " +
                            "FROM [" + navCompanyName + "Customer Order Header] mt " +
                            "JOIN [" + navCompanyName + "Customer Order Line] ol ON ol.[Document ID]=mt.[Document ID] " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]= mt.[Store No_] " +
                            "WHERE UPPER(ol.[Item Description]) LIKE UPPER(@search) " +
                            ") AS SalesEntries " +
                            "WHERE [Member Card No_]=@id " +
                            "ORDER BY [Date] DESC";
                    }
                    else
                    {
                        command.CommandText += "(" +
                            "SELECT DISTINCT mt.[Document No_] AS [Document ID],MAX(mt.[Store No_]) AS [Store No_],MAX(mt.[Date]) AS [Date]," +
                            "MAX(mt.[Member Card No_]) AS [Member Card No_],1 AS [Posted],0 AS [PayCount]," +
                            externalIdString + " AS [External ID],1 AS [CAC],'' AS [Ship-to Name]," +
                            "MAX(mt.[Transaction No_]) AS [Transaction No_],SUM(mt.[Quantity]) AS [Quantity]," +
                            "SUM(mt.[Net Amount]) AS [Net Amount],SUM(mt.[Gross Amount]) AS [Gross Amount],SUM(mt.[Discount Amount]) AS [Discount Amount]," +
                            "MAX(st.[Name]) AS [StName],MAX(mt.[POS Terminal No_]) AS [POS Terminal No_],1 AS [Transaction] " +
                            "FROM [" + navCompanyName + "Member Sales Entry] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                            "JOIN [" + navCompanyName + "Item] i ON mt.[Item No_]=i.[No_] " +
                            "WHERE [Transaction No_]!=0 AND UPPER(i.Description) LIKE UPPER(@search) GROUP BY [Document No_] " +
                            "UNION " +
                            "SELECT DISTINCT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_], 0 AS [Posted]," +
                            "(SELECT COUNT(*) FROM [" + navCompanyName + "Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS [PayCount]," +
                            "mt.[" + externalId + "],mt.[" + cac2 + "] AS [CAC],mt.[" + shipto + "Full Name] AS [Ship-to Name],'' AS [Transaction No_],0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                            "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [StName],'' AS [POS Terminal No_],0 AS [Transaction] " +
                            "FROM [" + navCompanyName + "Customer Order Header] mt " +
                            "JOIN [" + navCompanyName + "Customer Order Line] ol ON ol.[" + documentId + "]=mt.[" + documentId + "] " +
                            "WHERE UPPER([Item Description]) LIKE UPPER(@search) " +
                            "UNION " +
                            "SELECT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_],1 AS [Posted]," +
                            "(SELECT COUNT(*) FROM [" + navCompanyName + "Posted Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS [PayCount]," +
                            "mt.[" + externalId + "],mt.[" + cac + "] AS [CAC],mt.[" + shiptoPost + "] AS [Ship-to Name],'' AS [Transaction No_], 0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                            "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [StName],'' AS [POS Terminal No_], 0 AS [Transaction] " +
                            "FROM [" + navCompanyName + "Posted Customer Order Header] mt " +
                            "JOIN [" + navCompanyName + "Posted Customer Order Line] ol ON ol.[" + documentId + "]=mt.[" + documentId + "] " +
                            "WHERE (SELECT COUNT(*) FROM [" + navCompanyName + "Member Sales Entry] se WHERE se.[Document No_]=mt.[Receipt No_])=0 " +
                            "AND UPPER([Item Description]) LIKE UPPER(@search) " +
                            ") AS SalesEntries WHERE [Member Card No_]=@id ORDER BY [Date] DESC";
                    }

                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@search", searchWords);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int trans = 0;
                        while (reader.Read())
                        {
                            if (NavVersion > new Version("14.2"))
                                trans = (SQLHelper.GetInt32(reader["Transaction No_"]) > 0) ? 1 : 0;
                            else
                                trans = SQLHelper.GetInt32(reader["Transaction"]);

                            if (trans == 1)
                                list.Add(TransactionToSalesEntry(reader, false));
                            else
                                list.Add(OrderToSalesEntry(reader, trans));
                        }
                    }
                    connection.Close();
                }
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
                string select = "SELECT [" + documentId + "],SUM([Quantity]) AS Cnt,SUM([Discount Amount]) AS Disc,SUM([Net Amount]) AS NAmt,SUM([Amount]) AS Amt";

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM (" + select +
                                            " FROM [" + navCompanyName + "Customer Order Line] GROUP BY [" + documentId + "] " +
                                            "UNION " + select +
                                            " FROM [" + navCompanyName + "Posted Customer Order Line] GROUP BY [" + documentId + "] " +
                                            ") AS OrderTotals WHERE [" + documentId + "]=@id";
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

        public List<SalesEntryLine> TransSalesEntryLinesGet(string receiptId)
        {
            List<SalesEntryLine> list = new List<SalesEntryLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                "ml.[Transaction No_],ml.[Store No_],ml.[POS Terminal No_],ml.[Item No_],ml.[Variant Code],ml.[Unit of Measure]," +
                                "ml.[Quantity],ml.[Price],ml.[Net Price],ml.[Net Amount],ml.[Discount Amount],ml.[VAT Amount],ml.[Refund Qty_],ml.[Line No_],i.[Description]," +
                                "v.[Variant Dimension 1],v.[Variant Dimension 2],v.[Variant Dimension 3],v.[Variant Dimension 4],v.[Variant Dimension 5]" +
                                " FROM [" + navCompanyName + "Trans_ Sales Entry] ml" +
                                " JOIN [" + navCompanyName + "Item] i ON i.[No_]=ml.[Item No_]" +
                                " LEFT JOIN [" + navCompanyName + "Item Variant Registration] v ON v.[Item No_]=ml.[Item No_] AND v.[Variant]=ml.[Variant Code]" +
                                " WHERE ml.[Receipt No_]=@id ";

                    command.Parameters.AddWithValue("@id", receiptId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(TransToSalesEntryLine(reader));
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
                                          "FROM [" + navCompanyName + "Trans_ Discount Entry] ml " +
                                          "LEFT JOIN [" + navCompanyName + "Periodic Discount] pd ON pd.[No_]=ml.[Offer No_] " +
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
                    if (string.IsNullOrEmpty(custOrderNo))
                    {
                        command.CommandText = "SELECT " +
                                    "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Amount in Currency] AS Amt,ml.[Exchange Rate] AS Rate,ml.[Card or Account] AS No " +
                                    "FROM [" + navCompanyName + "Trans_ Payment Entry] ml " +
                                    "WHERE ml.[Receipt No_]=@id ORDER BY ml.[Line No_]";
                        command.Parameters.AddWithValue("@id", receiptNo);
                    }
                    else
                    {
                        if (NavVersion > new Version("14.2"))
                        {
                            command.CommandText = "SELECT " +
                                    "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Pre Approved Amount] AS Amt,ml.[Currency Factor] AS Rate,ml.[Card or Customer No_] AS No " +
                                    "FROM [" + navCompanyName + "Posted Customer Order Payment] ml " +
                                    "WHERE ml.[" + documentId + "]=@id AND [Type]=4 ORDER BY ml.[Line No_]";
                        }
                        else
                        {
                            command.CommandText = "SELECT " +
                                        "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Pre Approved Amount] AS Amt,ml.[Currency Factor] AS Rate," +
                                        ((NavVersion < new Version("13.6")) ? "ml.[Card or Customer number] AS No " : "ml.[Card or Customer No_] AS No ") +
                                        "FROM [" + navCompanyName + "Posted Customer Order Payment] ml " +
                                        "WHERE ml.[" + documentId + "]=@id ORDER BY ml.[Line No_]";
                        }
                        command.Parameters.AddWithValue("@id", custOrderNo);
                    }

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
                        if (NavVersion > new Version("14.2"))
                        {
                            command.CommandText = "SELECT " +
                                    "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Pre Approved Amount] AS Amt,ml.[Currency Factor] AS Rate,ml.[Card or Customer No_] AS No " +
                                    "FROM [" + navCompanyName + "Posted Customer Order Payment] ml " +
                                    "WHERE ml.[" + documentId + "]=@id AND [Type]=1 ORDER BY ml.[Line No_]";
                        }
                        else
                        {
                            command.CommandText = "SELECT " +
                                    "ml.[Line No_],ml.[Tender Type],ml.[Currency Code],ml.[Pre Approved Amount] AS Amt,ml.[Currency Factor] AS Rate," +
                                    ((NavVersion < new Version("13.6")) ? "ml.[Card or Customer number] AS No " : "ml.[Card or Customer No_] AS No ") +
                                    "FROM [" + navCompanyName + "Posted Customer Order Payment] ml " +
                                    "WHERE ml.[" + documentId + "]=@id ORDER BY ml.[Line No_]";
                        }
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
                        string orderid = (NavVersion > new Version("14.2")) ? "Customer Order ID" : "External Document No_";
                        command.CommandText = "SELECT [No_] FROM [" + navCompanyName + "Sales Invoice Header] WHERE [" + orderid + "]=@id";
                        command.Parameters.AddWithValue("@id", custId);
                        TraceSqlCommand(command);
                        string salesId = (string)command.ExecuteScalar();

                        //Use sales invoice id to get Rewarded points for customer orders
                        if (!string.IsNullOrEmpty(salesId))
                            entryId = salesId;
                    }

                    //Get Used points with entryId (receiptId/orderId)
                    command.CommandText = "SELECT [Points] FROM [" + navCompanyName + "Member Point Entry] WHERE [Document No_]=@id AND [Entry Type]=1"; //Entry type = 1 is redemption
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    var res = command.ExecuteScalar();
                    used = res == null ? 0 : -(decimal)res; //use '-' to convert to positive number

                    //Get rewarded points
                    command.CommandText = "SELECT [Points] FROM [" + navCompanyName + "Member Point Entry] WHERE [Document No_]=@id AND [Entry Type]=0";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    res = command.ExecuteScalar();
                    rewarded = res == null ? 0 : (decimal)res;
                }
                connection.Close();
            }

            logger.Debug(config.LSKey.Key, "Get Point Ballance: ReceiptNo:{0} CustOrderNo:{1} PointRew:{2} PointUse:{3}", 
                entryId, custId, rewarded, used);
        }

        private SalesEntry TransactionToSalesEntry(SqlDataReader reader, bool includeLines)
        {
            SalesEntry entry = new SalesEntry()
            {
                CustomerOrderNo = SQLHelper.GetString(reader["Document ID"]),
                LineItemCount = (int)SQLHelper.GetDecimal(reader, "Quantity", false),
                TotalNetAmount = SQLHelper.GetDecimal(reader, "Net Amount", true),
                TotalAmount = SQLHelper.GetDecimal(reader, "Gross Amount", true),
                TotalDiscount = SQLHelper.GetDecimal(reader, "Discount Amount", false),
                DocumentRegTime = SQLHelper.GetDateTime(reader["Date"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"]),
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                StoreName = SQLHelper.GetString(reader["StName"]),
                AnonymousOrder = false,
                Posted = true,
                Status = SalesEntryStatus.Complete,
                IdType = DocumentIdType.Receipt
            };

            if (NavVersion > new Version("14.2"))
            {
                entry.Id = SQLHelper.GetString(reader["Receipt No_"]);
                entry.ExternalId = SQLHelper.GetString(reader["External ID"]);
                entry.ShipToName = SQLHelper.GetString(reader["Ship-to Name"]);
            }
            else
            {
                entry.Id = entry.CustomerOrderNo;
            }

            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);

            SalesEntryPointsGetTotal(entry.Id, entry.CustomerOrderNo, out decimal rewarded, out decimal used);
            entry.PointsRewarded = rewarded;
            entry.PointsUsedInOrder = used;

            if (includeLines)
            {
                entry.Lines = TransSalesEntryLinesGet(entry.Id);
                entry.DiscountLines = DiscountLineGet(entry.Id);
                entry.Payments = TransSalesEntryPaymentGet(entry.Id, entry.CustomerOrderNo);
            }
            return entry;
        }

        private SalesEntry OrderToSalesEntry(SqlDataReader reader, int posted)
        {
            SalesEntry entry;
            if (NavVersion > new Version("14.2"))
            {
                entry = new SalesEntry
                {
                    Id = SQLHelper.GetString(reader["Document ID"]),
                    IdType = DocumentIdType.Order,
                    Status = (posted == 3) ? SalesEntryStatus.Canceled : SalesEntryStatus.Created,
                    Posted = false,
                    StoreId = SQLHelper.GetString(reader["Store No_"]),
                    DocumentRegTime = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson),
                    CardId = SQLHelper.GetString(reader["Member Card No_"]),
                    StoreName = SQLHelper.GetString(reader["StName"]),
                    ExternalId = SQLHelper.GetString(reader["External ID"]),
                    ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"]),

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
            }
            else
            {
                entry = new SalesEntry
                {
                    Id = SQLHelper.GetString(reader["Document ID"]),
                    StoreId = SQLHelper.GetString(reader["Store No_"]),
                    DocumentRegTime = SQLHelper.GetDateTime(reader["Date"]),
                    IdType = DocumentIdType.Order,
                    CardId = SQLHelper.GetString(reader["Member Card No_"]),
                    Status = SalesEntryStatus.Created,
                    StoreName = SQLHelper.GetString(reader["StName"]),
                    ShipToName = SQLHelper.GetString(reader["Ship-to Name"]),
                    ExternalId = SQLHelper.GetString(reader["External ID"]),
                    Posted = SQLHelper.GetBool(reader["Posted"]),
                    ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"])
                };
            }

            entry.CustomerOrderNo = entry.Id;
            entry.AnonymousOrder = string.IsNullOrEmpty(entry.CardId);
            entry.ShippingStatus = (entry.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;

            OrderLinesGetTotals(entry.Id, out int cnt, out decimal amt, out decimal namt, out decimal disc);
            entry.LineItemCount = cnt;
            entry.TotalAmount = amt;
            entry.TotalNetAmount = namt;
            entry.TotalDiscount = disc;

            if (entry.Posted)
            {
                entry.Status = SalesEntryStatus.Complete;
                SalesEntryPointsGetTotal(entry.Id, entry.CustomerOrderNo, out decimal rewarded, out decimal used);
                entry.PointsRewarded = rewarded;
                entry.PointsUsedInOrder = used;
            }
            return entry;
        }

        private SalesEntryLine TransToSalesEntryLine(SqlDataReader reader)
        {
            SalesEntryLine line = new SalesEntryLine()
            {
                LineNumber = Convert.ToInt32(SQLHelper.GetInt32(reader["Line No_"])),
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
            };

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

            ImageRepository imgrep = new ImageRepository(config);
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
