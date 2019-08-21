using System.Collections.Generic;
using System.ServiceModel;

using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
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
        SalesEntry OrderCreate(Order request);

        #endregion OrderQueue

        #region OrderMessage

        [OperationContract]
        void OrderMessageSave(string orderId, int status, string subject, string message);
        [OperationContract]
        string OrderMessageRequestPayment(string orderId, int status, decimal amount, string token);

        #endregion OrderMessage

        #region One List

        [OperationContract]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);
        [OperationContract]
        OneList OneListGetById(string oneListId, ListType listType, bool includeLines);
        [OperationContract]
        OneList OneListSave(OneList oneList, bool calculate);
        [OperationContract]
        bool OneListDeleteById(string oneListId, ListType listType);

        #endregion One List 

        #region LSRecommend

        [OperationContract]
        void LSRecommendSetting(string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock);

        #endregion
    }
}
