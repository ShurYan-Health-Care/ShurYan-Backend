using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/pharmacies/me")]
    [Authorize(Roles = "Pharmacy")]
    public class PharmacyProfileController : ControllerBase
    {
        private readonly IPharmacyProfileService _pharmacyProfileService;
        private readonly ILogger<PharmacyProfileController> _logger;

        public PharmacyProfileController(
            IPharmacyProfileService pharmacyProfileService,
            ILogger<PharmacyProfileController> logger)
        {
            _pharmacyProfileService = pharmacyProfileService;
            _logger = logger;
        }

        #region Basic Info Operations

        /// <summary>
        /// جلب المعلومات الأساسية للصيدلية
        /// GET /api/pharmacies/me/profile/basic
        /// </summary>
        [HttpGet("profile/basic")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyBasicInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PharmacyBasicInfoResponse>>> GetBasicInfo()
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy profile");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get basic info request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var basicInfo = await _pharmacyProfileService.GetBasicInfoAsync(currentPharmacyId);
                if (basicInfo == null)
                {
                    _logger.LogWarning("Pharmacy not found: {PharmacyId}", currentPharmacyId);
                    return NotFound(ApiResponse<object>.Failure(
                        "تعذر العثور على معلومات الصيدلية",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Basic info retrieved successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyBasicInfoResponse>.Success(
                    basicInfo,
                    "تم جلب المعلومات الأساسية بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving basic info for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المعلومات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("orders/{orderId}/status")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderStatusUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderStatusUpdateResponse>>> UpdateOrderStatus(
            Guid orderId,
            [FromBody] UpdatePharmacyOrderStatusRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update order status");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid order status update request for pharmacy: {PharmacyId}", currentPharmacyId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Pharmacy {PharmacyId} updating status for order {OrderId}", currentPharmacyId, orderId);

            try
            {
                var result = await _pharmacyProfileService.UpdateOrderStatusAsync(currentPharmacyId, orderId, request);

                _logger.LogInformation("Successfully updated status for order {OrderId}", orderId);

                return Ok(ApiResponse<PharmacyOrderStatusUpdateResponse>.Success(
                    result,
                    "تم تحديث حالة الطلب بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order or pharmacy not found for status update: {OrderId}, {PharmacyId}", orderId, currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for order status update: {OrderId}, {PharmacyId}", orderId, currentPharmacyId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث حالة الطلب",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث المعلومات الأساسية للصيدلية (Partial Update)
        /// PUT /api/pharmacies/me/profile/basic
        /// </summary>
        [HttpPut("profile/basic")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyBasicInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PharmacyBasicInfoResponse>>> UpdateBasicInfo(
            [FromBody] UpdatePharmacyBasicInfoRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update pharmacy basic info");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update basic info request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var updatedInfo = await _pharmacyProfileService.UpdateBasicInfoAsync(currentPharmacyId, request);

                _logger.LogInformation("Basic info updated successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyBasicInfoResponse>.Success(
                    updatedInfo,
                    "تم تحديث المعلومات الأساسية بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating basic info for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث المعلومات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// رفع صورة البروفايل للصيدلية
        /// PUT /api/pharmacies/me/profile/image
        /// </summary>
        [HttpPut("profile/image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfileImage([FromForm] UpdatePharmacyProfileImageRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update pharmacy profile image");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            if (request.ProfileImage == null || request.ProfileImage.Length == 0)
            {
                _logger.LogWarning("No image provided for pharmacy: {PharmacyId}", currentPharmacyId);
                return BadRequest(ApiResponse<object>.Failure(
                    "يجب تحميل صورة",
                    statusCode: 400
                ));
            }

            // Validate image type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(request.ProfileImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid image format for pharmacy: {PharmacyId}", currentPharmacyId);
                return BadRequest(ApiResponse<object>.Failure(
                    "صيغة الصورة غير مدعومة. الصيغ المدعومة: jpg, jpeg, png, gif",
                    statusCode: 400
                ));
            }

            // Validate image size (max 5MB)
            if (request.ProfileImage.Length > 5 * 1024 * 1024)
            {
                _logger.LogWarning("Image size too large for pharmacy: {PharmacyId}", currentPharmacyId);
                return BadRequest(ApiResponse<object>.Failure(
                    "حجم الصورة يجب ألا يتجاوز 5 ميجابايت",
                    statusCode: 400
                ));
            }

            _logger.LogInformation("Update profile image request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var imageUrl = await _pharmacyProfileService.UpdateProfileImageAsync(currentPharmacyId, request.ProfileImage);

                _logger.LogInformation("Profile image updated successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<object>.Success(
                    new { ImageUrl = imageUrl },
                    "تم تحديث صورة البروفايل بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث الصورة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Address Operations

        /// <summary>
        /// جلب عنوان الصيدلية
        /// GET /api/pharmacies/me/profile/address
        /// </summary>
        [HttpGet("profile/address")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyAddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PharmacyAddressResponse>>> GetAddress()
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy address");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get address request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var address = await _pharmacyProfileService.GetAddressAsync(currentPharmacyId);
                if (address == null)
                {
                    _logger.LogWarning("Pharmacy not found: {PharmacyId}", currentPharmacyId);
                    return NotFound(ApiResponse<object>.Failure(
                        "تعذر العثور على عنوان الصيدلية",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Address retrieved successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyAddressResponse>.Success(
                    address,
                    "تم جلب العنوان بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب العنوان",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث عنوان الصيدلية (Partial Update)
        /// PUT /api/pharmacies/me/profile/address
        /// </summary>
        [HttpPut("profile/address")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyAddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PharmacyAddressResponse>>> UpdateAddress(
            [FromBody] UpdatePharmacyAddressRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update pharmacy address");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update address request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var updatedAddress = await _pharmacyProfileService.UpdateAddressAsync(currentPharmacyId, request);

                _logger.LogInformation("Address updated successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyAddressResponse>.Success(
                    updatedAddress,
                    "تم تحديث العنوان بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث العنوان",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Working Hours Operations

        /// <summary>
        /// جلب ساعات وأيام عمل الصيدلية
        /// GET /api/pharmacies/me/profile/workinghours
        /// </summary>
        [HttpGet("profile/workinghours")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyWorkingHoursResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PharmacyWorkingHoursResponse>>> GetWorkingHours()
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy working hours");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get working hours request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var workingHours = await _pharmacyProfileService.GetWorkingHoursAsync(currentPharmacyId);

                _logger.LogInformation("Working hours retrieved successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyWorkingHoursResponse>.Success(
                    workingHours,
                    "تم جلب ساعات العمل بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving working hours for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب ساعات العمل",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث ساعات وأيام عمل الصيدلية
        /// PUT /api/pharmacies/me/profile/workinghours
        /// </summary>
        [HttpPut("profile/workinghours")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyWorkingHoursResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PharmacyWorkingHoursResponse>>> UpdateWorkingHours(
            [FromBody] UpdatePharmacyWorkingHoursRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update pharmacy working hours");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update working hours request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var updatedWorkingHours = await _pharmacyProfileService.UpdateWorkingHoursAsync(currentPharmacyId, request);

                _logger.LogInformation("Working hours updated successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyWorkingHoursResponse>.Success(
                    updatedWorkingHours,
                    "تم تحديث ساعات العمل بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating working hours for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث ساعات العمل",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Delivery Operations

        /// <summary>
        /// جلب إعدادات التوصيل للصيدلية
        /// GET /api/pharmacies/me/profile/delivery
        /// </summary>
        [HttpGet("profile/delivery")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyDeliveryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PharmacyDeliveryResponse>>> GetDeliverySettings()
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy delivery settings");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get delivery settings request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var deliverySettings = await _pharmacyProfileService.GetDeliverySettingsAsync(currentPharmacyId);

                _logger.LogInformation("Delivery settings retrieved successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyDeliveryResponse>.Success(
                    deliverySettings,
                    "تم جلب إعدادات التوصيل بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delivery settings for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب إعدادات التوصيل",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث إعدادات التوصيل للصيدلية
        /// POST /api/pharmacies/me/profile/delivery
        /// </summary>
        [HttpPost("profile/delivery")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyDeliveryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PharmacyDeliveryResponse>>> UpdateDeliverySettings(
            [FromBody] UpdatePharmacyDeliveryRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update pharmacy delivery settings");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update delivery settings request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var updatedDeliverySettings = await _pharmacyProfileService.UpdateDeliverySettingsAsync(currentPharmacyId, request);

                _logger.LogInformation("Delivery settings updated successfully for pharmacy: {PharmacyId}", currentPharmacyId);
                return Ok(ApiResponse<PharmacyDeliveryResponse>.Success(
                    updatedDeliverySettings,
                    "تم تحديث إعدادات التوصيل بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery settings for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث إعدادات التوصيل",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Prescription Operations

        /// <summary>
        /// جلب الروشتات المعلقة (Pending) للصيدلية
        /// GET /api/pharmacies/me/prescriptions/pending
        /// </summary>
        [HttpGet("prescriptions/pending")]
        [ProducesResponseType(typeof(ApiResponse<PendingPrescriptionsListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PendingPrescriptionsListResponse>>> GetPendingPrescriptions()
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get pending prescriptions");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get pending prescriptions request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var result = await _pharmacyProfileService.GetPendingPrescriptionsAsync(currentPharmacyId);

                _logger.LogInformation("Retrieved {Count} pending prescriptions for pharmacy: {PharmacyId}", 
                    result.TotalCount, currentPharmacyId);

                return Ok(ApiResponse<PendingPrescriptionsListResponse>.Success(
                    result,
                    "تم جلب الروشتات المعلقة بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    new[] { ex.Message },
                    404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending prescriptions for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الروشتات المعلقة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// جلب تفاصيل روشتة معينة
        /// GET /api/pharmacies/me/prescriptions/{orderId}/details
        /// </summary>
        [HttpGet("prescriptions/{orderId}/details")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PrescriptionDetailsResponse>>> GetPrescriptionDetails(Guid orderId)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get prescription details");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get prescription details request for order: {OrderId}, pharmacy: {PharmacyId}", 
                orderId, currentPharmacyId);

            try
            {
                var result = await _pharmacyProfileService.GetPrescriptionDetailsAsync(currentPharmacyId, orderId);

                _logger.LogInformation("Retrieved prescription details for order: {OrderId}, pharmacy: {PharmacyId}", 
                    orderId, currentPharmacyId);

                return Ok(ApiResponse<PrescriptionDetailsResponse>.Success(
                    result,
                    "تم جلب تفاصيل الروشتة بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Prescription or pharmacy not found. Order: {OrderId}, Pharmacy: {PharmacyId}", 
                    orderId, currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الروشتة أو الصيدلية",
                    new[] { ex.Message },
                    404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription details. Order: {OrderId}, Pharmacy: {PharmacyId}", 
                    orderId, currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب تفاصيل الروشتة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Orders Operations

        /// <summary>
        /// جلب جميع الطلبات الخاصة بالصيدلية مع pagination
        /// </summary>
        [HttpGet("orders")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrdersListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrdersListResponse>>> GetMyOrders([FromQuery] PaginationParams paginationParams)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy orders");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid pagination parameters for pharmacy orders request");
                return BadRequest(ApiResponse<object>.Failure(
                    "معاملات الصفحات غير صحيحة",
                    statusCode: 400
                ));
            }

            _logger.LogInformation("Get pharmacy orders request for pharmacy: {PharmacyId}, Page: {Page}, PageSize: {PageSize}", 
                currentPharmacyId, paginationParams.PageNumber, paginationParams.PageSize);

            try
            {
                var orders = await _pharmacyProfileService.GetPharmacyOrdersAsync(currentPharmacyId, paginationParams);
                
                _logger.LogInformation("Retrieved {Count} orders for pharmacy: {PharmacyId}", 
                    orders.Data.Count(), currentPharmacyId);
                
                return Ok(ApiResponse<PharmacyOrdersListResponse>.Success(
                    orders,
                    "تم جلب الطلبات بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "الصيدلية غير موجودة",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy orders for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الطلبات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// رد الصيدلية على طلب الروشتة بتوفر الأدوية والأسعار
        /// </summary>
        [HttpPost("orders/{orderId}/respond")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderResponseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderResponseResponse>>> RespondToOrder(
            Guid orderId,
            [FromBody] PharmacyOrderResponseRequest request)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to respond to order");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid order response request for pharmacy: {PharmacyId}", currentPharmacyId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Pharmacy {PharmacyId} responding to order {OrderId}", currentPharmacyId, orderId);

            try
            {
                var result = await _pharmacyProfileService.RespondToOrderAsync(currentPharmacyId, orderId, request);
                
                _logger.LogInformation("Successfully processed pharmacy response for order {OrderId}", orderId);
                
                return Ok(ApiResponse<PharmacyOrderResponseResponse>.Success(
                    result,
                    "تم إرسال رد الصيدلية بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for order response: {PharmacyId}, {OrderId}", currentPharmacyId, orderId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for order response: {PharmacyId}, {OrderId}", currentPharmacyId, orderId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pharmacy response for order {OrderId}", orderId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء معالجة رد الصيدلية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// جلب إحصائيات الصيدلية
        /// GET /api/pharmacies/me/statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyStatisticsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyStatisticsResponse>>> GetStatistics()
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy statistics");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get statistics request for pharmacy: {PharmacyId}", currentPharmacyId);

            try
            {
                var statistics = await _pharmacyProfileService.GetPharmacyStatisticsAsync(currentPharmacyId);

                _logger.LogInformation("Retrieved statistics for pharmacy: {PharmacyId}", currentPharmacyId);

                return Ok(ApiResponse<PharmacyStatisticsResponse>.Success(
                    statistics,
                    "تم جلب الإحصائيات بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    new[] { ex.Message },
                    404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الإحصائيات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// جلب قائمة الطلبات المحسّنة للصيدلية
        /// GET /api/pharmacies/me/orders/list
        /// </summary>
        [HttpGet("orders/list")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrdersListOptimizedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrdersListOptimizedResponse>>> GetOrdersList(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access pharmacy orders list");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get orders list request for pharmacy: {PharmacyId}, Page: {Page}, Size: {Size}",
                currentPharmacyId, pageNumber, pageSize);

            try
            {
                var orders = await _pharmacyProfileService.GetOptimizedOrdersAsync(
                    currentPharmacyId,
                    pageNumber,
                    pageSize);

                _logger.LogInformation("Retrieved {Count} orders for pharmacy: {PharmacyId}",
                    orders.Orders.Count, currentPharmacyId);

                return Ok(ApiResponse<PharmacyOrdersListOptimizedResponse>.Success(
                    orders,
                    "تم جلب الطلبات بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pharmacy not found: {PharmacyId}", currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الصيدلية",
                    new[] { ex.Message },
                    404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders list for pharmacy: {PharmacyId}", currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الطلبات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// جلب تفاصيل طلب معين
        /// GET /api/pharmacies/me/orders/{orderId}
        /// </summary>
        [HttpGet("orders/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<PharmacyOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PharmacyOrderDetailResponse>>> GetOrderDetail(Guid orderId)
        {
            var currentPharmacyId = GetCurrentPharmacyId();

            if (currentPharmacyId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access order detail");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get order detail request for order: {OrderId}, pharmacy: {PharmacyId}",
                orderId, currentPharmacyId);

            try
            {
                var orderDetail = await _pharmacyProfileService.GetOrderDetailAsync(
                    currentPharmacyId,
                    orderId);

                _logger.LogInformation("Retrieved order detail for order: {OrderId}", orderId);

                return Ok(ApiResponse<PharmacyOrderDetailResponse>.Success(
                    orderDetail,
                    "تم جلب تفاصيل الطلب بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order or pharmacy not found: {OrderId}, {PharmacyId}",
                    orderId, currentPharmacyId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الطلب",
                    new[] { ex.Message },
                    404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order detail: {OrderId}, {PharmacyId}",
                    orderId, currentPharmacyId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب تفاصيل الطلب",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentPharmacyId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
