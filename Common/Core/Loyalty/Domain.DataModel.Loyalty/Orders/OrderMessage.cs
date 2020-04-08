using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Orders
{
    public class OrderMessage
    {
        public string OrderId { get; set; }
        public string CardId { get; set; }
        public string HeaderStatus { get; set; }
        public string MsgSubject { get; set; }
        public string MsgDetail { get; set; }
        public List<OrderMessageLine> Lines { get; set; }
    }

    public class OrderMessageLine
    {
        public string LineNo { get; set; }
        public string PrevStatus { get; set; }
        public string NewStatus { get; set; }
    }
}
