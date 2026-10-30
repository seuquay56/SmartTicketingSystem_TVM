using System;
using System.Threading.Tasks;

namespace SmartTicketing.Core.Models
{
    public enum TransactionStatus
    {
        Pending,
        Success,
        Declined,
        Timeout,
        Refunded
    }

    public enum PaymentMethodType
    {
        CreditCard,
        DigitalWallet
    }

    /// <summary>
    /// Abstract base class for financial payment transactions.
    /// Synchronized with UML Class Diagram 3.3.3.
    /// </summary>
    public abstract class PaymentTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public PaymentMethodType MethodType { get; set; }
        public string MachineId { get; set; } = "TVM-01";
        public string StatusMessage { get; set; } = string.Empty;

        protected PaymentTransaction(decimal amount, PaymentMethodType methodType)
        {
            TransactionId = "TXN-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            Amount = amount;
            MethodType = methodType;
        }

        public abstract Task<bool> ProcessAsync();
        public abstract Task<bool> RefundAsync();
    }
}
