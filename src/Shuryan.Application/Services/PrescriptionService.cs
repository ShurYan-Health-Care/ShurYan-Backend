using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Prescription;
using Shuryan.Application.DTOs.Responses.Prescription;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public partial class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PrescriptionService> _logger;

        public PrescriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PrescriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Basic CRUD Operations
        public async Task<PrescriptionResponse?> GetPrescriptionByIdAsync(Guid id)
        {
            try
            {
                var prescription = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(id);

                if (prescription == null)
                {
                    _logger.LogWarning("Prescription with ID {PrescriptionId} not found", id);
                    return null;
                }

                return _mapper.Map<PrescriptionResponse>(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription with ID {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<PrescriptionResponse> CreatePrescriptionAsync(CreatePrescriptionRequest request)
        {
            try
            {
                // Validate that patient exists
                // TODO: Fix Patient Repository soft delete issue
                // var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
                // if (patient == null)
                // {
                //     throw new ArgumentException($"Patient with ID {request.PatientId} not found");
                // }

                // Validate that doctor exists
                // TODO: Fix Doctor Repository soft delete issue
                // var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
                // if (doctor == null)
                // {
                //     throw new ArgumentException($"Doctor with ID {request.DoctorId} not found");
                // }

                // Validate that appointment exists
                // TODO: Fix Appointment Repository soft delete issue
                // var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
                // if (appointment == null)
                // {
                //     throw new ArgumentException($"Appointment with ID {request.AppointmentId} not found");
                // }

                // Check if prescription number already exists
                var existingPrescription = await _unitOfWork.Prescriptions
                    .FindByPrescriptionNumberAsync(request.PrescriptionNumber);
                if (existingPrescription != null)
                {
                    throw new InvalidOperationException($"Prescription number {request.PrescriptionNumber} already exists");
                }

                // Create prescription entity
                var prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    PrescriptionNumber = request.PrescriptionNumber,
                    DigitalSignature = request.DigitalSignature,
                    AppointmentId = request.AppointmentId,
                    DoctorId = request.DoctorId,
                    PatientId = request.PatientId,
                    GeneralInstructions = request.GeneralInstructions,
                    Status = Core.Enums.PrescriptionStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                // Add prescribed medications
                foreach (var medicationRequest in request.PrescribedMedications)
                {
                    // Validate medication exists
                    var medication = await _unitOfWork.Medications.GetByIdAsync(medicationRequest.MedicationId);
                    if (medication == null)
                    {
                        throw new ArgumentException($"Medication with ID {medicationRequest.MedicationId} not found");
                    }

                    var prescribedMedication = new PrescribedMedication
                    {
                        MedicationPrescriptionId = prescription.Id,
                        MedicationId = medicationRequest.MedicationId,
                        Dosage = medicationRequest.Dosage,
                        Frequency = medicationRequest.Frequency,
                        DurationDays = medicationRequest.DurationDays,
                        SpecialInstructions = medicationRequest.SpecialInstructions
                    };

                    prescription.PrescribedMedications.Add(prescribedMedication);
                }

                await _unitOfWork.Prescriptions.AddAsync(prescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Prescription {PrescriptionNumber} created successfully for patient {PatientId}",
                    prescription.PrescriptionNumber, prescription.PatientId);

                // Reload with details
                var createdPrescription = await _unitOfWork.Prescriptions
                    .GetPrescriptionWithDetailsAsync(prescription.Id);

                return _mapper.Map<PrescriptionResponse>(createdPrescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                throw;
            }
        }

        public async Task<PrescriptionResponse> UpdatePrescriptionAsync(Guid id, UpdatePrescriptionRequest request)
        {
            try
            {
                var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(id);

                if (prescription == null)
                {
                    throw new ArgumentException($"Prescription with ID {id} not found");
                }

                // Update only the allowed fields
                if (request.GeneralInstructions != null)
                {
                    prescription.GeneralInstructions = request.GeneralInstructions;
                }

                prescription.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Prescriptions.Update(prescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Prescription {PrescriptionId} updated successfully", id);

                // Reload with details
                var updatedPrescription = await _unitOfWork.Prescriptions
                    .GetPrescriptionWithDetailsAsync(id);

                return _mapper.Map<PrescriptionResponse>(updatedPrescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription with ID {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<bool> DeletePrescriptionAsync(Guid id)
        {
            try
            {
                var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(id);
                if (prescription == null)
                    return false;

                // Hard delete (since we removed soft delete from AuditableEntity)
                _unitOfWork.Prescriptions.Delete(prescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Prescription {PrescriptionId} deleted", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                throw;
            }
        }
        #endregion


        #region Query Operations
        public async Task<PaginatedResponse<PrescriptionResponse>> GetPaginatedPrescriptionsAsync(PaginationParams request)
        {
            try
            {
                var query = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .OrderByDescending(p => p.CreatedAt);

                var totalCount = query.Count();
                var prescriptions = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var prescriptionResponses = _mapper.Map<List<PrescriptionResponse>>(prescriptions);

                return new PaginatedResponse<PrescriptionResponse>
                {
                    Data = prescriptionResponses,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated prescriptions");
                throw;
            }
        }

        public async Task<PrescriptionResponse?> GetPrescriptionByNumberAsync(string prescriptionNumber)
        {
            try
            {
                var prescription = await _unitOfWork.Prescriptions
                    .FindByPrescriptionNumberAsync(prescriptionNumber);

                if (prescription == null)
                {
                    _logger.LogWarning("Prescription with number {PrescriptionNumber} not found", prescriptionNumber);
                    return null;
                }

                return _mapper.Map<PrescriptionResponse>(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription by number {PrescriptionNumber}", prescriptionNumber);
                throw;
            }
        }

        public async Task<int> GetTotalPrescriptionsCountAsync()
        {
            try
            {
                var prescriptions = await _unitOfWork.Prescriptions.GetAllAsync();
                return prescriptions.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total prescriptions count");
                throw;
            }

        }
        #endregion


        #region Patient Related Operations
        public async Task<IEnumerable<PrescriptionResponse>> GetPatientPrescriptionsAsync(Guid patientId)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null || patient.IsDeleted)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescriptions = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Where(p => p.PatientId == patientId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<PaginatedResponse<PrescriptionResponse>> GetPaginatedPatientPrescriptionsAsync(Guid patientId, PaginationParams request)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null || patient.IsDeleted)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescriptions = await _unitOfWork.Prescriptions
                    .GetPagedPrescriptionsForPatientAsync(patientId, request.PageNumber, request.PageSize);

                var prescriptionsList = prescriptions.ToList();
                var totalCount = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Count(p => p.PatientId == patientId);

                var prescriptionResponses = _mapper.Map<List<PrescriptionResponse>>(prescriptionsList);

                return new PaginatedResponse<PrescriptionResponse>
                {
                    Data = prescriptionResponses,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<PrescriptionResponse>> GetActivePatientPrescriptionsAsync(Guid patientId)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null || patient.IsDeleted)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescriptions = await _unitOfWork.Prescriptions
                    .GetActivePrescriptionsForPatientAsync(patientId);

                var activePrescriptions = prescriptions.ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(activePrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<int> GetPatientPrescriptionsCountAsync(Guid patientId)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null || patient.IsDeleted)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var count = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Count(p => p.PatientId == patientId);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions count for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region Doctor Related Operations
        public async Task<IEnumerable<PrescriptionResponse>> GetDoctorPrescriptionsAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null || doctor.IsDeleted)
                {
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                var prescriptions = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Where(p => p.DoctorId == doctorId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<PaginatedResponse<PrescriptionResponse>> GetPaginatedDoctorPrescriptionsAsync(Guid doctorId, PaginationParams request)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null || doctor.IsDeleted)
                {
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                var query = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Where(p => p.DoctorId == doctorId)
                    .OrderByDescending(p => p.CreatedAt);

                var totalCount = query.Count();
                var prescriptions = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var prescriptionResponses = _mapper.Map<List<PrescriptionResponse>>(prescriptions);

                return new PaginatedResponse<PrescriptionResponse>
                {
                    Data = prescriptionResponses,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated prescriptions for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<int> GetDoctorPrescriptionsCountAsync(Guid doctorId)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null || doctor.IsDeleted)
                {
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                var count = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Count(p => p.DoctorId == doctorId);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions count for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Pharmacy Related Operations

        public async Task<IEnumerable<PrescriptionResponse>> GetPrescriptionsContainingMedicationAsync(Guid medicationId)
        {
            try
            {
                var medication = await _unitOfWork.Medications.GetByIdAsync(medicationId);
                if (medication == null)
                {
                    throw new ArgumentException($"Medication with ID {medicationId} not found");
                }

                var prescriptions = await _unitOfWork.Prescriptions
                    .GetPrescriptionsContainingMedicationAsync(medicationId);

                var activePrescriptions = prescriptions.ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(activePrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions containing medication {MedicationId}", medicationId);
                throw;
            }
        }

        public async Task<bool> MarkPrescriptionAsDispensedAsync(Guid prescriptionId)
        {
            try
            {
                var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(prescriptionId);

                if (prescription == null)
                {
                    _logger.LogWarning("Prescription with ID {PrescriptionId} not found", prescriptionId);
                    return false;
                }

                if (prescription.Status == Core.Enums.PrescriptionStatus.Cancelled)
                {
                    _logger.LogWarning("Cannot dispense cancelled prescription {PrescriptionId}", prescriptionId);
                    return false;
                }

                prescription.Status = Core.Enums.PrescriptionStatus.Dispensed;
                prescription.DispensedAt = DateTime.UtcNow;
                prescription.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Prescriptions.Update(prescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Prescription {PrescriptionId} marked as dispensed", prescriptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking prescription {PrescriptionId} as dispensed", prescriptionId);
                throw;
            }
        }

        #endregion

        #region Date Range & Analytics Operations
        public async Task<IEnumerable<PrescriptionResponse>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var prescriptions = await _unitOfWork.Prescriptions
                    .GetPrescriptionsByDateRangeAsync(startDate, endDate);

                var activePrescriptions = prescriptions.ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(activePrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions by date range");
                throw;
            }
        }

        public async Task<IEnumerable<PrescriptionResponse>> GetDoctorPrescriptionsByDateRangeAsync(Guid doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null || doctor.IsDeleted)
                {
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");
                }

                var prescriptions = await _unitOfWork.Prescriptions
                    .GetPrescriptionsByDateRangeAsync(startDate, endDate);

                var doctorPrescriptions = prescriptions
                    .Where(p => p.DoctorId == doctorId)
                    .ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(doctorPrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions by date range for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<PrescriptionResponse>> GetPatientPrescriptionsByDateRangeAsync(Guid patientId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null || patient.IsDeleted)
                {
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                var prescriptions = await _unitOfWork.Prescriptions
                    .GetPrescriptionsByDateRangeAsync(startDate, endDate);

                var patientPrescriptions = prescriptions
                    .Where(p => p.PatientId == patientId)
                    .ToList();

                return _mapper.Map<IEnumerable<PrescriptionResponse>>(patientPrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions by date range for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        #region  Appointment Related Operations
        public async Task<PrescriptionResponse?> GetPrescriptionByAppointmentIdAsync(Guid appointmentId)
        {
            try
            {
                var prescription = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .FirstOrDefault(p => p.AppointmentId == appointmentId);

                if (prescription == null)
                {
                    _logger.LogWarning("No prescription found for appointment {AppointmentId}", appointmentId);
                    return null;
                }

                // Get full details
                var prescriptionWithDetails = await _unitOfWork.Prescriptions
                    .GetPrescriptionWithDetailsAsync(prescription.Id);

                return _mapper.Map<PrescriptionResponse>(prescriptionWithDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription for appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<bool> AppointmentHasPrescriptionAsync(Guid appointmentId)
        {
            try
            {
                var prescription = (await _unitOfWork.Prescriptions.GetAllAsync())
                    .Any(p => p.AppointmentId == appointmentId);

                return prescription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if appointment {AppointmentId} has prescription", appointmentId);
                throw;
            }
        }
        #endregion

        #region Advanced Query Operations
        public async Task<PrescriptionQueryResponse> GetPrescriptionsAsync(PrescriptionQueryParams queryParams)
        {
            try
            {
                var query = (await _unitOfWork.Prescriptions.GetAllAsync()).AsQueryable();

                // Apply filters
                if (queryParams.PatientId.HasValue)
                    query = query.Where(p => p.PatientId == queryParams.PatientId.Value);

                if (queryParams.DoctorId.HasValue)
                    query = query.Where(p => p.DoctorId == queryParams.DoctorId.Value);

                if (queryParams.AppointmentId.HasValue)
                    query = query.Where(p => p.AppointmentId == queryParams.AppointmentId.Value);

                if (queryParams.Status.HasValue)
                    query = query.Where(p => p.Status == queryParams.Status.Value);

                if (queryParams.StartDate.HasValue)
                    query = query.Where(p => p.CreatedAt >= queryParams.StartDate.Value);

                if (queryParams.EndDate.HasValue)
                    query = query.Where(p => p.CreatedAt <= queryParams.EndDate.Value);

                if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
                {
                    var searchLower = queryParams.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.PrescriptionNumber.ToLower().Contains(searchLower));
                }

                // Sorting
                query = queryParams.SortBy?.ToLower() switch
                {
                    "createdat" => queryParams.SortDescending
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),
                    "updatedat" => queryParams.SortDescending
                        ? query.OrderByDescending(p => p.UpdatedAt)
                        : query.OrderBy(p => p.UpdatedAt),
                    "prescriptionnumber" => queryParams.SortDescending
                        ? query.OrderByDescending(p => p.PrescriptionNumber)
                        : query.OrderBy(p => p.PrescriptionNumber),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                // Pagination
                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize);

                var prescriptions = query
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToList();

                // Map to response with details
                var prescriptionResponses = new List<PrescriptionResponse>();
                foreach (var prescription in prescriptions)
                {
                    var details = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(prescription.Id);
                    if (details != null)
                    {
                        prescriptionResponses.Add(new PrescriptionResponse
                        {
                            Id = details.Id,
                            PrescriptionNumber = details.PrescriptionNumber,
                            DigitalSignature = details.DigitalSignature,
                            AppointmentId = details.AppointmentId,
                            DoctorId = details.DoctorId,
                            PatientId = details.PatientId,
                            GeneralInstructions = details.GeneralInstructions,
                            Status = details.Status,
                            DispensedAt = details.DispensedAt,
                            CancellationReason = details.CancellationReason,
                            CancelledAt = details.CancelledAt,
                            CreatedAt = details.CreatedAt,
                            UpdatedAt = details.UpdatedAt,
                            Doctor = details.Doctor != null ? new DTOs.Responses.Doctor.DoctorBasicResponse
                            {
                                Id = details.Doctor.Id,
                                FirstName = details.Doctor.FirstName,
                                LastName = details.Doctor.LastName,
                                MedicalSpecialty = details.Doctor.MedicalSpecialty,
                                ProfileImageUrl = details.Doctor.ProfileImageUrl,
                                YearsOfExperience = details.Doctor.YearsOfExperience
                            } : null,
                            Patient = details.Patient != null ? new DTOs.Responses.Patient.PatientBasicResponse
                            {
                                Id = details.Patient.Id,
                                FirstName = details.Patient.FirstName,
                                LastName = details.Patient.LastName,
                                PhoneNumber = details.Patient.PhoneNumber
                            } : null,
                            PrescribedMedications = details.PrescribedMedications?.Select(pm => new PrescribedMedicationResponse
                            {
                                MedicationId = pm.MedicationId,
                                Medication = pm.Medication != null ? new MedicationResponse
                                {
                                    Id = pm.Medication.Id,
                                    BrandName = pm.Medication.BrandName,
                                    GenericName = pm.Medication.GenericName
                                } : null,
                                Dosage = pm.Dosage,
                                Frequency = pm.Frequency,
                                DurationDays = pm.DurationDays,
                                SpecialInstructions = pm.SpecialInstructions
                            }).ToList() ?? new List<PrescribedMedicationResponse>()
                        });
                    }
                }

                return new PrescriptionQueryResponse
                {
                    Prescriptions = prescriptionResponses,
                    Page = queryParams.Page,
                    PageSize = queryParams.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = queryParams.Page > 1,
                    HasNextPage = queryParams.Page < totalPages,
                    AppliedFilters = new Dictionary<string, object?>
                    {
                        ["PatientId"] = queryParams.PatientId,
                        ["DoctorId"] = queryParams.DoctorId,
                        ["SearchTerm"] = queryParams.SearchTerm,
                        ["StartDate"] = queryParams.StartDate,
                        ["EndDate"] = queryParams.EndDate
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions with query params");
                throw;
            }
        }
        #endregion

        #region Prescription Lifecycle Operations
        public async Task<PrescriptionResponse> CancelPrescriptionAsync(Guid id, CancelPrescriptionRequest request)
        {
            try
            {
                var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(id);
                if (prescription == null)
                    throw new ArgumentException($"Prescription with ID {id} not found");

                if (prescription.Status == Core.Enums.PrescriptionStatus.Cancelled)
                    throw new InvalidOperationException("Prescription is already cancelled");

                // Update status
                prescription.Status = Core.Enums.PrescriptionStatus.Cancelled;
                prescription.CancellationReason = request.Reason;
                prescription.CancelledAt = DateTime.UtcNow;

                // Also add to instructions for backward compatibility
                prescription.GeneralInstructions += $"\n\n[CANCELLED] Reason: {request.Reason}";
                if (!string.IsNullOrWhiteSpace(request.Notes))
                    prescription.GeneralInstructions += $"\nNotes: {request.Notes}";

                _unitOfWork.Prescriptions.Update(prescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Prescription {PrescriptionId} cancelled", id);

                return _mapper.Map<PrescriptionResponse>(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling prescription {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<PrescriptionResponse> RenewPrescriptionAsync(Guid id, RenewPrescriptionRequest request)
        {
            try
            {
                var originalPrescription = await _unitOfWork.Prescriptions
                    .GetPrescriptionWithDetailsAsync(id);

                if (originalPrescription == null)
                    throw new ArgumentException($"Prescription with ID {id} not found");

                var newPrescription = new Prescription
                {
                    PrescriptionNumber = GenerateUniquePrescriptionNumber(),
                    PatientId = originalPrescription.PatientId,
                    DoctorId = originalPrescription.DoctorId,
                    AppointmentId = request.NewAppointmentId, // Don't copy old appointment (unique constraint)
                    GeneralInstructions = request.GeneralInstructions ?? originalPrescription.GeneralInstructions,
                    DigitalSignature = originalPrescription.DigitalSignature,
                    Status = Core.Enums.PrescriptionStatus.Active
                };

                newPrescription.GeneralInstructions += $"\n\n[RENEWED FROM: {originalPrescription.PrescriptionNumber}]";
                if (!string.IsNullOrWhiteSpace(request.RenewalReason))
                    newPrescription.GeneralInstructions += $"\nRenewal Reason: {request.RenewalReason}";

                // Copy medications if requested
                if (request.CopyAllMedications && originalPrescription.PrescribedMedications != null)
                {
                    foreach (var oldMed in originalPrescription.PrescribedMedications)
                    {
                        var newMed = new PrescribedMedication
                        {
                            MedicationPrescriptionId = newPrescription.Id,
                            MedicationId = oldMed.MedicationId,
                            Dosage = oldMed.Dosage,
                            Frequency = oldMed.Frequency,
                            DurationDays = request.DurationInDays > 0 ? request.DurationInDays : oldMed.DurationDays,
                            SpecialInstructions = oldMed.SpecialInstructions
                        };
                        newPrescription.PrescribedMedications.Add(newMed);
                    }
                }

                await _unitOfWork.Prescriptions.AddAsync(newPrescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Prescription {OriginalId} renewed as {NewId}",
                    id, newPrescription.Id);

                return _mapper.Map<PrescriptionResponse>(newPrescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing prescription {PrescriptionId}", id);
                throw;
            }
        }
        #endregion

        #region Patient Medication Operations
        public async Task<IEnumerable<CurrentMedicationResponse>> GetCurrentMedicationsAsync(Guid patientId)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                    throw new ArgumentException($"Patient with ID {patientId} not found");

                // Get all prescriptions for this patient (not cancelled)
                var allPrescriptions = await _unitOfWork.Prescriptions.GetAllAsync();
                var activePrescriptions = allPrescriptions
                    .Where(p => p.PatientId == patientId &&
                                p.Status != Core.Enums.PrescriptionStatus.Cancelled &&
                                p.Status != Core.Enums.PrescriptionStatus.Expired)
                    .ToList();

                var currentMedications = new List<CurrentMedicationResponse>();

                foreach (var prescription in activePrescriptions)
                {
                    // Get prescription with details
                    var prescriptionDetails = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(prescription.Id);
                    if (prescriptionDetails?.PrescribedMedications == null)
                        continue;

                    foreach (var prescribedMed in prescriptionDetails.PrescribedMedications)
                    {
                        // Calculate if medication is still active
                        var startDate = prescriptionDetails.CreatedAt;
                        var endDate = startDate.AddDays(prescribedMed.DurationDays);
                        var daysRemaining = (int)(endDate - DateTime.UtcNow).TotalDays;

                        // Only include medications that haven't expired yet (or show all for debugging)
                        if (endDate > DateTime.UtcNow || daysRemaining >= -30)  // Show medications from last 30 days
                        {
                            currentMedications.Add(new CurrentMedicationResponse
                            {
                                MedicationId = prescribedMed.MedicationId,
                                MedicationName = prescribedMed.Medication?.BrandName ?? "Unknown",
                                BrandName = prescribedMed.Medication?.BrandName,
                                GenericName = prescribedMed.Medication?.GenericName,
                                Strength = prescribedMed.Medication?.Strength,
                                DosageForm = prescribedMed.Dosage ?? "N/A",
                                PrescriptionId = prescriptionDetails.Id,
                                PrescriptionNumber = prescriptionDetails.PrescriptionNumber,
                                PrescribedDate = startDate,
                                Dosage = prescribedMed.Dosage,
                                Frequency = prescribedMed.Frequency,
                                DurationInDays = prescribedMed.DurationDays,
                                SpecialInstructions = prescribedMed.SpecialInstructions,
                                DoctorId = prescriptionDetails.DoctorId,
                                DoctorName = prescriptionDetails.Doctor != null
                                    ? $"{prescriptionDetails.Doctor.FirstName} {prescriptionDetails.Doctor.LastName}"
                                    : null,
                                DoctorSpecialization = null, // TODO: Add specialization if needed
                                IsDispensed = prescriptionDetails.Status == Core.Enums.PrescriptionStatus.Dispensed,
                                DispensedDate = prescriptionDetails.Status == Core.Enums.PrescriptionStatus.Dispensed
                                    ? prescriptionDetails.UpdatedAt
                                    : null,
                                RemainingDays = daysRemaining > 0 ? daysRemaining : 0,
                                StartDate = startDate,
                                EndDate = endDate,
                                DaysRemaining = daysRemaining
                            });
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} current medications for patient {PatientId}",
                    currentMedications.Count, patientId);

                return currentMedications.OrderBy(m => m.EndDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current medications for patient {PatientId}", patientId);
                throw;
            }
        }
        #endregion

        #region Patient-Doctor Prescription Operations

        public async Task<PrescriptionDetailedResponse?> GetPrescriptionBetweenPatientAndDoctorAsync(
            Guid prescriptionId,
            Guid patientId,
            Guid doctorId)
        {
            try
            {
                // جلب الروشتة بالتفاصيل
                var prescription = await _unitOfWork.Prescriptions.GetPrescriptionWithDetailsAsync(prescriptionId);

                if (prescription == null)
                {
                    _logger.LogWarning("Prescription {PrescriptionId} not found", prescriptionId);
                    return null;
                }

                // التحقق من أن الروشتة خاصة بالمريض والدكتور المحددين
                if (prescription.PatientId != patientId || prescription.DoctorId != doctorId)
                {
                    _logger.LogWarning(
                        "Prescription {PrescriptionId} does not belong to patient {PatientId} and doctor {DoctorId}",
                        prescriptionId, patientId, doctorId);
                    return null;
                }

                // تحويل البيانات للـ Response
                var response = new PrescriptionDetailedResponse
                {
                    Id = prescription.Id,
                    PrescriptionNumber = prescription.PrescriptionNumber,
                    CreatedAt = prescription.CreatedAt,
                    Medications = prescription.PrescribedMedications?.Select(pm => new MedicationDetailResponse
                    {
                        MedicationName = pm.Medication?.BrandName ?? "Unknown",
                        Dosage = pm.Dosage,
                        Frequency = pm.Frequency,
                        DurationDays = pm.DurationDays,
                        SpecialInstructions = pm.SpecialInstructions
                    }).ToList() ?? new List<MedicationDetailResponse>()
                };

                _logger.LogInformation(
                    "Retrieved prescription {PrescriptionId} between patient {PatientId} and doctor {DoctorId}",
                    prescriptionId, patientId, doctorId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting prescription {PrescriptionId} between patient {PatientId} and doctor {DoctorId}",
                    prescriptionId, patientId, doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<PrescriptionListItemResponse>> GetPrescriptionListBetweenPatientAndDoctorAsync(
            Guid patientId,
            Guid doctorId)
        {
            try
            {
                // جلب كل الروشتات بين المريض والدكتور
                var allPrescriptions = await _unitOfWork.Prescriptions.GetAllAsync();
                var prescriptions = allPrescriptions
                    .Where(p => p.PatientId == patientId && p.DoctorId == doctorId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PrescriptionListItemResponse
                    {
                        Id = p.Id,
                        PrescriptionNumber = p.PrescriptionNumber,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList();

                _logger.LogInformation(
                    "Retrieved {Count} prescriptions between patient {PatientId} and doctor {DoctorId}",
                    prescriptions.Count, patientId, doctorId);

                return prescriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting prescription list between patient {PatientId} and doctor {DoctorId}",
                    patientId, doctorId);
                throw;
            }
        }

        #endregion

        #region Medication Operations

        public async Task<IEnumerable<MedicationNameResponse>> GetAllMedicationNamesAsync(string? searchTerm = null)
        {
            try
            {
                // جلب كل الأدوية من الـ Repository
                var medications = await _unitOfWork.Medications.GetAllAsync();

                // تطبيق الـ search لو موجود
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower().Trim();
                    medications = medications.Where(m =>
                        m.BrandName.ToLower().Contains(searchLower) ||
                        (m.GenericName != null && m.GenericName.ToLower().Contains(searchLower))
                    ).ToList();
                }

                // تحويلها لـ Response
                var medicationNames = medications
                    .OrderBy(m => m.BrandName)
                    .Select(m => new MedicationNameResponse
                    {
                        Id = m.Id,
                        BrandName = m.BrandName,
                        GenericName = m.GenericName,
                        Strength = m.Strength,
                        DosageForm = m.DosageForm
                    })
                    .ToList();

                _logger.LogInformation(
                    "Retrieved {Count} medication names{SearchInfo}",
                    medicationNames.Count,
                    string.IsNullOrWhiteSpace(searchTerm) ? "" : $" matching '{searchTerm}'");

                return medicationNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication names with search term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        #endregion

        #region Patient Prescriptions

        public async Task<IEnumerable<PatientPrescriptionListResponse>> GetPatientPrescriptionsListAsync(Guid patientId)
        {
            try
            {
                _logger.LogInformation("Retrieving all prescriptions for patient {PatientId}", patientId);

                // التحقق من وجود المريض
                var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found: {PatientId}", patientId);
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                // جلب كل الروشتات مع الـ Doctor و Appointment
                var prescriptions = await _unitOfWork.Prescriptions.GetAllPrescriptionsForPatientWithDetailsAsync(patientId);
                
                var patientPrescriptions = prescriptions
                    .Select(p => new PatientPrescriptionListResponse
                    {
                        Id = p.Id,
                        PrescriptionNumber = p.PrescriptionNumber,
                        CreatedAt = p.CreatedAt,
                        Status = (int)p.Status,
                        StatusName = p.Status.ToString(),
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor != null ? $"{p.Doctor.FirstName} {p.Doctor.LastName}" : "Unknown",
                        DoctorSpecialty = p.Doctor != null ? GetMedicalSpecialtyDescription(p.Doctor.MedicalSpecialty) : "غير محدد",
                        DoctorProfileImageUrl = p.Doctor?.ProfileImageUrl,
                        AppointmentType = p.Appointment?.ConsultationType == Core.Enums.Appointments.ConsultationTypeEnum.FollowUp ? "followup" : "regular",
                        AppointmentId = p.AppointmentId
                    })
                    .ToList();

                _logger.LogInformation(
                    "Retrieved {Count} prescriptions for patient {PatientId}",
                    patientPrescriptions.Count, patientId);

                return patientPrescriptions;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        #endregion

        // Helper methods
        private string GenerateUniquePrescriptionNumber()
        {
            return $"RX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GetMedicalSpecialtyDescription(Shuryan.Core.Enums.Doctor.MedicalSpecialty specialty)
        {
            var type = specialty.GetType();
            var memberInfo = type.GetMember(specialty.ToString());
            
            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    return ((System.ComponentModel.DescriptionAttribute)attributes[0]).Description;
                }
            }
            
            return specialty.ToString();
        }
    }
}
