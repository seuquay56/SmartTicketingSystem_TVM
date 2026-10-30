using System;
using System.Threading.Tasks;

namespace SmartTicketing.Core.Models
{
    /// <summary>
    /// Handles Contactless / EMV Chip Credit & Debit Card payments.
    /// Synchronized with UML Class Diagram 3.3.3.
    /// </summary>
    public class CreditCardTransaction : PaymentTransaction
    {
        public string MaskedCardNumber { get; set; } = "•••• •••• •••• 4242";
        public string CardHolderName { get; set; } = "CARDHOLDER / VALUED GUEST";
        public string BankApprovalCode { get; set; } = string.Empty;
        public string CardType { get; set; } = "Visa Contactless";

        // Simulation flag for testing exception flows
        public bool SimulateDecline { get; set; } = false;

        public CreditCardTransaction(decimal amount) 
            : base(amount, PaymentMethodType.CreditCard)
        {
        }

        public override async Task<bool> ProcessAsync()
        {
            // Simulate Payment Gateway API round-trip delay
            await Task.Delay(1200);

            if (SimulateDecline)
            {
                Status = TransactionStatus.Declined;
                StatusMessage = "Bank declined transaction: Insufficient funds or card issuer restriction.";
                return false;
            }

            // Successful approval
            Status = TransactionStatus.Success;
            BankApprovalCode = "AUTH-" + new Random().Next(100000, 999999);
            StatusMessage = $"Payment approved by bank. Auth code: {BankApprovalCode}";
            return true;
        }

        public override async Task<bool> RefundAsync()
        {
            await Task.Delay(800);
            Status = TransactionStatus.Refunded;
            StatusMessage = "Transaction fully refunded back to card.";
            return true;
        }
    }
}
