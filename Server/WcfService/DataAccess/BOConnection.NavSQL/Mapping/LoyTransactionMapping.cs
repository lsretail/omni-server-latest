using System;
using System.Linq;

using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Mapping
{
    public class LoyTransactionMapping : BaseMapping
    {
        public LoyTransaction MapFromRootToLoyTransaction(NavWS.RootGetTransaction root)
        {
            NavWS.TransactionHeader header = root.TransactionHeader.FirstOrDefault();
            UnknownCurrency transactionCurrency = new UnknownCurrency(header.TransCurrency);

            LoyTransaction transaction = new LoyTransaction()
            {
                Id = header.TransactionNo.ToString(),
                Staff = header.StaffID,
                Amt = header.GrossAmount,
                DiscountAmt = header.DiscountAmount,
                NetAmt = header.NetAmount,
                Store = new Store(header.StoreNo),
                ReceiptNumber = header.ReceiptNo,
                Terminal = header.POSTerminalNo,
                Date = DateAndTimeFromNav(header.Date, header.Time)
            };

            foreach (NavWS.TransSalesEntry entry in root.TransSalesEntry)
            {
                transaction.SaleLines.Add(new LoySaleLine()
                {
                    Item = new LoyItem(entry.ItemNo),
                    VariantReg = new VariantRegistration(entry.VariantCode),
                    Uom = new UnitOfMeasure(entry.UnitofMeasure, entry.ItemNo),
                    Quantity = entry.Quantity,
                    ReturnQuantity = entry.RefundQty,
                    NetAmt = entry.NetAmount,
                    VatAmt = entry.VATAmount,
                    Amt = entry.Price,
                    DiscountAmt = entry.DiscountAmount
                });
            }

            if (root.TransPaymentEntry != null && root.TransPaymentEntry.Length > 0)
            {
                foreach (NavWS.TransPaymentEntry pay in root.TransPaymentEntry)
                {
                    transaction.TenderLines.Add(new LoyTenderLine()
                    {
                        Type = pay.TenderType,
                        Amt = pay.AmountTendered
                    });
                }
            }
            return transaction;
        }
    }
}
