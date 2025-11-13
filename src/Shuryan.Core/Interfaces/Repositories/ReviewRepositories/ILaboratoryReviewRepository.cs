using Shuryan.Core.Entities.System.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ReviewRepositories
{
    public interface ILaboratoryReviewRepository : IGenericRepository<LaboratoryReview>
    {
        Task<IEnumerable<LaboratoryReview>> GetReviewsByLaboratoryAsync(Guid laboratoryId);
        Task<IEnumerable<LaboratoryReview>> GetReviewsByPatientAsync(Guid patientId);
        Task<LaboratoryReview?> GetReviewByLabOrderAsync(Guid labOrderId);
        Task<double> GetAverageRatingForLaboratoryAsync(Guid laboratoryId);
        Task<int> GetReviewCountForLaboratoryAsync(Guid laboratoryId);
    }
}

