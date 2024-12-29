using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IOneListRepository
    {
        List<OneList> OneListGetByCardId(string cardId, bool includeLines, Statistics stat);
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines, Statistics stat);
        OneList OneListGetById(string oneListId, bool includeLines, Statistics stat);
        void OneListSave(OneList oneList, string contactName, bool calculate, Statistics stat);
        void OneListDeleteById(string oneListId, Statistics stat);
        List<OneList> OneListSearch(string contactId, string search, int maxNumberOfLists, ListType listType, bool includeLines, Statistics stat);
        void OneListLinking(string oneListId, string cardId, string name, LinkStatus status, Statistics stat);
        void OneListRemoveLinking(string cardId, Statistics stat);
        List<OneListLink> OneListLinkGetById(string oneListId);
    }
}
