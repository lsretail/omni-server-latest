using System.Collections.Generic;

namespace LSRetail.Omni.Domain.DataModel.Pos.Utils
{
    public class QRCodeObject
    {
        public string ContactId { get; set; }
        public string AccountId { get; set; }
        public string CardId { get; set; }
        public List<string> Coupons { get; set; }
        public List<string> Offers { get; set; }
    }
}
