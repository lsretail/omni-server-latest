using System.Collections.Generic;
using System.ServiceModel;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;
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
        StatusCode PingStatus(); //exposing the StatusCode to client
        [OperationContract]
        string Version();
        [OperationContract]
        OmniEnvironment Environment();

        #endregion Helpers

        #region OrderQueue

        [OperationContract]
        OrderQueue OrderQueueSave(OrderQueue order);
        [OperationContract]
        OrderQueue OrderQueueGetById(string orderId);
        [OperationContract]
        bool OrderQueueUpdateStatus(string orderId, OrderQueueStatus status);
        [OperationContract]
        List<OrderQueue> OrderQueueSearch(OrderSearchRequest searchRequest);
        [OperationContract]
        List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest);
        [OperationContract]
        Order OrderCreate(Order request);

        #endregion OrderQueue

        #region OrderMessage

        [OperationContract]
        OrderMessage OrderMessageSave(OrderMessage orderMessage);
        [OperationContract]
        OrderMessage OrderMessageGetById(string id);
        [OperationContract]
        List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest searchRequest);
        [OperationContract]
        string OrderMessageRequestPayment(string orderId, OrderMessagePayStatus status, decimal amount, string token);

        #endregion OrderMessage

        #region One List

        [OperationContract]
        List<OneList> OneListGetByContactId(string contactId, ListType listType, bool includeLines);
        [OperationContract]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);
        [OperationContract]
        OneList OneListGetById(string oneListId, ListType listType, bool includeLines);
        [OperationContract]
        OneList OneListSave(OneList oneList, bool calculate);
        [OperationContract]
        bool OneListDeleteById(string oneListId, ListType listType);

        #endregion One List 

        #region Common functions

        [OperationContract]
        bool UserDelete(string userName);

        #endregion

        #region LSRecommend

        [OperationContract]
        void LSRecommendSetting(string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock);

        #endregion
    }
}
