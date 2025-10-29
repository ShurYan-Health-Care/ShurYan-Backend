using AutoMapper;
using Microsoft.Extensions.Logging;
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
    }
}
