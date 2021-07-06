using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessage
    {
        [DataMember]
        public string OrderId { get; set; }
        [DataMember]
        public string CardId { get; set; }
        [DataMember]
        public string HeaderStatus { get; set; }
        [DataMember]
        public string ExtOrderStatus { get; set; }
        [DataMember]
        public string MsgSubject { get; set; }
        [DataMember]
        public string MsgDetail { get; set; }
        [DataMember]
        public List<OrderMessageLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessageLine
    {
        [DataMember]
        public string LineNo { get; set; }
        [DataMember]
        public string PrevStatus { get; set; }
        [DataMember]
        public string NewStatus { get; set; }
        [DataMember]
        public string ExtLineStatus { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessagePayment
    {
        [DataMember]
        public string OrderId { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public string AuthCode { get; set; }
        [DataMember]
        public string Reference { get; set; }
        [DataMember]
        public List<OrderMessagePaymentLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessagePaymentLine
    {
        [DataMember]
        public string LineNo { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class OrderMessageResult
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string message { get; set; }
    }
}
