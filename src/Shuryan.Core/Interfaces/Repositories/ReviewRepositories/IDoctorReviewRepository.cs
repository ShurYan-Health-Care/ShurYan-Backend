using Shuryan.Core.Entities.System.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ReviewRepositories
{
    public interface IDoctorReviewRepository : IGenericRepository<DoctorReview>
    {
        Task<IEnumerable<DoctorReview>> GetReviewsByDoctorAsync(Guid doctorId);
        Task<IEnumerable<DoctorReview>> GetReviewsByPatientAsync(Guid patientId);
        Task<DoctorReview?> GetReviewByAppointmentAsync(Guid appointmentId);
        Task<double> GetAverageRatingForDoctorAsync(Guid doctorId);
        Task<int> GetReviewCountForDoctorAsync(Guid doctorId);
        
        // Pagination support
        Task<(IEnumerable<DoctorReview> Reviews, int TotalCount)> GetPaginatedReviewsByDoctorAsync(
            Guid doctorId, 
            int pageNumber, 
            int pageSize);
        
        // Statistics support
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid doctorId);
        
        // Get review with patient details
        Task<DoctorReview?> GetReviewByIdWithPatientAsync(Guid reviewId, Guid doctorId);
        
        /// <summary>
        /// جلب متوسط التقييمات لمجموعة دكاترة دفعة واحدة (batch operation)
        /// </summary>
        Task<Dictionary<Guid, double?>> GetAverageRatingsForDoctorsAsync(IEnumerable<Guid> doctorIds);
    }
}

