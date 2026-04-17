using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class DoctorServicePricingService : IDoctorServicePricingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DoctorServicePricingService> _logger;

        public DoctorServicePricingService(
            IUnitOfWork unitOfWork,
            ILogger<DoctorServicePricingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Regular Checkup

        public async Task<ServicePricingResponse?> GetRegularCheckupAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting regular checkup pricing for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    return null;
                }

                // Get ConsultationType for Regular
                var allConsultationTypes = await _unitOfWork.ConsultationTypes.GetAllAsync();
                var regularType = allConsultationTypes.FirstOrDefault(ct => ct.ConsultationTypeEnum == ConsultationTypeEnum.Regular);

                if (regularType == null)
                {
                    _logger.LogWarning("Regular consultation type not found in system");
                    return new ServicePricingResponse { Price = 0, Duration = 30 };
                }

                var allConsultations = await _unitOfWork.DoctorConsultations.GetAllAsync();
                var regularCheckup = allConsultations.FirstOrDefault(c =>
                    c.DoctorId == doctorId &&
                    c.ConsultationTypeId == regularType.Id);

                if (regularCheckup == null)
                {
                    _logger.LogInformation("No regular checkup service found for doctor {DoctorId}, returning default values", doctorId);
                    return new ServicePricingResponse
                    {
                        Price = 0,
                        Duration = 30 // Default 30 minutes
                    };
                }

                var response = new ServicePricingResponse
                {
                    Price = regularCheckup.ConsultationFee,
                    Duration = regularCheckup.SessionDurationMinutes
                };

                _logger.LogInformation("Successfully retrieved regular checkup pricing for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regular checkup pricing for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<ServicePricingResponse> UpdateRegularCheckupAsync(Guid doctorId, UpdateServicePricingRequest request)
        {
            try
            {
                _logger.LogInformation("Updating regular checkup pricing for doctor {DoctorId}. Price: {Price}, Duration: {Duration}",
                    doctorId, request.Price, request.Duration);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get ConsultationType for Regular
                var allConsultationTypes = await _unitOfWork.ConsultationTypes.GetAllAsync();
                var regularType = allConsultationTypes.FirstOrDefault(ct => ct.ConsultationTypeEnum == ConsultationTypeEnum.Regular);

                if (regularType == null)
                    throw new InvalidOperationException("Regular consultation type not found in system");

                var allConsultations = await _unitOfWork.DoctorConsultations.GetAllAsync();
                var regularCheckup = allConsultations.FirstOrDefault(c =>
                    c.DoctorId == doctorId &&
                    c.ConsultationTypeId == regularType.Id);

                if (regularCheckup == null)
                {
                    // Create new consultation
                    _logger.LogInformation("Creating new regular checkup service for doctor {DoctorId}", doctorId);

                    regularCheckup = new DoctorConsultation
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctorId,
                        ConsultationTypeId = regularType.Id,
                        ConsultationFee = request.Price,
                        SessionDurationMinutes = request.Duration,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DoctorConsultations.AddAsync(regularCheckup);
                }
                else
                {
                    // Update existing consultation
                    _logger.LogInformation("Updating existing regular checkup service for doctor {DoctorId}", doctorId);

                    regularCheckup.ConsultationFee = request.Price;
                    regularCheckup.SessionDurationMinutes = request.Duration;
                    regularCheckup.UpdatedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated regular checkup pricing for doctor {DoctorId}", doctorId);
                return await GetRegularCheckupAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated regular checkup pricing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regular checkup pricing for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Re-examination

        public async Task<ServicePricingResponse?> GetReExaminationAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting re-examination pricing for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor {DoctorId} not found", doctorId);
                    return null;
                }

                // Get ConsultationType for FollowUp
                var allConsultationTypes = await _unitOfWork.ConsultationTypes.GetAllAsync();
                var followUpType = allConsultationTypes.FirstOrDefault(ct => ct.ConsultationTypeEnum == ConsultationTypeEnum.FollowUp);

                if (followUpType == null)
                {
                    _logger.LogWarning("FollowUp consultation type not found in system");
                    return new ServicePricingResponse { Price = 0, Duration = 20 };
                }

                var allConsultations = await _unitOfWork.DoctorConsultations.GetAllAsync();
                var reExamination = allConsultations.FirstOrDefault(c =>
                    c.DoctorId == doctorId &&
                    c.ConsultationTypeId == followUpType.Id);

                if (reExamination == null)
                {
                    _logger.LogInformation("No re-examination service found for doctor {DoctorId}, returning default values", doctorId);
                    return new ServicePricingResponse
                    {
                        Price = 0,
                        Duration = 20 // Default 20 minutes
                    };
                }

                var response = new ServicePricingResponse
                {
                    Price = reExamination.ConsultationFee,
                    Duration = reExamination.SessionDurationMinutes
                };

                _logger.LogInformation("Successfully retrieved re-examination pricing for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting re-examination pricing for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<ServicePricingResponse> UpdateReExaminationAsync(Guid doctorId, UpdateServicePricingRequest request)
        {
            try
            {
                _logger.LogInformation("Updating re-examination pricing for doctor {DoctorId}. Price: {Price}, Duration: {Duration}",
                    doctorId, request.Price, request.Duration);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get ConsultationType for FollowUp
                var allConsultationTypes = await _unitOfWork.ConsultationTypes.GetAllAsync();
                var followUpType = allConsultationTypes.FirstOrDefault(ct => ct.ConsultationTypeEnum == ConsultationTypeEnum.FollowUp);

                if (followUpType == null)
                    throw new InvalidOperationException("FollowUp consultation type not found in system");

                var allConsultations = await _unitOfWork.DoctorConsultations.GetAllAsync();
                var reExamination = allConsultations.FirstOrDefault(c =>
                    c.DoctorId == doctorId &&
                    c.ConsultationTypeId == followUpType.Id);

                if (reExamination == null)
                {
                    // Create new consultation
                    _logger.LogInformation("Creating new re-examination service for doctor {DoctorId}", doctorId);

                    reExamination = new DoctorConsultation
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = doctorId,
                        ConsultationTypeId = followUpType.Id,
                        ConsultationFee = request.Price,
                        SessionDurationMinutes = request.Duration,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.DoctorConsultations.AddAsync(reExamination);
                }
                else
                {
                    // Update existing consultation
                    _logger.LogInformation("Updating existing re-examination service for doctor {DoctorId}", doctorId);

                    reExamination.ConsultationFee = request.Price;
                    reExamination.SessionDurationMinutes = request.Duration;
                    reExamination.UpdatedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated re-examination pricing for doctor {DoctorId}", doctorId);
                return await GetReExaminationAsync(doctorId)
                    ?? throw new InvalidOperationException("Failed to retrieve updated re-examination pricing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating re-examination pricing for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion


        /// <summary>
        /// Get all services (normal disclosure + redisclosure) in a single response
        /// </summary>
        public async Task<DoctorServicesResponse> GetAllServicesAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting all services for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get regular checkup
                var regularCheckup = await GetRegularCheckupAsync(doctorId);

                // Get re-examination
                var reExamination = await GetReExaminationAsync(doctorId);

                var response = new DoctorServicesResponse
                {
                    RegularCheckup = new ServiceDetailsResponse
                    {
                        Price = regularCheckup?.Price ?? 0,
                        Duration = regularCheckup?.Duration ?? 30
                    },
                    ReExamination = new ServiceDetailsResponse
                    {
                        Price = reExamination?.Price ?? 0,
                        Duration = reExamination?.Duration ?? 20
                    }
                };

                _logger.LogInformation("Successfully retrieved all services for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all services for doctor {DoctorId}", doctorId);
                throw;
            }
        }

    }
}
