using System;
using System.Threading.Tasks;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment
{
    public class DropUIResult
    {
        public string Nonce { get; set; }
        public string Type { get; set; }
    }

    public interface IBraintreePaymentService
    {
        event EventHandler<string> OnTokenizationSuccessful;

        event EventHandler<string> OnTokenizationError;

        event EventHandler<DropUIResult> OnDropUISuccessful;

        event EventHandler<string> OnDropUIError;

        bool CanPay { get; }

        Task<bool> InitializeAsync(string clientToken);

        Task<string> TokenizeCard(string panNumber, string expirationMonth, string expirationYear, string cvv);

        Task<string> TokenizePlatform(double totalPrice, string merchantId);

        Task<DropUIResult> ShowDropUI(string clientToken, double totalPrice, string merchantId, int requestCode = 1234);
    }
}
