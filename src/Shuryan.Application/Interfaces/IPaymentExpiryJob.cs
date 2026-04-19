namespace Shuryan.Application.Interfaces
{
    /// <summary>
    /// Background job to expire stale pending payments.
    /// Runs periodically via Hangfire to cancel payments that have been
    /// in Pending status for more than 1 hour.
    /// </summary>
    public interface IPaymentExpiryJob
    {
        Task ExpirePendingPaymentsAsync();
    }
}
