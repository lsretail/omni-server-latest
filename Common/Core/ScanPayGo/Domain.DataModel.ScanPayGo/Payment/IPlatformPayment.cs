using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment
{
    public class OnPaymentSuccessEventArgs
    {
        public string Token { get; set; }
        public string CardNetwork { get; set; }
        public string CardDetails { get; set; }
        public string Name { get; set; }
        public Address BillingAddress { get; set; }
        public PaymentType Type { get; set; } 

        public enum PaymentType
        {
            Tokenized,
            Autherized
        }

        public override string ToString()
        {
            return $"{nameof(Token)}: {Token}, {nameof(CardNetwork)}: {CardNetwork}, {nameof(CardDetails)}: {CardDetails}, {nameof(Name)}: {Name}, {nameof(BillingAddress)}: {BillingAddress}";
        }
    }

    public interface IPlatformPayment
    {
        event EventHandler<OnPaymentSuccessEventArgs> OnPaymentSuccessful;
        event EventHandler<string> OnTokenizationError;
        event EventHandler OnTokenizationCancelled;

        string ButtonText { get; }

        void Init(ScanPayGoSetup setup);
        Task<bool> IsAvailable();
        void MakePayment(decimal price);
    }
}
