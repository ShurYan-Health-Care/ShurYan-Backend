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

        public async Task<DoctorReviewResponse> CreateReviewAsync(Guid patientId, CreateDoctorReviewRequest request)
        {
            try
            {
                _logger.LogInformation("Creating review for appointment {AppointmentId} by patient {PatientId}",
                    request.AppointmentId, patientId);

                // 1. Validate appointment exists
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", request.AppointmentId);
                    throw new ArgumentException($"Appointment with ID {request.AppointmentId} not found");
                }

                // 2. Validate patient owns this appointment
                if (appointment.PatientId != patientId)
                {
                    _logger.LogWarning("Patient {PatientId} attempted to review appointment {AppointmentId} that doesn't belong to them",
                        patientId, request.AppointmentId);
                    throw new UnauthorizedAccessException("You can only review your own appointments");
                }

                // 3. Validate appointment is completed
                if (appointment.Status != Core.Enums.Appointments.AppointmentStatus.Completed)
                {
                    _logger.LogWarning("Attempted to review non-completed appointment {AppointmentId}. Status: {Status}",
                        request.AppointmentId, appointment.Status);
                    throw new InvalidOperationException("You can only review completed appointments");
                }

                // 4. Check if review already exists
                var existingReview = await _unitOfWork.DoctorReviews.GetReviewByAppointmentAsync(request.AppointmentId);
                if (existingReview != null)
                {
                    _logger.LogWarning("Review already exists for appointment {AppointmentId}", request.AppointmentId);
                    throw new InvalidOperationException("You have already reviewed this appointment");
                }

                // 5. Create the review
                var review = new Core.Entities.System.Review.DoctorReview
                {
                    AppointmentId = request.AppointmentId,
                    PatientId = patientId,
                    DoctorId = appointment.DoctorId,
                    OverallSatisfaction = request.OverallSatisfaction,
                    WaitingTime = request.WaitingTime,
                    CommunicationQuality = request.CommunicationQuality,
                    ClinicCleanliness = request.ClinicCleanliness,
                    ValueForMoney = request.ValueForMoney,
                    Comment = request.Comment?.Trim(),
                    IsAnonymous = request.IsAnonymous,
                    IsEdited = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DoctorReviews.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Review {ReviewId} created successfully for appointment {AppointmentId}",
                    review.Id, request.AppointmentId);

                // 6. Return response
                return new DoctorReviewResponse
                {
                    Id = review.Id,
                    AppointmentId = review.AppointmentId,
                    PatientId = review.PatientId,
                    DoctorId = review.DoctorId,
                    OverallSatisfaction = review.OverallSatisfaction,
                    WaitingTime = review.WaitingTime,
                    CommunicationQuality = review.CommunicationQuality,
                    ClinicCleanliness = review.ClinicCleanliness,
                    ValueForMoney = review.ValueForMoney,
                    Comment = review.Comment,
                    IsAnonymous = review.IsAnonymous,
                    IsEdited = review.IsEdited,
                    DoctorReply = review.DoctorReply,
                    DoctorRepliedAt = review.DoctorRepliedAt,
                    CreatedAt = review.CreatedAt
                };
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not UnauthorizedAccessException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Error creating review for appointment {AppointmentId}", request.AppointmentId);
                throw;
            }
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
                    PatientName = review.IsAnonymous ? "مريض مجهول" : $"{review.Patient.FirstName} {review.Patient.LastName}".Trim(),
                    PatientProfileImage = review.IsAnonymous ? null : (review.Patient.ProfileImageUrl ?? review.Patient.ProfilePictureUrl),
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
                    PatientName = review.IsAnonymous ? "مريض مجهول" : $"{review.Patient.FirstName} {review.Patient.LastName}".Trim(),
                    PatientProfileImage = review.IsAnonymous ? null : (review.Patient.ProfileImageUrl ?? review.Patient.ProfilePictureUrl),
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

        public async Task<PaginatedResponse<DoctorReviewListItemResponse>> GetPublicDoctorReviewsAsync(
            Guid doctorId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting public reviews for doctor {DoctorId}, Page: {Page}, PageSize: {PageSize}",
                    doctorId, paginationParams.PageNumber, paginationParams.PageSize);

                var (reviews, totalCount) = await _unitOfWork.DoctorReviews
                    .GetPaginatedReviewsByDoctorAsync(doctorId, paginationParams.PageNumber, paginationParams.PageSize);

                var reviewItems = reviews.Select(review => new DoctorReviewListItemResponse
                {
                    ReviewId = review.Id,
                    PatientId = review.IsAnonymous ? Guid.Empty : review.PatientId,
                    PatientName = review.IsAnonymous ? "مريض مجهول" : $"{review.Patient.FirstName} {review.Patient.LastName}".Trim(),
                    PatientProfileImage = review.IsAnonymous ? null : (review.Patient.ProfileImageUrl ?? review.Patient.ProfilePictureUrl),
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
                _logger.LogError(ex, "Error retrieving public reviews for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorReviewStatisticsResponse> GetPublicReviewStatisticsAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting public review statistics for doctor {DoctorId}", doctorId);

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
                _logger.LogError(ex, "Error retrieving public review statistics for doctor {DoctorId}", doctorId);
                throw;
            }
        }
    }
}

