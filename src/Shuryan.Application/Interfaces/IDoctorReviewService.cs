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
    }
}

