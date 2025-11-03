using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Extensions;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Interfaces.UnitOfWork;
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
                var pendingCount = doctorDocuments.Count(d => d.Status == VerificationDocumentStatus.Pending);
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

                // جلب مواعيد اليوم
                var today = DateTime.UtcNow.Date;
                var todayAppointments = await _unitOfWork.Appointments.GetByDoctorIdAndDateAsync(doctorId, today);

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
                    AppointmentType = a.PreviousAppointmentId.HasValue ? "followup" : "regular",
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
                Core.Enums.Appointments.AppointmentStatus.CheckedIn => "pending",
                Core.Enums.Appointments.AppointmentStatus.Completed => "completed",
                Core.Enums.Appointments.AppointmentStatus.Cancelled => "cancelled",
                Core.Enums.Appointments.AppointmentStatus.NoShow => "cancelled",
                _ => "pending"
            };
        }
        #endregion

        #region Public Doctor Directory Operations

        /// <summary>
        /// الحصول على قائمة الدكاترة مع pagination - معلومات مختصرة للعرض في القائمة
        /// </summary>
        public async Task<PaginatedResponse<DoctorListItemResponse>> GetDoctorsListAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation("Getting doctors list. Page: {Page}, Size: {Size}",
                    paginationParams.PageNumber, paginationParams.PageSize);

                // جلب كل الدكاترة الموثقين فقط
                var allDoctors = await _unitOfWork.Doctors.GetVerifiedDoctorsAsync();

                // حساب الـ pagination
                var totalCount = allDoctors.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                // تطبيق الـ pagination
                var paginatedDoctors = allDoctors
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                // تحويل للـ response DTOs
                var doctorResponses = new List<DoctorListItemResponse>();

                foreach (var doctor in paginatedDoctors)
                {
                    // جلب معلومات العيادة والعنوان
                    var clinic = doctor.Clinic;
                    var address = clinic?.Address;

                    // حساب متوسط التقييم
                    var reviews = await _unitOfWork.DoctorReviews.GetAllAsync();
                    var doctorReviews = reviews.Where(r => r.DoctorId == doctor.Id).ToList();
                    var averageRating = doctorReviews.Any() ? doctorReviews.Average(r => r.AverageRating) : (double?)null;

                    // جلب أقرب موعد متاح
                    var nextAvailableSlot = await GetNextAvailableSlotAsync(doctor.Id);

                    // حساب سعر الكشف العادي
                    var regularConsultationFee = await GetRegularConsultationFeeAsync(doctor.Id);

                    doctorResponses.Add(new DoctorListItemResponse
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
                        RegularConsultationFee = regularConsultationFee,
                        ProfileImageUrl = doctor.ProfileImageUrl
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} doctors out of {Total}",
                    doctorResponses.Count, totalCount);

                return new PaginatedResponse<DoctorListItemResponse>
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
        /// حساب أقرب موعد متاح للدكتور
        /// </summary>
        private async Task<DateTime?> GetNextAvailableSlotAsync(Guid doctorId)
        {
            try
            {
                // جلب المواعيد المؤكدة للدكتور
                var appointments = await _unitOfWork.Appointments.GetAllAsync();
                var doctorAppointments = appointments
                    .Where(a => a.DoctorId == doctorId && 
                           a.Status == Core.Enums.Appointments.AppointmentStatus.Confirmed &&
                           a.ScheduledStartTime > DateTime.UtcNow)
                    .OrderBy(a => a.ScheduledStartTime)
                    .ToList();

                // جلب أوقات العمل الأساسية للدكتور
                var availabilities = await _unitOfWork.DoctorAvailabilities.GetByDoctorIdAsync(doctorId);
                
                // نبحث عن أقرب slot متاح في الأيام القادمة
                var currentDate = DateTime.UtcNow.Date;
                for (int i = 0; i < 14; i++) // نبحث في أول 14 يوم
                {
                    var checkDate = currentDate.AddDays(i);
                    var dayOfWeek = (Core.Enums.SysDayOfWeek)((int)checkDate.DayOfWeek);
                    
                    var dayAvailability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
                    if (dayAvailability != null)
                    {
                        var slotTime = checkDate.Add(dayAvailability.StartTime.ToTimeSpan());
                        if (slotTime > DateTime.UtcNow)
                        {
                            // تحقق إذا كان الموعد مشغول
                            var isBooked = doctorAppointments.Any(a => 
                                a.ScheduledStartTime.Date == checkDate && 
                                a.ScheduledStartTime.TimeOfDay >= dayAvailability.StartTime.ToTimeSpan() &&
                                a.ScheduledStartTime.TimeOfDay < dayAvailability.EndTime.ToTimeSpan());
                            
                            if (!isBooked)
                            {
                                return slotTime;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
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

        #endregion

        #region Doctor Patient Management Operations

        /// <summary>
        /// الحصول على قائمة المرضى الذين تعاملوا مع الدكتور (مع pagination)
        /// </summary>
        public async Task<PaginatedResponse<DoctorPatientResponse>> GetDoctorPatientsWithPaginationAsync(
            Guid doctorId, 
            PaginationParams paginationParams)
        {
            try
            {
                _logger.LogInformation(
                    "Getting paginated patients list for doctor {DoctorId}. Page: {Page}, Size: {Size}", 
                    doctorId, paginationParams.PageNumber, paginationParams.PageSize);

                // التحقق من وجود الدكتور
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // جلب المرضى من الـ Repository مع pagination
                var (patients, totalCount) = await _unitOfWork.Patients.GetDoctorPatientsAsync(
                    doctorId, 
                    paginationParams.PageNumber, 
                    paginationParams.PageSize);

                // تحويل البيانات إلى Response DTOs
                var patientResponses = patients.Select(patient =>
                {
                    // حساب عدد الجلسات المكتملة مع هذا الدكتور
                    var completedAppointments = patient.Appointments
                        .Where(a => a.DoctorId == doctorId && a.Status == Core.Enums.Appointments.AppointmentStatus.Completed)
                        .ToList();

                    var totalSessions = completedAppointments.Count;

                    // آخر جلسة
                    var lastVisit = completedAppointments
                        .OrderByDescending(a => a.ScheduledStartTime)
                        .FirstOrDefault();

                    // متوسط التقييم
                    var patientReview = patient.DoctorReviews
                        .Where(r => r.DoctorId == doctorId)
                        .FirstOrDefault();

                    // عنوان المريض
                    var address = patient.Address != null 
                        ? $"{patient.Address.City}, {patient.Address.Governorate}"
                        : null;

                    return new DoctorPatientResponse
                    {
                        Id = patient.Id,
                        FullName = $"{patient.FirstName} {patient.LastName}",
                        PhoneNumber = patient.PhoneNumber,
                        ProfileImageUrl = patient.ProfileImageUrl,
                        TotalSessions = totalSessions,
                        LastVisitDate = lastVisit?.ScheduledStartTime,
                        Address = address,
                        Rating = patientReview != null ? (decimal?)patientReview.AverageRating : null
                    };
                }).ToList();

                // إنشاء الـ Paginated Response
                var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

                var paginatedResponse = new PaginatedResponse<DoctorPatientResponse>
                {
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = paginationParams.PageNumber > 1,
                    HasNextPage = paginationParams.PageNumber < totalPages,
                    Data = patientResponses
                };

                _logger.LogInformation(
                    "Successfully retrieved {Count} patients out of {TotalCount} for doctor {DoctorId}",
                    patientResponses.Count, totalCount, doctorId);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated patients list for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على قائمة المرضى الذين تعاملوا مع الدكتور
        /// </summary>
        public async Task<IEnumerable<DoctorPatientListItemResponse>> GetDoctorPatientsAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting patients list for doctor {DoctorId}", doctorId);

                // التحقق من وجود الدكتور
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // جلب كل المواعيد المكتملة للدكتور مع بيانات المرضى
                var allAppointments = await _unitOfWork.Appointments.GetAllAsync();
                var doctorCompletedAppointments = allAppointments
                    .Where(a => a.DoctorId == doctorId && 
                                a.Status == Core.Enums.Appointments.AppointmentStatus.Completed &&
                                a.Patient != null) // التأكد من وجود المريض
                    .ToList();

                // تجميع المرضى (unique patients)
                var patientGroups = doctorCompletedAppointments
                    .GroupBy(a => a.PatientId)
                    .ToList();

                var patientResponses = new List<DoctorPatientListItemResponse>();

                foreach (var group in patientGroups)
                {
                    var patientId = group.Key;
                    var patientAppointments = group.ToList();
                    var patient = patientAppointments.First().Patient;

                    // حساب عدد الجلسات
                    var totalSessions = patientAppointments.Count;

                    // تاريخ آخر جلسة
                    var lastSession = patientAppointments
                        .OrderByDescending(a => a.ScheduledStartTime)
                        .First();

                    // حساب متوسط تقييم المريض للدكتور
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

                // ترتيب حسب تاريخ آخر جلسة (الأحدث أولاً)
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
        /// الحصول على السجل الطبي الكامل لمريض معين
        /// </summary>
        public async Task<PatientMedicalRecordResponse?> GetPatientMedicalRecordAsync(Guid patientId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting medical record for patient {PatientId} by doctor {DoctorId}", 
                    patientId, doctorId);

                // التحقق من وجود المريض
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient {PatientId} not found", patientId);
                    return null;
                }

                // التحقق من أن الدكتور له جلسات مع المريض
                var allAppointments = await _unitOfWork.Appointments.GetAllAsync();
                var hasAppointments = allAppointments.Any(a => 
                    a.DoctorId == doctorId && 
                    a.PatientId == patientId && 
                    a.Status == Core.Enums.Appointments.AppointmentStatus.Completed);

                if (!hasAppointments)
                {
                    _logger.LogWarning("Doctor {DoctorId} has no completed appointments with patient {PatientId}", 
                        doctorId, patientId);
                    return null;
                }

                // جلب السجل الطبي للمريض
                var allMedicalHistory = await _unitOfWork.MedicalHistoryItems.GetAllAsync();
                var patientMedicalHistory = allMedicalHistory
                    .Where(m => m.PatientId == patientId)
                    .ToList();

                // تجميع حسب النوع
                var drugAllergies = patientMedicalHistory
                    .Where(m => m.Type == Core.Enums.MedicalHistoryType.DrugAllergy)
                    .Select(m => ParseDrugAllergy(m))
                    .ToList();

                var currentMedications = patientMedicalHistory
                    .Where(m => m.Type == Core.Enums.MedicalHistoryType.CurrentMedication)
                    .Select(m => ParseCurrentMedication(m))
                    .ToList();

                var chronicDiseases = patientMedicalHistory
                    .Where(m => m.Type == Core.Enums.MedicalHistoryType.ChronicDisease)
                    .Select(m => ParseChronicDisease(m))
                    .ToList();

                var previousSurgeries = patientMedicalHistory
                    .Where(m => m.Type == Core.Enums.MedicalHistoryType.PreviousSurgery)
                    .Select(m => ParsePreviousSurgery(m))
                    .ToList();

                // تاريخ آخر تحديث
                var lastUpdated = patientMedicalHistory.Any() 
                    ? patientMedicalHistory.Max(m => m.UpdatedAt ?? m.CreatedAt) 
                    : (DateTime?)null;

                var response = new PatientMedicalRecordResponse
                {
                    PatientId = patientId,
                    PatientFullName = $"{patient.FirstName} {patient.LastName}",
                    LastUpdatedAt = lastUpdated,
                    DrugAllergies = drugAllergies,
                    CurrentMedications = currentMedications,
                    ChronicDiseases = chronicDiseases,
                    PreviousSurgeries = previousSurgeries
                };

                _logger.LogInformation("Successfully retrieved medical record for patient {PatientId}", patientId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical record for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على توثيق جميع الجلسات لمريض معين مع الدكتور
        /// </summary>
        public async Task<PatientSessionDocumentationListResponse?> GetPatientSessionDocumentationsAsync(Guid patientId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting session documentations for patient {PatientId} by doctor {DoctorId}", 
                    patientId, doctorId);

                // التحقق من وجود المريض
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient {PatientId} not found", patientId);
                    return null;
                }

                // جلب كل المواعيد المكتملة بين الدكتور والمريض
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

                // جلب سجلات الاستشارة لكل موعد
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
        /// الحصول على جميع الروشتات لمريض معين من الدكتور
        /// </summary>
        public async Task<PatientPrescriptionsListResponse?> GetPatientPrescriptionsAsync(Guid patientId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting prescriptions for patient {PatientId} by doctor {DoctorId}", 
                    patientId, doctorId);

                // التحقق من وجود المريض
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient {PatientId} not found", patientId);
                    return null;
                }

                // جلب كل الروشتات للمريض من هذا الدكتور
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

                // جلب الأدوية المكتوبة في كل روشتة
                var allPrescribedMedications = await _unitOfWork.PrescribedMedications.GetAllAsync();
                var allMedications = await _unitOfWork.Medications.GetAllAsync();

                var prescriptionResponses = new List<PatientPrescriptionResponse>();

                foreach (var prescription in patientPrescriptions)
                {
                    // جلب الأدوية الخاصة بهذه الروشتة
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

        #region Helper Methods for Medical Record Parsing

        /// <summary>
        /// تحليل نص الحساسية من الأدوية
        /// النص المتوقع: "اسم الدواء|الأثر"
        /// </summary>
        private DrugAllergyResponse ParseDrugAllergy(Core.Entities.Shared.MedicalHistoryItem item)
        {
            var parts = item.Text.Split('|');
            return new DrugAllergyResponse
            {
                Id = item.Id,
                DrugName = parts.Length > 0 ? parts[0].Trim() : item.Text,
                Reaction = parts.Length > 1 ? parts[1].Trim() : string.Empty,
                CreatedAt = item.CreatedAt
            };
        }

        /// <summary>
        /// تحليل نص الأدوية الحالية
        /// النص المتوقع: "اسم الدواء|الجرعة|التكرار|تاريخ البدء|السبب"
        /// </summary>
        private CurrentMedicationResponse ParseCurrentMedication(Core.Entities.Shared.MedicalHistoryItem item)
        {
            var parts = item.Text.Split('|');
            DateTime? startDate = null;
            
            if (parts.Length > 3 && DateTime.TryParse(parts[3].Trim(), out var parsedDate))
            {
                startDate = parsedDate;
            }

            return new CurrentMedicationResponse
            {
                Id = item.Id,
                MedicationName = parts.Length > 0 ? parts[0].Trim() : item.Text,
                Dosage = parts.Length > 1 ? parts[1].Trim() : string.Empty,
                Frequency = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                StartDate = startDate,
                Reason = parts.Length > 4 ? parts[4].Trim() : string.Empty,
                CreatedAt = item.CreatedAt
            };
        }

        /// <summary>
        /// تحليل نص الأمراض المزمنة
        /// النص المتوقع: "اسم المرض"
        /// </summary>
        private ChronicDiseaseResponse ParseChronicDisease(Core.Entities.Shared.MedicalHistoryItem item)
        {
            return new ChronicDiseaseResponse
            {
                Id = item.Id,
                DiseaseName = item.Text.Trim(),
                CreatedAt = item.CreatedAt
            };
        }

        /// <summary>
        /// تحليل نص العمليات الجراحية
        /// النص المتوقع: "اسم العملية|تاريخ العملية"
        /// </summary>
        private PreviousSurgeryResponse ParsePreviousSurgery(Core.Entities.Shared.MedicalHistoryItem item)
        {
            var parts = item.Text.Split('|');
            DateTime? surgeryDate = null;
            
            if (parts.Length > 1 && DateTime.TryParse(parts[1].Trim(), out var parsedDate))
            {
                surgeryDate = parsedDate;
            }

            return new PreviousSurgeryResponse
            {
                Id = item.Id,
                SurgeryName = parts.Length > 0 ? parts[0].Trim() : item.Text,
                SurgeryDate = surgeryDate,
                CreatedAt = item.CreatedAt
            };
        }

        #endregion
    }
}
