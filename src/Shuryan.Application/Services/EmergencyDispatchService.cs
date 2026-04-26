using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Emergency;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class EmergencyDispatchService : IEmergencyDispatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmergencyAuditService _auditService;
        private readonly INotificationHubService _notificationHub;
        private readonly IPatientService _patientService;
        private readonly ILogger<EmergencyDispatchService> _logger;

        public EmergencyDispatchService(
            IUnitOfWork unitOfWork,
            IEmergencyAuditService auditService,
            INotificationHubService notificationHub,
            IPatientService patientService,
            ILogger<EmergencyDispatchService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _notificationHub = notificationHub;
            _patientService = patientService;
            _logger = logger;
        }

        public async Task<bool> DispatchSosAsync(Guid patientId, SosDispatchRequest request)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (patient == null)
            {
                throw new ArgumentException("Patient not found", nameof(patientId));
            }

            if (!patient.EmergencyModeActive || !patient.EmergencyModeActivatedById.HasValue)
            {
                _logger.LogWarning("Failed SOS dispatch for patient {PatientId} - Emergency mode is inactive.", patientId);
                throw new InvalidOperationException("لا يمكن طلب الطوارئ حيث أن وضع الطوارئ غير مفعل.");
            }

            var activatingDoctorId = patient.EmergencyModeActivatedById.Value;

            // Fetch Medical Record
            var medicalRecord = await _patientService.GetPatientMedicalRecordAsync(patientId);
            
            // Create snapshot using System.Text.Json (we assume this is sufficient for snapshot storage)
            var medicalRecordSnapshot = JsonSerializer.Serialize(medicalRecord);

            var emergencyEvent = new EmergencyEvent
            {
                PatientId = patientId,
                ActivatingDoctorId = activatingDoctorId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp,
                MedicalRecordSnapshot = medicalRecordSnapshot,
                Status = EmergencyEventStatus.Dispatched
            };

            await _unitOfWork.EmergencyEvents.AddAsync(emergencyEvent);
            await _unitOfWork.SaveChangesAsync();

            // Log the action permanently
            await _auditService.LogActionAsync(activatingDoctorId, patientId, EmergencyActionType.SosDispatched);

            // Notify the doctor via SignalR
            var notificationMessage = $"قام المريض بتفعيل زر الطوارئ (SOS). يرجى المتابعة الفورية.";
            await _notificationHub.SendNotificationToUserAsync(
                activatingDoctorId,
                "حالة طوارئ",
                notificationMessage,
                new { 
                    EmergencyEventId = emergencyEvent.Id,
                    PatientId = patientId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Priority = "Urgent" 
                }
            );

            _logger.LogInformation("SOS Dispatched successfully for patient {PatientId} by doctor {DoctorId}", patientId, activatingDoctorId);
            
            return true;
        }
    }
}
