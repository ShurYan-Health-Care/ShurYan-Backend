using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.LabTests;
using Shuryan.Application.DTOs.Responses.LabTests;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    /// <summary>
    /// Service مسؤول عن إدارة طلبات التحاليل الطبية
    /// </summary>
    public class LabTestService : ILabTestService
    {
        private readonly ILabPrescriptionRepository _labPrescriptionRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LabTestService> _logger;

        public LabTestService(
            ILabPrescriptionRepository labPrescriptionRepository,
            IAppointmentRepository appointmentRepository,
            IUnitOfWork unitOfWork,
            ILogger<LabTestService> logger)
        {
            _labPrescriptionRepository = labPrescriptionRepository;
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// طلب تحاليل طبية جديدة
        /// </summary>
        public async Task<LabTestsResponse> RequestLabTestsAsync(
            Guid appointmentId, 
            Guid doctorId, 
            RequestLabTestsRequest request)
        {
            try
            {
                _logger.LogInformation("Requesting lab tests for Appointment {AppointmentId} by Doctor {DoctorId}", 
                    appointmentId, doctorId);

                // ==================== Validation ====================

                // 1. التحقق من وجود الموعد
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    throw new ArgumentException("الموعد غير موجود");
                }

                // 2. التحقق من أن الموعد يخص الدكتور
                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to request lab tests for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                // 3. التحقق من وجود تحليل واحد على الأقل
                if (request.Tests == null || !request.Tests.Any())
                {
                    throw new ArgumentException("يجب إضافة تحليل واحد على الأقل");
                }

                // ==================== Create Lab Prescription ====================

                var labPrescription = new LabPrescription
                {
                    AppointmentId = appointmentId,
                    DoctorId = doctorId,
                    PatientId = appointment.PatientId,
                    GeneralNotes = request.Notes
                };

                await _labPrescriptionRepository.AddAsync(labPrescription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Lab prescription {LabPrescriptionId} created successfully with {TestCount} tests", 
                    labPrescription.Id, request.Tests.Count);

                // ==================== Prepare Response ====================

                return new LabTestsResponse
                {
                    LabRequestId = labPrescription.Id,
                    AppointmentId = labPrescription.AppointmentId,
                    Tests = request.Tests,
                    Notes = labPrescription.GeneralNotes,
                    CreatedAt = labPrescription.CreatedAt
                };
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error requesting lab tests for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على طلبات التحاليل للموعد
        /// </summary>
        public async Task<LabTestsResponse?> GetLabTestsAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting lab tests for Appointment {AppointmentId}", appointmentId);

                // التحقق من وجود الموعد وأنه يخص الدكتور
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    return null;
                }

                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to access lab tests for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                // الحصول على طلب التحاليل
                var labPrescription = await _labPrescriptionRepository.GetByAppointmentIdAsync(appointmentId);
                if (labPrescription == null)
                {
                    _logger.LogInformation("No lab tests found for Appointment {AppointmentId}", appointmentId);
                    return null;
                }

                // استخراج أسماء التحاليل من الـ Items
                var testNames = labPrescription.Items?
                    .Select(item => item.LabTest?.Name ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList() ?? new System.Collections.Generic.List<string>();

                return new LabTestsResponse
                {
                    LabRequestId = labPrescription.Id,
                    AppointmentId = labPrescription.AppointmentId,
                    Tests = testNames,
                    Notes = labPrescription.GeneralNotes,
                    CreatedAt = labPrescription.CreatedAt
                };
            }
            catch (Exception ex) when (ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error getting lab tests for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }
    }
}
