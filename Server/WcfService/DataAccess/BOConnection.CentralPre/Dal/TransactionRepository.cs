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
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class TransactionRepository : BaseRepository
    {
        public TransactionRepository(BOConfiguration config) : base(config)
        {
        }

        public RetailTransaction TransactionGetByReceipt(string receiptNo, bool includeLines, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            RetailTransaction trans = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                                    "mt.[Transaction No_],mt.[Store No_],mt.[POS Terminal No_],mt.[Receipt No_],mt.[Refund Receipt No_],mt.[Staff ID]," +
                                    "mt.[Trans_ Currency],mt.[No_ of Items],mt.[Net Amount],mt.[Cost Amount],mt.[Gross Amount]," +
                                    "mt.[Payment],mt.[Discount Amount],mt.[Date],mt.[Time],mt.[Customer Order ID] AS OrderNo " +
                                    "FROM [" + navCompanyName + "LSC Transaction Header$5ecfc871-5d82-43f1-9c54-59685e82318d] mt " +
                                    "WHERE mt.[Receipt No_]=@id";

                    command.Parameters.AddWithValue("@id", receiptNo);
                    connection.Open();
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            trans = ReaderToLoyTransHeader(reader, includeLines, stat);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return trans;
        }

        public List<SaleLine> SalesLineGet(string transId, string storeId, string terminalId, Currency currency, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<SaleLine> list = new List<SaleLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " + 
                            "ml.[Transaction No_],ml.[Store No_],ml.[POS Terminal No_],ml.[Item No_],ml.[Variant Code],ml.[Unit of Measure]," +
                            "ml.[Quantity],ml.[Price],ml.[Net Price],ml.[Net Amount],ml.[Discount Amount],ml.[VAT Amount],ml.[Refund Qty_],ml.[Line No_] " +
                            "FROM [" + navCompanyName + "LSC Trans_ Sales Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                            "WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid ORDER BY ml.[Line No_]";

                    command.Parameters.AddWithValue("@id", transId);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToSaleLine(reader, storeId, terminalId, currency, stat));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<DiscountLine> DiscountLineGet(string transId, string storeId, string terminalId, int lineNo, Currency currency, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<DiscountLine> list = new List<DiscountLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT " +
                            "ml.[Transaction No_],ml.[Store No_],ml.[POS Terminal No_],ml.[Line No_],ml.[Offer No_]," +
                            "ml.[Offer Type],ml.[Discount Amount],pd.[Description],pd.[Type] " +
                            "FROM [" + navCompanyName + "LSC Trans_ Discount Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                            "LEFT JOIN [" + navCompanyName + "LSC Periodic Discount$5ecfc871-5d82-43f1-9c54-59685e82318d] pd ON pd.[No_]=ml.[Offer No_] " +
                            "WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid AND ml.[Line No_]=@no";

                    command.Parameters.AddWithValue("@id", transId);
                    command.Parameters.AddWithValue("@Sid", storeId);
                    command.Parameters.AddWithValue("@Tid", terminalId);
                    command.Parameters.AddWithValue("@no", lineNo);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToDiscountLine(reader, currency));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
            return list;
        }

        public List<PaymentLine> LoyTenderLineGet(string transId, string storeId, string terminalId, Statistics stat)
        {
            logger.StatisticStartSub(false, ref stat, out int index);
            List<PaymentLine> list = new List<PaymentLine>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT ml.[Store No_],ml.[POS Terminal No_],ml.[Transaction No_],ml.[Line No_],ml.[Tender Type]," +
                                          "ml.[Amount Tendered],ml.[Currency Code],ml.[Amount in Currency],t.[Description] " +
                                          "FROM [" + navCompanyName + "LSC Trans_ Payment Entry$5ecfc871-5d82-43f1-9c54-59685e82318d] ml " +
                                          "LEFT JOIN [" + navCompanyName + "LSC Tender Type$5ecfc871-5d82-43f1-9c54-59685e82318d] t " +
                                          "ON t.[Code]=ml.[Tender Type] AND t.[Store No_]=ml.[Store No_] " +
                                          "WHERE ml.[Transaction No_]=@id AND ml.[Store No_]=@Sid AND ml.[POS Terminal No_]=@Tid " +
                                          "ORDER BY ml.[Line No_]";
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
                            list.Add(ReaderToLoyTender(reader, lineno++));
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            logger.StatisticEndSub(ref stat, index);
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

        private RetailTransaction ReaderToLoyTransHeader(SqlDataReader reader, bool includeLines, Statistics stat)
        {
            Currency cur = new Currency(SQLHelper.GetString(reader["Trans_ Currency"]));

            RetailTransaction trans = new RetailTransaction()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                Terminal = new Terminal(SQLHelper.GetString(reader["POS Terminal No_"])),
                ReceiptNumber = SQLHelper.GetString(reader["Receipt No_"]),
                RefundedReceiptNo = SQLHelper.GetString(reader["Refund Receipt No_"]),
                GrossAmount = new Money(SQLHelper.GetDecimal(reader, "Gross Amount", true), cur),
                NetAmount = new Money(SQLHelper.GetDecimal(reader ,"Net Amount", true), cur),
                TotalDiscount = new Money(SQLHelper.GetDecimal(reader ,"Discount Amount", true), cur),
                NumberOfItems = SQLHelper.GetInt32(reader["No_ of Items"])
            };

            trans.Terminal.Staff = new Staff(SQLHelper.GetString(reader["Staff ID"]));

            StoreRepository strep = new StoreRepository(config, LSCVersion);
            trans.Terminal.Store = strep.StoreLoyGetById(SQLHelper.GetString(reader["Store No_"]), false);

            DateTime navdate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Date"]), config.IsJson);
            DateTime navtime = ConvertTo.SafeJsonTime(SQLHelper.GetDateTime(reader["Time"]), config.IsJson);
            trans.BeginDateTime = new DateTime(navdate.Year, navdate.Month, navdate.Day, navtime.Hour, navtime.Minute, navtime.Second);

            if (includeLines)
            {
                trans.SaleLines = SalesLineGet(trans.Id, trans.Terminal.Store.Id, trans.Terminal.Id, cur, stat);
                trans.TenderLines = LoyTenderLineGet(trans.Id, trans.Terminal.Store.Id, trans.Terminal.Id, stat);
            }
            else
            {
                trans.SaleLines = new List<SaleLine>();
                trans.TenderLines = new List<PaymentLine>();
            }
            return trans;
        }

        private SaleLine ReaderToSaleLine(SqlDataReader reader, string storeId, string terminalId, Currency currency, Statistics stat)
        {
            SaleLine line = new SaleLine()
            {
                Id = SQLHelper.GetString(reader["Transaction No_"]),
                LineNumber = SQLHelper.GetInt32(reader["Line No_"]),
                Quantity = SQLHelper.GetDecimal(reader, "Quantity", true),
                ReturnQuantity = SQLHelper.GetDecimal(reader, "Refund Qty_", false),
                UnitPriceWithTax = new Money(SQLHelper.GetDecimal(reader, "Price"), currency),
                UnitPrice = new Money(SQLHelper.GetDecimal(reader, "Net Price"), currency),
                NetAmount = new Money(SQLHelper.GetDecimal(reader, "Net Amount", true), currency),
                TaxAmount = new Money(SQLHelper.GetDecimal(reader, "VAT Amount", true), currency),
            };

            ItemRepository itemRepo = new ItemRepository(config, LSCVersion);
            LoyItem item = itemRepo.ItemLoyGetById(SQLHelper.GetString(reader["Item No_"]), storeId, string.Empty, false, stat);
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
                ItemVariantRegistrationRepository vrepo = new ItemVariantRegistrationRepository(config, LSCVersion);
                ritem.SelectedVariant = vrepo.VariantRegGetById(vid, ritem.Id, stat);
            }

            decimal discount = SQLHelper.GetDecimal(reader, "Discount Amount", false);
            if (discount > 0)
            {
                line.Discounts = DiscountLineGet(line.Id, storeId, terminalId, line.LineNumber, currency, stat);
            }

            line.Item = ritem;
            return line;
        }

        private DiscountLine ReaderToDiscountLine(SqlDataReader reader, Currency currency)
        {
            return new DiscountLine()
            {
                Amount = new Money(SQLHelper.GetDecimal(reader, "Discount Amount"), currency),
                Description = SQLHelper.GetString(reader["Description"]),
                EntryType = DiscountEntryType.Amount,
                No = string.Empty,
                OfferNo = SQLHelper.GetString(reader["Offer No_"]),
                Percentage = 0,
                PeriodicType = (PeriodicDiscType)SQLHelper.GetInt32(reader["Type"]),
                Type = (DiscountType)SQLHelper.GetInt32(reader["Offer Type"])
            };
        }

        private PaymentLine ReaderToLoyTender(SqlDataReader reader, int lineno)
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
    }
}