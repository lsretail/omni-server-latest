using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IOneListRepository
    {
        List<OneList> OneListGetByCardId(string cardId, bool includeLines);
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines = false);
        OneList OneListGetById(string oneListId, ListType listType, bool includeLines = true);
        void OneListSave(OneList oneList, bool calculate);
        void OneListDeleteById(string oneListId, ListType listType);
        List<OneList> OneListSearch(string contactId, string search, int maxNumberOfLists, ListType listType, bool includeLines = false);
    }
}
