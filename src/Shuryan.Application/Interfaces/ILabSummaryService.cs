using Shuryan.Application.DTOs.Responses.LabTests;

namespace Shuryan.Application.Interfaces
{
    public interface ILabSummaryService
    {
        Task<LabSummaryResponse> SummarizeLabOrderAsync(Guid patientId, Guid orderId);
    }
}
