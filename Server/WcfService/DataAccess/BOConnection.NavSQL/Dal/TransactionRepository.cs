using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class TransactionRepository : BaseRepository
    {
        private string sqltransheadcol = string.Empty;
        private string sqltransheadfrom = string.Empty;

        private string sqlsalelinecol = string.Empty;
        private string sqlsalelinefrom = string.Empty;

        private string sqltenderecol = string.Empty;
        private string sqltenderfrom = string.Empty;

        public TransactionRepository(Version navVersion) : base(navVersion)
        {
            sqltransheadcol = "mt.[Transaction No_],mt.[Store No_],mt.[POS Terminal No_],mt.[Receipt No_],mt.[Staff ID],mt.[Trans_ Currency]," +
                              "mt.[No_ of Items],mt.[Net Amount],mt.[Cost Amount],mt.[Gross Amount],mt.[Payment],mt.[Discount Amount],mt.[Date],mt.[Time],mt.[Order No_]";
            sqltransheadfrom = " FROM [" + navCompanyName + "Transaction Header] mt";

            sqlsalelinecol = "ml.[Transaction No_],ml.[Store No_],ml.[POS Terminal No_],ml.[Item No_],ml.[Variant Code],ml.[Unit of Measure]," +
                             "ml.[Quantity],ml.[Price],ml.[Net Price],ml.[Net Amount],ml.[Discount Amount],ml.[VAT Amount],ml.[Refund Qty_],ml.[Line No_]";
            sqlsalelinefrom = " FROM [" + navCompanyName + "Trans_ Sales Entry] ml";

            sqltenderecol = "ml.[Store No_],ml.[POS Terminal No_],ml.[Transaction No_],ml.[Line No_],ml.[Tender Type],ml.[Amount Tendered],ml.[Currency Code],ml.[Amount in Currency],t.[Description]";
            sqltenderfrom = " FROM [" + navCompanyName + "Trans_ Payment Entry] ml LEFT OUTER JOIN [" + navCompanyName + "Tender Type] t ON t.[Code]=ml.[Tender Type] AND t.[Store No_]=ml.[Store No_]";
        }

        public string FormatAmountToString(decimal amount, string culture)
        {
            return FormatAmount(amount, culture);
        }

        public LoyTransaction TransactionGetByReceipt(string receiptNo, string culture, bool includeLines)
        {
            LoyTransaction trans = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sqlmain = "SELECT " + sqltransheadcol + sqltransheadfrom;
                    string sqlwhere = " WHERE mt.[Receipt No_]=@id";

                    command.Parameters.AddWithValue("@id", receiptNo);
                    command.CommandText = sqlmain + sqlwhere;
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            trans = ReaderToLoyTransHeader(reader, culture, includeLines);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return trans;
        }

        public List<LoyTransaction> LoyTransactionHeadersGetByContactId(string contactId, int maxNumberOfTransactions, string culture)
        {
            List<LoyTransaction> list = new List<LoyTransaction>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string top = (maxNumberOfTransactions > 0) ? string.Format("TOP({0}) ", maxNumberOfTransactions) : string.Empty;
                    command.CommandText = "SELECT " + top +
                                          "[Entry No_],COUNT(*) AS CNT,MAX([Source Type]) AS SourceType,MAX([Document No_]) AS DocNo,SUM([Quantity]) AS Qty," +
                                          "SUM([Net Amount]) AS NetAmt,SUM([Gross Amount]) AS Amt,SUM([Discount Amount]) AS DiscAmt," +
                                          "MAX([Date]) AS RegDate,MAX([Store No_]) AS Store,MAX([POS Terminal No_]) AS Terminal " +
                                          "FROM [" + navCompanyName + "Member Sales Entry] " +
                                          "WHERE [Member Contact No_]=@id GROUP BY [Entry No_] ORDER BY [Entry No_] DESC";
                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        StoreRepository strep = new StoreRepository(NavVersion);

                        while (reader.Read())
                        {
                            LoyTransaction trans = new LoyTransaction()
                            {
                                Id = SQLHelper.GetString(reader["Entry No_"]),
                                SaleLinesCount = SQLHelper.GetInt32(reader["CNT"]),
                                DocumentType = (EntryDocumentType)SQLHelper.GetInt32(reader["SourceType"]),
                                DocumentNumber = SQLHelper.GetString(reader["DocNo"]),
                                TotalQty = SQLHelper.GetDecimal(reader["Qty"], true),
                                NetAmt = SQLHelper.GetDecimal(reader["NetAmt"], true),
                                Amt = SQLHelper.GetDecimal(reader["Amt"], true),
                                DiscountAmt = SQLHelper.GetDecimal(reader["DiscAmt"], true),
                                Date = SQLHelper.GetDateTime(reader["RegDate"]),
                                Store = strep.StoreLoyGetById(SQLHelper.GetString(reader["Store"]), false),
                                Terminal = SQLHelper.GetString(reader["Terminal"])
                            };

                            trans.ReceiptNumber = (trans.DocumentType == EntryDocumentType.ReceiptNumber) ? trans.DocumentNumber : string.Empty;
                            trans.VatAmt = trans.Amt - trans.NetAmt;
                            list.Add(trans);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public LoyTransaction SalesEntryGetById(string entryId)
        {
            LoyTransaction trans = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Line No_],mt.[Quantity],mt.[Net Amount],mt.[Gross Amount],mt.[Discount Amount]," +
                                          "mt.[Cost Amount],mt.[Points Type],mt.[Points],mt.[Date],mt.[Item No_]," +
                                          "mt.[Store No_],mt.[POS Terminal No_],mt.[Source Type],mt.[Document No_],mt.[Item Variant Code] " +
                                          "FROM [" + navCompanyName + "Member Sales Entry] mt WHERE [Entry No_]=@id";

                    command.Parameters.AddWithValue("@id", entryId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        trans = new LoyTransaction();
                        trans.SaleLines = new List<LoySaleLine>();

                        ItemRepository irepo = new ItemRepository();
                        ItemVariantRegistrationRepository vrepo = new ItemVariantRegistrationRepository();

                        while (reader.Read())
                        {
                            if (trans.TotalQty == 0)
                            {
                                // first line for sale, add Header info
                                trans.Id = entryId;
                                trans.Store = new Store(SQLHelper.GetString(reader["Store No_"]));
                                trans.Terminal = SQLHelper.GetString(reader["POS Terminal No_"]);
                                trans.Date = SQLHelper.GetDateTime(reader["Date"]);
                                trans.DocumentNumber = SQLHelper.GetString(reader["Document No_"]);
                                trans.DocumentType = (EntryDocumentType)SQLHelper.GetInt32(reader["Source Type"]);
                                trans.ReceiptNumber = (trans.DocumentType == EntryDocumentType.ReceiptNumber) ? trans.DocumentNumber : string.Empty;
                            }

                            LoySaleLine line = new LoySaleLine()
                            {
                                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                                Item = irepo.ItemLoyGetById(SQLHelper.GetString(reader["Item No_"]), trans.Store.Id, string.Empty, false),
                                VariantReg = vrepo.VariantRegGetById(SQLHelper.GetString(reader["Item Variant Code"]), SQLHelper.GetString(reader["Item No_"])),
                                Quantity = SQLHelper.GetDecimal(reader["Quantity"], true),
                                NetAmt = SQLHelper.GetDecimal(reader["Net Amount"], true),
                                Amt = SQLHelper.GetDecimal(reader["Gross Amount"], true),
                                DiscountAmt = SQLHelper.GetDecimal(reader["Discount Amount"], true),
                                Price = SQLHelper.GetDecimal(reader["Cost Amount"], true),
                            };

                            trans.SaleLines.Add(line);
                            trans.SaleLinesCount++;
                            trans.TotalQty += line.Quantity;
                            trans.Amt += line.Amt;
                            trans.DiscountAmt += line.DiscountAmt;
                            trans.NetAmt += line.NetAmt;
                            trans.VatAmt += line.Amt - line.NetAmt;
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
            return trans;
        }

        public List<LoyTransaction> LoyTransactionGetByContactId(string contactId, int maxNumberOfTransactions, string culture, bool includeLines)
        {
            List<LoyTransaction> list = new List<LoyTransaction>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("SELECT {0}{1}{2}",
                                        (maxNumberOfTransactions > 0) ? string.Format("TOP({0}) ", maxNumberOfTransactions) : string.Empty,
                                        sqltransheadcol, sqltransheadfrom) +
                                    " INNER JOIN [" + navCompanyName + "Membership Card] mc ON mc.[Card No_]=mt.[Member Card No_]" +
                                    " WHERE mc.[Contact No_]=@id AND mt.[Entry Status] NOT IN (1,3)" +
                                    " ORDER BY mt.[Date] DESC, mt.[Time]";
                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyTransHeader(reader, culture, includeLines));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<LoyTransaction> LoyTransactionSearch(string search, string contactId, int maxNumberOfTransactions, string culture, bool includeLines)
        {
            List<LoyTransaction> list = new List<LoyTransaction>();
            if (string.IsNullOrWhiteSpace(search))
                return list;

            if (search.Contains("'"))
                search = search.Replace("'", "''");

            char[] sep = new char[] { ' ' };
            string[] searchitems = search.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            string sqlwhere = string.Empty;
            foreach (string si in searchitems)
            {
                sqlwhere += string.Format(" AND i.[Description] LIKE N'%{0}%' {1}", si, GetDbCICollation());
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = GetSQL(true, maxNumberOfTransactions, false, true) + sqltransheadcol + sqltransheadfrom +
                                    " INNER JOIN [" + navCompanyName + "Trans_ Sales Entry] ts ON ts.[Store No_]=mt.[Store No_] AND ts.[POS Terminal No_]=mt.[POS Terminal No_] AND ts.[Transaction No_]=mt.[Transaction No_]" +
                                    " INNER JOIN [" + navCompanyName + "Item] i ON i.[No_]=ts.[Item No_]" +
                                    " INNER JOIN [" + navCompanyName + "Membership Card] mc ON mc.[Card No_]=mt.[Member Card No_]" +
                                    " WHERE mc.[Contact No_]=@id AND mt.[Entry Status] NOT IN (1,3)" + sqlwhere;
                    command.Parameters.AddWithValue("@id", contactId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyTransHeader(reader, culture, includeLines));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public LoyTransaction LoyTransactionGetByIds(string storeId, string terminalId, string id, string contactId, string culture, bool includeLines)
        {
            LoyTransaction trans = new LoyTransaction();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    string sqlmain = "SELECT " + sqltransheadcol + sqltransheadfrom;
                    string sqlwhere = " WHERE mt.[Transaction No_]=@id AND mt.[Store No_]=@Sid AND mt.[POS Terminal No_]=@Tid";

                    if (string.IsNullOrWhiteSpace(contactId) == false)
                    {
                        sqlmain += " INNER JOIN [" + navCompanyName + "Membership Card] mc ON mc.[Card No_]=mt.[Member Card No_]" +
                                   " INNER JOIN [" + navCompanyName + "Member Contact] co ON co.[Contact No_]=mc.[Contact No_]";

                        sqlwhere += " AND co.[Contact No_]=@Cid AND co.Blocked=0";
                        command.Parameters.AddWithValue("@Cid", contactId);
                    }

                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    command.CommandText = sqlmain + sqlwhere;
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            trans = ReaderToLoyTransHeader(reader, culture, includeLines);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return trans;
        }

        public List<LoySaleLine> LoySalesLineGet(string transId, string storeId, string terminalId, string culture)
        {
            List<LoySaleLine> list = new List<LoySaleLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqlsalelinecol + sqlsalelinefrom +
                                          " WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid ORDER BY ml.[Line No_]";
                    command.Parameters.AddWithValue("@id", transId);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoySaleLine(reader, culture));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<LoyTenderLine> LoyTenderLineGet(string transId, string storeId, string terminalId, string culture)
        {
            List<LoyTenderLine> list = new List<LoyTenderLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + sqltenderecol + sqltenderfrom +
                                          " WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid ORDER BY ml.[Line No_]";
                    command.Parameters.AddWithValue("@id", transId);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyTender(reader, culture));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public void LoySaleLinesGetTotals(string transId, string storeId, string terminalId, out int itemCount, out decimal totalAmount, out decimal totalNetAmount, out decimal totalDiscount)
        {
            itemCount = 0;
            totalAmount = 0;
            totalNetAmount = 0;
            totalDiscount = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT SUM([Quantity]) AS Cnt,SUM([Discount Amount]) AS Disc,SUM([Total Rounded Amt_]) AS Amt,SUM([Net Amount]) AS NAmt" + 
                                          sqlsalelinefrom + " WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid";

                    command.Parameters.AddWithValue("@id", transId);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            itemCount = SQLHelper.GetInt32(reader["Cnt"], true);
                            totalAmount = SQLHelper.GetDecimal(reader["Amt"], true);
                            totalNetAmount = SQLHelper.GetDecimal(reader["NAmt"], true);
                            totalDiscount = SQLHelper.GetDecimal(reader["Disc"], true);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
        }

        private LoyTransaction ReaderToLoyTransHeader(SqlDataReader reader, string culture, bool includeLines)
        {
            LoyTransaction trans = new LoyTransaction()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                Terminal = SQLHelper.GetString(reader["POS Terminal No_"]),
                Staff = SQLHelper.GetString(reader["Staff ID"]),
                DocumentNumber = SQLHelper.GetString(reader["Receipt No_"]),
                Amt = SQLHelper.GetDecimal(reader["Gross Amount"], true),
                NetAmt = SQLHelper.GetDecimal(reader["Net Amount"], true),
                DiscountAmt = SQLHelper.GetDecimal(reader["Discount Amount"], true),
                TotalQty = SQLHelper.GetDecimal(reader["No_ of Items"])
            };

            string orderno = SQLHelper.GetString(reader["Order No_"]);
            if (string.IsNullOrEmpty(orderno))
            {
                trans.DocumentType =  EntryDocumentType.ReceiptNumber;
                trans.ReceiptNumber = SQLHelper.GetString(reader["Receipt No_"]);
            }
            else
            {
                trans.DocumentType = EntryDocumentType.SalesInvoice;
            }

            StoreRepository strep = new StoreRepository(NavVersion);
            trans.Store = strep.StoreLoyGetById(SQLHelper.GetString(reader["Store No_"]), false);

            DateTime navdate = SQLHelper.GetDateTime(reader["Date"]);
            DateTime navtime = SQLHelper.GetDateTime(reader["Time"]);
            trans.Date = new DateTime(navdate.Year, navdate.Month, navdate.Day, navtime.Hour, navtime.Minute, navtime.Second);

            if (includeLines)
            {
                trans.SaleLines = LoySalesLineGet(trans.Id, trans.Store.Id, trans.Terminal, culture);
                trans.SaleLinesCount = trans.SaleLines.Count;
                trans.TenderLines = LoyTenderLineGet(trans.Id, trans.Store.Id, trans.Terminal, culture);
                trans.TenderLinesCount = trans.TenderLines.Count;
            }
            else
            {
                trans.SaleLines = new List<LoySaleLine>();
                trans.TenderLines = new List<LoyTenderLine>();
                trans.SaleLinesCount = 1;
                trans.TenderLinesCount = 1;
            }

            trans.VatAmt = trans.Amt - trans.NetAmt;
            trans.Platform = (trans.Amt == 0) ? Platform.Mobile : Platform.Standard;

            trans.Amount = FormatAmount(trans.Amt, culture);
            trans.VatAmount = FormatAmount(trans.VatAmt, culture);
            trans.NetAmount = FormatAmount(trans.NetAmt, culture);
            trans.DiscountAmount = FormatAmount(trans.DiscountAmt, culture);
            return trans;
        }

        private LoySaleLine ReaderToLoySaleLine(SqlDataReader reader, string culture)
        {
            LoySaleLine line = new LoySaleLine()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                Quantity = SQLHelper.GetDecimal(reader["Quantity"], true),
                ReturnQuantity = SQLHelper.GetDecimal(reader["Refund Qty_"], false),
                Price = SQLHelper.GetDecimal(reader["Price"]),
                NetPrice = SQLHelper.GetDecimal(reader["Net Price"]),
                NetAmt = SQLHelper.GetDecimal(reader["Net Amount"], true),
                DiscountAmt = SQLHelper.GetDecimal(reader["Discount Amount"], true),
                VatAmt = SQLHelper.GetDecimal(reader["VAT Amount"], true)
            };

            ItemRepository itemRepo = new ItemRepository();
            line.Item = itemRepo.ItemLoyGetById(SQLHelper.GetString(reader["Item No_"]), line.StoreId, culture, false);

            string uid = SQLHelper.GetString(reader["Unit of Measure"]);
            if (string.IsNullOrEmpty(uid) == false)
            {
                UnitOfMeasureRepository urepo = new UnitOfMeasureRepository();
                line.Uom = urepo.UnitOfMeasureGetById(uid);
                line.Uom.ItemId = line.Item.Id;
            }

            string vid = SQLHelper.GetString(reader["Variant Code"]);
            if (string.IsNullOrEmpty(vid) == false)
            {
                ItemVariantRegistrationRepository vrepo = new ItemVariantRegistrationRepository();
                line.VariantReg = vrepo.VariantRegGetById(vid, line.Item.Id);
            }

            line.TransactionId = line.Id;
            line.Amt = line.NetAmt + line.VatAmt;
            line.Amount = FormatAmount(line.Amt, culture);
            line.NetAmount = FormatAmount(line.NetAmt, culture);
            line.DiscountAmount = FormatAmount(line.DiscountAmt, culture);
            line.VatAmount = FormatAmount(line.VatAmt, culture);
            return line;
        }

        private LoyTenderLine ReaderToLoyTender(SqlDataReader reader, string culture)
        {
            LoyTenderLine line = new LoyTenderLine()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                LineNo = SQLHelper.GetString(reader["Line No_"]),

                Description = SQLHelper.GetString(reader["Description"]),
                Type = SQLHelper.GetString(reader["Tender Type"]),

                Amt = SQLHelper.GetDecimal(reader["Amount Tendered"], true)
            };

            line.TransactionId = line.Id;
            line.Amount = FormatAmount(line.Amt, culture);
            return line;
        }
    }
}