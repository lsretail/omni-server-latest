using System;
using System.Collections.Generic;

using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class TransactionBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private IImageRepository iImageRepository;
        private string tenderMapping;

        public TransactionBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            this.iImageRepository = GetDbRepository<IImageRepository>(config);
            
            tenderMapping = config.SettingsGetByKey(ConfigKey.TenderType_Mapping);   //will throw exception if not found
        }

        public List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfTransactions)
        {
            //base.ValidateContact(contactId);
            return BOLoyConnection.SalesEntriesGetByCardId(cardId, maxNumberOfTransactions, base.GetAppSettingCurrencyCulture());
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type)
        {
            if (string.IsNullOrEmpty(entryId))
                throw new LSOmniException(StatusCode.Error, "Id can not be empty");

            return BOLoyConnection.SalesEntryGet(entryId, type, tenderMapping);
        }
    }
}

 