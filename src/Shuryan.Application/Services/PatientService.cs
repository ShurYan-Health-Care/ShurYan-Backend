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
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Interfaces;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;

        public PatientService(
            IPatientRepository patientRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
        /// Update patient information
        /// </summary>
        public async Task<PatientResponse> UpdatePatientAsync(Guid id, UpdatePatientRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    throw new ArgumentException($"Patient with ID {id} not found");
                }

                // Update only provided fields (partial update)
                if (!string.IsNullOrWhiteSpace(request.FirstName))
                {
                    patient.FirstName = request.FirstName;
                }

                if (!string.IsNullOrWhiteSpace(request.LastName))
                {
                    patient.LastName = request.LastName;
                }

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    patient.PhoneNumber = request.PhoneNumber;
                    patient.PhoneNumberConfirmed = false; // Reset confirmation
                }

                if (request.BirthDate.HasValue)
                {
                    patient.BirthDate = request.BirthDate;
                }

                if (request.Gender.HasValue)
                {
                    patient.Gender = request.Gender;
                }

                if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl))
                {
                    patient.ProfileImageUrl = request.ProfileImageUrl;
                }

                // Handle Address - Create if doesn't exist, Update if exists
                if (request.Address != null)
                {
                    // Reload patient with address details
                    patient = await _patientRepository.GetByIdWithDetailsAsync(id);
                    
                    if (patient.Address == null)
                    {
                        // Create new address
                        var newAddress = _mapper.Map<Address>(request.Address);
                        newAddress.Id = Guid.NewGuid();
                        newAddress.CreatedAt = DateTime.UtcNow;
                        
                        patient.Address = newAddress;
                        patient.AddressId = newAddress.Id;
                        
                        _logger.LogInformation("Created new address for patient {PatientId}", id);
                    }
                    else
                    {
                        // Update existing address
                        if (!string.IsNullOrWhiteSpace(request.Address.Street))
                        {
                            patient.Address.Street = request.Address.Street;
                        }

                        if (!string.IsNullOrWhiteSpace(request.Address.City))
                        {
                            patient.Address.City = request.Address.City;
                        }

                        if (request.Address.Governorate.HasValue)
                        {
                            patient.Address.Governorate = request.Address.Governorate.Value;
                        }

                        if (!string.IsNullOrWhiteSpace(request.Address.BuildingNumber))
                        {
                            patient.Address.BuildingNumber = request.Address.BuildingNumber;
                        }

                        if (request.Address.Latitude.HasValue)
                        {
                            patient.Address.Latitude = request.Address.Latitude;
                        }

                        if (request.Address.Longitude.HasValue)
                        {
                            patient.Address.Longitude = request.Address.Longitude;
                        }

                        patient.Address.UpdatedAt = DateTime.UtcNow;
                        
                        _logger.LogInformation("Updated address for patient {PatientId}", id);
                    }
                }

                patient.UpdatedAt = DateTime.UtcNow;
                patient.UpdatedBy = patient.Id; // Should be current user ID in real scenario

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated patient with ID {PatientId}", id);

                // Return updated patient with full details
                var updatedPatient = await _patientRepository.GetByIdWithDetailsAsync(id);
                return _mapper.Map<PatientResponse>(updatedPatient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID {PatientId}", id);
                throw;
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
                    .Where(lo => lo.Status == LabOrderStatus.PaidPendingLabConfirmation || lo.Status == LabOrderStatus.InProgress)
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
        /// Update patient address
        /// </summary>
        public async Task<AddressResponse> UpdatePatientAddressAsync(Guid patientId, UpdateAddressRequest request)
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

                if (patient.Address == null)
                {
                    throw new InvalidOperationException($"Patient {patientId} does not have an address to update. Create one first.");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(request.Street))
                {
                    patient.Address.Street = request.Street;
                }

                if (!string.IsNullOrWhiteSpace(request.City))
                {
                    patient.Address.City = request.City;
                }

                if (request.Governorate.HasValue)
                {
                    patient.Address.Governorate = request.Governorate.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.BuildingNumber))
                {
                    patient.Address.BuildingNumber = request.BuildingNumber;
                }

                if (request.Latitude.HasValue)
                {
                    patient.Address.Latitude = request.Latitude;
                }

                if (request.Longitude.HasValue)
                {
                    patient.Address.Longitude = request.Longitude;
                }

                patient.Address.UpdatedAt = DateTime.UtcNow;
                patient.UpdatedAt = DateTime.UtcNow;

                _patientRepository.Update(patient);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated address for patient {PatientId}", patientId);

                return _mapper.Map<AddressResponse>(patient.Address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address for patient {PatientId}", patientId);
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

        #endregion
    }
}