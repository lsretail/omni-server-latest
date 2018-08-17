using System;
using System.Collections.Generic;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class TransactionBLL : BaseLoyBLL
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IImageRepository iImageRepository;

        public TransactionBLL(string securityToken, int timeoutInSeconds)
            : base(securityToken, timeoutInSeconds)
        {
            this.iImageRepository = GetDbRepository<IImageRepository>();
        }

        public List<LoyTransaction> SalesEntriesGetByContactId(string contactId, int maxNumberOfTransactions)
        {
            base.ValidateContact(contactId);
            return BOLoyConnection.SalesEntriesGetByContactId(contactId, maxNumberOfTransactions, base.GetAppSettingCurrencyCulture());
        }

        public virtual LoyTransaction TransactionGetByReceiptNoWithPrint(string receiptNo)
        {
            logger.Debug("receipt: {0}", receiptNo);

            LoyTransaction trans = BOLoyConnection.TransactionGetByReceiptNo(receiptNo, false);
            if (trans == null)
                return null;

            trans = BOLoyConnection.TransactionNavGetByIdWithPrint(trans.Store.Id, trans.Terminal, trans.Id);
            if (trans == null)
                return null;

            trans.Amount = BOLoyConnection.FormatAmount(trans.Amt, base.GetAppSettingCurrencyCulture());
            trans.DiscountAmount = BOLoyConnection.FormatAmount(trans.DiscountAmt, base.GetAppSettingCurrencyCulture());
            trans.NetAmount = BOLoyConnection.FormatAmount(trans.NetAmt, base.GetAppSettingCurrencyCulture());

            if (trans.SaleLines != null)
            {
                foreach (TaxLine tl in trans.TaxLines)
                {
                    tl.Amount = BOLoyConnection.FormatAmount(tl.Amt, base.GetAppSettingCurrencyCulture());
                    tl.NetAmt = BOLoyConnection.FormatAmount(tl.NetAmnt, base.GetAppSettingCurrencyCulture());
                    tl.TaxAmount = BOLoyConnection.FormatAmount(tl.TaxAmt, base.GetAppSettingCurrencyCulture());
                }

                foreach (LoyTenderLine tl in trans.TenderLines)
                {
                    tl.Amount = BOLoyConnection.FormatAmount(tl.Amt, base.GetAppSettingCurrencyCulture());
                }

                foreach (LoySaleLine sl in trans.SaleLines)
                {
                    sl.Amount = BOLoyConnection.FormatAmount(sl.Amt, base.GetAppSettingCurrencyCulture());
                    sl.DiscountAmount = BOLoyConnection.FormatAmount(sl.DiscountAmt, base.GetAppSettingCurrencyCulture());
                    sl.NetAmount = BOLoyConnection.FormatAmount(sl.NetAmt, base.GetAppSettingCurrencyCulture());
                    sl.Item.Images = iImageRepository.ItemImagesByItemId(sl.Item.Id);
                }
            }
            return trans;
        }

        public virtual LoyTransaction SalesEntryGetById(string entryId)
        {
            return BOLoyConnection.SalesEntryGetById(entryId);
        }

        public virtual List<LoyTransaction> TransactionsSearch(string contactId, string itemSearch, int maxNumberOfTransactions, bool includeLines)
        {
            base.ValidateContact(contactId);
            //TODO get from NAV
            return BOLoyConnection.TransactionSearch(itemSearch, contactId, maxNumberOfTransactions, base.GetAppSettingCurrencyCulture(), includeLines);
        }
    }
}

 