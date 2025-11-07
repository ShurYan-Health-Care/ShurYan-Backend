using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuryan.Core.Entities.External.Payments;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment?> GetPaymentWithTransactionsAsync(Guid paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(string orderType, Guid orderId);
        Task<Payment?> GetPaymentByProviderTransactionIdAsync(string providerTransactionId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync(int pageNumber = 1, int pageSize = 10);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetPaymentCountByStatusAsync(PaymentStatus status);
    }
}
