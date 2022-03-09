using System.Collections.Generic;
using System.ServiceModel;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.Service
{
    /// <summary>
    ///  /BOService.svc
    /// </summary>
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/BO/2017/Service")]
    public interface IBOService
    {
        #region Helpers

        [OperationContract]
        string Ping();
        [OperationContract]
        string Version();
        [OperationContract]
        OmniEnvironment Environment();

        #endregion Helpers

        #region OrderMessage

        [OperationContract]
        bool OrderMessageSave(string orderId, int status, string subject, string message);

        [OperationContract]
        bool OrderMessageStatusUpdate(OrderMessageStatus orderMessage);

        [OperationContract]
        string OrderMessageRequestPayment(string orderId, int status, decimal amount, string token, string authcode, string reference);

        [OperationContract]
        bool OrderMessageRequestPaymentEx(string orderId, int status, decimal amount, string curCode, string token, string authcode, string reference, ref string message);

        [OperationContract]
        bool OrderMessagePayment(OrderMessagePayment orderPayment, ref string message);

        [OperationContract]
        OrderMessageShippingResult OrderMessageShipping(OrderMessageShipping orderShipping);

        #endregion OrderMessage

        #region OneList

        [OperationContract]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);
        [OperationContract]
        OneList OneListGetById(string oneListId, bool includeLines);
        [OperationContract]
        OneList OneListSave(OneList oneList, bool calculate);
        [OperationContract]
        bool OneListDeleteById(string oneListId);
        [OperationContract]
        OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate);
        [OperationContract]
        bool OneListLinking(string oneListId, string cardId, string email, LinkStatus status);

        #endregion One List 

        #region LSRecommend

        [OperationContract]
        bool LSRecommendSetting(string lsKey, string batchNo, string modelReaderURL, string authenticationURL, string clientId, string clientSecret, string userName, string password, int numberOfDownloadedItems, int numberOfDisplayedItems, bool filterByInventory, decimal minInvStock);

        #endregion
    }
}
