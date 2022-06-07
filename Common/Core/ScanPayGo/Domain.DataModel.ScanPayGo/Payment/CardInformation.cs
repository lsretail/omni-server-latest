using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment
{
    public class CardInformation
    {
        public string MaskedCardNumber { get; set; }
        public string CardToken { get; set; }
        public string CardIssuer { get; set; }
        public string ExpiryDate { get; set; }

        public string CardLastFour
        {
            get
            {
                return "- " + MaskedCardNumber.Substring((Math.Max(0, ((MaskedCardNumber.Length - 4)))));
            }
        }
    }
}
