using System;
using System.Threading.Tasks;

namespace SmartTicketing.Core.Models
{
    /// <summary>
    /// Handles Dynamic QR Code payments via E-Wallet and Mobile Banking APIs.
    /// Synchronized with UML Class Diagram 3.3.3 and strict 60s timeout requirement.
    /// </summary>
    public class DigitalWalletTransaction : PaymentTransaction
    {
        public string WalletProvider { get; set; } = "Banking / E-wallet QR";
        public string QRCodeData { get; set; } = string.Empty;
        public string GatewayReferenceId { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 60;
        public int RemainingSeconds { get; set; } = 60;

        // Simulation flag for testing timeout flow
        public bool SimulateTimeout { get; set; } = false;

        public DigitalWalletTransaction(decimal amount, string provider = "Banking / E-wallet QR")
            : base(amount, PaymentMethodType.DigitalWallet)
        {
            WalletProvider = provider;
            GatewayReferenceId = "GW-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            GenerateDynamicQRCode();
        }

        public void GenerateDynamicQRCode()
        {
            // Standard VietQR / EMVCo dynamic QR string format
            QRCodeData = $"METROLINK|ORDER:{TransactionId}|AMT:{Amount:F2}|REF:{GatewayReferenceId}|EXP:{DateTime.UtcNow.AddSeconds(60):O}";
            RemainingSeconds = TimeoutSeconds;
        }

        public override async Task<bool> ProcessAsync()
        {
            await Task.Delay(1000);

            if (SimulateTimeout)
            {
                Status = TransactionStatus.Timeout;
                StatusMessage = "Transaction cancelled: 60-second QR payment window expired.";
                return false;
            }

            Status = TransactionStatus.Success;
            StatusMessage = $"Payment confirmed via {WalletProvider}. Ref: {GatewayReferenceId}";
            return true;
        }

        public async Task<TransactionStatus> PollPaymentStatusAsync()
        {
            await Task.Delay(200);
            return Status;
        }

        public override async Task<bool> RefundAsync()
        {
            await Task.Delay(600);
            Status = TransactionStatus.Refunded;
            StatusMessage = $"Payment reversed to {WalletProvider} balance.";
            return true;
        }
    }
}
