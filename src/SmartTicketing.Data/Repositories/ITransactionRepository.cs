using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTicketing.Core.Models;

namespace SmartTicketing.Data.Repositories
{
    public interface ITransactionRepository
    {
        Task<bool> SaveTransactionAsync(PaymentTransaction transaction, List<Ticket> tickets);
        Task<List<PaymentTransaction>> GetAllTransactionsAsync();
        Task<List<Ticket>> GetAllIssuedTicketsAsync();
        Task<bool> LogAuditEventAsync(string machineId, string eventType, string severity, string message);
    }
}
