using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Requests.Patient;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.DTOs.Responses.Patient;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/patients/me")]
    [Authorize(Roles = "Patient")]
    public class PatientProfileController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILabOrderService _labOrderService;
        private readonly ILogger<PatientProfileController> _logger;

        public PatientProfileController(
            IPatientService patientService,
            IFileUploadService fileUploadService,
            ILabOrderService labOrderService,
            ILogger<PatientProfileController> logger)
        {
            _patientService = patientService;
            _fileUploadService = fileUploadService;
            _labOrderService = labOrderService;
            _logger = logger;
        }

        #region Profile Operations

        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> GetMyProfile()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access patient profile");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get personal info request for patient: {PatientId}", currentPatientId);

            try
            {
                var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient profile not found for patient: {PatientId}", currentPatientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "تعذر العثور على المعلومات الشخصية",
                        statusCode: 404
                    ));
                }

                _logger.LogInformation("Personal info retrieved successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<PatientResponse>.Success(
                    patient,
                    "تم جلب المعلومات بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personal info for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المعلومات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث البيانات الشخصية للمريض (Partial Update)
        /// بيحدث بس الحاجات اللي انت بعتها، مش كل الـ fields
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ApiResponse<PatientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PatientResponse>>> UpdateMyProfile([FromBody] UpdatePatientRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update personal info");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            if (request == null)
            {
                _logger.LogWarning("Null request received for patient: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    "البيانات المرسلة فارغة",
                    statusCode: 400
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update personal info request for patient: {PatientId}", currentPatientId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update personal info request for patient: {PatientId}", currentPatientId);

            try
            {
                var patient = await _patientService.UpdatePatientAsync(currentPatientId, request);
                _logger.LogInformation("Personal info updated successfully: {PatientId}", currentPatientId);
                
                return Ok(ApiResponse<PatientResponse>.Success(
                    patient,
                    "تم تحديث المعلومات بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error for patient: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error updating personal info: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث المعلومات",
                    new[] { ex.Message },
                    500
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating personal info: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ غير متوقع",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Address Operations

        [HttpGet("address")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AddressResponse?>>> GetMyAddress()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access address");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get address request for patient: {PatientId}", currentPatientId);

            try
            {
                var address = await _patientService.GetPatientAddressAsync(currentPatientId);
                
                if (address == null)
                {
                    _logger.LogInformation("No address found for patient: {PatientId}", currentPatientId);
                    return Ok(ApiResponse<AddressResponse?>.Success(
                        null,
                        "لا يوجد عنوان مسجل"
                    ));
                }

                _logger.LogInformation("Address retrieved successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<AddressResponse?>.Success(
                    address,
                    "تم جلب العنوان بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found: {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    "المريض غير موجود",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب العنوان",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تحديث أو إنشاء عنوان المريض (Partial Update)
        /// لو العنوان موجود هيتحدث، لو مش موجود هيتعمل create
        /// </summary>
        [HttpPut("address")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AddressResponse>>> UpdateMyAddress([FromBody] UpdateAddressRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update address");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            if (request == null)
            {
                _logger.LogWarning("Null address request for patient: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    "البيانات المرسلة فارغة",
                    statusCode: 400
                ));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update address request for patient: {PatientId}", currentPatientId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update/Create address request for patient: {PatientId}", currentPatientId);

            try
            {
                var existingAddress = await _patientService.GetPatientAddressAsync(currentPatientId);
                bool isCreating = existingAddress == null;

                var address = await _patientService.UpdatePatientAddressAsync(currentPatientId, request);
                
                var message = isCreating ? "تم إنشاء العنوان بنجاح" : "تم تحديث العنوان بنجاح";
                _logger.LogInformation("{Action} address for patient: {PatientId}", isCreating ? "Created" : "Updated", currentPatientId);
                
                return Ok(ApiResponse<AddressResponse>.Success(
                    address,
                    message
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found: {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    "المريض غير موجود",
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating/creating address: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء حفظ العنوان",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Medical Record Operations

        [HttpGet("medical-record")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse?>>> GetMyMedicalRecord()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to access medical record");
                return Unauthorized(ApiResponse<object>.Failure(
                    "Invalid or missing authentication token",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get medical record request for patient: {PatientId}", currentPatientId);

            try
            {
                var medicalRecord = await _patientService.GetPatientMedicalRecordAsync(currentPatientId);
                
                _logger.LogInformation("Medical record retrieved for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<MedicalRecordResponse?>.Success(
                    medicalRecord,
                    medicalRecord != null ? "تم جلب الملف الطبي بنجاح" : "لا يوجد ملف طبي مسجل"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical record for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الملف الطبي",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpPut("medical-record")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> UpdateMyMedicalRecord([FromBody] UpdateMedicalRecordRequest request)
        {
            var currentPatientId = GetCurrentPatientId();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update medical record request for patient: {PatientId}", currentPatientId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            _logger.LogInformation("Update medical record request for patient: {PatientId}", currentPatientId);

            try
            {
                var medicalRecord = await _patientService.UpdatePatientMedicalRecordAsync(currentPatientId, request);
                _logger.LogInformation("Medical record updated successfully: {PatientId}", currentPatientId);
                return Ok(ApiResponse<MedicalRecordResponse>.Success(
                    medicalRecord,
                    "تم تحديث الملف الطبي بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Patient not found: {PatientId}", currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical record: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث الملف الطبي",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Profile Image Operations

        /// <summary>
        /// تحديث الصورة الشخصية للمريض
        /// بيحذف الصورة القديمة من Cloudinary (لو موجودة) ويرفع الصورة الجديدة
        /// </summary>
        [HttpPut("profile-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfileImage([FromForm] UpdateProfileImageRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update profile image request");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(ApiResponse<object>.Failure(
                    "بيانات غير صحيحة",
                    errors,
                    400
                ));
            }

            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to update profile image");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Update profile image request for patient: {PatientId}", currentPatientId);

            try
            {
                var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found for profile image update: {PatientId}", currentPatientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "المريض غير موجود",
                        statusCode: 404
                    ));
                }

                // Delete old image from Cloudinary if exists
                if (!string.IsNullOrWhiteSpace(patient.ProfileImageUrl))
                {
                    try
                    {
                        await _fileUploadService.DeleteFileAsync(patient.ProfileImageUrl);
                        _logger.LogInformation("Deleted old profile image from Cloudinary for patient: {PatientId}", currentPatientId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image from Cloudinary, continuing with upload");
                    }
                }

                // Upload the new profile image
                var uploadResult = await _fileUploadService.UploadProfileImageAsync(request.ProfileImage, currentPatientId.ToString());
                _logger.LogInformation("Uploaded new profile image to Cloudinary for patient: {PatientId}", currentPatientId);
                
                // Update patient's profile image URL in database
                var result = await _patientService.UpdateProfileImageAsync(currentPatientId, uploadResult.FileUrl);
                
                if (!result)
                {
                    _logger.LogWarning("Failed to update profile image URL in database: {PatientId}", currentPatientId);
                    return StatusCode(500, ApiResponse<object>.Failure(
                        "فشل تحديث الصورة في قاعدة البيانات",
                        statusCode: 500
                    ));
                }
                
                _logger.LogInformation("Profile image updated successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<object>.Success(
                    new { 
                        PatientId = currentPatientId, 
                        ProfileImageUrl = uploadResult.FileUrl 
                    },
                    "تم تحديث الصورة الشخصية بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for profile image update: {PatientId}", currentPatientId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تحديث الصورة الشخصية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// حذف الصورة الشخصية للمريض
        /// بيحذف الصورة من Cloudinary وبيحذف الـ URL من قاعدة البيانات
        /// </summary>
        [HttpDelete("profile-image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveProfileImage()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to remove profile image");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Remove profile image request for patient: {PatientId}", currentPatientId);

            try
            {
                var patient = await _patientService.GetPatientByIdAsync(currentPatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found for profile image removal: {PatientId}", currentPatientId);
                    return NotFound(ApiResponse<object>.Failure(
                        "المريض غير موجود",
                        statusCode: 404
                    ));
                }

                if (string.IsNullOrWhiteSpace(patient.ProfileImageUrl))
                {
                    _logger.LogInformation("Patient {PatientId} does not have a profile image to remove", currentPatientId);
                    return Ok(ApiResponse<object>.Success(
                        new { PatientId = currentPatientId },
                        "لا توجد صورة شخصية لحذفها"
                    ));
                }

                // Delete from Cloudinary
                try
                {
                    await _fileUploadService.DeleteFileAsync(patient.ProfileImageUrl);
                    _logger.LogInformation("Deleted profile image from Cloudinary for patient: {PatientId}", currentPatientId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image from Cloudinary, continuing with database update");
                }

                // Remove from database
                var result = await _patientService.RemoveProfileImageAsync(currentPatientId);
                if (!result)
                {
                    _logger.LogWarning("Failed to remove profile image URL from database: {PatientId}", currentPatientId);
                    return StatusCode(500, ApiResponse<object>.Failure(
                        "فشل حذف الصورة من قاعدة البيانات",
                        statusCode: 500
                    ));
                }

                _logger.LogInformation("Profile image removed successfully for patient: {PatientId}", currentPatientId);
                return Ok(ApiResponse<object>.Success(
                    new { PatientId = currentPatientId },
                    "تم حذف الصورة الشخصية بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing profile image for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء حذف الصورة الشخصية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Prescription Operations
        
        // تم نقل endpoint إرسال الروشتة إلى PatientsController لتجنب التضارب

        /// <summary>
        /// جلب رد الصيدلية على طلب المريض
        /// </summary>
        [HttpGet("orders/{orderId}/pharmacy-response")]
        [ProducesResponseType(typeof(ApiResponse<PatientPharmacyResponseView>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PatientPharmacyResponseView>>> GetPharmacyResponse(Guid orderId)
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get pharmacy response");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get pharmacy response request for order {OrderId} from patient: {PatientId}", 
                orderId, currentPatientId);

            try
            {
                var result = await _patientService.GetPharmacyResponseAsync(currentPatientId, orderId);
                
                _logger.LogInformation("Successfully retrieved pharmacy response for order {OrderId} for patient: {PatientId}", 
                    orderId, currentPatientId);
                
                return Ok(ApiResponse<PatientPharmacyResponseView>.Success(
                    result,
                    "تم جلب رد الصيدلية بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for pharmacy response request: {PatientId}, {OrderId}", currentPatientId, orderId);
                return NotFound(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for pharmacy response request: {PatientId}, {OrderId}", currentPatientId, orderId);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    statusCode: 400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pharmacy response for order {OrderId} for patient: {PatientId}", 
                    orderId, currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب رد الصيدلية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// تأكيد طلب الصيدلية بعد الدفع
        /// PUT /api/patients/me/orders/{orderId}/confirm
        /// </summary>
        [HttpPut("orders/{orderId}/confirm")]
        [ProducesResponseType(typeof(ApiResponse<ConfirmOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ConfirmOrderResponse>>> ConfirmOrder(Guid orderId)
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to confirm order");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Patient {PatientId} attempting to confirm order {OrderId}",
                currentPatientId, orderId);

            try
            {
                var result = await _patientService.ConfirmPharmacyOrderAsync(currentPatientId, orderId);

                _logger.LogInformation("Order {OrderId} confirmed successfully by patient {PatientId}",
                    orderId, currentPatientId);

                return Ok(ApiResponse<ConfirmOrderResponse>.Success(
                    result,
                    "تم تأكيد الطلب بنجاح"
                ));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order or patient not found: {OrderId}, {PatientId}",
                    orderId, currentPatientId);
                return NotFound(ApiResponse<object>.Failure(
                    "تعذر العثور على الطلب",
                    new[] { ex.Message },
                    404
                ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ApiResponse<object>.Failure(
                    ex.Message,
                    new[] { ex.Message },
                    400
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId} for patient {PatientId}",
                    orderId, currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء تأكيد الطلب",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Lab Orders Operations

        /// <summary>
        /// جلب طلبات التحاليل النشطة للمريض (الطلبات الجديدة وقيد التنفيذ)
        /// GET /api/patients/me/lab-orders/active
        /// </summary>
        [HttpGet("lab-orders/active")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabOrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LabOrderResponse>>>> GetMyActiveLabOrders()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get active lab orders");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get active lab orders request for patient: {PatientId}", currentPatientId);

            try
            {
                var orders = await _labOrderService.GetPatientActiveLabOrdersAsync(currentPatientId);
                
                _logger.LogInformation("Successfully retrieved {Count} active lab orders for patient: {PatientId}", 
                    orders.Count(), currentPatientId);
                
                return Ok(ApiResponse<IEnumerable<LabOrderResponse>>.Success(
                    orders,
                    "تم جلب طلبات التحاليل النشطة بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active lab orders for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب طلبات التحاليل",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        /// <summary>
        /// جلب طلبات التحاليل المكتملة للمريض (النتائج جاهزة أو تم الاستلام)
        /// GET /api/patients/me/lab-orders/completed
        /// </summary>
        [HttpGet("lab-orders/completed")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabOrderResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<LabOrderResponse>>>> GetMyCompletedLabOrders()
        {
            var currentPatientId = GetCurrentPatientId();

            if (currentPatientId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized attempt to get completed lab orders");
                return Unauthorized(ApiResponse<object>.Failure(
                    "غير مصرح لك بالوصول",
                    statusCode: 401
                ));
            }

            _logger.LogInformation("Get completed lab orders request for patient: {PatientId}", currentPatientId);

            try
            {
                var orders = await _labOrderService.GetPatientCompletedLabOrdersAsync(currentPatientId);
                
                _logger.LogInformation("Successfully retrieved {Count} completed lab orders for patient: {PatientId}", 
                    orders.Count(), currentPatientId);
                
                return Ok(ApiResponse<IEnumerable<LabOrderResponse>>.Success(
                    orders,
                    "تم جلب نتائج التحاليل بنجاح"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed lab orders for patient: {PatientId}", currentPatientId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب نتائج التحاليل",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentPatientId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
