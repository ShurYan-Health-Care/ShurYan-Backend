using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Laboratory;
using System.Security.Claims;

namespace Shuryan.API.Controllers
{
        [ApiController]
        [Route("api/patients/me")]
        [Authorize(Roles = "Patient")]
        public class PatientLabController : ControllerBase
        {
                private readonly IPatientLabService _patientLabService;
                private readonly ILogger<PatientLabController> _logger;

                public PatientLabController(IPatientLabService patientLabService, ILogger<PatientLabController> logger)
                {
                        _patientLabService = patientLabService;
                        _logger = logger;
                }

                #region Laboratory Search

                /// <summary>
                /// البحث عن معامل قريبة
                /// GET /api/patients/me/laboratories/nearby
                /// </summary>
                [HttpGet("laboratories/nearby")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<NearbyLaboratoryResponse>>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                public async Task<ActionResult<ApiResponse<IEnumerable<NearbyLaboratoryResponse>>>> GetNearbyLaboratories(
                    [FromQuery] double latitude,
                    [FromQuery] double longitude,
                    [FromQuery] double radiusInKm = 10,
                    [FromQuery] bool? offersHomeSampleCollection = null,
                    [FromQuery] string? search = null,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 20)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var laboratories = await _patientLabService.GetNearbyLaboratoriesAsync(
                                    patientId, latitude, longitude, radiusInKm,
                                    offersHomeSampleCollection, search, pageNumber, pageSize);

                                return Ok(ApiResponse<IEnumerable<NearbyLaboratoryResponse>>.Success(
                                    laboratories, "تم جلب المعامل القريبة بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting nearby laboratories for patient {PatientId}", patientId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// تفاصيل معمل معين
                /// GET /api/patients/me/laboratories/{labId}
                /// </summary>
                [HttpGet("laboratories/{labId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryDetailResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                public async Task<ActionResult<ApiResponse<LaboratoryDetailResponse>>> GetLaboratoryDetails(
                    Guid labId,
                    [FromQuery] double? latitude = null,
                    [FromQuery] double? longitude = null)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var laboratory = await _patientLabService.GetLaboratoryDetailsAsync(patientId, labId, latitude, longitude);
                                if (laboratory == null)
                                        return NotFound(ApiResponse<object>.Failure("المعمل غير موجود", statusCode: 404));

                                return Ok(ApiResponse<LaboratoryDetailResponse>.Success(laboratory, "تم جلب تفاصيل المعمل"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting laboratory {LabId} details", labId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// خدمات معمل معين
                /// GET /api/patients/me/laboratories/{labId}/services
                /// </summary>
                [HttpGet("laboratories/{labId:guid}/services")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<LaboratoryServiceResponse>>), StatusCodes.Status200OK)]
                public async Task<ActionResult<ApiResponse<IEnumerable<LaboratoryServiceResponse>>>> GetLaboratoryServices(
                    Guid labId,
                    [FromQuery] string? category = null)
                {
                        try
                        {
                                var services = await _patientLabService.GetLaboratoryServicesAsync(labId, category);
                                return Ok(ApiResponse<IEnumerable<LaboratoryServiceResponse>>.Success(services, "تم جلب خدمات المعمل"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting services for laboratory {LabId}", labId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// تقييمات معمل معين
                /// GET /api/patients/me/laboratories/{labId}/reviews
                /// </summary>
                [HttpGet("laboratories/{labId:guid}/reviews")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<LaboratoryReviewResponse>>), StatusCodes.Status200OK)]
                public async Task<ActionResult<ApiResponse<IEnumerable<LaboratoryReviewResponse>>>> GetLaboratoryReviews(
                    Guid labId,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 20)
                {
                        try
                        {
                                var reviews = await _patientLabService.GetLaboratoryReviewsAsync(labId, pageNumber, pageSize);
                                return Ok(ApiResponse<IEnumerable<LaboratoryReviewResponse>>.Success(reviews, "تم جلب تقييمات المعمل"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting reviews for laboratory {LabId}", labId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Lab Prescriptions

                /// <summary>
                /// جلب روشتات التحاليل
                /// GET /api/patients/me/lab-prescriptions
                /// </summary>
                [HttpGet("lab-prescriptions")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatientLabPrescriptionResponse>>), StatusCodes.Status200OK)]
                public async Task<ActionResult<ApiResponse<IEnumerable<PatientLabPrescriptionResponse>>>> GetLabPrescriptions()
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var prescriptions = await _patientLabService.GetPatientLabPrescriptionsAsync(patientId);
                                return Ok(ApiResponse<IEnumerable<PatientLabPrescriptionResponse>>.Success(
                                    prescriptions, "تم جلب روشتات التحاليل"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting lab prescriptions for patient {PatientId}", patientId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// تفاصيل روشتة تحاليل
                /// GET /api/patients/me/lab-prescriptions/{prescriptionId}
                /// </summary>
                [HttpGet("lab-prescriptions/{prescriptionId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<PatientLabPrescriptionResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                public async Task<ActionResult<ApiResponse<PatientLabPrescriptionResponse>>> GetLabPrescriptionDetails(Guid prescriptionId)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var prescription = await _patientLabService.GetLabPrescriptionDetailsAsync(patientId, prescriptionId);
                                if (prescription == null)
                                        return NotFound(ApiResponse<object>.Failure("الروشتة غير موجودة", statusCode: 404));

                                return Ok(ApiResponse<PatientLabPrescriptionResponse>.Success(prescription, "تم جلب تفاصيل الروشتة"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting prescription {PrescriptionId}", prescriptionId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Lab Orders

                /// <summary>
                /// إنشاء طلب تحاليل
                /// POST /api/patients/me/lab-orders
                /// </summary>
                [HttpPost("lab-orders")]
                [ProducesResponseType(typeof(ApiResponse<PatientLabOrderResponse>), StatusCodes.Status201Created)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                public async Task<ActionResult<ApiResponse<PatientLabOrderResponse>>> CreateLabOrder(
                    [FromBody] CreatePatientLabOrderRequest request)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var order = await _patientLabService.CreateLabOrderAsync(patientId, request);
                                return StatusCode(201, ApiResponse<PatientLabOrderResponse>.Success(order, "تم إنشاء الطلب بنجاح", 201));
                        }
                        catch (ArgumentException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (InvalidOperationException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error creating lab order for patient {PatientId}", patientId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// جلب طلبات التحاليل
                /// GET /api/patients/me/lab-orders
                /// </summary>
                [HttpGet("lab-orders")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatientLabOrderResponse>>), StatusCodes.Status200OK)]
                public async Task<ActionResult<ApiResponse<IEnumerable<PatientLabOrderResponse>>>> GetLabOrders(
                    [FromQuery] LabOrderStatus? status = null)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var orders = await _patientLabService.GetPatientLabOrdersAsync(patientId, status);
                                return Ok(ApiResponse<IEnumerable<PatientLabOrderResponse>>.Success(orders, "تم جلب الطلبات"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting lab orders for patient {PatientId}", patientId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// تفاصيل طلب تحاليل
                /// GET /api/patients/me/lab-orders/{orderId}
                /// </summary>
                [HttpGet("lab-orders/{orderId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<PatientLabOrderResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                public async Task<ActionResult<ApiResponse<PatientLabOrderResponse>>> GetLabOrderDetails(Guid orderId)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var order = await _patientLabService.GetLabOrderDetailsAsync(patientId, orderId);
                                if (order == null)
                                        return NotFound(ApiResponse<object>.Failure("الطلب غير موجود", statusCode: 404));

                                return Ok(ApiResponse<PatientLabOrderResponse>.Success(order, "تم جلب تفاصيل الطلب"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// طلب خدمة سحب العينة من البيت
                /// POST /api/patients/me/lab-orders/{orderId}/home-collection
                /// </summary>
                [HttpPost("lab-orders/{orderId:guid}/home-collection")]
                [ProducesResponseType(typeof(ApiResponse<PatientLabOrderResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                public async Task<ActionResult<ApiResponse<PatientLabOrderResponse>>> RequestHomeSampleCollection(
                    Guid orderId,
                    [FromBody] RequestHomeSampleCollectionRequest request)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var order = await _patientLabService.RequestHomeSampleCollectionAsync(patientId, orderId, request);
                                return Ok(ApiResponse<PatientLabOrderResponse>.Success(order, "تم طلب خدمة سحب العينة من البيت"));
                        }
                        catch (ArgumentException ex)
                        {
                                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
                        }
                        catch (InvalidOperationException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error requesting home collection for order {OrderId}", orderId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// إلغاء طلب تحاليل
                /// POST /api/patients/me/lab-orders/{orderId}/cancel
                /// </summary>
                [HttpPost("lab-orders/{orderId:guid}/cancel")]
                [ProducesResponseType(typeof(ApiResponse<PatientLabOrderResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                public async Task<ActionResult<ApiResponse<PatientLabOrderResponse>>> CancelLabOrder(
                    Guid orderId,
                    [FromBody] CancelLabOrderRequest request)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var order = await _patientLabService.CancelLabOrderAsync(patientId, orderId, request.Reason);
                                return Ok(ApiResponse<PatientLabOrderResponse>.Success(order, "تم إلغاء الطلب"));
                        }
                        catch (ArgumentException ex)
                        {
                                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
                        }
                        catch (InvalidOperationException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// جلب نتائج طلب تحاليل
                /// GET /api/patients/me/lab-orders/{orderId}/results
                /// </summary>
                [HttpGet("lab-orders/{orderId:guid}/results")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatientLabResultResponse>>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                public async Task<ActionResult<ApiResponse<IEnumerable<PatientLabResultResponse>>>> GetLabOrderResults(Guid orderId)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        try
                        {
                                var results = await _patientLabService.GetLabOrderResultsAsync(patientId, orderId);
                                return Ok(ApiResponse<IEnumerable<PatientLabResultResponse>>.Success(results, "تم جلب النتائج"));
                        }
                        catch (ArgumentException ex)
                        {
                                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting results for order {OrderId}", orderId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// تقييم المعمل
                /// POST /api/patients/me/lab-orders/{orderId}/review
                /// </summary>
                [HttpPost("lab-orders/{orderId:guid}/review")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryReviewResponse>), StatusCodes.Status201Created)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                public async Task<ActionResult<ApiResponse<LaboratoryReviewResponse>>> CreateLaboratoryReview(
                    Guid orderId,
                    [FromBody] CreateLaboratoryReviewRequest request)
                {
                        var patientId = GetCurrentPatientId();
                        if (patientId == Guid.Empty)
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح", statusCode: 401));

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var review = await _patientLabService.CreateLaboratoryReviewAsync(patientId, orderId, request);
                                return StatusCode(201, ApiResponse<LaboratoryReviewResponse>.Success(review, "تم إضافة التقييم بنجاح", 201));
                        }
                        catch (ArgumentException ex)
                        {
                                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
                        }
                        catch (InvalidOperationException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error creating review for order {OrderId}", orderId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Helper Methods

                private Guid GetCurrentPatientId()
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

        public class CancelLabOrderRequest
        {
                public string Reason { get; set; } = string.Empty;
        }
}
