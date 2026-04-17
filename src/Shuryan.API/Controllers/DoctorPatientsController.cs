using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/doctors/me/patients")]
    [Authorize(Roles = "Doctor")]
    public class DoctorPatientsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorPatientsController> _logger;

        public DoctorPatientsController(
            IDoctorService doctorService,
            ILogger<DoctorPatientsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DoctorPatientResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorPatientResponse>>>> GetMyPatients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation(
                "Get patients request for doctor: {DoctorId}. Page: {Page}, Size: {Size}",
                currentDoctorId, pageNumber, pageSize);

            try
            {
                var paginationParams = new PaginationParams
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var patients = await _doctorService.GetDoctorPatientsWithPaginationAsync(
                    currentDoctorId,
                    paginationParams);

                _logger.LogInformation(
                    "Patients retrieved successfully for doctor: {DoctorId}. Count: {Count}, TotalCount: {TotalCount}",
                    currentDoctorId, patients.Data.Count(), patients.TotalCount);

                return Ok(ApiResponse<PaginatedResponse<DoctorPatientResponse>>.Success(
                    patients,
                    "Patients retrieved successfully"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor not found: {DoctorId}", currentDoctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients for doctor: {DoctorId}", currentDoctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving patients",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{patientId}/medical-record")]
        [ProducesResponseType(typeof(ApiResponse<PatientMedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PatientMedicalRecordResponse>>> GetPatientMedicalRecord(Guid patientId)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get medical record request for patient: {PatientId} by doctor: {DoctorId}",
                patientId, currentDoctorId);

            try
            {
                var medicalRecord = await _doctorService.GetPatientMedicalRecordAsync(patientId, currentDoctorId);

                if (medicalRecord == null)
                {
                    _logger.LogWarning("Medical record not found for patient: {PatientId} or doctor has no access", patientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Patient not found or you don't have access to this patient's medical record",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved medical record for patient: {PatientId}", patientId);
                return Ok(ApiResponse<PatientMedicalRecordResponse>.Success(
                    medicalRecord,
                    "Medical record retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical record for patient: {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving medical record",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{patientId}/session-documentations")]
        [ProducesResponseType(typeof(ApiResponse<PatientSessionDocumentationListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PatientSessionDocumentationListResponse>>> GetPatientSessionDocumentations(Guid patientId)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get session documentations request for patient: {PatientId} by doctor: {DoctorId}",
                patientId, currentDoctorId);

            try
            {
                var sessionDocumentations = await _doctorService.GetPatientSessionDocumentationsAsync(patientId, currentDoctorId);

                if (sessionDocumentations == null)
                {
                    _logger.LogWarning("Session documentations not found for patient: {PatientId} or doctor has no sessions with patient", patientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Patient not found or no completed sessions found with this patient",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved {Count} session documentations for patient: {PatientId}",
                    sessionDocumentations.Sessions.Count, patientId);

                return Ok(ApiResponse<PatientSessionDocumentationListResponse>.Success(
                    sessionDocumentations,
                    "Session documentations retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session documentations for patient: {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving session documentations",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{patientId}/prescriptions")]
        [ProducesResponseType(typeof(ApiResponse<PatientPrescriptionsListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PatientPrescriptionsListResponse>>> GetPatientPrescriptions(Guid patientId)
        {
            var currentDoctorId = GetCurrentDoctorId();
            _logger.LogInformation("Get prescriptions request for patient: {PatientId} by doctor: {DoctorId}",
                patientId, currentDoctorId);

            try
            {
                var prescriptions = await _doctorService.GetPatientPrescriptionsAsync(patientId, currentDoctorId);

                if (prescriptions == null)
                {
                    _logger.LogWarning("Prescriptions not found for patient: {PatientId} from doctor: {DoctorId}",
                        patientId, currentDoctorId);
                    return NotFound(ApiResponse<object>.Failure(
                        "Patient not found or no prescriptions found for this patient",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Successfully retrieved {Count} prescriptions for patient: {PatientId}",
                    prescriptions.TotalPrescriptions, patientId);

                return Ok(ApiResponse<PatientPrescriptionsListResponse>.Success(
                    prescriptions,
                    "Prescriptions retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions for patient: {PatientId}", patientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "An unexpected error occurred while retrieving prescriptions",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #region Helper Methods

        private Guid GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
