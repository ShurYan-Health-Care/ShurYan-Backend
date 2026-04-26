using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Responses.Emergency;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class EmergencyModeService : IEmergencyModeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmergencyAuditService _auditService;
        private readonly ILogger<EmergencyModeService> _logger;

        public EmergencyModeService(
            IUnitOfWork unitOfWork,
            IEmergencyAuditService auditService,
            ILogger<EmergencyModeService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<bool> ActivateEmergencyModeAsync(Guid doctorId, Guid appointmentId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new ArgumentException("Appointment not found", nameof(appointmentId));
            }

            if (appointment.DoctorId != doctorId)
            {
                throw new UnauthorizedAccessException("Only the attending doctor can activate emergency mode from this appointment.");
            }

            var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
            if (patient == null)
            {
                throw new ArgumentException("Patient not found", nameof(appointment.PatientId));
            }

            if (patient.EmergencyModeActive)
            {
                return true; // Already active
            }

            // Validate missing fields
            var missingFields = new List<string>();
            if (!patient.BloodType.HasValue) missingFields.Add("فصيلة الدم (Blood Type)");
            if (!patient.WeightKg.HasValue) missingFields.Add("الوزن (Weight)");
            if (!patient.HeightCm.HasValue) missingFields.Add("الطول (Height)");
            
            if (string.IsNullOrWhiteSpace(patient.EmergencyContactName) || 
                string.IsNullOrWhiteSpace(patient.EmergencyContactPhone) || 
                string.IsNullOrWhiteSpace(patient.EmergencyContactRelationship))
            {
                missingFields.Add("بيانات اتصال الطوارئ كاملة (Complete Emergency Contact)");
            }

            if (missingFields.Count > 0)
            {
                var errorMsg = $"لا يمكن تفعيل وضع الطوارئ. يرجى التأكد من اكتمال بيانات المريض التالية: {string.Join("، ", missingFields)}";
                _logger.LogWarning("Failed to activate emergency mode for patient {PatientId}. Missing: {MissingFields}", patient.Id, string.Join(", ", missingFields));
                throw new InvalidOperationException(errorMsg);
            }

            patient.EmergencyModeActive = true;
            patient.EmergencyModeActivatedById = doctorId;
            
            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActionAsync(doctorId, patient.Id, EmergencyActionType.EmergencyModeActivated);

            _logger.LogInformation("Doctor {DoctorId} activated emergency mode for Patient {PatientId}", doctorId, patient.Id);
            return true;
        }

        public async Task<bool> DeactivateEmergencyModeAsync(Guid doctorId, Guid patientId)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (patient == null)
            {
                throw new ArgumentException("Patient not found", nameof(patientId));
            }

            if (!patient.EmergencyModeActive)
            {
                return true; // Already inactive
            }

            patient.EmergencyModeActive = false;
            
            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActionAsync(doctorId, patient.Id, EmergencyActionType.EmergencyModeDeactivated);

            _logger.LogInformation("Doctor {DoctorId} deactivated emergency mode for Patient {PatientId}", doctorId, patientId);
            return true;
        }

        public async Task<IEnumerable<EmergencyPatientResponse>> GetDoctorEmergencyPatientsAsync(Guid doctorId)
        {
            var patients = await _unitOfWork.Patients.FindAsync(p =>
                p.EmergencyModeActive && p.EmergencyModeActivatedById == doctorId);

            return patients.Select(p => new EmergencyPatientResponse
            {
                PatientId = p.Id,
                PatientName = $"{p.FirstName} {p.LastName}",
                BloodType = p.BloodType,
                WeightKg = p.WeightKg,
                HeightCm = p.HeightCm,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone,
                EmergencyContactRelationship = p.EmergencyContactRelationship,
                IsPregnant = p.IsPregnant ?? false,
                PhysicalDisabilities = p.PhysicalDisabilities
            });
        }

        public async Task<IEnumerable<EmergencyEventResponse>> GetDoctorSosEventsAsync(Guid doctorId)
        {
            var doctorPatients = await _unitOfWork.Patients.FindAsync(p =>
                p.EmergencyModeActivatedById == doctorId);

            var patientIds = doctorPatients.Select(p => p.Id).ToHashSet();

            var events = await _unitOfWork.EmergencyEvents.FindAsync(e =>
                patientIds.Contains(e.PatientId));

            var patientLookup = doctorPatients.ToDictionary(p => p.Id);

            return events.Select(e =>
            {
                patientLookup.TryGetValue(e.PatientId, out var p);
                return new EmergencyEventResponse
                {
                    Id = e.Id,
                    PatientId = e.PatientId,
                    PatientName = p != null ? $"{p.FirstName} {p.LastName}" : string.Empty,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    Timestamp = e.Timestamp,
                    Status = e.Status,
                    BloodType = p?.BloodType,
                    WeightKg = p?.WeightKg,
                    HeightCm = p?.HeightCm,
                    IsPregnant = p?.IsPregnant ?? false,
                    PhysicalDisabilities = p?.PhysicalDisabilities,
                    EmergencyContactName = p?.EmergencyContactName,
                    EmergencyContactPhone = p?.EmergencyContactPhone,
                    EmergencyContactRelationship = p?.EmergencyContactRelationship,
                };
            }).OrderByDescending(e => e.Timestamp);
        }

        public async Task<bool> ResolveEmergencyEventAsync(Guid doctorId, Guid eventId)
        {
            var emergencyEvent = await _unitOfWork.EmergencyEvents.GetByIdAsync(eventId);
            if (emergencyEvent == null)
            {
                throw new ArgumentException("Emergency event not found", nameof(eventId));
            }

            if (emergencyEvent.ActivatingDoctorId != doctorId)
            {
                throw new UnauthorizedAccessException("Only the activating doctor can resolve this emergency event.");
            }

            emergencyEvent.Status = EmergencyEventStatus.Resolved;
            _unitOfWork.EmergencyEvents.Update(emergencyEvent);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActionAsync(doctorId, emergencyEvent.PatientId, EmergencyActionType.SosResolved);

            _logger.LogInformation("Doctor {DoctorId} resolved emergency event {EventId}", doctorId, eventId);
            return true;
        }
    }
}
