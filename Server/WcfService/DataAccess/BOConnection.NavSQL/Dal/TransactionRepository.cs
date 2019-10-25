using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions;
using LSRetail.Omni.Domain.DataModel.Pos.Payments;
using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;
using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

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

        public TransactionRepository(BOConfiguration config, Version navVersion) : base(config, navVersion)
        {
            sqltransheadcol = "mt.[Transaction No_],mt.[Store No_],mt.[POS Terminal No_],mt.[Receipt No_],mt.[Staff ID],mt.[Trans_ Currency]," +
                              "mt.[No_ of Items],mt.[Net Amount],mt.[Cost Amount],mt.[Gross Amount],mt.[Payment],mt.[Discount Amount],mt.[Date],mt.[Time]";

            sqltransheadcol += (navVersion >= new Version("13.4")) ? (navVersion >= new Version("14.2")) ? ",mt.[Customer Order ID] AS OrderNo" : ",mt.[Customer Order No_] AS OrderNo" : ",mt.[Order No_] AS OrderNo";

            sqltransheadfrom = " FROM [" + navCompanyName + "Transaction Header] mt";

            sqlsalelinecol = "ml.[Transaction No_],ml.[Store No_],ml.[POS Terminal No_],ml.[Item No_],ml.[Variant Code],ml.[Unit of Measure]," +
                             "ml.[Quantity],ml.[Price],ml.[Net Price],ml.[Net Amount],ml.[Discount Amount],ml.[VAT Amount],ml.[Refund Qty_],ml.[Line No_]";
            sqlsalelinefrom = " FROM [" + navCompanyName + "Trans_ Sales Entry] ml";

            sqltenderecol = "ml.[Store No_],ml.[POS Terminal No_],ml.[Transaction No_],ml.[Line No_],ml.[Tender Type],ml.[Amount Tendered],ml.[Currency Code],ml.[Amount in Currency],t.[Description]";
            sqltenderfrom = " FROM [" + navCompanyName + "Trans_ Payment Entry] ml LEFT OUTER JOIN [" + navCompanyName + "Tender Type] t ON t.[Code]=ml.[Tender Type] AND t.[Store No_]=ml.[Store No_]";
        }

        public RetailTransaction TransactionGetByReceipt(string receiptNo, string culture, bool includeLines)
        {
            RetailTransaction trans = null;
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

        public List<SaleLine> SalesLineGet(string transId, string storeId, string terminalId, Currency currency)
        {
            List<SaleLine> list = new List<SaleLine>();
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
                            list.Add(ReaderToSaleLine(reader, storeId, currency));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<PaymentLine> LoyTenderLineGet(string transId, string storeId, string terminalId, string culture)
        {
            List<PaymentLine> list = new List<PaymentLine>();
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
                        int lineno = 1;
                        while (reader.Read())
                        {
                            list.Add(ReaderToLoyTender(reader, lineno++, culture));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        public List<ReceiptInfo> ReceiptGet(string id)
        {
            List<ReceiptInfo> list = new List<ReceiptInfo>();
            /*
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Line No_],[Key],[Value],[Transaction No_],[Type],[LargeValue]" +
                                          " FROM [" + navCompanyName + "MobileReceiptInfo] WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToReceiptInfo(reader));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            */
            return list;
        }

        private RetailTransaction ReaderToLoyTransHeader(SqlDataReader reader, string culture, bool includeLines)
        {
            Currency cur = new Currency(SQLHelper.GetString(reader["Trans_ Currency"]));

            RetailTransaction trans = new RetailTransaction()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                Terminal = new Terminal(SQLHelper.GetString(reader["POS Terminal No_"])),
                ReceiptNumber = SQLHelper.GetString(reader["Receipt No_"]),
                GrossAmount = new Money(SQLHelper.GetDecimal(reader["Gross Amount"], true), cur),
                NetAmount = new Money(SQLHelper.GetDecimal(reader["Net Amount"], true), cur),
                TotalDiscount = new Money(SQLHelper.GetDecimal(reader["Discount Amount"], true), cur),
                NumberOfItems = SQLHelper.GetInt32(reader["No_ of Items"])
            };

            trans.Terminal.Staff = new Staff(SQLHelper.GetString(reader["Staff ID"]));

            StoreRepository strep = new StoreRepository(config, NavVersion);
            trans.Terminal.Store = strep.StoreLoyGetById(SQLHelper.GetString(reader["Store No_"]), false);

            DateTime navdate = SQLHelper.GetDateTime(reader["Date"]);
            DateTime navtime = SQLHelper.GetDateTime(reader["Time"]);
            trans.BeginDateTime = new DateTime(navdate.Year, navdate.Month, navdate.Day, navtime.Hour, navtime.Minute, navtime.Second);

            if (includeLines)
            {
                trans.SaleLines = SalesLineGet(trans.Id, trans.Terminal.Store.Id, trans.Terminal.Id, cur);
                trans.TenderLines = LoyTenderLineGet(trans.Id, trans.Terminal.Store.Id, trans.Terminal.Id, culture);
            }
            else
            {
                trans.SaleLines = new List<SaleLine>();
                trans.TenderLines = new List<PaymentLine>();
            }
            return trans;
        }

        private SaleLine ReaderToSaleLine(SqlDataReader reader, string storeId, Currency currency)
        {
            SaleLine line = new SaleLine()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                Quantity = SQLHelper.GetDecimal(reader, "Quantity", true),
                ReturnQuantity = SQLHelper.GetDecimal(reader, "Refund Qty_", false),
                UnitPriceWithTax = new Money(SQLHelper.GetDecimal(reader, "Price"), currency),
                UnitPrice = new Money(SQLHelper.GetDecimal(reader["Net Price"]), currency),
                NetAmount = new Money(SQLHelper.GetDecimal(reader, "Net Amount", true), currency),
                TaxAmount = new Money(SQLHelper.GetDecimal(reader, "VAT Amount", true), currency),
            };

            ItemRepository itemRepo = new ItemRepository(config, NavVersion);
            LoyItem item = itemRepo.ItemLoyGetById(SQLHelper.GetString(reader["Item No_"]), storeId, string.Empty, false);
            RetailItem ritem = new RetailItem()
            {
                Id = item.Id,
                Description = item.Description,
            };

            string uid = SQLHelper.GetString(reader["Unit of Measure"]);
            if (string.IsNullOrEmpty(uid) == false)
            {
                UnitOfMeasureRepository urepo = new UnitOfMeasureRepository(config);
                ritem.UnitOfMeasure = urepo.UnitOfMeasureGetById(uid);
                ritem.UnitOfMeasure.ItemId = ritem.Id;
            }

            string vid = SQLHelper.GetString(reader["Variant Code"]);
            if (string.IsNullOrEmpty(vid) == false)
            {
                ItemVariantRegistrationRepository vrepo = new ItemVariantRegistrationRepository(config);
                ritem.SelectedVariant = vrepo.VariantRegGetById(vid, ritem.Id);
            }

            line.Item = ritem;
            return line;
        }

        private PaymentLine ReaderToLoyTender(SqlDataReader reader, int lineno, string currency)
        {
            Payment pay = new Payment()
            {
                TransactionId = SQLHelper.GetString(reader["Transaction No_"]),
                StoreId = SQLHelper.GetString(reader["Store No_"]),
                TerminalId = SQLHelper.GetString(reader["POS Terminal No_"]),
                TenderType = new TenderType(SQLHelper.GetString(reader["Tender Type"])),
                Amount = new Money(SQLHelper.GetDecimal(reader, "Amount Tendered", false), string.Empty),
            };
            pay.ReceiptInfo = ReceiptGet(pay.TransactionId);
            return new PaymentLine(lineno, pay);
        }

        private ReceiptInfo ReaderToReceiptInfo(SqlDataReader reader)
        {
            //[Line No_],[Key],[Value],[Transaction No_],[Type],[LargeValue]
            ReceiptInfo info = new ReceiptInfo()
            {
                Key = SQLHelper.GetString(reader["Key"]),
                ValueAsText = ConvertTo.Base64Decode(SQLHelper.GetString(reader["LargeValue"])),
                Type = SQLHelper.GetString(reader["Type"]),
                IsLargeValue = true
            };

            if (string.IsNullOrEmpty(info.ValueAsText))
            {
                info.IsLargeValue = false;
                info.ValueAsText = SQLHelper.GetString(reader["Value"]);
            }
            return info;
        }
    }
}