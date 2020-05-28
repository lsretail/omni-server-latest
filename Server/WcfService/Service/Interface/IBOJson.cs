using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

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

        #region OrderMessage

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        void OrderMessageSave(string orderId, int status, string subject, string message);

        /// <summary>
        /// Send Order Status to ECOM platform and Notifications to Devices
        /// </summary>
        /// <remarks>
        /// Notifications are only sent if MsgSubject/Details are provided, if not only status data will be sent to ECom
        /// </remarks>
        /// <example>
        /// Sample request
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        ///{
        ///    "orderMessage": {
        ///        "OrderId": "CO000001",
        ///        "CardId": "10021",
        ///        "HeaderStatus": "OPEN",
        ///        "MsgSubject": "your order 001",
        ///        "MsgDetail": "is ready for delivery",
        ///        "Lines":
        ///        [{
        ///                "LineNo": "1",
        ///                "PrevStatus": "OPEN",
        ///                "NewStatus": "PICKING"
        ///            }, {
        ///                "LineNo": "2",
        ///                "PrevStatus": "OPEN",
        ///                "NewStatus": "PACKING"
        ///            }
        ///        ]
        ///    }
        ///}
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="orderMessage"></param>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        void OrderMessageStatusUpdate(OrderMessage orderMessage);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OrderMessageRequestPayment(string orderId, int status, decimal amount, string token, string authcode, string reference);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderMessageRequestPaymentEx(string orderId, int status, decimal amount, string token, string authcode, string reference, ref string message);

        #endregion OrderMessage

        #region OneList

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string oneListId, bool includeLines);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListSave(OneList oneList, bool calculate);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListItemModify(string onelistId, OneListItem item, bool remove, bool calculate);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListLinking(string oneListId, string cardId, string email, LinkStatus status);

        #endregion One List 

        #region LSRecommend

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        void LSRecommendSetting(string lsKey, string endPointUrl, string accountConnection, string azureAccountKey, string azureName, int numberOfRecommendedItems, bool calculateStock, string wsURI, string wsUserName, string wsPassword, string wsDomain, string storeNo, string location, int minStock);

        #endregion
    }
}
