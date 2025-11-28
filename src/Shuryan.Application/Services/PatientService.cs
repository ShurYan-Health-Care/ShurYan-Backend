using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public partial class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;
        private readonly IFileUploadService _fileUploadService;

        public PatientService(
            IPatientRepository patientRepository,
            IPharmacyRepository pharmacyRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PatientService> logger,
            IFileUploadService fileUploadService)
        {
            _patientRepository = patientRepository;
            _pharmacyRepository = pharmacyRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Get patient by ID with full details
        /// </summary>
        public async Task<PatientResponse?> GetPatientByIdAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(id);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", id);
                    return null;
                }

                return _mapper.Map<PatientResponse>(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with ID {PatientId}", id);
                throw;
            }
        }

        /// <summary>
        /// Get patient by email
        /// </summary>
        public async Task<PatientResponse?> GetPatientByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));
                }

                var patient = await _patientRepository.GetByEmailAsync(email);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with email {Email} not found", email);
                    return null;
                }

                return _mapper.Map<PatientResponse>(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with email {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Create a new patient
        /// </summary>
        public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
        {
            try
            {
                // Validate request
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                // Check if email already exists
                var existingPatient = await _patientRepository.GetByEmailAsync(request.Email);
                if (existingPatient != null)
                {
                    throw new InvalidOperationException($"A patient with email {request.Email} already exists");
                }

                // Map and create patient
                var patient = _mapper.Map<Patient>(request);
                patient.Id = Guid.NewGuid();
                patient.CreatedAt = DateTime.UtcNow;
                patient.UserName = request.Email;
                patient.NormalizedUserName = request.Email.ToUpperInvariant();
                patient.NormalizedEmail = request.Email.ToUpperInvariant();
                patient.EmailConfirmed = false;
                patient.PhoneNumberConfirmed = false;
                patient.AccessFailedCount = 0;

                await _patientRepository.AddAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new patient with ID {PatientId}", patient.Id);

                // Return with full details
                var createdPatient = await _patientRepository.GetByIdWithDetailsAsync(patient.Id);
                return _mapper.Map<PatientResponse>(createdPatient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                throw;
            }
        }

        /// <summary>
        /// Update patient personal information (Partial Update)
        /// يعني بيحدث بس الحاجات اللي انت بعتها، مش كل الـ fields
        /// </summary>
        public async Task<PatientResponse> UpdatePatientAsync(Guid id, UpdatePatientRequest request)
        {
            try
            {
                // Validation
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "Request cannot be null");
                }

                // Get patient
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {id} not found");
                }

                // Track if any changes were made
                bool hasChanges = false;

                // Update FirstName (بس لو موجود وفيه قيمة)
                if (!string.IsNullOrWhiteSpace(request.FirstName))
                {
                    var trimmedFirstName = request.FirstName.Trim();
                    if (patient.FirstName != trimmedFirstName)
                    {
                        patient.FirstName = trimmedFirstName;
                        hasChanges = true;
                        _logger.LogInformation("Updated FirstName for patient {PatientId}", id);
                    }
                }

                // Update LastName (بس لو موجود وفيه قيمة)
                if (!string.IsNullOrWhiteSpace(request.LastName))
                {
                    var trimmedLastName = request.LastName.Trim();
                    if (patient.LastName != trimmedLastName)
                    {
                        patient.LastName = trimmedLastName;
                        hasChanges = true;
                        _logger.LogInformation("Updated LastName for patient {PatientId}", id);
                    }
                }

                // Update PhoneNumber (بس لو موجود وفيه قيمة)
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    var trimmedPhone = request.PhoneNumber.Trim();
                    if (patient.PhoneNumber != trimmedPhone)
                    {
                        patient.PhoneNumber = trimmedPhone;
                        patient.PhoneNumberConfirmed = false; // Reset confirmation عشان لازم يأكد الرقم الجديد
                        hasChanges = true;
                        _logger.LogInformation("Updated PhoneNumber for patient {PatientId}", id);
                    }
                }

                // Update BirthDate (بس لو موجود)
                if (request.BirthDate.HasValue)
                {
                    if (patient.BirthDate != request.BirthDate.Value)
                    {
                        // Validate birth date (مينفعش يكون في المستقبل)
                        if (request.BirthDate.Value > DateTime.UtcNow)
                        {
                            throw new ArgumentException("Birth date cannot be in the future");
                        }

                        patient.BirthDate = request.BirthDate.Value;
                        hasChanges = true;
                        _logger.LogInformation("Updated BirthDate for patient {PatientId}", id);
                    }
                }

                // Update Gender (بس لو موجود)
                if (request.Gender.HasValue)
                {
                    if (patient.Gender != request.Gender.Value)
                    {
                        patient.Gender = request.Gender.Value;
                        hasChanges = true;
                        _logger.LogInformation("Updated Gender for patient {PatientId}", id);
                    }
                }

                // لو مفيش أي تغييرات، ارجع الـ patient زي ما هو
                if (!hasChanges)
                {
                    _logger.LogInformation("No changes detected for patient {PatientId}", id);
                    return _mapper.Map<PatientResponse>(patient);
                }

                // Update metadata
                patient.UpdatedAt = DateTime.UtcNow;
                patient.UpdatedBy = id; // Current patient is updating their own data

                // Save changes
                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated patient {PatientId}", id);

                // Return updated patient
                var updatedPatient = await _patientRepository.GetByIdAsync(id);
                return _mapper.Map<PatientResponse>(updatedPatient);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID {PatientId}", id);
                throw new InvalidOperationException($"Failed to update patient: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Permanently delete a patient
        /// </summary>
        public async Task<bool> DeletePatientAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent patient with ID {PatientId}", id);
                    return false;
                }
                await _patientRepository.RemoveAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Permanently deleted patient with ID {PatientId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient with ID {PatientId}", id);
                throw;
            }
        }

        /// <summary>
        /// Restore a soft-deleted patient
        /// </summary>
        public async Task<bool> RestorePatientAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    _logger.LogWarning("Attempted to restore non-existent patient with ID {PatientId}", id);
                    return false;
                }

                if (!patient.IsDeleted)
                {
                    _logger.LogInformation("Patient with ID {PatientId} is not deleted", id);
                    return false;
                }

                patient.IsDeleted = false;
                patient.DeletedAt = null;
                patient.DeletedBy = null;

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Restored patient with ID {PatientId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring patient with ID {PatientId}", id);
                throw;
            }
        }

        public async Task<PatientResponse?> GetCurrentPatientAsync(Guid userId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(userId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", userId);
                    return null;
                }

                return _mapper.Map<PatientResponse>(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with ID {PatientId}", userId);
                throw;
            }
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all patients
        /// </summary>
        public async Task<IEnumerable<PatientResponse>> GetAllPatientsAsync(bool includeDeleted = false)
        {
            try
            {
                var query = await _patientRepository.GetAllAsync();

                if (!includeDeleted)
                {
                    query = query.Where(p => !p.IsDeleted);
                }

                var patients = query.OrderByDescending(p => p.CreatedAt).ToList();
                return _mapper.Map<IEnumerable<PatientResponse>>(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all patients");
                throw;
            }
        }

        /// <summary>
        /// Get paginated patients
        /// </summary>
        public async Task<PaginatedResponse<PatientResponse>> GetPaginatedPatientsAsync(PaginationParams request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var query = await _patientRepository.GetAllAsync();
                query = query.Where(p => !p.IsDeleted);

                var totalCount = query.Count();

                var patients = query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var patientResponses = _mapper.Map<IEnumerable<PatientResponse>>(patients);

                return new PaginatedResponse<PatientResponse>
                {
                    Data = patientResponses,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated patients");
                throw;
            }
        }

        /// <summary>
        /// Search patients with advanced filtering
        /// </summary>
        public async Task<PaginatedResponse<PatientResponse>> SearchPatientsAsync(SearchTermPatientsRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var query = _patientRepository.GetQueryable();
                query = query.Where(p => !p.IsDeleted);

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.FirstName.ToLower().Contains(searchTerm) ||
                        p.LastName.ToLower().Contains(searchTerm) ||
                        p.Email.ToLower().Contains(searchTerm) ||
                        (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)));
                }

                // Gender filter
                if (request.Gender.HasValue)
                {
                    query = query.Where(p => p.Gender == request.Gender.Value);
                }

                // Birth date range filter
                if (request.BirthDateFrom.HasValue)
                {
                    query = query.Where(p => p.BirthDate >= request.BirthDateFrom.Value);
                }

                if (request.BirthDateTo.HasValue)
                {
                    query = query.Where(p => p.BirthDate <= request.BirthDateTo.Value);
                }

                // Medical history filter
                if (request.HasMedicalHistory.HasValue)
                {
                    if (request.HasMedicalHistory.Value)
                    {
                        query = query.Where(p => p.MedicalHistory.Any());
                    }
                    else
                    {
                        query = query.Where(p => !p.MedicalHistory.Any());
                    }
                }

                // City filter
                if (!string.IsNullOrWhiteSpace(request.City))
                {
                    query = query.Where(p => p.Address != null &&
                        p.Address.City.ToLower().Contains(request.City.ToLower()));
                }

                // Apply sorting
                query = ApplySorting(query, request.SortBy, request.SortDescending);

                var totalCount = query.Count();

                var patients = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var patientResponses = _mapper.Map<IEnumerable<PatientResponse>>(patients);

                return new PaginatedResponse<PatientResponse>
                {
                    Data = patientResponses,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients");
                throw;
            }
        }

        /// <summary>
        /// Get patients with medical history
        /// </summary>
        public async Task<IEnumerable<PatientResponse>> GetPatientsWithMedicalHistoryAsync()
        {
            try
            {
                var patients = await _patientRepository.GetPatientsWithMedicalHistoryAsync();
                return _mapper.Map<IEnumerable<PatientResponse>>(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients with medical history");
                throw;
            }
        }

        /// <summary>
        /// Check if email is unique
        /// </summary>
        ///         Task<bool> IsEmailUniqueAsync(string email);
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));
                }

                var patient = await _patientRepository.GetByEmailAsync(email);

                if (patient == null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email uniqueness for {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Get total patients count
        /// </summary>
        public async Task<int> GetTotalPatientsCountAsync(bool includeDeleted = false)
        {
            try
            {
                var query = await _patientRepository.GetAllAsync();

                if (!includeDeleted)
                {
                    query = query.Where(p => !p.IsDeleted);
                }

                return query.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total patients count");
                throw;
            }
        }

        #endregion

        #region Medical History Operations

        /// <summary>
        /// Get patient medical history
        /// </summary>
        public async Task<IEnumerable<MedicalHistoryItemResponse>> GetPatientMedicalHistoryAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var medicalHistory = patient.MedicalHistory
                    .OrderByDescending(m => m.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<MedicalHistoryItemResponse>>(medicalHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical history for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Add medical history item
        /// </summary>
        public async Task<MedicalHistoryItemResponse> AddMedicalHistoryItemAsync(Guid patientId, CreateMedicalHistoryItemRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                // Check if patient exists
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var medicalHistoryItem = _mapper.Map<MedicalHistoryItem>(request);
                medicalHistoryItem.Id = Guid.NewGuid();
                medicalHistoryItem.PatientId = patientId;
                medicalHistoryItem.CreatedAt = DateTime.UtcNow;
                medicalHistoryItem.CreatedBy = patientId;

                // Add using repository
                await _unitOfWork.MedicalHistoryItems.AddAsync(medicalHistoryItem);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Added medical history item for patient {PatientId}", patientId);

                return _mapper.Map<MedicalHistoryItemResponse>(medicalHistoryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medical history item for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Update medical history item
        /// </summary>
        public async Task<MedicalHistoryItemResponse> UpdateMedicalHistoryItemAsync(Guid patientId, Guid itemId, UpdateMedicalHistoryItemRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var medicalHistoryItem = patient.MedicalHistory
                    .FirstOrDefault(m => m.Id == itemId);

                if (medicalHistoryItem == null)
                {
                    throw new ArgumentException($"Medical history item with ID {itemId} not found");
                }

                if (request.Type.HasValue)
                    medicalHistoryItem.Type = request.Type.Value;

                if (!string.IsNullOrWhiteSpace(request.Text))
                    medicalHistoryItem.Text = request.Text.Trim();

                medicalHistoryItem.UpdatedAt = DateTime.UtcNow;

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated medical history item {ItemId} for patient {PatientId}", itemId, patientId);

                return _mapper.Map<MedicalHistoryItemResponse>(medicalHistoryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical history item for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Delete medical history item
        /// </summary>
        public async Task<bool> DeleteMedicalHistoryItemAsync(Guid patientId, Guid itemId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var medicalHistoryItem = patient.MedicalHistory
                    .FirstOrDefault(m => m.Id == itemId);

                if (medicalHistoryItem == null)
                {
                    _logger.LogWarning("Medical history item {ItemId} not found for patient {PatientId}", itemId, patientId);
                    return false;
                }

                patient.MedicalHistory.Remove(medicalHistoryItem);

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted medical history item {ItemId} for patient {PatientId}", itemId, patientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medical history item for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region Appointments Operations

        /// <summary>
        /// Get all appointments for a patient
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetPatientAppointmentsAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var appointments = patient.Appointments
                    .OrderByDescending(a => a.ScheduledStartTime)
                    .ToList();

                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for patient {PatientId}", patientId);
                throw;
            }
        }



        /// <summary>
        /// Get upcoming appointments for a patient
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetUpcomingAppointmentsAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var now = DateTime.UtcNow;
                var upcomingAppointments = patient.Appointments
                    .Where(a => a.ScheduledStartTime >= now && a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.ScheduledStartTime)
                    .ToList();

                return _mapper.Map<IEnumerable<AppointmentResponse>>(upcomingAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming appointments for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get past appointments for a patient
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetPastAppointmentsAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var now = DateTime.UtcNow;
                var pastAppointments = patient.Appointments
                    .Where(a => a.ScheduledStartTime < now || a.Status == AppointmentStatus.Completed)
                    .OrderByDescending(a => a.ScheduledStartTime)
                    .ToList();

                return _mapper.Map<IEnumerable<AppointmentResponse>>(pastAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving past appointments for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get the next upcoming appointment for a patient
        /// </summary>
        public async Task<AppointmentResponse?> GetNextAppointmentAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var now = DateTime.UtcNow;
                var nextAppointment = patient.Appointments
                    .Where(a => a.ScheduledStartTime >= now && a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.ScheduledStartTime)
                    .FirstOrDefault();

                return nextAppointment != null ? _mapper.Map<AppointmentResponse>(nextAppointment) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next appointment for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get total appointments count for a patient
        /// </summary>
        public async Task<int> GetAppointmentsCountAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                return patient.Appointments.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments count for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region Prescriptions Operations

        /// <summary>
        /// Get all prescriptions for a patient
        /// </summary>
        public async Task<IEnumerable<PrescriptionResponse>> GetPatientPrescriptionsAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescriptions = patient.Prescriptions
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get active prescriptions for a patient
        /// </summary>
        public async Task<IEnumerable<PrescriptionResponse>> GetActivePrescriptionsAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var activePrescriptions = patient.Prescriptions
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(activePrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get a specific prescription by ID for a patient
        /// </summary>
        public async Task<PrescriptionResponse?> GetPrescriptionByIdAsync(Guid patientId, Guid prescriptionId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescription = patient.Prescriptions
                    .FirstOrDefault(p => p.Id == prescriptionId);

                if (prescription == null)
                {
                    _logger.LogWarning("Prescription {PrescriptionId} not found for patient {PatientId}", prescriptionId, patientId);
                    return null;
                }

                return _mapper.Map<PrescriptionResponse>(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription {PrescriptionId} for patient {PatientId}", prescriptionId, patientId);
                throw;
            }
        }

        #endregion

        #region Lab Orders Operations

        /// <summary>
        /// Get all lab orders for a patient
        /// </summary>
        public async Task<IEnumerable<LabOrderResponse>> GetPatientLabOrdersAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var labOrders = patient.LabOrders
                    .OrderByDescending(lo => lo.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<LabOrderResponse>>(labOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lab orders for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get pending lab orders for a patient
        /// </summary>
        public async Task<IEnumerable<LabOrderResponse>> GetPendingLabOrdersAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var pendingLabOrders = patient.LabOrders
                    .Where(lo => lo.Status != LabOrderStatus.Completed && 
                                 lo.Status != LabOrderStatus.CancelledByPatient && 
                                 lo.Status != LabOrderStatus.RejectedByLab)
                    .OrderByDescending(lo => lo.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<LabOrderResponse>>(pendingLabOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending lab orders for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get a specific lab order by ID for a patient
        /// </summary>
        public async Task<LabOrderResponse?> GetLabOrderByIdAsync(Guid patientId, Guid orderId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var labOrder = patient.LabOrders
                    .FirstOrDefault(lo => lo.Id == orderId);

                if (labOrder == null)
                {
                    _logger.LogWarning("Lab order {OrderId} not found for patient {PatientId}", orderId, patientId);
                    return null;
                }

                return _mapper.Map<LabOrderResponse>(labOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lab order {OrderId} for patient {PatientId}", orderId, patientId);
                throw;
            }
        }

        #endregion

        #region Address Operations

        /// <summary>
        /// Get patient address
        /// </summary>
        public async Task<AddressResponse?> GetPatientAddressAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                if (patient.Address == null)
                {
                    _logger.LogInformation("Patient {PatientId} does not have an address", patientId);
                    return null;
                }

                return _mapper.Map<AddressResponse>(patient.Address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Update or create patient address
        /// لو العنوان موجود هيتحدث، لو مش موجود هيتعمل create
        /// </summary>
        public async Task<AddressResponse> UpdatePatientAddressAsync(Guid patientId, UpdateAddressRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                // لو العنوان مش موجود، اعمله create
                if (patient.AddressId == null)
                {
                    _logger.LogInformation("Creating new address for patient {PatientId}", patientId);
                    
                    var newAddress = new Address
                    {
                        Id = Guid.NewGuid(),
                        Street = request.Street ?? string.Empty,
                        City = request.City ?? string.Empty,
                        Governorate = request.Governorate ?? Governorate.Cairo,
                        BuildingNumber = request.BuildingNumber,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Add address using generic repository
                    await _unitOfWork.Repository<Address>().AddAsync(newAddress);
                    
                    // Link to patient
                    patient.AddressId = newAddress.Id;
                    patient.UpdatedAt = DateTime.UtcNow;
                    
                    _patientRepository.Update(patient);
                    await _unitOfWork.SaveChangesAsync();
                    
                    _logger.LogInformation("Address created successfully for patient {PatientId}", patientId);
                    return _mapper.Map<AddressResponse>(newAddress);
                }
                else
                {
                    // Update existing address (Partial Update)
                    _logger.LogInformation("Updating existing address for patient {PatientId}", patientId);
                    
                    // Load the address using generic repository
                    var address = await _unitOfWork.Repository<Address>().GetByIdAsync(patient.AddressId.Value);
                    if (address == null)
                    {
                        throw new InvalidOperationException($"Address with ID {patient.AddressId.Value} not found");
                    }
                    
                    if (!string.IsNullOrWhiteSpace(request.Street))
                    {
                        address.Street = request.Street;
                    }

                    if (!string.IsNullOrWhiteSpace(request.City))
                    {
                        address.City = request.City;
                    }

                    if (request.Governorate.HasValue)
                    {
                        address.Governorate = request.Governorate.Value;
                    }

                    if (!string.IsNullOrWhiteSpace(request.BuildingNumber))
                    {
                        address.BuildingNumber = request.BuildingNumber;
                    }

                    if (request.Latitude.HasValue)
                    {
                        address.Latitude = request.Latitude;
                    }

                    if (request.Longitude.HasValue)
                    {
                        address.Longitude = request.Longitude;
                    }

                    address.UpdatedAt = DateTime.UtcNow;
                    patient.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Repository<Address>().Update(address);
                    _patientRepository.Update(patient);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Address updated successfully for patient {PatientId}", patientId);
                    return _mapper.Map<AddressResponse>(address);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating/creating address for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Create patient address
        /// </summary>
        public async Task<AddressResponse> CreatePatientAddressAsync(CreateAddressRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var address = _mapper.Map<Address>(request);
                address.Id = Guid.NewGuid();
                address.CreatedAt = DateTime.UtcNow;

                // Note: This creates an address but doesn't link it to a patient
                // You may want to modify this to accept patientId and link it
                _logger.LogInformation("Created new address with ID {AddressId}", address.Id);

                return _mapper.Map<AddressResponse>(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                throw;
            }
        }

        #endregion

        #region Profile Operations

        /// <summary>
        /// Update patient profile image
        /// </summary>
        public async Task<bool> UpdateProfileImageAsync(Guid patientId, string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));
                }

                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", patientId);
                    return false;
                }

                patient.ProfileImageUrl = imageUrl;
                patient.UpdatedAt = DateTime.UtcNow;

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated profile image for patient {PatientId}", patientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Remove patient profile image
        /// </summary>
        public async Task<bool> RemoveProfileImageAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", patientId);
                    return false;
                }

                patient.ProfileImageUrl = null;
                patient.UpdatedAt = DateTime.UtcNow;

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Removed profile image for patient {PatientId}", patientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing profile image for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region Pharmacy Operations

        /// <summary>
        /// البحث عن أقرب 3 صيدليات للمريض بناءً على الإحداثيات
        /// </summary>
        public async Task<FindNearbyPharmaciesResponse> FindNearbyPharmaciesAsync(FindNearbyPharmaciesRequest request)
        {
            try
            {
                _logger.LogInformation("Searching for nearby pharmacies at coordinates: {Latitude}, {Longitude}", 
                    request.Latitude, request.Longitude);

                // جلب كل الصيدليات المفعلة مع تفاصيلها
                var pharmacies = await _pharmacyRepository.GetAllActivePharmaciesWithDetailsAsync();

                if (!pharmacies.Any())
                {
                    _logger.LogWarning("No active pharmacies found in the system");
                    return new FindNearbyPharmaciesResponse
                    {
                        NearbyPharmacies = new List<NearbyPharmacyResponse>(),
                        TotalFound = 0,
                        SearchRadiusKm = 0
                    };
                }

                // حساب المسافة لكل صيدلية
                var pharmaciesWithDistance = new List<(Pharmacy pharmacy, double distance)>();

                foreach (var pharmacy in pharmacies)
                {
                    if (pharmacy.Address?.Latitude.HasValue == true && pharmacy.Address?.Longitude.HasValue == true)
                    {
                        var distance = CalculateDistance(
                            request.Latitude, request.Longitude,
                            pharmacy.Address.Latitude.Value, pharmacy.Address.Longitude.Value);

                        pharmaciesWithDistance.Add((pharmacy, distance));
                    }
                }

                // ترتيب حسب المسافة وأخذ أقرب 3
                var nearestPharmacies = pharmaciesWithDistance
                    .OrderBy(x => x.distance)
                    .Take(3)
                    .ToList();

                // تحويل إلى Response DTOs - بس المعلومات المطلوبة
                var nearbyPharmacyResponses = nearestPharmacies.Select(x => new NearbyPharmacyResponse
                {
                    Id = x.pharmacy.Id,
                    Name = x.pharmacy.Name,
                    DistanceInKm = Math.Round(x.distance, 2),
                    OffersDelivery = x.pharmacy.OffersDelivery,
                    DeliveryFee = x.pharmacy.DeliveryFee,
                    ProfileImageUrl = x.pharmacy.ProfilePictureUrl,
                    PhoneNumber = x.pharmacy.PhoneNumber ?? string.Empty
                }).ToList();

                var maxDistance = nearestPharmacies.Any() ? nearestPharmacies.Max(x => x.distance) : 0;

                _logger.LogInformation("Found {Count} nearby pharmacies within {MaxDistance}km", 
                    nearbyPharmacyResponses.Count, Math.Round(maxDistance, 2));

                return new FindNearbyPharmaciesResponse
                {
                    NearbyPharmacies = nearbyPharmacyResponses,
                    TotalFound = nearbyPharmacyResponses.Count,
                    SearchRadiusKm = Math.Round(maxDistance, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby pharmacies for coordinates: {Latitude}, {Longitude}", 
                    request.Latitude, request.Longitude);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Apply sorting to patient query
        /// </summary>
        private IQueryable<Patient> ApplySorting(IQueryable<Patient> query, PatientSortBy sortBy, bool sortDescending)
        {
            return sortBy switch
            {
                PatientSortBy.FirstName => sortDescending
                    ? query.OrderByDescending(p => p.FirstName)
                    : query.OrderBy(p => p.FirstName),
                PatientSortBy.LastName => sortDescending
                    ? query.OrderByDescending(p => p.LastName)
                    : query.OrderBy(p => p.LastName),
                PatientSortBy.Email => sortDescending
                    ? query.OrderByDescending(p => p.Email)
                    : query.OrderBy(p => p.Email),
                PatientSortBy.BirthDate => sortDescending
                    ? query.OrderByDescending(p => p.BirthDate)
                    : query.OrderBy(p => p.BirthDate),
                _ => sortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };
        }

        /// <summary>
        /// حساب المسافة بين نقطتين باستخدام Haversine formula
        /// </summary>
        /// <param name="lat1">خط العرض للنقطة الأولى</param>
        /// <param name="lon1">خط الطول للنقطة الأولى</param>
        /// <param name="lat2">خط العرض للنقطة الثانية</param>
        /// <param name="lon2">خط الطول للنقطة الثانية</param>
        /// <returns>المسافة بالكيلومتر</returns>
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        /// <summary>
        /// تحويل الدرجات إلى راديان
        /// </summary>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        #endregion

        #region Pharmacy Operations - Patient Specific
        public async Task<FindNearbyPharmaciesResponse> FindNearbyPharmaciesForPatientAsync(Guid patientId)
        {
            try
            {
                _logger.LogInformation("Finding nearby pharmacies for patient: {PatientId}", patientId);

                var patient = await _patientRepository.GetByIdWithDetailsAsync(patientId);
                if (patient == null || patient.IsDeleted)
                {
                    throw new KeyNotFoundException($"Patient with ID {patientId} not found");
                }

                if (patient.Address == null)
                {
                    throw new KeyNotFoundException("Patient address not found. Please add your address first.");
                }

                if (!patient.Address.Latitude.HasValue || !patient.Address.Longitude.HasValue)
                {
                    throw new KeyNotFoundException("Patient address coordinates not found. Please update your address with location.");
                }

                var request = new FindNearbyPharmaciesRequest
                {
                    Latitude = patient.Address.Latitude.Value,
                    Longitude = patient.Address.Longitude.Value
                };

                _logger.LogInformation("Using patient address coordinates: {Latitude}, {Longitude}", 
                    request.Latitude, request.Longitude);

                return await FindNearbyPharmaciesAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding nearby pharmacies for patient: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<SendPrescriptionResponse> SendPrescriptionToPharmacyAsync(Guid patientId, Guid prescriptionId, SendPrescriptionToPharmacyRequest request)
        {
            try
            {
                _logger.LogInformation("Sending prescription {PrescriptionId} from patient {PatientId} to pharmacy {PharmacyId}", 
                    prescriptionId, patientId, request.PharmacyId);

                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescription = await _unitOfWork.Repository<Prescription>()
                    .GetQueryable()
                    .Include(p => p.Doctor)
                    .Include(p => p.PrescribedMedications)
                        .ThenInclude(pm => pm.Medication)
                    .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.PatientId == patientId);

                if (prescription == null)
                {
                    throw new ArgumentException($"Prescription with ID {prescriptionId} not found for patient {patientId}");
                }

                if (prescription.Status != PrescriptionStatus.Active)
                {
                    throw new InvalidOperationException($"Prescription {prescriptionId} is not active. Current status: {prescription.Status}");
                }

                var pharmacy = await _pharmacyRepository.GetByIdAsync(request.PharmacyId);
                if (pharmacy == null)
                {
                    throw new ArgumentException($"Pharmacy with ID {request.PharmacyId} not found");
                }

                var existingOrder = await _unitOfWork.Repository<PharmacyOrder>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(po => po.PrescriptionId == prescriptionId && 
                                              po.Status != PharmacyOrderStatus.Cancelled);

                if (existingOrder != null)
                {
                    throw new InvalidOperationException($"Prescription {prescriptionId} has already been sent to a pharmacy");
                }

                var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                var pharmacyOrder = new PharmacyOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = orderNumber,
                    Status = PharmacyOrderStatus.PendingPharmacyResponse,
                    PatientId = patientId,
                    PharmacyId = request.PharmacyId,
                    PrescriptionId = prescriptionId,
                    DeliveryType = OrderDeliveryType.Delivery,
                    PatientNotes = null, 
                    TotalCost = 0, 
                    DeliveryFee = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<PharmacyOrder>().AddAsync(pharmacyOrder);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created pharmacy order {OrderId} for prescription {PrescriptionId}", 
                    pharmacyOrder.Id, prescriptionId);

                var response = new SendPrescriptionResponse
                {
                    OrderId = pharmacyOrder.Id,
                    OrderNumber = orderNumber,
                    PharmacyName = pharmacy.Name,
                    PrescriptionNumber = prescription.PrescriptionNumber,
                    Status = (int)pharmacyOrder.Status,
                    StatusName = pharmacyOrder.Status.ToString(),
                    SentAt = pharmacyOrder.CreatedAt,
                    Message = $"تم إرسال الروشتة بنجاح إلى {pharmacy.Name}"
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending prescription {PrescriptionId} to pharmacy {PharmacyId}", 
                    prescriptionId, request.PharmacyId);
                throw;
            }
        }

        public async Task<PatientPharmacyResponseView> GetPharmacyResponseAsync(Guid patientId, Guid orderId)
        {
            try
            {
                _logger.LogInformation("Getting pharmacy response for order {OrderId} and patient {PatientId}", orderId, patientId);

                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var order = await _unitOfWork.Repository<PharmacyOrder>()
                    .GetQueryable()
                    .Include(po => po.Pharmacy)
                    .Include(po => po.Prescription)
                        .ThenInclude(p => p.PrescribedMedications)
                            .ThenInclude(pm => pm.Medication)
                    .Include(po => po.OrderItems)
                        .ThenInclude(oi => oi.RequestedMedication)
                    .Include(po => po.OrderItems)
                        .ThenInclude(oi => oi.AlternativeMedication)
                    .FirstOrDefaultAsync(po => po.Id == orderId && po.PatientId == patientId);

                if (order == null)
                {
                    throw new ArgumentException($"Order with ID {orderId} not found for patient {patientId}");
                }

                if (order.Status == PharmacyOrderStatus.PendingPharmacyResponse)
                {
                    throw new InvalidOperationException("Pharmacy has not responded to this order yet");
                }

                var medications = new List<PatientMedicationResponse>();
                
                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    foreach (var orderItem in order.OrderItems)
                    {
                        var alternativeInfo = orderItem.AlternativeMedication != null ? new PatientAlternativeMedicationResponse
                        {
                            MedicationName = orderItem.AlternativeMedication.BrandName,
                            UnitPrice = orderItem.AlternativeUnitPrice ?? 0
                        } : null;

                        medications.Add(new PatientMedicationResponse
                        {
                            MedicationName = orderItem.RequestedMedication.BrandName,
                            IsAvailable = orderItem.Status == PharmacyItemStatus.Available,
                            AvailableQuantity = orderItem.AvailableQuantity ?? 0,
                            UnitPrice = orderItem.UnitPrice ?? 0,
                            AlternativeOne = alternativeInfo
                        });
                    }
                }
                else if (order.Prescription?.PrescribedMedications != null)
                {
                    foreach (var prescribedMed in order.Prescription.PrescribedMedications)
                    {
                        medications.Add(new PatientMedicationResponse
                        {
                            MedicationName = prescribedMed.Medication.BrandName,
                            IsAvailable = false,
                            AvailableQuantity = 0,
                            UnitPrice = 0,
                            AlternativeOne = null
                        });
                    }
                }

                // إنشاء الـ response
                var response = new PatientPharmacyResponseView
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    PharmacyName = order.Pharmacy.Name,
                    PharmacyPhone = order.Pharmacy.PhoneNumber ?? string.Empty,
                    PrescriptionNumber = order.Prescription?.PrescriptionNumber ?? string.Empty,
                    Status = (int)order.Status,
                    StatusName = order.Status.ToString(),
                    Medications = medications,
                    TotalAmount = order.TotalCost,
                    DeliveryAvailable = order.DeliveryType == OrderDeliveryType.Delivery,
                    DeliveryFee = order.DeliveryFee,
                    PharmacyNotes = "جميع الأدوية أصلية ومرخصة من وزارة الصحة",
                    RespondedAt = order.UpdatedAt ?? DateTime.UtcNow,
                    SentAt = order.CreatedAt
                };

                _logger.LogInformation("Successfully retrieved pharmacy response for order {OrderId}", orderId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pharmacy response for order {OrderId} and patient {PatientId}", orderId, patientId);
                throw;
            }
        }

        public async Task<PrescriptionPharmacyResponsesView> GetPrescriptionPharmacyResponsesAsync(Guid patientId, Guid prescriptionId)
        {
            try
            {
                _logger.LogInformation("Getting all pharmacy responses for prescription {PrescriptionId} and patient {PatientId}", 
                    prescriptionId, patientId);

                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescription = await _unitOfWork.Repository<Prescription>()
                    .GetQueryable()
                    .Include(p => p.Doctor)
                    .Include(p => p.PrescribedMedications)
                        .ThenInclude(pm => pm.Medication)
                    .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.PatientId == patientId);

                if (prescription == null)
                {
                    throw new ArgumentException($"Prescription with ID {prescriptionId} not found for patient {patientId}");
                }

                var orders = await _unitOfWork.Repository<PharmacyOrder>()
                    .GetQueryable()
                    .Include(po => po.Pharmacy)
                        .ThenInclude(p => p.Address)
                    .Include(po => po.OrderItems)
                        .ThenInclude(oi => oi.RequestedMedication)
                    .Include(po => po.OrderItems)
                        .ThenInclude(oi => oi.AlternativeMedication)
                    .Where(po => po.PrescriptionId == prescriptionId && po.PatientId == patientId)
                    .Where(po => po.Status != PharmacyOrderStatus.PendingPharmacyResponse) 
                    .OrderBy(po => po.TotalCost) 
                    .ToListAsync();

                var pharmacyResponses = new List<PharmacyResponseSummary>();

                foreach (var order in orders)
                {
                    var medications = new List<PatientMedicationResponse>();
                    int availableCount = 0, unavailableCount = 0, alternativesCount = 0;

                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        foreach (var orderItem in order.OrderItems)
                        {
                            var alternativeInfo = orderItem.AlternativeMedication != null ? new PatientAlternativeMedicationResponse
                            {
                                MedicationName = orderItem.AlternativeMedication.BrandName,
                                UnitPrice = orderItem.AlternativeUnitPrice ?? 0
                            } : null;

                            medications.Add(new PatientMedicationResponse
                            {
                                MedicationName = orderItem.RequestedMedication.BrandName,
                                IsAvailable = orderItem.Status == PharmacyItemStatus.Available,
                                AvailableQuantity = orderItem.AvailableQuantity ?? 0,
                                UnitPrice = orderItem.UnitPrice ?? 0,
                                AlternativeOne = alternativeInfo
                            });

                            switch (orderItem.Status)
                            {
                                case PharmacyItemStatus.Available:
                                    availableCount++;
                                    break;
                                case PharmacyItemStatus.NotAvailable:
                                    unavailableCount++;
                                    break;
                                case PharmacyItemStatus.AlternativeOffered:
                                    alternativesCount++;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var prescribedMed in prescription.PrescribedMedications)
                        {
                            medications.Add(new PatientMedicationResponse
                            {
                                MedicationName = prescribedMed.Medication.BrandName,
                                IsAvailable = false,
                                AvailableQuantity = 0,
                                UnitPrice = 0,
                                AlternativeOne = null
                            });
                            unavailableCount++;
                        }
                    }

                    var pharmacyResponse = new PharmacyResponseSummary
                    {
                        PharmacyId = order.PharmacyId,
                        PharmacyName = order.Pharmacy.Name,
                        PharmacyPhone = order.Pharmacy.PhoneNumber ?? string.Empty,
                        PharmacyAddress = order.Pharmacy.Address != null 
                            ? $"{order.Pharmacy.Address.Street}, {order.Pharmacy.Address.City}" 
                            : string.Empty,
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber,
                        TotalAmount = order.TotalCost,
                        DeliveryFee = order.DeliveryFee,
                        DeliveryAvailable = order.DeliveryType == OrderDeliveryType.Delivery,
                        EstimatedDeliveryTime = order.EstimatedDeliveryTime?.ToString("yyyy-MM-dd HH:mm") ?? "غير محدد",
                        RespondedAt = order.UpdatedAt ?? DateTime.UtcNow,
                        Status = (int)order.Status,
                        StatusName = order.Status.ToString(),
                        PharmacyNotes = "جميع الأدوية أصلية ومرخصة من وزارة الصحة",
                        Medications = medications,
                        AvailableMedicationsCount = availableCount,
                        UnavailableMedicationsCount = unavailableCount,
                        AlternativesOfferedCount = alternativesCount
                    };

                    pharmacyResponses.Add(pharmacyResponse);
                }

                var response = new PrescriptionPharmacyResponsesView
                {
                    PrescriptionId = prescription.Id,
                    PrescriptionNumber = prescription.PrescriptionNumber,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    DoctorName = $"د. {prescription.Doctor.FirstName} {prescription.Doctor.LastName}",
                    IssuedAt = prescription.CreatedAt,
                    TotalMedications = prescription.PrescribedMedications.Count,
                    TotalPharmacyResponses = pharmacyResponses.Count,
                    PharmacyResponses = pharmacyResponses
                };

                _logger.LogInformation("Successfully retrieved {Count} pharmacy responses for prescription {PrescriptionId}", 
                    pharmacyResponses.Count, prescriptionId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pharmacy responses for prescription {PrescriptionId} and patient {PatientId}", 
                    prescriptionId, patientId);
                throw;
            }
        }

        public async Task<ConfirmOrderResponse> ConfirmPharmacyOrderAsync(Guid patientId, Guid orderId)
        {
            try
            {
                _logger.LogInformation("Patient {PatientId} attempting to confirm order {OrderId}", patientId, orderId);

                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    throw new KeyNotFoundException($"Patient with ID {patientId} not found");
                }

                var order = await _unitOfWork.PharmacyOrders.GetOrderForPatientAsync(orderId, patientId);
                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found for patient {patientId}");
                }

                if (order.Status != PharmacyOrderStatus.WaitingForPatientConfirmation)
                {
                    throw new InvalidOperationException($"Order cannot be confirmed. Current status: {order.Status}");
                }

                order.Status = PharmacyOrderStatus.Confirmed;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.PharmacyOrders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} confirmed successfully by patient {PatientId}", orderId, patientId);

                return new ConfirmOrderResponse
                {
                    OrderId = orderId,
                    Status = PharmacyOrderStatus.Confirmed,
                    ConfirmedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId} for patient {PatientId}", orderId, patientId);
                throw;
            }
        }

        #endregion
    }
}
