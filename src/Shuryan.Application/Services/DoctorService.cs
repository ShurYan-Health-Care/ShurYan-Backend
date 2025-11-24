using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Extensions;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public DoctorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DoctorService> logger, IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region Profile Operations - GET
        public async Task<DoctorProfileResponse?> GetDoctorProfileAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    return null;

                var clinic = doctor.Clinic;
                var reviews = await _unitOfWork.DoctorReviews.GetAllAsync();
                var doctorReviews = reviews.Where(r => r.DoctorId == doctorId).ToList();

                var response = new DoctorProfileResponse
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Email = doctor.Email,
                    PhoneNumber = doctor.PhoneNumber,
                    ProfilePictureUrl = doctor.ProfileImageUrl,
                    Gender = doctor.Gender ?? Core.Enums.Identity.Gender.Male,
                    GenderName = (doctor.Gender ?? Core.Enums.Identity.Gender.Male).ToString(),
                    DateOfBirth = doctor.BirthDate,
                    MedicalSpecialty = doctor.MedicalSpecialty,
                    MedicalSpecialtyName = doctor.MedicalSpecialty.ToString(),
                    Biography = doctor.Biography,
                    VerificationStatus = doctor.VerificationStatus,
                    VerificationStatusName = doctor.VerificationStatus.ToString()
                };

                _logger.LogInformation("Retrieved doctor profile {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor profile {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorPersonalProfileResponse?> GetPersonalProfileAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    return null;

                var response = new DoctorPersonalProfileResponse
                {
                    Id = doctor.Id,
                    ProfilePictureUrl = doctor.ProfileImageUrl,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Email = doctor.Email,
                    PhoneNumber = doctor.PhoneNumber,
                    DateOfBirth = doctor.BirthDate,
                    Gender = doctor.Gender ?? Core.Enums.Identity.Gender.Male,
                    GenderName = (doctor.Gender ?? Core.Enums.Identity.Gender.Male).ToString(),
                    Biography = doctor.Biography
                };

                _logger.LogInformation("Retrieved personal profile for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personal profile for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorProfessionalInfoResponse?> GetProfessionalInfoAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    return null;

                // Get all documents for this doctor
                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var doctorDocuments = allDocuments.Where(d => d.DoctorId == doctorId).ToList();

                // Map documents to response DTOs
                var doctorName = $"د. {doctor.FirstName} {doctor.LastName}";
                var documentResponses = doctorDocuments.Select(doc => new DoctorDocumentResponse
                {
                    Id = doc.Id,
                    DocumentUrl = doc.DocumentUrl,
                    Type = doc.Type,
                    TypeName = doc.Type.GetDescription(),
                    Status = doc.Status,
                    StatusName = doc.Status.ToString(),
                    RejectionReason = doc.RejectionReason,
                    DoctorId = doc.DoctorId,
                    DoctorName = doctorName,
                    CreatedAt = doc.CreatedAt,
                    CreatedBy = doc.CreatedBy,
                    UpdatedAt = doc.UpdatedAt,
                    UpdatedBy = doc.UpdatedBy
                }).ToList();

                // Calculate document statistics
                var approvedCount = doctorDocuments.Count(d => d.Status == VerificationDocumentStatus.Approved);
                var pendingCount = doctorDocuments.Count(d => d.Status == VerificationDocumentStatus.UnderReview);
                var rejectedCount = doctorDocuments.Count(d => d.Status == VerificationDocumentStatus.Rejected);

                var response = new DoctorProfessionalInfoResponse
                {
                    DoctorId = doctor.Id,
                    DoctorName = doctorName,
                    MedicalSpecialty = doctor.MedicalSpecialty,
                    SpecialtyName = doctor.MedicalSpecialty.GetDescription(),
                    YearsOfExperience = doctor.YearsOfExperience,
                    Documents = documentResponses,
                    TotalDocuments = doctorDocuments.Count,
                    ApprovedDocuments = approvedCount,
                    PendingDocuments = pendingCount,
                    RejectedDocuments = rejectedCount
                };

                _logger.LogInformation("Retrieved professional info for doctor {DoctorId} with {DocumentCount} documents", 
                    doctorId, doctorDocuments.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting professional info for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorSpecialtyExperienceResponse?> GetSpecialtyExperienceAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    return null;

                var response = new DoctorSpecialtyExperienceResponse
                {
                    DoctorId = doctor.Id,
                    MedicalSpecialty = doctor.MedicalSpecialty,
                    SpecialtyName = doctor.MedicalSpecialty.GetDescription(),
                    YearsOfExperience = doctor.YearsOfExperience
                };

                _logger.LogInformation("Retrieved specialty and experience for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting specialty and experience for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion
        #region Profile Operations - UPDATE

        public async Task<DoctorProfileResponse> UpdateDoctorProfileAsync(Guid doctorId, UpdateDoctorProfileRequest request)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Update only the fields that are provided (not null)
                if (!string.IsNullOrWhiteSpace(request.FirstName))
                    doctor.FirstName = request.FirstName;

                if (!string.IsNullOrWhiteSpace(request.LastName))
                    doctor.LastName = request.LastName;

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    doctor.PhoneNumber = request.PhoneNumber;

                // Upload profile image if provided
                if (request.ProfileImage != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrWhiteSpace(doctor.ProfileImageUrl))
                    {
                        await _fileUploadService.DeleteFileAsync(doctor.ProfileImageUrl);
                    }

                    // Upload new image
                    var uploadResult = await _fileUploadService.UploadProfileImageAsync(request.ProfileImage, doctorId.ToString());
                    doctor.ProfileImageUrl = uploadResult.FileUrl;
                }

                if (request.Gender.HasValue)
                    doctor.Gender = request.Gender;

                if (request.DateOfBirth.HasValue)
                    doctor.BirthDate = request.DateOfBirth;

                if (request.MedicalSpecialty.HasValue)
                    doctor.MedicalSpecialty = request.MedicalSpecialty.Value;

                if (request.YearsOfExperience.HasValue)
                    doctor.YearsOfExperience = request.YearsOfExperience.Value;

                if (!string.IsNullOrWhiteSpace(request.Biography))
                    doctor.Biography = request.Biography;

                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated doctor profile {DoctorId}", doctorId);
                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor profile {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorProfileResponse> UpdatePersonalInfoAsync(Guid doctorId, UpdatePersonalInfoRequest request)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                if (!string.IsNullOrWhiteSpace(request.FirstName))
                    doctor.FirstName = request.FirstName;

                if (!string.IsNullOrWhiteSpace(request.LastName))
                    doctor.LastName = request.LastName;

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    doctor.PhoneNumber = request.PhoneNumber;

                if (request.DateOfBirth.HasValue)
                    doctor.BirthDate = request.DateOfBirth;

                if (request.Gender.HasValue)
                    doctor.Gender = request.Gender;

                if (!string.IsNullOrWhiteSpace(request.Biography))
                    doctor.Biography = request.Biography;

                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated personal info for doctor {DoctorId}", doctorId);
                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating personal info for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorSpecialtyExperienceResponse> UpdateSpecialtyExperienceAsync(Guid doctorId, UpdateSpecialtyExperienceRequest request)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Update specialty and experience
                doctor.MedicalSpecialty = request.MedicalSpecialty;
                doctor.YearsOfExperience = request.YearsOfExperience;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated specialty and experience for doctor {DoctorId}. Specialty: {Specialty}, Experience: {Years} years",
                    doctorId, request.MedicalSpecialty, request.YearsOfExperience);

                return await GetSpecialtyExperienceAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated specialty and experience");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating specialty and experience for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Document Operations - UPLOAD

        public async Task<DoctorDocumentResponse?> GetDocumentByIdAsync(Guid documentId)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    return null;

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(document.DoctorId);
                var doctorName = doctor != null ? $"د. {doctor.FirstName} {doctor.LastName}" : "";

                var response = new DoctorDocumentResponse
                {
                    Id = document.Id,
                    DocumentUrl = document.DocumentUrl,
                    Type = document.Type,
                    TypeName = document.Type.GetDescription(),
                    Status = document.Status,
                    StatusName = document.Status.ToString(),
                    RejectionReason = document.RejectionReason,
                    DoctorId = document.DoctorId,
                    DoctorName = doctorName,
                    CreatedAt = document.CreatedAt,
                    CreatedBy = document.CreatedBy,
                    UpdatedAt = document.UpdatedAt,
                    UpdatedBy = document.UpdatedBy
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorDocumentResponse>> GetRequiredDocumentsAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Required document types
                var requiredTypes = new[]
                {
                    DoctorDocumentType.NationalId,
                    DoctorDocumentType.MedicalPracticeLicense,
                    DoctorDocumentType.SyndicateMembershipCard,
                    DoctorDocumentType.MedicalGraduationCertificate,
                    DoctorDocumentType.SpecialtyCertificate
                };

                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var requiredDocuments = allDocuments
                    .Where(d => d.DoctorId == doctorId && requiredTypes.Contains(d.Type))
                    .ToList();

                var doctorName = $"د. {doctor.FirstName} {doctor.LastName}";
                var responses = requiredDocuments.Select(doc => new DoctorDocumentResponse
                {
                    Id = doc.Id,
                    DocumentUrl = doc.DocumentUrl,
                    Type = doc.Type,
                    TypeName = doc.Type.GetDescription(),
                    Status = doc.Status,
                    StatusName = doc.Status.ToString(),
                    RejectionReason = doc.RejectionReason,
                    DoctorId = doc.DoctorId,
                    DoctorName = doctorName,
                    CreatedAt = doc.CreatedAt,
                    CreatedBy = doc.CreatedBy,
                    UpdatedAt = doc.UpdatedAt,
                    UpdatedBy = doc.UpdatedBy
                }).ToList();

                _logger.LogInformation("Retrieved {Count} required documents for doctor {DoctorId}", responses.Count, doctorId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting required documents for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorDocumentResponse>> GetResearchPapersAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var researchPapers = allDocuments
                    .Where(d => d.DoctorId == doctorId && d.Type == DoctorDocumentType.PublishedResearch)
                    .ToList();

                var doctorName = $"د. {doctor.FirstName} {doctor.LastName}";
                var responses = researchPapers.Select(doc => new DoctorDocumentResponse
                {
                    Id = doc.Id,
                    DocumentUrl = doc.DocumentUrl,
                    Type = doc.Type,
                    TypeName = doc.Type.GetDescription(),
                    Status = doc.Status,
                    StatusName = doc.Status.ToString(),
                    RejectionReason = doc.RejectionReason,
                    DoctorId = doc.DoctorId,
                    DoctorName = doctorName,
                    CreatedAt = doc.CreatedAt,
                    CreatedBy = doc.CreatedBy,
                    UpdatedAt = doc.UpdatedAt,
                    UpdatedBy = doc.UpdatedBy
                }).ToList();

                _logger.LogInformation("Retrieved {Count} research papers for doctor {DoctorId}", responses.Count, doctorId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting research papers for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorDocumentResponse>> GetAwardCertificatesAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var awards = allDocuments
                    .Where(d => d.DoctorId == doctorId && d.Type == DoctorDocumentType.AwardsAndRecognitions)
                    .ToList();

                var doctorName = $"د. {doctor.FirstName} {doctor.LastName}";
                var responses = awards.Select(doc => new DoctorDocumentResponse
                {
                    Id = doc.Id,
                    DocumentUrl = doc.DocumentUrl,
                    Type = doc.Type,
                    TypeName = doc.Type.GetDescription(),
                    Status = doc.Status,
                    StatusName = doc.Status.ToString(),
                    RejectionReason = doc.RejectionReason,
                    DoctorId = doc.DoctorId,
                    DoctorName = doctorName,
                    CreatedAt = doc.CreatedAt,
                    CreatedBy = doc.CreatedBy,
                    UpdatedAt = doc.UpdatedAt,
                    UpdatedBy = doc.UpdatedBy
                }).ToList();

                _logger.LogInformation("Retrieved {Count} awards/certificates for doctor {DoctorId}", responses.Count, doctorId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting awards/certificates for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Document Operations - UPLOAD
        public async Task<DoctorDocumentResponse> UploadOrUpdateRequiredDocumentAsync(Guid doctorId, UploadDoctorDocumentRequest request)
        {
            try
            {
                // Validate that the document type is one of the required types
                var requiredTypes = new[]
                {
                    DoctorDocumentType.NationalId,
                    DoctorDocumentType.MedicalPracticeLicense,
                    DoctorDocumentType.SyndicateMembershipCard,
                    DoctorDocumentType.MedicalGraduationCertificate,
                    DoctorDocumentType.SpecialtyCertificate
                };

                if (!requiredTypes.Contains(request.Type))
                {
                    throw new ArgumentException($"Document type '{request.Type.GetDescription()}' is not a required document type. " +
                        $"Required types are: NationalId, MedicalPracticeLicense, SyndicateMembershipCard, MedicalGraduationCertificate, SpecialtyCertificate");
                }

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Check if document already exists
                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var existingDocument = allDocuments.FirstOrDefault(d =>
                    d.DoctorId == doctorId &&
                    d.Type == request.Type);

                if (existingDocument != null)
                {
                    // Update existing document
                    _logger.LogInformation("Updating existing required document {DocumentId} of type {Type} for doctor {DoctorId}",
                        existingDocument.Id, request.Type, doctorId);

                    // Delete old file from Cloudinary
                    if (!string.IsNullOrWhiteSpace(existingDocument.DocumentUrl))
                    {
                        await _fileUploadService.DeleteFileAsync(existingDocument.DocumentUrl);
                    }

                    // Upload new file
                    var uploadResult = await _fileUploadService.UploadDocumentAsync(request.DocumentFile, doctorId.ToString());
                    existingDocument.DocumentUrl = uploadResult.FileUrl;
                    existingDocument.Status = VerificationDocumentStatus.Draft;
                    existingDocument.RejectionReason = null;
                    existingDocument.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync();

                    return await GetDocumentByIdAsync(existingDocument.Id)
                        ?? throw new InvalidOperationException("Failed to retrieve updated document");
                }
                else
                {
                    // Create new document
                    _logger.LogInformation("Creating new required document of type {Type} for doctor {DoctorId}",
                        request.Type, doctorId);

                    var uploadResult = await _fileUploadService.UploadDocumentAsync(request.DocumentFile, doctorId.ToString());

                    var document = new DoctorDocument
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctorId,
                        DocumentUrl = uploadResult.FileUrl,
                        Type = request.Type,
                        Status = VerificationDocumentStatus.Draft,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DoctorDocuments.AddAsync(document);
                    await _unitOfWork.SaveChangesAsync();

                    return await GetDocumentByIdAsync(document.Id)
                        ?? throw new InvalidOperationException("Failed to retrieve created document");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading/updating required document for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> UploadOrUpdateAwardCertificateAsync(Guid doctorId, UploadDoctorDocumentRequest request)
        {
            try
            {
                // Validate that the document type is AwardsAndRecognitions
                if (request.Type != DoctorDocumentType.AwardsAndRecognitions)
                {
                    throw new ArgumentException($"Document type must be 'AwardsAndRecognitions' for this endpoint. Provided: {request.Type.GetDescription()}");
                }

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Check existing awards count (maximum 3)
                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var existingAwards = allDocuments.Where(d =>
                    d.DoctorId == doctorId &&
                    d.Type == DoctorDocumentType.AwardsAndRecognitions).ToList();

                if (existingAwards.Count >= 3)
                {
                    throw new InvalidOperationException($"Maximum of 3 awards/certificates allowed. Current count: {existingAwards.Count}. " +
                        $"Please delete an existing award before uploading a new one.");
                }

                // Upload new award
                _logger.LogInformation("Creating new award/certificate for doctor {DoctorId}. Current count: {Count}",
                    doctorId, existingAwards.Count);

                var uploadResult = await _fileUploadService.UploadDocumentAsync(request.DocumentFile, doctorId.ToString());

                var document = new DoctorDocument
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    DocumentUrl = uploadResult.FileUrl,
                    Type = DoctorDocumentType.AwardsAndRecognitions,
                    Status = VerificationDocumentStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DoctorDocuments.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Award/certificate uploaded successfully. Document ID: {DocumentId}", document.Id);

                return await GetDocumentByIdAsync(document.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created award document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading award/certificate for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> UploadOrUpdateResearchPaperAsync(Guid doctorId, UploadDoctorDocumentRequest request)
        {
            try
            {
                // Validate that the document type is PublishedResearch
                if (request.Type != DoctorDocumentType.PublishedResearch)
                {
                    throw new ArgumentException($"Document type must be 'PublishedResearch' for this endpoint. Provided: {request.Type.GetDescription()}");
                }

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Check existing research papers count (maximum 3)
                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var existingPapers = allDocuments.Where(d =>
                    d.DoctorId == doctorId &&
                    d.Type == DoctorDocumentType.PublishedResearch).ToList();

                if (existingPapers.Count >= 3)
                {
                    throw new InvalidOperationException($"Maximum of 3 research papers allowed. Current count: {existingPapers.Count}. " +
                        $"Please delete an existing paper before uploading a new one.");
                }

                // Upload new research paper
                _logger.LogInformation("Creating new research paper for doctor {DoctorId}. Current count: {Count}",
                    doctorId, existingPapers.Count);

                var uploadResult = await _fileUploadService.UploadDocumentAsync(request.DocumentFile, doctorId.ToString());

                var document = new DoctorDocument
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    DocumentUrl = uploadResult.FileUrl,
                    Type = DoctorDocumentType.PublishedResearch,
                    Status = VerificationDocumentStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DoctorDocuments.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Research paper uploaded successfully. Document ID: {DocumentId}", document.Id);

                return await GetDocumentByIdAsync(document.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created research paper document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading research paper for doctor {DoctorId}", doctorId);
                throw;
            }
        }
        #endregion
        
        #region Utilities
        public IEnumerable<SpecialtyResponse> GetSpecialties()
        {
            var specialties = Enum.GetValues(typeof(Core.Enums.Doctor.MedicalSpecialty))
                .Cast<Core.Enums.Doctor.MedicalSpecialty>()
                .Select(s => new SpecialtyResponse
                {
                    Id = (int)s,
                    Name = s.ToString()
                })
                .ToList();

            return specialties;
        }
        #endregion

        #region Dashboard Operations
        public async Task<DoctorDashboardStatsResponse> GetDashboardStatsAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting dashboard stats for doctor {DoctorId}", doctorId);

                // التحقق من وجود الدكتور
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // جلب الإحصائيات
                var totalPatients = await _unitOfWork.Appointments.GetUniquePatientsCountAsync(doctorId);
                var todayAppointments = await _unitOfWork.Appointments.GetTodayAppointmentsCountAsync(doctorId);
                var completedAppointments = await _unitOfWork.Appointments.GetCompletedAppointmentsCountAsync(doctorId);
                var totalRevenue = await _unitOfWork.Appointments.GetTotalRevenueAsync(doctorId);
                
                var now = DateTime.UtcNow;
                var monthlyRevenue = await _unitOfWork.Appointments.GetMonthlyRevenueAsync(doctorId, now.Year, now.Month);
                
                var pendingAppointments = await _unitOfWork.Appointments.GetPendingAppointmentsCountAsync(doctorId);
                var cancelledAppointments = await _unitOfWork.Appointments.GetCancelledAppointmentsCountAsync(doctorId);
                
                var averageRating = await _unitOfWork.DoctorReviews.GetAverageRatingForDoctorAsync(doctorId);
                var totalReviews = await _unitOfWork.DoctorReviews.GetReviewCountForDoctorAsync(doctorId);

                var response = new DoctorDashboardStatsResponse
                {
                    TotalPatients = totalPatients,
                    TodayAppointments = todayAppointments,
                    CompletedAppointments = completedAppointments,
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    PendingAppointments = pendingAppointments,
                    CancelledAppointments = cancelledAppointments,
                    AverageRating = averageRating,
                    TotalReviews = totalReviews
                };

                _logger.LogInformation("Successfully retrieved dashboard stats for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<PaginatedResponse<TodayAppointmentResponse>> GetTodayAppointmentsAsync(Guid doctorId, PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting today's appointments for doctor {DoctorId}. Page: {Page}, Size: {Size}",
                    doctorId, paginationParams.PageNumber, paginationParams.PageSize);

                // التحقق من وجود الدكتور
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // جلب مواعيد اليوم (بتوقيت مصر)
                var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                var todayEgypt = nowEgypt.Date;
                
                _logger.LogInformation("Current time - UTC: {UtcNow}, Egypt: {EgyptNow}, Today (Egypt): {TodayEgypt}",
                    DateTime.UtcNow, nowEgypt, todayEgypt);
                
                // نجيب appointments اللي تاريخها = today (المخزون في الـ DB كـ Egypt local time)
                var todayAppointments = await _unitOfWork.Appointments.GetByDoctorIdAndDateAsync(doctorId, todayEgypt);

                // تحويل الـ Appointments لـ Response DTOs
                var appointmentResponses = todayAppointments.Select(a => new TodayAppointmentResponse
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    PatientPhoneNumber = a.Patient.PhoneNumber,
                    AppointmentTime = a.ScheduledStartTime.ToString("HH:mm"),
                    AppointmentDate = a.ScheduledStartTime.ToString("yyyy-MM-dd"),
                    Duration = a.SessionDurationMinutes,
                    AppointmentType = a.ConsultationType == Core.Enums.Appointments.ConsultationTypeEnum.FollowUp ? "followup" : "regular",
                    Status = MapAppointmentStatus(a.Status),
                    Notes = a.ConsultationRecord?.ChiefComplaint,
                    Price = a.ConsultationFee
                }).ToList();

                // تطبيق الـ Pagination
                var totalCount = appointmentResponses.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedAppointments = appointmentResponses
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} appointments out of {Total} for doctor {DoctorId}",
                    paginatedAppointments.Count, totalCount, doctorId);

                return new PaginatedResponse<TodayAppointmentResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = paginatedAppointments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's appointments for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        private string MapAppointmentStatus(Core.Enums.Appointments.AppointmentStatus status)
        {
            return status switch
            {
                Core.Enums.Appointments.AppointmentStatus.Confirmed => "pending",
                Core.Enums.Appointments.AppointmentStatus.Completed => "completed",
                Core.Enums.Appointments.AppointmentStatus.Cancelled => "cancelled",
                Core.Enums.Appointments.AppointmentStatus.NoShow => "cancelled",
                _ => "pending"
            };
        }
        #endregion

        #region Public Doctor Directory Operations

        /// <summary>
        /// الحصول على قائمة الدكاترة مع pagination وفلترة - معلومات مختصرة للعرض في القائمة
        /// </summary>
        public async Task<PaginatedResponse<DoctorListItemResponse>> GetDoctorsListAsync(SearchDoctorsRequest searchRequest)
        {
            try
            {
                _logger.LogInformation("Getting doctors list with filters. Page: {Page}, Size: {Size}, SearchTerm: {SearchTerm}, Specialty: {Specialty}, Governorate: {Governorate}, City: {City}, MinRating: {MinRating}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, AvailableToday: {AvailableToday}",
                    searchRequest.PageNumber, searchRequest.PageSize, searchRequest.SearchTerm, 
                    searchRequest.Specialty ?? searchRequest.MedicalSpecialty, searchRequest.Governorate, 
                    searchRequest.City, searchRequest.MinRating, searchRequest.MinPrice, searchRequest.MaxPrice, searchRequest.AvailableToday);

                // 1️⃣ جلب الدكاترة مع كل البيانات المطلوبة في query واحد (Eager Loading)
                var allDoctors = await _unitOfWork.Doctors.GetVerifiedDoctorsWithDetailsForListAsync();
                
                // 2️⃣ تطبيق الفلاتر الأساسية على الـ entities قبل التحويل
                var filteredDoctors = allDoctors.AsEnumerable();

                // فلتر البحث بالاسم
                if (!string.IsNullOrWhiteSpace(searchRequest.SearchTerm))
                {
                    var searchTerm = searchRequest.SearchTerm.Trim().ToLower();
                    filteredDoctors = filteredDoctors.Where(d =>
                        d.FirstName.ToLower().Contains(searchTerm) ||
                        d.LastName.ToLower().Contains(searchTerm));
                }

                // فلتر التخصص
                var specialtyFilter = searchRequest.Specialty ?? searchRequest.MedicalSpecialty;
                if (specialtyFilter.HasValue)
                {
                    filteredDoctors = filteredDoctors.Where(d => d.MedicalSpecialty == specialtyFilter.Value);
                }

                // فلتر المحافظة
                if (searchRequest.Governorate.HasValue)
                {
                    filteredDoctors = filteredDoctors.Where(d => 
                        d.Clinic?.Address?.Governorate == searchRequest.Governorate.Value);
                }

                // فلتر المدينة
                if (!string.IsNullOrWhiteSpace(searchRequest.City))
                {
                    var citySearch = searchRequest.City.Trim().ToLower();
                    filteredDoctors = filteredDoctors.Where(d => 
                        d.Clinic?.Address?.City != null && 
                        d.Clinic.Address.City.ToLower().Contains(citySearch));
                }

                var filteredList = filteredDoctors.ToList();
                
                // 3️⃣ جلب البيانات الإضافية دفعة واحدة (Batch Operations)
                var doctorIds = filteredList.Select(d => d.Id).ToList();
                
                // جلب التقييمات لكل الدكاترة دفعة واحدة
                var averageRatings = await _unitOfWork.DoctorReviews.GetAverageRatingsForDoctorsAsync(doctorIds);
                
                // جلب أسعار الكشف لكل الدكاترة دفعة واحدة
                var consultationFees = await _unitOfWork.DoctorConsultations.GetRegularConsultationFeesForDoctorsAsync(doctorIds);

                // 4️⃣ تحويل للـ DTOs مع البيانات المحسوبة
                var doctorResponses = filteredList.Select(doctor =>
                {
                    var address = doctor.Clinic?.Address;
                    var averageRating = averageRatings.GetValueOrDefault(doctor.Id);
                    var consultationFee = consultationFees.GetValueOrDefault(doctor.Id, 0);
                    
                    // حساب NextAvailableSlot بشكل محلي (بدون queries إضافية)
                    var nextAvailableSlot = CalculateNextAvailableSlotLocally(doctor);

                    return new DoctorListItemResponse
                    {
                        Id = doctor.Id,
                        FirstName = doctor.FirstName,
                        LastName = doctor.LastName,
                        MedicalSpecialty = doctor.MedicalSpecialty,
                        MedicalSpecialtyName = doctor.MedicalSpecialty.GetDescription(),
                        Governorate = address?.Governorate.GetDescription() ?? string.Empty,
                        City = address?.City ?? string.Empty,
                        Longitude = address?.Longitude,
                        Latitude = address?.Latitude,
                        NextAvailableSlot = nextAvailableSlot,
                        AverageRating = averageRating,
                        RegularConsultationFee = consultationFee,
                        ProfileImageUrl = doctor.ProfileImageUrl
                    };
                }).ToList();

                // 5️⃣ تطبيق الفلاتر المتبقية على الـ DTOs
                var finalFiltered = doctorResponses.AsEnumerable();

                // فلتر التقييم
                if (searchRequest.MinRating.HasValue)
                {
                    finalFiltered = finalFiltered.Where(d => 
                        d.AverageRating.HasValue && d.AverageRating.Value >= searchRequest.MinRating.Value);
                }

                // فلتر السعر
                if (searchRequest.MinPrice.HasValue)
                {
                    finalFiltered = finalFiltered.Where(d => d.RegularConsultationFee >= searchRequest.MinPrice.Value);
                }
                if (searchRequest.MaxPrice.HasValue)
                {
                    finalFiltered = finalFiltered.Where(d => d.RegularConsultationFee <= searchRequest.MaxPrice.Value);
                }

                // فلتر المتاحين اليوم
                if (searchRequest.AvailableToday.HasValue && searchRequest.AvailableToday.Value)
                {
                    var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    var todayEgypt = nowEgypt.Date;
                    
                    finalFiltered = finalFiltered.Where(d => 
                        d.NextAvailableSlot.HasValue && d.NextAvailableSlot.Value.Date == todayEgypt);
                }

                var finalList = finalFiltered.ToList();

                // 6️⃣ Pagination
                var totalCount = finalList.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)searchRequest.PageSize);

                var paginatedDoctors = finalList
                    .Skip((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                    .Take(searchRequest.PageSize)
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} doctors out of {Total} (after filtering)",
                    paginatedDoctors.Count, totalCount);

                return new PaginatedResponse<DoctorListItemResponse>
                {
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = searchRequest.PageNumber > 1,
                    HasNextPage = searchRequest.PageNumber < totalPages,
                    Data = paginatedDoctors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors list");
                throw;
            }
        }

        /// <summary>
        /// الحصول على التفاصيل الكاملة للدكتور مع معلومات العيادة
        /// </summary>
        public async Task<DoctorDetailsWithClinicResponse?> GetDoctorDetailsWithClinicAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting doctor details with clinic for doctor {DoctorId}", doctorId);

                // جلب الدكتور مع العيادة
                var doctor = await _unitOfWork.Doctors.GetByIdWithClinicAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    return null;
                }

                // حساب متوسط التقييم وعدد التقييمات
                var reviews = await _unitOfWork.DoctorReviews.GetAllAsync();
                var doctorReviews = reviews.Where(r => r.DoctorId == doctorId).ToList();
                var averageRating = doctorReviews.Any() ? doctorReviews.Average(r => r.AverageRating) : (double?)null;
                var totalReviews = doctorReviews.Count;

                // بناء الـ response
                var response = new DoctorDetailsWithClinicResponse
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    MedicalSpecialty = doctor.MedicalSpecialty,
                    MedicalSpecialtyName = doctor.MedicalSpecialty.GetDescription(),
                    Gender = doctor.Gender,
                    GenderName = doctor.Gender?.ToString(),
                    DateOfBirth = doctor.BirthDate,
                    ProfileImageUrl = doctor.ProfileImageUrl,
                    Biography = doctor.Biography,
                    YearsOfExperience = doctor.YearsOfExperience,
                    AverageRating = averageRating,
                    TotalReviews = totalReviews
                };

                // إضافة معلومات العيادة إذا كانت موجودة
                if (doctor.Clinic != null)
                {
                    var clinic = doctor.Clinic;
                    var clinicPhoneNumbers = await _unitOfWork.ClinicPhoneNumbers.GetClinicPhoneNumbersAsync(clinic.Id);
                    var clinicServices = await _unitOfWork.ClinicServices.GetClinicServicesAsync(clinic.Id);
                    var clinicPhotos = await _unitOfWork.ClinicPhotos.GetClinicPhotosAsync(clinic.Id);

                    response.Clinic = new ClinicDetailsResponse
                    {
                        Id = clinic.Id,
                        Name = clinic.Name,
                        PhoneNumbers = clinicPhoneNumbers.Select(p => new Application.DTOs.Responses.Clinic.ClinicPhoneNumberResponse
                        {
                            Id = p.Id,
                            Number = p.Number,
                            Type = p.Type.GetDescription(),
                            ClinicId = p.ClinicId,
                            CreatedAt = p.CreatedAt,
                            UpdatedAt = p.UpdatedAt
                        }).ToList(),
                        OfferedServices = clinicServices.Select(s => new Application.DTOs.Responses.Clinic.ClinicServiceResponse
                        {
                            Id = s.Id,
                            ServiceName = s.ServiceType.GetDescription(),
                            ClinicId = s.ClinicId,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt
                        }).ToList(),
                        Photos = clinicPhotos.Select(p => new Application.DTOs.Responses.Clinic.ClinicPhotoResponse
                        {
                            Id = p.Id,
                            PhotoUrl = p.PhotoUrl,
                            ClinicId = p.ClinicId,
                            CreatedAt = p.CreatedAt,
                            UpdatedAt = p.UpdatedAt
                        }).ToList()
                    };

                    // إضافة العنوان
                    if (clinic.Address != null)
                    {
                        response.Clinic.Address = new Application.DTOs.Responses.Clinic.ClinicAddressResponse
                        {
                            Governorate = clinic.Address.Governorate.GetDescription(),
                            City = clinic.Address.City,
                            Street = clinic.Address.Street,
                            BuildingNumber = clinic.Address.BuildingNumber ?? string.Empty,
                            Latitude = clinic.Address.Latitude,
                            Longitude = clinic.Address.Longitude
                        };
                    }
                }

                // إضافة معلومات الشركاء المقترحين
                var partnerSuggestion = doctor.PartnerSuggestion;
                if (partnerSuggestion != null)
                {
                    response.PartnerSuggestions = new PartnerSuggestionsResponse();

                    // جلب معلومات الصيدلية المقترحة
                    if (partnerSuggestion.SuggestedPharmacyId.HasValue)
                    {
                        var pharmacy = await _unitOfWork.Pharmacies.GetByIdAsync(partnerSuggestion.SuggestedPharmacyId.Value);
                        if (pharmacy != null)
                        {
                            response.PartnerSuggestions.SuggestedPharmacy = new PartnerBasicInfoResponse
                            {
                                Id = pharmacy.Id,
                                Name = pharmacy.Name
                            };
                        }
                    }

                    // جلب معلومات المعمل المقترح
                    if (partnerSuggestion.SuggestedLaboratoryId.HasValue)
                    {
                        var laboratory = await _unitOfWork.Laboratories.GetByIdAsync(partnerSuggestion.SuggestedLaboratoryId.Value);
                        if (laboratory != null)
                        {
                            response.PartnerSuggestions.SuggestedLaboratory = new PartnerBasicInfoResponse
                            {
                                Id = laboratory.Id,
                                Name = laboratory.Name
                            };
                        }
                    }
                }

                _logger.LogInformation("Successfully retrieved doctor details for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor details for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// حساب أقرب موعد متاح للدكتور بناءً على:
        /// - الجدول الأسبوعي الثابت (DoctorAvailability)
        /// - الأيام الاستثنائية (DoctorOverride) - بتعمل override على الأيام الثابتة
        /// - المواعيد المحجوزة (Confirmed, CheckedIn, InProgress)
        /// - مدة الجلسة من DoctorConsultation
        /// </summary>
        private async Task<DateTime?> GetNextAvailableSlotAsync(Guid doctorId)
        {
            try
            {
                // نستخدم UTC للحسابات الداخلية
                var nowUtc = DateTime.UtcNow;
                
                // نحول لـ Egypt timezone (UTC+2) عشان نقارن بأوقات العمل المحلية
                var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, egyptTimeZone);
                
                var searchStartDate = nowLocal.Date;
                var searchEndDate = searchStartDate.AddDays(30); // نبحث في أول 30 يوم

                _logger.LogDebug("Calculating next available slot for doctor {DoctorId}. Current time: UTC={UtcTime}, Local={LocalTime}", 
                    doctorId, nowUtc, nowLocal);

                // 1. جلب الجدول الأسبوعي الثابت
                var regularAvailabilities = await _unitOfWork.DoctorAvailabilities.GetByDoctorIdAsync(doctorId);
                if (!regularAvailabilities.Any())
                    return null; // الدكتور مضافش أي أيام عمل

                // 2. جلب الأيام الاستثنائية في الفترة القادمة
                // نحول الـ search dates لـ UTC عشان نقارن بالـ database
                var searchStartDateUtc = TimeZoneInfo.ConvertTimeToUtc(searchStartDate, egyptTimeZone);
                var searchEndDateUtc = TimeZoneInfo.ConvertTimeToUtc(searchEndDate, egyptTimeZone);
                
                var overrides = await _unitOfWork.DoctorOverrides.GetByDoctorIdAndDateRangeAsync(
                    doctorId, searchStartDateUtc, searchEndDateUtc);

                // 3. جلب كل المواعيد المحجوزة (مش بس Confirmed) - بنستخدم method محسنة
                var activeStatuses = new List<Core.Enums.Appointments.AppointmentStatus>
                {
                    Core.Enums.Appointments.AppointmentStatus.Confirmed,
                    Core.Enums.Appointments.AppointmentStatus.CheckedIn,
                    Core.Enums.Appointments.AppointmentStatus.InProgress
                };
                
                var bookedAppointments = (await _unitOfWork.Appointments.GetByDoctorIdAndDateRangeAsync(
                    doctorId, nowUtc, searchEndDateUtc, activeStatuses))
                    .ToList();

                _logger.LogInformation("=== BOOKED APPOINTMENTS DEBUG ===");
                _logger.LogInformation("Doctor ID: {DoctorId}", doctorId);
                _logger.LogInformation("Search Range: {Start} to {End} (UTC)", nowUtc, searchEndDateUtc);
                _logger.LogInformation("Found {Count} booked appointments", bookedAppointments.Count);
                
                // CRITICAL FIX: ALL appointments in database are stored as Egypt local time (not UTC)
                // We need to convert them to UTC for proper comparison
                var correctedAppointments = new List<Appointment>();
                
                if (bookedAppointments.Any())
                {
                    foreach (var apt in bookedAppointments)
                    {
                        _logger.LogInformation("  - Appointment {Id}: {Start} to {End} (stored in DB), Status: {Status}", 
                            apt.Id, apt.ScheduledStartTime, apt.ScheduledEndTime, apt.Status);
                        
                        // ALWAYS treat database times as Egypt local time and convert to UTC
                        var correctedStart = TimeZoneInfo.ConvertTimeToUtc(
                            DateTime.SpecifyKind(apt.ScheduledStartTime, DateTimeKind.Unspecified), 
                            egyptTimeZone);
                        var correctedEnd = TimeZoneInfo.ConvertTimeToUtc(
                            DateTime.SpecifyKind(apt.ScheduledEndTime, DateTimeKind.Unspecified), 
                            egyptTimeZone);
                        
                        _logger.LogWarning("    >>> CONVERTED TO UTC: {Start} to {End}",
                            correctedStart, correctedEnd);
                        
                        // Create a corrected copy
                        var correctedApt = new Core.Entities.Medical.Appointment
                        {
                            Id = apt.Id,
                            DoctorId = apt.DoctorId,
                            PatientId = apt.PatientId,
                            ScheduledStartTime = correctedStart,
                            ScheduledEndTime = correctedEnd,
                            Status = apt.Status,
                            ConsultationType = apt.ConsultationType,
                            SessionDurationMinutes = apt.SessionDurationMinutes
                        };
                        correctedAppointments.Add(correctedApt);
                    }
                    
                    // Replace with corrected appointments
                    bookedAppointments = correctedAppointments;
                }
                else
                {
                    _logger.LogWarning("NO BOOKED APPOINTMENTS FOUND! This might be the problem.");
                }
                _logger.LogInformation("=================================");

                // 4. جلب مدة الجلسة الافتراضية (Regular Consultation)
                var consultations = await _unitOfWork.DoctorConsultations.GetByDoctorIdAsync(doctorId);
                var regularConsultation = consultations
                    .FirstOrDefault(c => c.ConsultationType.ConsultationTypeEnum == Core.Enums.Appointments.ConsultationTypeEnum.Regular);
                
                int sessionDurationMinutes = regularConsultation?.SessionDurationMinutes ?? 30; // default 30 minutes

                // 5. نبحث يوم بيوم عن أول slot متاح
                for (int dayOffset = 0; dayOffset < 30; dayOffset++)
                {
                    var checkDate = searchStartDate.AddDays(dayOffset);
                    
                    // تحويل System.DayOfWeek إلى SysDayOfWeek بشكل صحيح
                    var dayOfWeek = checkDate.DayOfWeek switch
                    {
                        DayOfWeek.Saturday => Core.Enums.SysDayOfWeek.Saturday,
                        DayOfWeek.Sunday => Core.Enums.SysDayOfWeek.Sunday,
                        DayOfWeek.Monday => Core.Enums.SysDayOfWeek.Monday,
                        DayOfWeek.Tuesday => Core.Enums.SysDayOfWeek.Tuesday,
                        DayOfWeek.Wednesday => Core.Enums.SysDayOfWeek.Wednesday,
                        DayOfWeek.Thursday => Core.Enums.SysDayOfWeek.Thursday,
                        DayOfWeek.Friday => Core.Enums.SysDayOfWeek.Friday,
                        _ => Core.Enums.SysDayOfWeek.Saturday
                    };

                    // تحقق من وجود override لهذا اليوم
                    var dayOverrides = overrides
                        .Where(o => o.StartTime.Date == checkDate)
                        .OrderBy(o => o.StartTime)
                        .ToList();

                    // تحديد أوقات العمل لهذا اليوم
                    List<(TimeOnly Start, TimeOnly End)> workingHours = new List<(TimeOnly, TimeOnly)>();

                    // لو فيه override من نوع Unavailable بيغطي اليوم كله، نتخطى اليوم
                    var fullDayUnavailable = dayOverrides.Any(o => 
                        o.Type == Core.Enums.Appointments.OverrideType.Unavailable &&
                        o.StartTime.TimeOfDay == TimeSpan.Zero &&
                        o.EndTime.Date > checkDate);

                    if (fullDayUnavailable)
                        continue;

                    // 1. نبدأ بالجدول الثابت
                    var regularSchedule = regularAvailabilities
                        .Where(a => a.DayOfWeek == dayOfWeek)
                        .ToList();

                    foreach (var schedule in regularSchedule)
                    {
                        workingHours.Add((schedule.StartTime, schedule.EndTime));
                    }

                    // 2. نضيف الأوقات الاستثنائية Available (أوقات إضافية)
                    var availableOverrides = dayOverrides
                        .Where(o => o.Type == Core.Enums.Appointments.OverrideType.Available)
                        .ToList();

                    foreach (var ovr in availableOverrides)
                    {
                        workingHours.Add((TimeOnly.FromDateTime(ovr.StartTime), TimeOnly.FromDateTime(ovr.EndTime)));
                    }

                    // لو مفيش أي أوقات عمل (لا ثابتة ولا استثنائية)، نتخطى اليوم
                    if (!workingHours.Any())
                        continue;

                    // 3. نجهز الأوقات الـ Unavailable عشان نستبعدها من الـ slots
                    var unavailableOverrides = dayOverrides
                        .Where(o => o.Type == Core.Enums.Appointments.OverrideType.Unavailable)
                        .ToList();

                    // 6. تقسيم أوقات العمل إلى slots بناءً على مدة الجلسة
                    foreach (var (workStart, workEnd) in workingHours)
                    {
                        var currentSlotTime = checkDate.Add(workStart.ToTimeSpan());
                        var workEndDateTime = checkDate.Add(workEnd.ToTimeSpan());

                        _logger.LogDebug("Processing work hours for doctor {DoctorId} on {Date}: {Start} to {End}, Session: {Duration}min", 
                            doctorId, checkDate.ToString("yyyy-MM-dd"), workStart, workEnd, sessionDurationMinutes);

                        // نسمح بالـ slots اللي بتبدأ قبل نهاية وقت العمل
                        // (حتى لو الجلسة هتنتهي بعد وقت العمل بشوية)
                        while (currentSlotTime < workEndDateTime)
                        {
                            var slotEnd = currentSlotTime.AddMinutes(sessionDurationMinutes);

                            // تحقق إن الـ slot في المستقبل (مقارنة بالـ local time)
                            if (currentSlotTime <= nowLocal)
                            {
                                _logger.LogDebug("Skipping slot {SlotTime} - in the past (now: {Now})", currentSlotTime, nowLocal);
                                currentSlotTime = currentSlotTime.AddMinutes(sessionDurationMinutes);
                                continue;
                            }

                            // نحول الـ slot لـ UTC عشان نقارنه بالـ overrides والـ appointments
                            var currentSlotTimeUtc = TimeZoneInfo.ConvertTimeToUtc(currentSlotTime, egyptTimeZone);
                            var slotEndUtc = TimeZoneInfo.ConvertTimeToUtc(slotEnd, egyptTimeZone);

                            // تحقق إن الـ slot مش بيتداخل مع وقت Unavailable
                            bool isInUnavailableTime = unavailableOverrides.Any(u =>
                                // الـ slot يتداخل مع unavailable time
                                (currentSlotTimeUtc >= u.StartTime && currentSlotTimeUtc < u.EndTime) ||
                                (slotEndUtc > u.StartTime && slotEndUtc <= u.EndTime) ||
                                (currentSlotTimeUtc <= u.StartTime && slotEndUtc >= u.EndTime));

                            if (isInUnavailableTime)
                            {
                                _logger.LogDebug("Skipping slot {SlotTime} (UTC: {SlotTimeUtc}) - unavailable override", 
                                    currentSlotTime, currentSlotTimeUtc);
                                currentSlotTime = currentSlotTime.AddMinutes(sessionDurationMinutes);
                                continue;
                            }

                            // نجيب المواعيد المحجوزة في نفس اليوم بس (عشان نحسن الـ performance)
                            var dayAppointments = bookedAppointments
                                .Where(a => a.ScheduledStartTime.Date == currentSlotTimeUtc.Date)
                                .ToList();

                            _logger.LogDebug("=== CHECKING SLOT: {SlotLocal} (UTC: {SlotUtc}) ===", currentSlotTime, currentSlotTimeUtc);
                            _logger.LogDebug("  Slot End: {SlotEndLocal} (UTC: {SlotEndUtc})", slotEnd, slotEndUtc);
                            _logger.LogDebug("  Day has {Count} appointments", dayAppointments.Count);

                            // تحقق إن الـ slot مش محجوز
                            bool isBooked = false;
                            foreach (var apt in dayAppointments)
                            {
                                bool overlap1 = currentSlotTimeUtc >= apt.ScheduledStartTime && currentSlotTimeUtc < apt.ScheduledEndTime;
                                bool overlap2 = slotEndUtc > apt.ScheduledStartTime && slotEndUtc <= apt.ScheduledEndTime;
                                bool overlap3 = currentSlotTimeUtc <= apt.ScheduledStartTime && slotEndUtc >= apt.ScheduledEndTime;
                                
                                _logger.LogDebug("    Comparing with appointment {Id}: {Start} to {End} (UTC)", 
                                    apt.Id, apt.ScheduledStartTime, apt.ScheduledEndTime);
                                _logger.LogDebug("      Overlap checks: start-in={O1}, end-in={O2}, covers={O3}", 
                                    overlap1, overlap2, overlap3);
                                
                                if (overlap1 || overlap2 || overlap3)
                                {
                                    isBooked = true;
                                    _logger.LogWarning("    >>> CONFLICT DETECTED! Slot overlaps with appointment {Id}", apt.Id);
                                    break;
                                }
                            }

                            if (isBooked)
                            {
                                _logger.LogDebug("  RESULT: Slot is BOOKED - skipping");
                            }
                            else
                            {
                                // لقينا أول slot متاح! نرجعه بـ UTC
                                _logger.LogInformation("  RESULT: Slot is AVAILABLE! ✓✓✓");
                                _logger.LogInformation(">>> FOUND NEXT AVAILABLE SLOT for doctor {DoctorId}: Local={SlotTime}, UTC={SlotTimeUtc}", 
                                    doctorId, currentSlotTime, currentSlotTimeUtc);
                                return currentSlotTimeUtc;
                            }

                            currentSlotTime = currentSlotTime.AddMinutes(sessionDurationMinutes);
                        }
                        
                        _logger.LogDebug("Finished processing work hours {Start}-{End}, last slot checked: {LastSlot}", 
                            workStart, workEnd, currentSlotTime.AddMinutes(-sessionDurationMinutes));
                    }
                }

                return null; // مفيش slots متاحة في الـ 30 يوم القادمة
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next available slot for doctor {DoctorId}", doctorId);
                return null;
            }
        }

        /// <summary>
        /// حساب سعر الكشف العادي للدكتور
        /// </summary>
        private async Task<decimal> GetRegularConsultationFeeAsync(Guid doctorId)
        {
            try
            {
                // جلب آخر consultation type للدكتور أو استخدام القيمة الافتراضية
                var consultations = await _unitOfWork.DoctorConsultations.GetByDoctorIdAsync(doctorId);
                var regularConsultation = consultations
                    .FirstOrDefault(c => c.ConsultationType.ConsultationTypeEnum == Core.Enums.Appointments.ConsultationTypeEnum.Regular);

                return regularConsultation?.ConsultationFee ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// حساب NextAvailableSlot محلياً بدون database queries إضافية
        /// يستخدم البيانات المحملة مسبقاً (Availabilities, Overrides, Consultations)
        /// </summary>
        private DateTime? CalculateNextAvailableSlotLocally(Doctor doctor)
        {
            try
            {
                // التحقق من وجود availabilities
                if (doctor.Availabilities == null || !doctor.Availabilities.Any())
                    return null;

                var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                var nowUtc = DateTime.UtcNow;
                var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, egyptTimeZone);
                
                // البحث في أول 7 أيام فقط (للسرعة)
                var searchEndDate = nowLocal.AddDays(7);
                
                // جلب مدة الجلسة الافتراضية
                var regularConsultation = doctor.Consultations?
                    .FirstOrDefault(c => c.ConsultationType?.ConsultationTypeEnum == ConsultationTypeEnum.Regular);
                int sessionDurationMinutes = regularConsultation?.SessionDurationMinutes ?? 30;

                // البحث عن أول slot متاح
                for (var currentDate = nowLocal.Date; currentDate <= searchEndDate; currentDate = currentDate.AddDays(1))
                {
                    var dayOfWeek = (SysDayOfWeek)((int)currentDate.DayOfWeek + 1);
                    
                    // جلب ساعات العمل لهذا اليوم
                    var availability = doctor.Availabilities
                        .FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
                    
                    if (availability == null)
                        continue;

                    // تحويل TimeOnly إلى DateTime
                    var workStart = currentDate.Add(availability.StartTime.ToTimeSpan());
                    var workEnd = currentDate.Add(availability.EndTime.ToTimeSpan());
                    
                    // لو اليوم هو اليوم الحالي، نبدأ من الوقت الحالي
                    var startTime = currentDate.Date == nowLocal.Date && nowLocal > workStart 
                        ? nowLocal 
                        : workStart;

                    // التحقق من وجود slot متاح في ساعات العمل
                    if (startTime < workEnd)
                    {
                        // نرجع أول slot متاح (simplified - بدون التحقق من المواعيد المحجوزة)
                        return TimeZoneInfo.ConvertTimeToUtc(startTime, egyptTimeZone);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating next available slot locally for doctor {DoctorId}", doctor.Id);
                return null;
            }
        }

        #endregion

        #region Doctor Patient Management Operations

        public async Task<PaginatedResponse<DoctorPatientResponse>> GetDoctorPatientsWithPaginationAsync(
            Guid doctorId, 
            PaginationParams paginationParams)
        {
            _logger.LogInformation(
                "Getting paginated patients list for doctor {DoctorId}. Page: {Page}, Size: {Size}", 
                doctorId, paginationParams.PageNumber, paginationParams.PageSize);
            
            var (patients, totalCount) = await _unitOfWork.Patients.GetDoctorPatientsOptimizedAsync(
                doctorId, 
                paginationParams.PageNumber, 
                paginationParams.PageSize);

            var patientResponses = patients.Select(p => new DoctorPatientResponse
            {
                Id = p.PatientId,
                FullName = $"{p.FirstName} {p.LastName}",
                PhoneNumber = p.PhoneNumber,
                ProfileImageUrl = p.ProfileImageUrl,
                TotalSessions = p.TotalSessions,
                LastVisitDate = p.LastVisitDate,
                Address = !string.IsNullOrEmpty(p.City) && !string.IsNullOrEmpty(p.Governorate)
                    ? $"{p.City}, {p.Governorate}"
                    : null
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

            _logger.LogInformation(
                "Successfully retrieved {Count} patients out of {TotalCount} for doctor {DoctorId}",
                patientResponses.Count, totalCount, doctorId);

            return new PaginatedResponse<DoctorPatientResponse>
            {
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = paginationParams.PageNumber > 1,
                HasNextPage = paginationParams.PageNumber < totalPages,
                Data = patientResponses
            };
        }

        /// <summary>
        /// Get a list of patients treated by the doctor.
        /// </summary>
        public async Task<IEnumerable<DoctorPatientListItemResponse>> GetDoctorPatientsAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting patients list for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allAppointments = await _unitOfWork.Appointments.GetAllAsync();
                var doctorCompletedAppointments = allAppointments
                    .Where(a => a.DoctorId == doctorId && 
                                a.Status == Core.Enums.Appointments.AppointmentStatus.Completed &&
                                a.Patient != null) 
                    .ToList();

                var patientGroups = doctorCompletedAppointments
                    .GroupBy(a => a.PatientId)
                    .ToList();

                var patientResponses = new List<DoctorPatientListItemResponse>();

                foreach (var group in patientGroups)
                {
                    var patientId = group.Key;
                    var patientAppointments = group.ToList();
                    var patient = patientAppointments.First().Patient;

                    var totalSessions = patientAppointments.Count;

                    var lastSession = patientAppointments
                        .OrderByDescending(a => a.ScheduledStartTime)
                        .First();

                    var allReviews = await _unitOfWork.DoctorReviews.GetAllAsync();
                    var patientReviews = allReviews
                        .Where(r => r.DoctorId == doctorId && r.PatientId == patientId)
                        .ToList();
                    
                    var averageRating = patientReviews.Any() 
                        ? (double?)patientReviews.Average(r => r.AverageRating) 
                        : null;

                    patientResponses.Add(new DoctorPatientListItemResponse
                    {
                        PatientId = patientId,
                        FirstName = patient.FirstName,
                        LastName = patient.LastName,
                        PhoneNumber = patient.PhoneNumber,
                        TotalSessions = totalSessions,
                        LastSessionDate = lastSession.ScheduledStartTime,
                        PatientRating = averageRating
                    });
                }

                patientResponses = patientResponses
                    .OrderByDescending(p => p.LastSessionDate)
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} patients for doctor {DoctorId}", 
                    patientResponses.Count, doctorId);

                return patientResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patients list for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// Get the complete medical history of a specific patient.
        /// </summary>
        public async Task<PatientMedicalRecordResponse?> GetPatientMedicalRecordAsync(Guid patientId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting medical record for patient {PatientId} by doctor {DoctorId}", 
                    patientId, doctorId);

                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient {PatientId} not found", patientId);
                    return null;
                }

                var allAppointments = await _unitOfWork.Appointments.GetAllAsync();
                var hasAppointments = allAppointments.Any(a => 
                    a.DoctorId == doctorId && 
                    a.PatientId == patientId &&
                    a.Status != Core.Enums.Appointments.AppointmentStatus.Cancelled);

                if (!hasAppointments)
                {
                    _logger.LogWarning("Doctor {DoctorId} has no appointments with patient {PatientId}", 
                        doctorId, patientId);
                    return null;
                }

                var allMedicalHistory = await _unitOfWork.MedicalHistoryItems.GetAllAsync();
                var patientMedicalHistory = allMedicalHistory
                    .Where(m => m.PatientId == patientId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToList();

                var medicalHistoryItems = patientMedicalHistory.Select(m => new MedicalHistoryItemResponse
                {
                    Id = m.Id,
                    Type = m.Type,
                    TypeName = GetMedicalHistoryTypeName(m.Type),
                    Text = m.Text,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                }).ToList();

                var response = new PatientMedicalRecordResponse
                {
                    PatientId = patientId,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    MedicalHistory = medicalHistoryItems
                };

                _logger.LogInformation("Successfully retrieved medical record for patient {PatientId} - {Count} items", 
                    patientId, medicalHistoryItems.Count);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical record for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Bring the name of the type of medical information in Arabic
        /// </summary>
        private string GetMedicalHistoryTypeName(Core.Enums.MedicalHistoryType type)
        {
            return type switch
            {
                Core.Enums.MedicalHistoryType.DrugAllergy => "الحساسية من الأدوية",
                Core.Enums.MedicalHistoryType.ChronicDisease => "الأمراض المزمنة",
                Core.Enums.MedicalHistoryType.CurrentMedication => "الأدوية الحالية",
                Core.Enums.MedicalHistoryType.PreviousSurgery => "العمليات الجراحية السابقة",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Get documentation of all sessions for a specific patient with the doctor.
        /// </summary>
        public async Task<PatientSessionDocumentationListResponse?> GetPatientSessionDocumentationsAsync(Guid patientId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting session documentations for patient {PatientId} by doctor {DoctorId}", 
                    patientId, doctorId);

                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient {PatientId} not found", patientId);
                    return null;
                }

                var allAppointments = await _unitOfWork.Appointments.GetAllAsync();
                var completedAppointments = allAppointments
                    .Where(a => a.DoctorId == doctorId && 
                               a.PatientId == patientId && 
                               a.Status == Core.Enums.Appointments.AppointmentStatus.Completed)
                    .OrderByDescending(a => a.ScheduledStartTime)
                    .ToList();

                if (!completedAppointments.Any())
                {
                    _logger.LogWarning("No completed appointments found between doctor {DoctorId} and patient {PatientId}", 
                        doctorId, patientId);
                    return null;
                }

                var allConsultationRecords = await _unitOfWork.ConsultationRecords.GetAllAsync();
                
                var sessions = new List<SessionDocumentationResponse>();

                foreach (var appointment in completedAppointments)
                {
                    var consultationRecord = allConsultationRecords
                        .FirstOrDefault(cr => cr.AppointmentId == appointment.Id);

                    if (consultationRecord != null)
                    {
                        sessions.Add(new SessionDocumentationResponse
                        {
                            AppointmentId = appointment.Id,
                            ConsultationRecordId = consultationRecord.Id,
                            SessionDate = appointment.ScheduledStartTime.Date,
                            SessionTime = appointment.ScheduledStartTime.TimeOfDay,
                            SessionType = appointment.ConsultationType,
                            SessionTypeName = appointment.ConsultationType.ToString(),
                            SessionDurationMinutes = appointment.SessionDurationMinutes,
                            ChiefComplaint = consultationRecord.ChiefComplaint,
                            HistoryOfPresentIllness = consultationRecord.HistoryOfPresentIllness,
                            PhysicalExamination = consultationRecord.PhysicalExamination,
                            Diagnosis = consultationRecord.Diagnosis,
                            ManagementPlan = consultationRecord.ManagementPlan,
                            CreatedAt = consultationRecord.CreatedAt
                        });
                    }
                }

                var response = new PatientSessionDocumentationListResponse
                {
                    PatientId = patientId,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    TotalSessions = completedAppointments.Count,
                    Sessions = sessions
                };

                _logger.LogInformation("Successfully retrieved {Count} session documentations for patient {PatientId}", 
                    sessions.Count, patientId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session documentations for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get all prescriptions for a specific patient from the doctor.
        /// </summary>
        public async Task<PatientPrescriptionsListResponse?> GetPatientPrescriptionsAsync(Guid patientId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting prescriptions for patient {PatientId} by doctor {DoctorId}", 
                    patientId, doctorId);

                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient {PatientId} not found", patientId);
                    return null;
                }

                var allPrescriptions = await _unitOfWork.Prescriptions.GetAllAsync();
                var patientPrescriptions = allPrescriptions
                    .Where(p => p.PatientId == patientId && p.DoctorId == doctorId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                if (!patientPrescriptions.Any())
                {
                    _logger.LogWarning("No prescriptions found for patient {PatientId} from doctor {DoctorId}", 
                        patientId, doctorId);
                    return null;
                }

                var allPrescribedMedications = await _unitOfWork.PrescribedMedications.GetAllAsync();
                var allMedications = await _unitOfWork.Medications.GetAllAsync();

                var prescriptionResponses = new List<PatientPrescriptionResponse>();

                foreach (var prescription in patientPrescriptions)
                {
                    var prescribedMeds = allPrescribedMedications
                        .Where(pm => pm.MedicationPrescriptionId == prescription.Id)
                        .ToList();

                    var medications = new List<PatientPrescriptionMedicationResponse>();

                    foreach (var prescribedMed in prescribedMeds)
                    {
                        var medication = allMedications.FirstOrDefault(m => m.Id == prescribedMed.MedicationId);
                        
                        if (medication != null)
                        {
                            medications.Add(new PatientPrescriptionMedicationResponse
                            {
                                MedicationName = medication.BrandName,
                                Dosage = prescribedMed.Dosage,
                                Frequency = prescribedMed.Frequency,
                                DurationDays = prescribedMed.DurationDays,
                                SpecialInstructions = prescribedMed.SpecialInstructions
                            });
                        }
                    }

                    prescriptionResponses.Add(new PatientPrescriptionResponse
                    {
                        PrescriptionId = prescription.Id,
                        PatientFullName = $"{patient.FirstName} {patient.LastName}",
                        PrescriptionNumber = prescription.PrescriptionNumber,
                        PrescriptionDate = prescription.CreatedAt,
                        AppointmentId = prescription.AppointmentId,
                        Medications = medications
                    });
                }

                var response = new PatientPrescriptionsListResponse
                {
                    PatientId = patientId,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    TotalPrescriptions = patientPrescriptions.Count,
                    Prescriptions = prescriptionResponses
                };

                _logger.LogInformation("Successfully retrieved {Count} prescriptions for patient {PatientId}", 
                    prescriptionResponses.Count, patientId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region Verification Operations

        /// <summary>
        /// Submit review request - Change verification status to "Sent"
        /// </summary>
        public async Task<bool> SubmitForReviewAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Doctor {DoctorId} submitting profile for review", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                if (doctor.VerificationStatus == VerificationStatus.Sent)
                {
                    _logger.LogWarning("Doctor {DoctorId} has already submitted for review", doctorId);
                    throw new InvalidOperationException("Your profile has already been submitted for review");
                }

                if (doctor.VerificationStatus == VerificationStatus.UnderReview)
                {
                    _logger.LogWarning("Doctor {DoctorId} profile is already under review", doctorId);
                    throw new InvalidOperationException("Your profile is already under review");
                }

                if (doctor.VerificationStatus == VerificationStatus.Verified)
                {
                    _logger.LogWarning("Doctor {DoctorId} is already verified", doctorId);
                    throw new InvalidOperationException("Your profile is already verified");
                }

                doctor.VerificationStatus = VerificationStatus.Sent;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor {DoctorId} successfully submitted profile for review. Status changed to Sent", doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting profile for review for doctor {DoctorId}", doctorId);
                throw;
            }
        }
        #endregion
    }
}
