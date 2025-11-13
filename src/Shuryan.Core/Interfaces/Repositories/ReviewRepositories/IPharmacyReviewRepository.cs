using Shuryan.Core.Entities.System.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ReviewRepositories
{
    public interface IPharmacyReviewRepository : IGenericRepository<PharmacyReview>
    {
        Task<IEnumerable<PharmacyReview>> GetByPharmacyIdAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyReview>> GetReviewsByPharmacyAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyReview>> GetReviewsByPatientAsync(Guid patientId);
        Task<PharmacyReview?> GetReviewByPharmacyOrderAsync(Guid pharmacyOrderId);
        Task<double> GetAverageRatingForPharmacyAsync(Guid pharmacyId);
        Task<int> GetReviewCountForPharmacyAsync(Guid pharmacyId);
    }
}

