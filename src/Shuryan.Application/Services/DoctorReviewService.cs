using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Review;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class DoctorReviewService : IDoctorReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorReviewService> _logger;

        public DoctorReviewService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DoctorReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResponse<DoctorReviewListItemResponse>> GetDoctorReviewsAsync(
            Guid doctorId, 
            PaginationParams paginationParams)
        {
            try
            {
                var (reviews, totalCount) = await _unitOfWork.DoctorReviews
                    .GetPaginatedReviewsByDoctorAsync(doctorId, paginationParams.PageNumber, paginationParams.PageSize);

                var reviewItems = reviews.Select(review => new DoctorReviewListItemResponse
                {
                    ReviewId = review.Id,
                    PatientId = review.PatientId,
                    PatientName = $"{review.Patient.FirstName} {review.Patient.LastName}".Trim(),
                    PatientProfileImage = review.Patient.ProfileImageUrl ?? review.Patient.ProfilePictureUrl,
                    Rating = (int)Math.Round(review.AverageRating),
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                }).ToList();

                return new PaginatedResponse<DoctorReviewListItemResponse>
                {
                    Data = reviewItems,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize),
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated reviews for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorReviewStatisticsResponse> GetReviewStatisticsAsync(Guid doctorId)
        {
            try
            {
                var averageRating = await _unitOfWork.DoctorReviews.GetAverageRatingForDoctorAsync(doctorId);
                var totalReviews = await _unitOfWork.DoctorReviews.GetReviewCountForDoctorAsync(doctorId);
                var ratingDistribution = await _unitOfWork.DoctorReviews.GetRatingDistributionAsync(doctorId);

                return new DoctorReviewStatisticsResponse
                {
                    AverageRating = Math.Round(averageRating, 1),
                    TotalReviews = totalReviews,
                    RatingDistribution = ratingDistribution.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => kvp.Value
                    )
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorReviewDetailsResponse?> GetReviewDetailsAsync(Guid reviewId, Guid doctorId)
        {
            try
            {
                var review = await _unitOfWork.DoctorReviews.GetReviewByIdWithPatientAsync(reviewId, doctorId);
                
                if (review == null)
                {
                    _logger.LogWarning("Review {ReviewId} not found for doctor {DoctorId}", reviewId, doctorId);
                    return null;
                }

                return new DoctorReviewDetailsResponse
                {
                    ReviewId = review.Id,
                    PatientId = review.PatientId,
                    PatientName = $"{review.Patient.FirstName} {review.Patient.LastName}".Trim(),
                    PatientProfileImage = review.Patient.ProfileImageUrl ?? review.Patient.ProfilePictureUrl,
                    OverallSatisfaction = review.OverallSatisfaction,
                    WaitingTime = review.WaitingTime,
                    CommunicationQuality = review.CommunicationQuality,
                    ClinicCleanliness = review.ClinicCleanliness,
                    ValueForMoney = review.ValueForMoney,
                    AverageRating = review.AverageRating,
                    Comment = review.Comment,
                    HasDoctorReply = !string.IsNullOrWhiteSpace(review.DoctorReply),
                    DoctorReply = review.DoctorReply,
                    DoctorRepliedAt = review.DoctorRepliedAt,
                    CreatedAt = review.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review details {ReviewId} for doctor {DoctorId}", reviewId, doctorId);
                throw;
            }
        }

        public async Task<DoctorReviewDetailsResponse> ReplyToReviewAsync(
            Guid reviewId, 
            Guid doctorId, 
            ReplyToReviewRequest request)
        {
            try
            {
                var review = await _unitOfWork.DoctorReviews.GetReviewByIdWithPatientAsync(reviewId, doctorId);
                
                if (review == null)
                {
                    _logger.LogWarning("Review {ReviewId} not found for doctor {DoctorId}", reviewId, doctorId);
                    throw new InvalidOperationException($"Review with ID {reviewId} not found for doctor {doctorId}");
                }

                review.DoctorReply = request.Reply;
                review.DoctorRepliedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor {DoctorId} replied to review {ReviewId}", doctorId, reviewId);

                return new DoctorReviewDetailsResponse
                {
                    ReviewId = review.Id,
                    PatientId = review.PatientId,
                    PatientName = $"{review.Patient.FirstName} {review.Patient.LastName}".Trim(),
                    PatientProfileImage = review.Patient.ProfileImageUrl ?? review.Patient.ProfilePictureUrl,
                    OverallSatisfaction = review.OverallSatisfaction,
                    WaitingTime = review.WaitingTime,
                    CommunicationQuality = review.CommunicationQuality,
                    ClinicCleanliness = review.ClinicCleanliness,
                    ValueForMoney = review.ValueForMoney,
                    AverageRating = review.AverageRating,
                    Comment = review.Comment,
                    HasDoctorReply = true,
                    DoctorReply = review.DoctorReply,
                    DoctorRepliedAt = review.DoctorRepliedAt,
                    CreatedAt = review.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to review {ReviewId} for doctor {DoctorId}", reviewId, doctorId);
                throw;
            }
        }
    }
}

