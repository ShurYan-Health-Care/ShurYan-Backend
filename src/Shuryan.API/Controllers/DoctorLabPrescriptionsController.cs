using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
        [ApiController]
        [Route("api/doctors/me")]
        [Authorize(Roles = "Doctor")]
        public class DoctorLabPrescriptionsController : ControllerBase
        {
                private readonly ILabPrescriptionService _labPrescriptionService;
                private readonly ILabOrderService _labOrderService;
                private readonly ILogger<DoctorLabPrescriptionsController> _logger;

                public DoctorLabPrescriptionsController(
                    ILabPrescriptionService labPrescriptionService,
                    ILabOrderService labOrderService,
                    ILogger<DoctorLabPrescriptionsController> logger)
                {
                        _labPrescriptionService = labPrescriptionService;
                        _labOrderService = labOrderService;
                        _logger = logger;
                }

                #region Lab Prescriptions

                /// <summary>
                /// كتابة روشتة تحاليل للمريض
                /// POST /api/doctors/me/lab-prescriptions
                /// </summary>
                [HttpPost("lab-prescriptions")]
                [ProducesResponseType(typeof(ApiResponse<LabPrescriptionResponse>), StatusCodes.Status201Created)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                public async Task<ActionResult<ApiResponse<LabPrescriptionResponse>>> CreateLabPrescription(
                    [FromBody] CreateLabPrescriptionRequest request)
                {
                        var doctorId = GetCurrentDoctorId();
                        if (doctorId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        // Ensure the doctor ID matches
                        request.DoctorId = doctorId;

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var prescription = await _labPrescriptionService.CreateLabPrescriptionAsync(request);
                                return StatusCode(201, ApiResponse<LabPrescriptionResponse>.Success(
                                    prescription, "تم إنشاء روشتة التحاليل بنجاح", 201));
                        }
                        catch (ArgumentException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error creating lab prescription for doctor {DoctorId}", doctorId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// تفاصيل روشتة تحاليل
                /// GET /api/doctors/me/lab-prescriptions/{prescriptionId}
                /// </summary>
                [HttpGet("lab-prescriptions/{prescriptionId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<LabPrescriptionResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                public async Task<ActionResult<ApiResponse<LabPrescriptionResponse>>> GetLabPrescriptionDetails(
                    Guid prescriptionId)
                {
                        var doctorId = GetCurrentDoctorId();
                        if (doctorId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var prescription = await _labPrescriptionService.GetLabPrescriptionByIdAsync(prescriptionId);
                                if (prescription == null)
                                        return NotFound(ApiResponse<object>.Failure("الروشتة غير موجودة", statusCode: 404));

                                // Verify ownership
                                if (prescription.DoctorId != doctorId)
                                        return Forbid();

                                return Ok(ApiResponse<LabPrescriptionResponse>.Success(prescription, "تم جلب تفاصيل الروشتة"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting prescription {PrescriptionId}", prescriptionId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Patient Lab Results

                /// <summary>
                /// جلب نتائج تحاليل مريض معين
                /// GET /api/doctors/me/patients/{patientId}/lab-results
                /// </summary>
                [HttpGet("patients/{patientId:guid}/lab-results")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatientLabResultsResponse>>), StatusCodes.Status200OK)]
                public async Task<ActionResult<ApiResponse<IEnumerable<PatientLabResultsResponse>>>> GetPatientLabResults(
                    Guid patientId)
                {
                        var doctorId = GetCurrentDoctorId();
                        if (doctorId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                // Get all prescriptions written by this doctor for this patient
                                var prescriptions = await _labPrescriptionService.GetAllLabPrescriptionsAsync(
                                    doctorId: doctorId,
                                    patientId: patientId);

                                var results = new List<PatientLabResultsResponse>();

                                foreach (var prescription in prescriptions)
                                {
                                        // Get lab order for this prescription
                                        var orders = await _labOrderService.GetLabOrdersByPrescriptionAsync(prescription.Id);

                                        foreach (var order in orders)
                                        {
                                                if (order.Status == Core.Enums.Laboratory.LabOrderStatus.ResultsReady ||
                                                    order.Status == Core.Enums.Laboratory.LabOrderStatus.Completed)
                                                {
                                                        var labResults = await _labOrderService.GetLabResultsAsync(order.Id);

                                                        results.Add(new PatientLabResultsResponse
                                                        {
                                                                PrescriptionId = prescription.Id,
                                                                OrderId = order.Id,
                                                                LaboratoryName = order.LaboratoryName,
                                                                PrescriptionDate = prescription.CreatedAt,
                                                                ResultsDate = order.UpdatedAt ?? order.CreatedAt,
                                                                Status = order.Status.ToString(),
                                                                Results = labResults.Select(r => new LabResultItemResponse
                                                                {
                                                                        Id = r.Id,
                                                                        TestName = r.TestName,
                                                                        TestCode = r.TestCode,
                                                                        ResultValue = r.ResultValue,
                                                                        ReferenceRange = r.ReferenceRange,
                                                                        Unit = r.Unit,
                                                                        Notes = r.Notes,
                                                                        AttachmentUrl = r.AttachmentUrl,
                                                                        CreatedAt = r.CreatedAt
                                                                }).ToList()
                                                        });
                                                }
                                        }
                                }

                                return Ok(ApiResponse<IEnumerable<PatientLabResultsResponse>>.Success(
                                    results.OrderByDescending(r => r.ResultsDate), "تم جلب نتائج التحاليل"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting lab results for patient {PatientId}", patientId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Helper Methods

                private Guid GetCurrentDoctorId()
                {
                        var userIdClaim = User.Claims.FirstOrDefault(c =>
                            c.Type == "sub" ||
                            c.Type == "uid" ||
                            c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                                return userId;

                        return Guid.Empty;
                }

                #endregion
        }

        #region Response DTOs

        public class PatientLabResultsResponse
        {
                public Guid PrescriptionId { get; set; }
                public Guid OrderId { get; set; }
                public string LaboratoryName { get; set; } = string.Empty;
                public DateTime PrescriptionDate { get; set; }
                public DateTime ResultsDate { get; set; }
                public string Status { get; set; } = string.Empty;
                public List<LabResultItemResponse> Results { get; set; } = new();
        }

        public class LabResultItemResponse
        {
                public Guid Id { get; set; }
                public string TestName { get; set; } = string.Empty;
                public string TestCode { get; set; } = string.Empty;
                public string ResultValue { get; set; } = string.Empty;
                public string? ReferenceRange { get; set; }
                public string? Unit { get; set; }
                public string? Notes { get; set; }
                public string? AttachmentUrl { get; set; }
                public DateTime CreatedAt { get; set; }
        }

        #endregion
}
