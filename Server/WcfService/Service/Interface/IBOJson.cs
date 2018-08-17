using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Orders;

namespace LSOmni.Service
{
    /// <summary>
    /// /BOJson.svc
    /// </summary>
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/BO/2017/Json")]
    public interface IBOJson
    {
        #region Helpers
        [OperationContract]
        [WebGet(UriTemplate = "/Ping", ResponseFormat = WebMessageFormat.Json)]
        string PingGet(); //REST http://localhost/LSOmniService/Json.svc/ping 

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Ping();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        StatusCode PingStatus(); //exposing the StatusCode to client

        //both GET and POST of Version supported
        [OperationContract]
        [WebGet(UriTemplate = "/Version", ResponseFormat = WebMessageFormat.Json)]
        string VersionGet();
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Version();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OmniEnvironment Environment();
        #endregion Helpers

        #region OrderQueue

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderQueue OrderQueueSave(OrderQueue order);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderQueue OrderQueueGetById(string orderId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderQueueUpdateStatus(string orderId, OrderQueueStatus status);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OrderQueue> OrderQueueSearch(OrderSearchRequest searchRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Order> OrderSearchClickCollect(OrderSearchRequest searchRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OrderCreate(Order request);

        #endregion OrderQueue

        #region OrderMessage

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderMessage OrderMessageSave(OrderMessage orderMessage);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderMessage OrderMessageGetById(string id);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OrderMessage> OrderMessageSearch(OrderMessageSearchRequest searchRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OrderMessageRequestPayment(string orderId, OrderMessagePayStatus status, decimal amount, string token);

        #endregion OrderMessage

        #region One List  LOY
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByContactId(string contactId, ListType listType, bool includeLines);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string oneListId, ListType listType, bool includeLines);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListSave(OneList oneList, bool calculate);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId, ListType listType);
        #endregion One List 

        #region Common functions

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool UserDelete(string userName);

        #endregion

        #region LSRecommend

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        void LSRecommendSetting(string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock);

        #endregion
    }
}
