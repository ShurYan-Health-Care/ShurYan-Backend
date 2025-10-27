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

        public DoctorService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DoctorService> logger,
            IFileUploadService fileUploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region Profile Management

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

        public async Task<IEnumerable<DoctorProfileResponse>> GetAllDoctorsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var pagedDoctors = doctors
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in pagedDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} doctors", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all doctors");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> SearchDoctorsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var filteredDoctors = doctors
                    .Where(d =>
                        d.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        d.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        d.MedicalSpecialty.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in filteredDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Found {Count} doctors matching '{SearchTerm}'", responses.Count, searchTerm);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching doctors with term '{SearchTerm}'", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> GetDoctorsBySpecialtyAsync(string specialty, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var filteredDoctors = doctors
                    .Where(d => d.MedicalSpecialty.ToString().Equals(specialty, StringComparison.OrdinalIgnoreCase))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in filteredDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} doctors with specialty '{Specialty}'", responses.Count, specialty);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors by specialty '{Specialty}'", specialty);
                throw;
            }
        }

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



        public async Task<bool> DeleteDoctorAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    return false;

                // Soft delete
                doctor.IsDeleted = true;
                doctor.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted doctor {DoctorId}", doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorProfileResponse> UpdateProfileImageAsync(Guid doctorId, string imageUrl)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // This method is kept for backward compatibility
                // It's recommended to use UpdateDoctorProfileAsync with IFormFile instead
                doctor.ProfileImageUrl = imageUrl;
                doctor.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated profile image for doctor {DoctorId}", doctorId);
                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> GetTopRatedDoctorsAsync(int count = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var reviews = await _unitOfWork.DoctorReviews.GetAllAsync();

                var doctorsWithRatings = doctors.Select(d => new
                {
                    Doctor = d,
                    AverageRating = reviews.Where(r => r.DoctorId == d.Id).Any()
                        ? reviews.Where(r => r.DoctorId == d.Id).Average(r => r.AverageRating)
                        : 0
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(count)
                .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var item in doctorsWithRatings)
                {
                    var response = await GetDoctorProfileAsync(item.Doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} top-rated doctors", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top-rated doctors");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> GetDoctorsByGovernorateAsync(string governorate, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Note: This requires the Doctor to have a Clinic, and the Clinic to have an Address
                // The relationship is: Doctor → Clinic → Address → Governorate
                // If a doctor doesn't have a clinic or the clinic doesn't have an address, they won't be included
                
                var allDoctors = await _unitOfWork.Doctors.GetAllAsync();
                
                // Filter doctors by governorate through Clinic → Address → Governorate
                var filteredDoctors = allDoctors
                    .Where(d => d.Clinic != null && 
                                d.Clinic.Address != null && 
                                d.Clinic.Address.Governorate.ToString().Equals(governorate, StringComparison.OrdinalIgnoreCase))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in filteredDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} doctors in governorate '{Governorate}'", responses.Count, governorate);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors by governorate '{Governorate}'", governorate);
                throw;
            }
        }

        public async Task<DoctorStatisticsResponse> GetDoctorStatisticsAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var appointments = await _unitOfWork.Appointments.GetAllAsync();
                var doctorAppointments = appointments.Where(a => a.DoctorId == doctorId).ToList();

                var reviews = await _unitOfWork.DoctorReviews.GetAllAsync();
                var doctorReviews = reviews.Where(r => r.DoctorId == doctorId).ToList();

                // Get unique patients
                var uniquePatients = doctorAppointments.Select(a => a.PatientId).Distinct().Count();

                // Calculate appointments by status
                var completedAppointments = doctorAppointments.Count(a => a.Status == Core.Enums.Appointments.AppointmentStatus.Completed);
                var cancelledAppointments = doctorAppointments.Count(a => a.Status == Core.Enums.Appointments.AppointmentStatus.Cancelled);
                var upcomingAppointments = doctorAppointments.Count(a => 
                    a.Status == Core.Enums.Appointments.AppointmentStatus.Confirmed && 
                    a.ScheduledStartTime >= DateTime.UtcNow);

                // Calculate revenue
                var totalRevenue = doctorAppointments
                    .Where(a => a.Status == Core.Enums.Appointments.AppointmentStatus.Completed)
                    .Sum(a => a.ConsultationFee);

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var monthlyRevenue = doctorAppointments
                    .Where(a => a.Status == Core.Enums.Appointments.AppointmentStatus.Completed &&
                                a.ScheduledStartTime.Month == currentMonth &&
                                a.ScheduledStartTime.Year == currentYear)
                    .Sum(a => a.ConsultationFee);

                var statistics = new DoctorStatisticsResponse
                {
                    TotalPatients = uniquePatients,
                    TotalAppointments = doctorAppointments.Count,
                    CompletedAppointments = completedAppointments,
                    CancelledAppointments = cancelledAppointments,
                    UpcomingAppointments = upcomingAppointments,
                    AverageRating = doctorReviews.Any() ? doctorReviews.Average(r => r.AverageRating) : null,
                    TotalReviews = doctorReviews.Count,
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue
                };

                _logger.LogInformation("Retrieved statistics for doctor {DoctorId}", doctorId);
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Document Management

        public async Task<IEnumerable<DoctorDocumentResponse>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            try
            {
                var documents = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var doctorDocuments = documents.Where(d => d.DoctorId == doctorId).ToList();

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                var doctorName = doctor != null ? $"د. {doctor.FirstName} {doctor.LastName}" : "";

                var responses = doctorDocuments.Select(doc => new DoctorDocumentResponse
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

                _logger.LogInformation("Retrieved {Count} documents for doctor {DoctorId}", responses.Count, doctorId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for doctor {DoctorId}", doctorId);
                throw;
            }
        }

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

        public async Task<DoctorDocumentResponse> UploadDocumentAsync(Guid doctorId, UploadDoctorDocumentRequest request)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Check if doctor already has a document of this type that is not Rejected or Expired
                var allDocuments = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var existingDocument = allDocuments.FirstOrDefault(d => 
                    d.DoctorId == doctorId && 
                    d.Type == request.Type &&
                    d.Status != VerificationDocumentStatus.Rejected &&
                    d.Status != VerificationDocumentStatus.Expired);

                if (existingDocument != null)
                {
                    throw new InvalidOperationException(
                        $"A document of type '{request.Type.GetDescription()}' already exists with status '{existingDocument.Status.GetDescription()}'. " +
                        $"Please update the existing document (ID: {existingDocument.Id}) instead of uploading a new one.");
                }

                // Upload document file to Cloudinary
                var uploadResult = await _fileUploadService.UploadDocumentAsync(request.DocumentFile, doctorId.ToString());

                var document = new DoctorDocument
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    DocumentUrl = uploadResult.FileUrl,
                    Type = request.Type,
                    Status = VerificationDocumentStatus.Draft, // Start as Draft
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DoctorDocuments.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Uploaded document {DocumentId} for doctor {DoctorId}", document.Id, doctorId);

                return await GetDocumentByIdAsync(document.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve uploaded document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    return false;

                // Hard delete - permanently remove from database
                _unitOfWork.DoctorDocuments.Delete(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Hard deleted document {DocumentId}", documentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> SubmitDocumentForReviewAsync(Guid documentId)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    throw new ArgumentException($"Document with ID {documentId} not found");

                // Can only submit Draft or Rejected documents
                if (document.Status != VerificationDocumentStatus.Draft && 
                    document.Status != VerificationDocumentStatus.Rejected)
                    throw new InvalidOperationException($"Cannot submit document with status {document.Status}. Only Draft or Rejected documents can be submitted.");

                document.Status = VerificationDocumentStatus.Pending;
                document.RejectionReason = null;
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Submitted document {DocumentId} for review", documentId);

                return await GetDocumentByIdAsync(documentId)
                    ?? throw new InvalidOperationException("Failed to retrieve submitted document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting document {DocumentId} for review", documentId);
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> ApproveDocumentAsync(Guid documentId)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    throw new ArgumentException($"Document with ID {documentId} not found");

                // Can only approve Pending documents
                if (document.Status != VerificationDocumentStatus.Pending)
                    throw new InvalidOperationException($"Cannot approve document with status {document.Status}. Only Pending documents can be approved.");

                document.Status = VerificationDocumentStatus.Approved;
                document.RejectionReason = null;
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Approved document {DocumentId}", documentId);

                return await GetDocumentByIdAsync(documentId)
                    ?? throw new InvalidOperationException("Failed to retrieve approved document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> RejectDocumentAsync(Guid documentId, string rejectionReason)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    throw new ArgumentException($"Document with ID {documentId} not found");

                // Can only reject Pending documents
                if (document.Status != VerificationDocumentStatus.Pending)
                    throw new InvalidOperationException($"Cannot reject document with status {document.Status}. Only Pending documents can be rejected.");

                document.Status = VerificationDocumentStatus.Rejected;
                document.RejectionReason = rejectionReason;
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Rejected document {DocumentId}", documentId);

                return await GetDocumentByIdAsync(documentId)
                    ?? throw new InvalidOperationException("Failed to retrieve rejected document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> MarkDocumentAsExpiredAsync(Guid documentId)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    throw new ArgumentException($"Document with ID {documentId} not found");

                // Can only mark Approved documents as Expired
                if (document.Status != VerificationDocumentStatus.Approved)
                    throw new InvalidOperationException($"Cannot mark document with status {document.Status} as expired. Only Approved documents can be marked as expired.");

                document.Status = VerificationDocumentStatus.Expired;
                document.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Marked document {DocumentId} as expired", documentId);

                return await GetDocumentByIdAsync(documentId)
                    ?? throw new InvalidOperationException("Failed to retrieve expired document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking document {DocumentId} as expired", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorDocumentResponse>> GetPendingDocumentsAsync()
        {
            try
            {
                var documents = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var pendingDocuments = documents.Where(d => d.Status == VerificationDocumentStatus.Pending).ToList();

                var responses = new List<DoctorDocumentResponse>();
                foreach (var doc in pendingDocuments)
                {
                    var doctor = await _unitOfWork.Doctors.GetByIdAsync(doc.DoctorId);
                    var doctorName = doctor != null ? $"د. {doctor.FirstName} {doctor.LastName}" : "";

                    responses.Add(new DoctorDocumentResponse
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
                    });
                }

                _logger.LogInformation("Retrieved {Count} pending documents", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending documents");
                throw;
            }
        }

        public async Task<DoctorDocumentResponse> UpdateDocumentAsync(Guid documentId, UploadDoctorDocumentRequest request)
        {
            try
            {
                var document = await _unitOfWork.DoctorDocuments.GetByIdAsync(documentId);
                if (document == null)
                    throw new ArgumentException($"Document with ID {documentId} not found");

                // Can only update Draft, Rejected, or Expired documents
                if (document.Status != VerificationDocumentStatus.Draft && 
                    document.Status != VerificationDocumentStatus.Rejected &&
                    document.Status != VerificationDocumentStatus.Expired)
                    throw new InvalidOperationException($"Cannot update document with status {document.Status}. Only Draft, Rejected, or Expired documents can be updated.");

                // Upload new document file if provided
                if (request.DocumentFile != null)
                {
                    // Delete old document
                    if (!string.IsNullOrWhiteSpace(document.DocumentUrl))
                    {
                        await _fileUploadService.DeleteFileAsync(document.DocumentUrl);
                    }

                    // Upload new document
                    var uploadResult = await _fileUploadService.UploadDocumentAsync(request.DocumentFile, document.DoctorId.ToString());
                    document.DocumentUrl = uploadResult.FileUrl;
                }

                document.Type = request.Type;
                // Keep it as Draft after update, doctor needs to submit it
                document.Status = VerificationDocumentStatus.Draft;
                document.RejectionReason = null;
                document.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated document {DocumentId}", documentId);

                return await GetDocumentByIdAsync(documentId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorDocumentResponse>> GetApprovedDocumentsAsync()
        {
            try
            {
                var documents = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var approvedDocuments = documents.Where(d => d.Status == VerificationDocumentStatus.Approved).ToList();

                var responses = new List<DoctorDocumentResponse>();
                foreach (var doc in approvedDocuments)
                {
                    var doctor = await _unitOfWork.Doctors.GetByIdAsync(doc.DoctorId);
                    var doctorName = doctor != null ? $"د. {doctor.FirstName} {doctor.LastName}" : "";

                    responses.Add(new DoctorDocumentResponse
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
                    });
                }

                _logger.LogInformation("Retrieved {Count} approved documents", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approved documents");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorDocumentResponse>> GetRejectedDocumentsAsync()
        {
            try
            {
                var documents = await _unitOfWork.DoctorDocuments.GetAllAsync();
                var rejectedDocuments = documents.Where(d => d.Status == VerificationDocumentStatus.Rejected).ToList();

                var responses = new List<DoctorDocumentResponse>();
                foreach (var doc in rejectedDocuments)
                {
                    var doctor = await _unitOfWork.Doctors.GetByIdAsync(doc.DoctorId);
                    var doctorName = doctor != null ? $"د. {doctor.FirstName} {doctor.LastName}" : "";

                    responses.Add(new DoctorDocumentResponse
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
                    });
                }

                _logger.LogInformation("Retrieved {Count} rejected documents", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rejected documents");
                throw;
            }
        }

        #endregion

        #region Verification

        public async Task<DoctorProfileResponse> VerifyDoctorAsync(Guid doctorId, Guid verifierId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                doctor.VerificationStatus = VerificationStatus.Verified;
                doctor.VerifiedAt = DateTime.UtcNow;
                doctor.VerifierId = verifierId;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Verified doctor {DoctorId} by verifier {VerifierId}", doctorId, verifierId);

                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve verified doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorProfileResponse> UnverifyDoctorAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                doctor.VerificationStatus = VerificationStatus.Unverified;
                doctor.VerifiedAt = null;
                doctor.VerifierId = null;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Unverified doctor {DoctorId}", doctorId);

                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve unverified doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unverifying doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> GetPendingVerificationDoctorsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var pendingDoctors = doctors
                    .Where(d => d.VerificationStatus == VerificationStatus.UnderReview)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in pendingDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} doctors pending verification", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctors pending verification");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> GetVerifiedDoctorsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var verifiedDoctors = doctors
                    .Where(d => d.VerificationStatus == VerificationStatus.Verified)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in verifiedDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} verified doctors", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verified doctors");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorProfileResponse>> GetUnverifiedDoctorsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var doctors = await _unitOfWork.Doctors.GetAllAsync();
                var unverifiedDoctors = doctors
                    .Where(d => d.VerificationStatus == VerificationStatus.Unverified)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var responses = new List<DoctorProfileResponse>();
                foreach (var doctor in unverifiedDoctors)
                {
                    var response = await GetDoctorProfileAsync(doctor.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} unverified doctors", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unverified doctors");
                throw;
            }
        }

        public async Task<DoctorProfileResponse> SuspendDoctorAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                doctor.VerificationStatus = VerificationStatus.Suspended;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Suspended doctor {DoctorId}", doctorId);

                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve suspended doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<DoctorProfileResponse> ActivateDoctorAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                doctor.VerificationStatus = VerificationStatus.Verified;
                doctor.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Activated doctor {DoctorId}", doctorId);

                return await GetDoctorProfileAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve activated doctor profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Specialized Document Management

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
    }
}
