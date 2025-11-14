using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Extensions;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{

    public class VerifierService : IVerifierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VerifierService> _logger;

        public VerifierService(IUnitOfWork unitOfWork, ILogger<VerifierService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Doctor Verification Status Management
        public async Task<bool> StartDoctorReviewAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Starting review for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                doctor.VerificationStatus = VerificationStatus.UnderReview;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Review started for doctor {DoctorId}", doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting review for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> VerifyDoctorAsync(Guid doctorId, Guid verifierId)
        {
            try
            {
                _logger.LogInformation("Verifying doctor {DoctorId} by verifier {VerifierId}", doctorId, verifierId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                doctor.VerificationStatus = VerificationStatus.Verified;
                doctor.VerifiedAt = DateTime.UtcNow;
                doctor.UpdatedAt = DateTime.UtcNow;
                doctor.VerifierId = verifierId;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor {DoctorId} has been verified by verifier {VerifierId}", doctorId, verifierId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying doctor {DoctorId}", doctorId);
                throw;
            }
        }


        public async Task<bool> RejectDoctorAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Rejecting doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                doctor.VerificationStatus = VerificationStatus.Rejected;
                doctor.VerifiedAt = null;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor {DoctorId} has been rejected", doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Get Doctors by Verification Status
        public async Task<PaginatedResponse<DoctorVerificationListResponse>> GetDoctorsWithSentStatusAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting doctors with Sent status. Page: {Page}, Size: {Size}",
                    paginationParams.PageNumber, paginationParams.PageSize);

                var allDoctors = await _unitOfWork.Doctors.GetAllAsync();
                var sentDoctors = allDoctors.Where(d => d.VerificationStatus == VerificationStatus.Sent).ToList();

                var totalCount = sentDoctors.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedDoctors = sentDoctors
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var doctorResponses = await BuildDoctorVerificationListResponses(paginatedDoctors);

                _logger.LogInformation("Successfully retrieved {Count} doctors with Sent status out of {Total}",
                    doctorResponses.Count, totalCount);

                return new PaginatedResponse<DoctorVerificationListResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = doctorResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors with Sent status");
                throw;
            }
        }

        public async Task<PaginatedResponse<DoctorVerificationListResponse>> GetDoctorsUnderReviewAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting doctors under review. Page: {Page}, Size: {Size}",
                    paginationParams.PageNumber, paginationParams.PageSize);

                var allDoctors = await _unitOfWork.Doctors.GetAllAsync();
                var underReviewDoctors = allDoctors.Where(d => d.VerificationStatus == VerificationStatus.UnderReview).ToList();

                var totalCount = underReviewDoctors.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedDoctors = underReviewDoctors
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var doctorResponses = await BuildDoctorVerificationListResponses(paginatedDoctors);

                _logger.LogInformation("Successfully retrieved {Count} doctors under review out of {Total}",
                    doctorResponses.Count, totalCount);

                return new PaginatedResponse<DoctorVerificationListResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = doctorResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors under review");
                throw;
            }
        }

        public async Task<PaginatedResponse<DoctorVerificationListResponse>> GetVerifiedDoctorsAsync(PaginationParams paginationParams, Guid verifierId)
        {
            try
            {
                _logger.LogInformation("Getting verified doctors by verifier {VerifierId}. Page: {Page}, Size: {Size}",
                    verifierId, paginationParams.PageNumber, paginationParams.PageSize);

                var allDoctors = await _unitOfWork.Doctors.GetVerifiedDoctorsAsync();
                
                var verifiedByCurrentVerifier = allDoctors.Where(d => d.VerifierId == verifierId).ToList();

                var totalCount = verifiedByCurrentVerifier.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedDoctors = verifiedByCurrentVerifier
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var doctorResponses = await BuildDoctorVerificationListResponses(paginatedDoctors);

                _logger.LogInformation("Successfully retrieved {Count} verified doctors by verifier {VerifierId} out of {Total}",
                    doctorResponses.Count, verifierId, totalCount);

                return new PaginatedResponse<DoctorVerificationListResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = doctorResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verified doctors for verifier {VerifierId}", verifierId);
                throw;
            }
        }

        public async Task<PaginatedResponse<DoctorVerificationListResponse>> GetRejectedDoctorsAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting rejected doctors. Page: {Page}, Size: {Size}",
                    paginationParams.PageNumber, paginationParams.PageSize);

                var allDoctors = await _unitOfWork.Doctors.GetAllAsync();
                var rejectedDoctors = allDoctors.Where(d => d.VerificationStatus == VerificationStatus.Rejected).ToList();

                var totalCount = rejectedDoctors.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedDoctors = rejectedDoctors
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var doctorResponses = await BuildDoctorVerificationListResponses(paginatedDoctors);

                _logger.LogInformation("Successfully retrieved {Count} rejected doctors out of {Total}",
                    doctorResponses.Count, totalCount);

                return new PaginatedResponse<DoctorVerificationListResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = doctorResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rejected doctors");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method لبناء DoctorVerificationListResponse من قائمة الأطباء
        /// </summary>
        private async Task<List<DoctorVerificationListResponse>> BuildDoctorVerificationListResponses(List<Core.Entities.Identity.Doctor> doctors)
        {
            var doctorResponses = new List<DoctorVerificationListResponse>();

            foreach (var doctor in doctors)
            {
                var clinic = doctor.Clinic;
                var address = clinic?.Address;

                // جلب المستندات الخاصة بالدكتور
                var documents = await _unitOfWork.DoctorDocuments.GetByDoctorIdAsync(doctor.Id);
                var documentResponses = documents.Select(d => new DoctorDocumentItemResponse
                {
                    Id = d.Id,
                    DocumentUrl = d.DocumentUrl,
                    Type = d.Type,
                    TypeName = d.Type.GetDescription(),
                    Status = d.Status,
                    StatusName = d.Status.GetDescription(),
                    RejectionReason = d.RejectionReason,
                    CreatedAt = d.CreatedAt
                }).ToList();

                doctorResponses.Add(new DoctorVerificationListResponse
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    MedicalSpecialty = doctor.MedicalSpecialty,
                    MedicalSpecialtyName = doctor.MedicalSpecialty.GetDescription(),
                    Governorate = address?.Governorate.GetDescription() ?? string.Empty,
                    City = address?.City ?? string.Empty,
                    ProfileImageUrl = doctor.ProfileImageUrl,
                    YearsOfExperience = doctor.YearsOfExperience,
                    PhoneNumber = doctor.PhoneNumber,
                    Email = doctor.Email,
                    VerificationStatus = doctor.VerificationStatus,
                    VerificationStatusName = doctor.VerificationStatus.GetDescription(),
                    Documents = documentResponses
                });
            }

            return doctorResponses;
        }

        #endregion

        #region Document Verification

        public async Task<List<DoctorDocumentItemResponse>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting documents for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                var documents = await _unitOfWork.DoctorDocuments.GetByDoctorIdAsync(doctorId);
                
                var documentResponses = documents.Select(d => new DoctorDocumentItemResponse
                {
                    Id = d.Id,
                    DocumentUrl = d.DocumentUrl,
                    Type = d.Type,
                    TypeName = d.Type.GetDescription(),
                    Status = d.Status,
                    StatusName = d.Status.GetDescription(),
                    RejectionReason = d.RejectionReason,
                    CreatedAt = d.CreatedAt
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} documents for doctor {DoctorId}", 
                    documentResponses.Count, doctorId);

                return documentResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> ApproveDocumentAsync(Guid documentId)
        {
            try
            {
                _logger.LogInformation("Approving document {DocumentId}", documentId);

                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document {DocumentId} not found", documentId);
                    throw new ArgumentException($"Document with ID {documentId} not found");
                }

                document.Status = Shuryan.Core.Enums.VerificationDocumentStatus.Approved;
                document.RejectionReason = null;
                document.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Document {DocumentId} has been approved", documentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<bool> RejectDocumentAsync(Guid documentId, string? rejectionReason)
        {
            try
            {
                _logger.LogInformation("Rejecting document {DocumentId} with reason: {Reason}", documentId, rejectionReason ?? "No reason provided");

                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document {DocumentId} not found", documentId);
                    throw new ArgumentException($"Document with ID {documentId} not found");
                }

                document.Status = Shuryan.Core.Enums.VerificationDocumentStatus.Rejected;
                document.RejectionReason = rejectionReason; 
                document.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Document {DocumentId} has been rejected", documentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document {DocumentId}", documentId);
                throw;
            }
        }

        #endregion
    }
}
