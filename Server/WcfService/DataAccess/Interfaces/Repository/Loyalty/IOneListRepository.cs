using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IOneListRepository
    {
        List<OneList> OneListGetByCardId(string cardId, bool includeLines);
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);
        OneList OneListGetById(string oneListId, bool includeLines);
        void OneListSave(OneList oneList, string contactName, bool calculate);
        void OneListDeleteById(string oneListId);
        List<OneList> OneListSearch(string contactId, string search, int maxNumberOfLists, ListType listType, bool includeLines = false);
        void OneListLinking(string oneListId, string cardId, string name, LinkStatus status);
    }
}
