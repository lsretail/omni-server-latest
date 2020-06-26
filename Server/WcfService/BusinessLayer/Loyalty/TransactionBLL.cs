using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSOmni.BLL.Loyalty
{
    public class TransactionBLL : BaseLoyBLL
    {
        private string tenderMapping;

        public TransactionBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
            tenderMapping = config.SettingsGetByKey(ConfigKey.TenderType_Mapping);   //will throw exception if not found
        }

        public List<SalesEntry> SalesEntriesGetByCardId(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries)
        {
            return BOLoyConnection.SalesEntriesGetByCardId(cardId, storeId, date, dateGreaterThan , maxNumberOfEntries);
        }

        public virtual SalesEntry SalesEntryGet(string entryId, DocumentIdType type)
        {
            if (string.IsNullOrEmpty(entryId))
                throw new LSOmniException(StatusCode.TransacitionIdMissing, "Id can not be empty");

            return BOLoyConnection.SalesEntryGet(entryId, type, tenderMapping);
        }
    }
}

 