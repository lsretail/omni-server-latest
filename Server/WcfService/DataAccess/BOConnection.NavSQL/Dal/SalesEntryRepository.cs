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
        private string sql = string.Empty;
        private string sqlSearch = string.Empty;
        private string documentId = string.Empty;

        public SalesEntryRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            documentId = (navVersion > new Version("13.5")) ? "Document ID" : "Document Id";
            string externalId = (navVersion > new Version("13.5")) ? "External ID" : "Web Trans_ GUID";
            string externalIdString = (navVersion > new Version("13.5")) ? "''" : "NULL";
            string cac = (navVersion > new Version("13.5")) ? "Click and Collect Order" : "ClickAndCollectOrder";
            string cac2 = (navVersion > new Version("13.5")) ? "Click and Collect Order" : "Click And Collect Order";
            string shipto = (navVersion > new Version("13.5")) ? "Ship-to " : "Ship To ";
            string shiptoPost = (navVersion > new Version("13.5")) ? "Ship-to Full Name" : "ShipToFullName";

            if (navVersion > new Version("14.2"))
            {
                sql = "(" +
                    "SELECT mt.[Customer Order ID] AS [Document ID],mt.[Store No_],(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date]," +
                    "co.[External ID] AS [External ID],mt.[Member Card No_],1 AS [Posted],mt.[Receipt No_],mt.[Customer Order] AS [CAC],co.[Ship-to Name] AS [ShipName]," +
                    "mt.[No_ of Items] AS [Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount],st.[Name],mt.[POS Terminal No_],mt.[Transaction No_] " +
                    "FROM [" + navCompanyName + "Transaction Header] mt " +
                    "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                    "LEFT OUTER JOIN [" + navCompanyName + "Posted Customer Order Header] co ON co.[Document ID]=mt.[Customer Order ID] " +
                    "UNION " +
                    "SELECT mt.[Document ID] AS [Document ID],mt.[Store No_],mt.[Created] AS [Date]," +
                    "mt.[External ID],mt.[Member Card No_],0 AS Posted,'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC],mt.[Ship-to Name] AS [ShipName]," +
                    "0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name],'' AS [POS Terminal No_],'' AS [Transaction No_] " +
                    "FROM [" + navCompanyName + "Customer Order Header] mt " +
                    "JOIN [" + navCompanyName + "Store] st ON st.[No_]= mt.[Store No_] " +
                    ") AS SalesEntries " +
                    "WHERE [Member Card No_]=@id " +
                    "ORDER BY [Date] DESC";

                sqlSearch = "(" +
                    "SELECT DISTINCT mt.[Customer Order ID] AS [Document ID],mt.[Store No_],(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date]," +
                    "co.[External ID] AS [External ID],mt.[Member Card No_],1 AS [Posted],mt.[Receipt No_],mt.[Customer Order] AS [CAC],co.[Ship-to Name] AS [ShipName]," +
                    "mt.[No_ of Items] AS [Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount],st.[Name],mt.[POS Terminal No_],mt.[Transaction No_] " +
                    "FROM [" + navCompanyName + "Transaction Header] mt " +
                    "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                    "INNER JOIN [" + navCompanyName + "Trans_ Sales Entry] tl ON tl.[Receipt No_]=mt.[Receipt No_] " +
                    "INNER JOIN [" + navCompanyName + "Item] i ON i.[No_]=tl.[Item No_]" +
                    "LEFT OUTER JOIN [" + navCompanyName + "Posted Customer Order Header] co ON co.[Document ID]=mt.[Customer Order ID] " +
                    "WHERE UPPER(i.[Description]) LIKE UPPER(@search) " +
                    "UNION " +
                    "SELECT DISTINCT mt.[Document ID] AS [Document ID],mt.[Store No_],mt.[Created] AS [Date]," +
                    "mt.[External ID],mt.[Member Card No_],0 AS Posted,'' AS [Receipt No_],mt.[Click and Collect Order] AS [CAC],mt.[Ship-to Name] AS [ShipName]," +
                    "0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount],st.[Name],'' AS [POS Terminal No_],'' AS [Transaction No_] " +
                    "FROM [" + navCompanyName + "Customer Order Header] mt " +
                    "INNER JOIN [" + navCompanyName + "Customer Order Line] ol ON ol.[Document ID]=mt.[Document ID] " +
                    "JOIN [" + navCompanyName + "Store] st ON st.[No_]= mt.[Store No_] " +
                    "WHERE UPPER(ol.[Item Description]) LIKE UPPER(@search) " +
                    ") AS SalesEntries " +
                    "WHERE [Member Card No_]=@id " +
                    "ORDER BY [Date] DESC";
            }
            else
            {
                sql = "(" +
                    "SELECT mt.[Document No_] AS [Document ID],MAX(mt.[Store No_]) AS [Store No_],MAX(mt.[Date]) AS [Date]," +
                    "MAX(mt.[Member Card No_]) AS [Member Card No_],1 AS [Posted],0 AS [PayCount]," +
                    externalIdString + " AS [External ID],1 AS [CAC],'' AS [ShipName]," +
                    "MAX(mt.[Transaction No_]) AS [Transaction No_],SUM(mt.[Quantity]) AS [Quantity]," +
                    "SUM(mt.[Net Amount]) AS [Net Amount],SUM(mt.[Gross Amount]) AS [Gross Amount],SUM(mt.[Discount Amount]) AS [Discount Amount]," +
                    "MAX(st.[Name]) AS [Name],MAX(mt.[POS Terminal No_]) AS [POS Terminal No_],1 AS [Transaction] " +
                    "FROM [" + navCompanyName + "Member Sales Entry] mt " +
                    "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                    "WHERE [Transaction No_]!=0 GROUP BY [Document No_] " +
                    "UNION " +
                    "SELECT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_], 0 AS Posted," +
                    "(SELECT COUNT(*) FROM [" + navCompanyName + "Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS PayCount," +
                    "mt.[" + externalId + "],mt.[" + cac2 + "] AS [CAC],mt.[" + shipto + "Full Name] AS ShipName," +
                    "'' AS [Transaction No_],0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                    "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [Name],'' AS [POS Terminal No_],0 AS [Transaction] " +
                    "FROM [" + navCompanyName + "Customer Order Header] mt " +
                    "UNION " +
                    "SELECT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_],1 AS Posted," +
                    "(SELECT COUNT(*) FROM [" + navCompanyName + "Posted Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS PayCount," +
                    "mt.[" + externalId + "],mt.[" + cac + "] AS [CAC],mt.[" + shiptoPost + "] AS ShipName," +
                    "'' AS [Transaction No_],0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                    "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [Name],'' AS [POS Terminal No_],0 AS [Transaction] " +
                    "FROM [" + navCompanyName + "Posted Customer Order Header] mt " +
                    "WHERE (SELECT COUNT(*) FROM [" + navCompanyName + "Member Sales Entry] se WHERE se.[Document No_]=mt.[Receipt No_])=0" +
                    ") AS SalesEntries WHERE [Member Card No_]=@id ORDER BY [Date] DESC";

                sqlSearch = "(" +
                    "SELECT DISTINCT mt.[Document No_] AS [Document ID],MAX(mt.[Store No_]) AS [Store No_],MAX(mt.[Date]) AS [Date]," +
                    "MAX(mt.[Member Card No_]) AS [Member Card No_],1 AS [Posted],0 AS [PayCount]," +
                    externalIdString + " AS [External ID],1 AS [CAC],'' AS [ShipName]," +
                    "MAX(mt.[Transaction No_]) AS [Transaction No_],SUM(mt.[Quantity]) AS [Quantity]," +
                    "SUM(mt.[Net Amount]) AS [Net Amount],SUM(mt.[Gross Amount]) AS [Gross Amount],SUM(mt.[Discount Amount]) AS [Discount Amount]," +
                    "MAX(st.[Name]) AS [Name],MAX(mt.[POS Terminal No_]) AS [POS Terminal No_],1 AS [Transaction] " +
                    "FROM [" + navCompanyName + "Member Sales Entry] mt " +
                    "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                    "INNER JOIN [" + navCompanyName + "Item] i ON mt.[Item No_]=i.[No_] " +
                    "WHERE [Transaction No_]!=0 AND UPPER(i.Description) LIKE UPPER(@search) GROUP BY [Document No_] " +
                    "UNION " +
                    "SELECT DISTINCT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_], 0 AS [Posted]," +
                    "(SELECT COUNT(*) FROM [" + navCompanyName + "Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS [PayCount]," +
                    "mt.[" + externalId + "],mt.[" + cac2 + "] AS [CAC],mt.[" + shipto + "Full Name] AS [ShipName],'' AS [Transaction No_],0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                    "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [Name],'' AS [POS Terminal No_],0 AS [Transaction] " +
                    "FROM [" + navCompanyName + "Customer Order Header] mt " +
                    "INNER JOIN [" + navCompanyName + "Customer Order Line] ol ON ol.[" + documentId + "]=mt.[" + documentId + "] " +
                    "WHERE UPPER([Item Description]) LIKE UPPER(@search) " +
                    "UNION " +
                    "SELECT mt.[" + documentId + "],mt.[Store No_],mt.[Document DateTime],mt.[Member Card No_],1 AS [Posted]," +
                    "(SELECT COUNT(*) FROM [" + navCompanyName + "Posted Customer Order Payment] cop WHERE cop.[" + documentId + "]=mt.[" + documentId + "]) AS [PayCount]," +
                    "mt.[" + externalId + "],mt.[" + cac + "] AS [CAC],mt.[" + shiptoPost + "] AS [ShipName],'' AS [Transaction No_], 0 AS [Quantity],0 AS [Net Amount],0 AS [Gross Amount],0 AS [Discount Amount]," +
                    "(SELECT [Name] FROM [" + navCompanyName + "Store] WHERE [No_]=mt.[Store No_]) AS [Name],'' AS [POS Terminal No_], 0 AS [Transaction] " +
                    "FROM [" + navCompanyName + "Posted Customer Order Header] mt " +
                    "INNER JOIN [" + navCompanyName + "Posted Customer Order Line] ol ON ol.[" + documentId + "]=mt.[" + documentId + "] " +
                    "WHERE (SELECT COUNT(*) FROM [" + navCompanyName + "Member Sales Entry] se WHERE se.[Document No_]=mt.[Receipt No_])=0 " +
                    "AND UPPER([Item Description]) LIKE UPPER(@search) " +
                    ") AS SalesEntries WHERE [Member Card No_]=@id ORDER BY [Date] DESC";
            }
        }

        public List<SalesEntry> SalesEntriesByCardId(string cardId, int maxNrOfEntries)
        {
            List<SalesEntry> list = new List<SalesEntry>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Parameters.Clear();
                    command.CommandText = "SELECT " + (maxNrOfEntries > 0 ? "TOP " + maxNrOfEntries : "") + " * FROM " + sql;
                    command.Parameters.AddWithValue("@id", cardId);
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        bool trans = false;
                        while (reader.Read())
                        {
                            trans = (NavVersion > new Version("14.2")) ? (SQLHelper.GetInt32(reader["Transaction No_"]) > 0) : SQLHelper.GetBool(reader["Transaction"]);
                            if (trans)
                                list.Add(TransactionToSalesEntry(reader, false));
                            else
                                list.Add(OrderToSalesEntry(reader, false));
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
                            "(mt.[Date]+CAST((CONVERT(time,mt.[Time])) AS DATETIME)) AS [Date],mt.[Store No_],st.[Name],mt.[POS Terminal No_],mt.[Transaction No_]," +
                            "co.[External ID] AS [External ID],co.[Ship-to Name] AS [ShipName],mt.[Receipt No_],mt.[Customer Order ID] AS [Document ID],mt.[Customer Order] AS [CAC] " +
                            "FROM [" + navCompanyName + "Transaction Header] mt " +
                            "JOIN [" + navCompanyName + "Store] st ON st.[No_]=mt.[Store No_] " +
                            "LEFT OUTER JOIN [" + navCompanyName + "Posted Customer Order Header] co ON co.[Document ID]=mt.[Customer Order ID] " +
                            "WHERE [Receipt No_]=@id";
                    }
                    else
                    {
                        command.CommandText = "SELECT mt.[Transaction No_],MAX(mt.[Member Card No_]) AS [Member Card No_],SUM(mt.[Quantity]) AS [Quantity],1 AS [CAC]," +
                            "SUM(mt.[Net Amount]) AS [Net Amount],SUM(mt.[Gross Amount]) AS[Gross Amount],SUM(mt.[Discount Amount]) AS[Discount Amount],MAX(mt.[Date]) AS [Date]," +
                            "MAX(mt.[Store No_]) AS [Store No_], MAX(st.[Name]) AS [Name],MAX(mt.[POS Terminal No_]) AS[POS Terminal No_],MAX(mt.[Document No_]) AS [Document ID] " +
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

        public List<SalesEntry> SalesEntrySearch(string search, string cardId, int maxNumberOfTransactions, bool includeLines)
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
                    command.CommandText = "SELECT " + ((maxNumberOfTransactions > 0) ? "TOP " + maxNumberOfTransactions + " * FROM " : " * FROM ") + sqlSearch;

                    command.Parameters.AddWithValue("@id", cardId);
                    command.Parameters.AddWithValue("@search", searchWords);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        bool trans = false;
                        while (reader.Read())
                        {
                            trans = (NavVersion > new Version("14.2")) ? (SQLHelper.GetInt32(reader["Transaction No_"]) > 0) : SQLHelper.GetBool(reader["Transaction"]);
                            if (trans)
                                list.Add(TransactionToSalesEntry(reader, includeLines));
                            else
                                list.Add(OrderToSalesEntry(reader, includeLines));
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

        private SalesEntry TransactionToSalesEntry(SqlDataReader reader, bool includeLines)
        {
            SalesEntry entry = new SalesEntry()
            {
                Id = SQLHelper.GetString(reader["Document ID"]),
                IdType = DocumentIdType.Receipt,
                LineItemCount = (int)SQLHelper.GetDecimal(reader, "Quantity", false),
                TotalNetAmount = SQLHelper.GetDecimal(reader, "Net Amount", true),
                TotalAmount = SQLHelper.GetDecimal(reader, "Gross Amount", true),
                TotalDiscount = SQLHelper.GetDecimal(reader, "Discount Amount", false),
                DocumentRegTime = SQLHelper.GetDateTime(reader["Date"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"]),
                AnonymousOrder = false,
                Status = SalesEntryStatus.Complete,
                PaymentStatus = PaymentStatus.Posted,
                Posted = true,
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                StoreName = SQLHelper.GetString(reader["Name"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),
                ShipToName = SQLHelper.GetString(reader["ShipName"])
            };

            if (NavVersion > new Version("14.2"))
            {
                entry.ReceiptNo = SQLHelper.GetString(reader["Receipt No_"]);
                if (string.IsNullOrEmpty(entry.Id))
                    entry.Id = entry.ReceiptNo;
            }
            else
            {
                entry.ReceiptNo = entry.Id;
            }

            SalesEntryPointsGetTotal(entry.Id, out decimal rewarded, out decimal used);
            entry.PointsRewarded = rewarded;
            entry.PointsUsedInOrder = used;

            if (includeLines)
            {
                entry.Lines = TransSalesEntryLinesGet(entry.ReceiptNo);
                entry.Payments = TransSalesEntryPaymentGet(SQLHelper.GetString(reader["Transaction No_"]), entry.StoreId, entry.TerminalId, "");
            }
            return entry;
        }

        private SalesEntry OrderToSalesEntry(SqlDataReader reader, bool includeLines)
        {
            SalesEntry salesEntry = new SalesEntry
            {
                Id = SQLHelper.GetString(reader["Document ID"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                DocumentRegTime = SQLHelper.GetDateTime(reader["Date"]),
                IdType = DocumentIdType.Order,
                CardId = SQLHelper.GetString(reader["Member Card No_"]),
                Status = SalesEntryStatus.Created,
                StoreName = SQLHelper.GetString(reader["Name"]),
                ShipToName = SQLHelper.GetString(reader["ShipName"]),
                ExternalId = SQLHelper.GetString(reader["External ID"]),
                Posted = SQLHelper.GetBool(reader["Posted"]),
                ClickAndCollectOrder = SQLHelper.GetBool(reader["CAC"])
            };

            salesEntry.AnonymousOrder = string.IsNullOrEmpty(salesEntry.CardId);
            salesEntry.ShippingStatus = (salesEntry.ClickAndCollectOrder) ? ShippingStatus.ShippigNotRequired : ShippingStatus.NotYetShipped;

            OrderLinesGetTotals(salesEntry.Id, out int cnt, out decimal amt, out decimal namt, out decimal disc);
            salesEntry.LineItemCount = cnt;
            salesEntry.TotalAmount = amt;
            salesEntry.TotalNetAmount = namt;
            salesEntry.TotalDiscount = disc;

            if (salesEntry.Posted)
            {
                salesEntry.Status = SalesEntryStatus.Complete;
                SalesEntryPointsGetTotal(salesEntry.Id, out decimal rewarded, out decimal used);
                salesEntry.PointsRewarded = rewarded;
                salesEntry.PointsUsedInOrder = used;
            }
            return salesEntry;
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
                                " INNER JOIN [" + navCompanyName + "Item] i ON i.[No_]=ml.[Item No_]" +
                                " LEFT OUTER JOIN [" + navCompanyName + "Item Variant Registration] v ON v.[Item No_]=ml.[Item No_] AND v.[Variant]=ml.[Variant Code]" +
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

        public List<SalesEntryPayment> TransSalesEntryPaymentGet(string transId, string storeId, string terminalId, string culture)
        {
            List<SalesEntryPayment> list = new List<SalesEntryPayment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                "ml.[Line No_],ml.[Tender Type],ml.[Exchange Rate],ml.[Currency Code],ml.[Amount in Currency],ml.[Card or Account]" +
                                " FROM [" + navCompanyName + "Trans_ Payment Entry] ml" +
                                " WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid" +
                                " ORDER BY ml.[Line No_]";
                    command.Parameters.AddWithValue("@id", transId);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(TransToSalesEntryPayment(reader, culture));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public void SalesEntryPointsGetTotal(string entryId, out decimal rewarded, out decimal used)
        {
            rewarded = 0;
            used = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string orderid = (NavVersion > new Version("14.2")) ? "Customer Order ID" : "Customer Order No_";
                    command.CommandText = "SELECT [Receipt No_] FROM [" + navCompanyName + "Transaction Header] WHERE [" + orderid + "]=@id";
                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    logger.Info(config.LSKey.Key, "ReceiptNo: " + entryId);
                    connection.Open();
                    string salesId = (string)command.ExecuteScalar();

                    //Use salesinvoice id to get Rewarded points for customer orders
                    if (!string.IsNullOrEmpty(salesId))
                        entryId = salesId;

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
                DiscountAmount = SQLHelper.GetDecimal(reader, "Discount Amount", true),
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

        private SalesEntryPayment TransToSalesEntryPayment(SqlDataReader reader, string culture)
        {
            return new SalesEntryPayment()
            {
                LineNumber = ConvertTo.SafeInt(SQLHelper.GetString(reader["Line No_"])),
                TenderType = SQLHelper.GetString(reader["Tender Type"]),
                Amount = SQLHelper.GetDecimal(reader["Amount in Currency"], false),
                CurrencyFactor = SQLHelper.GetDecimal(reader["Exchange Rate"], false),
                CardNo = SQLHelper.GetString(reader["Card or Account"]),
                CurrencyCode = SQLHelper.GetString(reader["Currency Code"])
            };
        }

        public string FormatAmountToString(decimal amount, string culture)
        {
            return FormatAmount(amount, culture);
        }
    }
}
