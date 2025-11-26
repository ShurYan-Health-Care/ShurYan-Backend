using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Review;
using Shuryan.Application.DTOs.Responses.Review;
using System;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface IDoctorReviewService
    {
        /// <summary>
        /// Create a new review for a doctor (Patient only)
        /// </summary>
        Task<DoctorReviewResponse> CreateReviewAsync(Guid patientId, CreateDoctorReviewRequest request);

        /// <summary>
        /// Get paginated reviews for a doctor
        /// </summary>
        Task<PaginatedResponse<DoctorReviewListItemResponse>> GetDoctorReviewsAsync(Guid doctorId, PaginationParams paginationParams);

        /// <summary>
        /// Get review statistics for a doctor
        /// </summary>
        Task<DoctorReviewStatisticsResponse> GetReviewStatisticsAsync(Guid doctorId);

        /// <summary>
        /// Get detailed review by ID
        /// </summary>
        Task<DoctorReviewDetailsResponse?> GetReviewDetailsAsync(Guid reviewId, Guid doctorId);

        /// <summary>
        /// Reply to a review
        /// </summary>
        Task<DoctorReviewDetailsResponse> ReplyToReviewAsync(Guid reviewId, Guid doctorId, ReplyToReviewRequest request);

        /// <summary>
        /// Get paginated public reviews for a specific doctor (for patients to view)
        /// </summary>
        Task<PaginatedResponse<DoctorReviewListItemResponse>> GetPublicDoctorReviewsAsync(Guid doctorId, PaginationParams paginationParams);

        /// <summary>
        /// Get public review statistics for a specific doctor (for patients to view)
        /// </summary>
        Task<DoctorReviewStatisticsResponse> GetPublicReviewStatisticsAsync(Guid doctorId);
    }
}

