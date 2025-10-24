using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class LabPrescriptionService : ILabPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LabPrescriptionService> _logger;

        public LabPrescriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LabPrescriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Get All Prescriptions

        public async Task<IEnumerable<LabPrescriptionResponse>> GetAllLabPrescriptionsAsync(
            Guid? doctorId = null,
            Guid? patientId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var prescriptions = (await _unitOfWork.LabPrescriptions.GetAllAsync()).ToList();

                if (doctorId.HasValue)
                    prescriptions = prescriptions.Where(p => p.DoctorId == doctorId.Value).ToList();

                if (patientId.HasValue)
                    prescriptions = prescriptions.Where(p => p.PatientId == patientId.Value).ToList();

                if (startDate.HasValue)
                    prescriptions = prescriptions.Where(p => p.CreatedAt >= startDate.Value).ToList();

                if (endDate.HasValue)
                    prescriptions = prescriptions.Where(p => p.CreatedAt <= endDate.Value).ToList();

                var responses = new List<LabPrescriptionResponse>();
                foreach (var prescription in prescriptions)
                {
                    var response = await GetLabPrescriptionByIdAsync(prescription.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} lab prescriptions", responses.Count);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all lab prescriptions");
                throw;
            }
        }

        #endregion

        #region Get Prescription By Id

        public async Task<LabPrescriptionResponse?> GetLabPrescriptionByIdAsync(Guid id)
        {
            try
            {
                var prescription = await _unitOfWork.LabPrescriptions.GetPrescriptionWithDetailsAsync(id);
                if (prescription == null)
                    return null;

                var response = _mapper.Map<LabPrescriptionResponse>(prescription);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(prescription.DoctorId);
                if (doctor != null)
                {
                    response.DoctorName = $"د. {doctor.FirstName} {doctor.LastName}";
                }

                var patient = await _unitOfWork.Patients.GetByIdAsync(prescription.PatientId);
                if (patient != null)
                {
                    response.PatientName = $"{patient.FirstName} {patient.LastName}";
                }

                response.Items = prescription.Items.Select(item => new LabPrescriptionItemResponse
                {
                    LabTestId = item.LabTestId,
                    SpecialInstructions = item.DoctorNotes
                }).ToList();

                _logger.LogInformation("Retrieved lab prescription {PrescriptionId}", id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescription {PrescriptionId}", id);
                throw;
            }
        }

        #endregion

        #region Get Prescription By AppointmentId

        public async Task<LabPrescriptionResponse?> GetLabPrescriptionByAppointmentIdAsync(Guid appointmentId)
        {
            try
            {
                var prescriptions = await _unitOfWork.LabPrescriptions.GetAllAsync();
                var prescription = prescriptions.FirstOrDefault(p => p.AppointmentId == appointmentId);

                if (prescription == null)
                    return null;

                return await GetLabPrescriptionByIdAsync(prescription.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescription for appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        #endregion

        #region Get Prescriptions By Patient/Doctor

        public async Task<IEnumerable<LabPrescriptionResponse>> GetPatientLabPrescriptionsAsync(Guid patientId)
        {
            try
            {
                var prescriptions = await _unitOfWork.LabPrescriptions.GetAllAsync();
                var patientPrescriptions = prescriptions.Where(p => p.PatientId == patientId).ToList();

                var responses = new List<LabPrescriptionResponse>();
                foreach (var prescription in patientPrescriptions)
                {
                    var response = await GetLabPrescriptionByIdAsync(prescription.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} lab prescriptions for patient {PatientId}", responses.Count, patientId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescriptions for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<LabPrescriptionResponse>> GetDoctorLabPrescriptionsAsync(Guid doctorId)
        {
            try
            {
                var prescriptions = await _unitOfWork.LabPrescriptions.GetAllAsync();
                var doctorPrescriptions = prescriptions.Where(p => p.DoctorId == doctorId).ToList();

                var responses = new List<LabPrescriptionResponse>();
                foreach (var prescription in doctorPrescriptions)
                {
                    var response = await GetLabPrescriptionByIdAsync(prescription.Id);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }

                _logger.LogInformation("Retrieved {Count} lab prescriptions for doctor {DoctorId}", responses.Count, doctorId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lab prescriptions for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Create / Update / Delete Prescription

        public async Task<LabPrescriptionResponse> CreateLabPrescriptionAsync(CreateLabPrescriptionRequest request)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.AppointmentId);
                if (appointment == null)
                    throw new ArgumentException($"Appointment with ID {request.AppointmentId} not found");

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(request.DoctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {request.DoctorId} not found");

                var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
                if (patient == null)
                    throw new ArgumentException($"Patient with ID {request.PatientId} not found");

                var prescription = new LabPrescription
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = request.AppointmentId,
                    DoctorId = request.DoctorId,
                    PatientId = request.PatientId,
                    GeneralNotes = request.GeneralNotes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.LabPrescriptions.AddAsync(prescription);

                if (request.Items != null && request.Items.Any())
                {
                    foreach (var itemRequest in request.Items)
                    {
                        var labTest = await _unitOfWork.LabTests.GetByIdAsync(itemRequest.LabTestId);
                        if (labTest == null)
                            throw new ArgumentException($"Lab test with ID {itemRequest.LabTestId} not found");

                        var item = new LabPrescriptionItem
                        {
                            Id = Guid.NewGuid(),
                            LabPrescriptionId = prescription.Id,
                            LabTestId = itemRequest.LabTestId,
                            DoctorNotes = itemRequest.SpecialInstructions,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.LabPrescriptionItems.AddAsync(item);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created lab prescription {PrescriptionId}", prescription.Id);

                return await GetLabPrescriptionByIdAsync(prescription.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve created lab prescription");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab prescription");
                throw;
            }
        }

        public async Task<LabPrescriptionResponse> UpdateLabPrescriptionAsync(Guid id, CreateLabPrescriptionRequest request)
        {
            try
            {
                var prescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(id);
                if (prescription == null)
                    throw new ArgumentException($"Lab prescription with ID {id} not found");

                prescription.GeneralNotes = request.GeneralNotes;
                prescription.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated lab prescription {PrescriptionId}", id);

                return await GetLabPrescriptionByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve updated lab prescription");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lab prescription {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLabPrescriptionAsync(Guid id)
        {
            try
            {
                var prescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(id);
                if (prescription == null)
                    return false;

                prescription.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted lab prescription {PrescriptionId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lab prescription {PrescriptionId}", id);
                throw;
            }
        }

        #endregion

        #region Prescription Items Management

        public async Task<IEnumerable<LabPrescriptionItemResponse>> GetPrescriptionItemsAsync(Guid prescriptionId)
        {
            try
            {
                var items = await _unitOfWork.LabPrescriptionItems.GetAllAsync();
                var prescriptionItems = items.Where(i => i.LabPrescriptionId == prescriptionId).ToList();

                var responses = _mapper.Map<IEnumerable<LabPrescriptionItemResponse>>(prescriptionItems);
                _logger.LogInformation("Retrieved {Count} items for prescription {PrescriptionId}", responses.Count(), prescriptionId);
                return responses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items for prescription {PrescriptionId}", prescriptionId);
                throw;
            }
        }

        public async Task<LabPrescriptionItemResponse> AddPrescriptionItemAsync(Guid prescriptionId, CreateLabPrescriptionItemRequest request)
        {
            try
            {
                var prescription = await _unitOfWork.LabPrescriptions.GetByIdAsync(prescriptionId);
                if (prescription == null)
                    throw new ArgumentException($"Lab prescription with ID {prescriptionId} not found");

                var labTest = await _unitOfWork.LabTests.GetByIdAsync(request.LabTestId);
                if (labTest == null)
                    throw new ArgumentException($"Lab test with ID {request.LabTestId} not found");

                var item = _mapper.Map<LabPrescriptionItem>(request);
                item.Id = Guid.NewGuid();
                item.LabPrescriptionId = prescriptionId;
                item.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.LabPrescriptionItems.AddAsync(item);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Added item {ItemId} to prescription {PrescriptionId}", item.Id, prescriptionId);

                var response = _mapper.Map<LabPrescriptionItemResponse>(item);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to prescription {PrescriptionId}", prescriptionId);
                throw;
            }
        }

        public async Task<bool> RemovePrescriptionItemAsync(Guid itemId)
        {
            try
            {
                var item = await _unitOfWork.LabPrescriptionItems.GetByIdAsync(itemId);
                if (item == null)
                    return false;

                item.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Removed prescription item {ItemId}", itemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing prescription item {ItemId}", itemId);
                throw;
            }
        }

        #endregion
    }
}
