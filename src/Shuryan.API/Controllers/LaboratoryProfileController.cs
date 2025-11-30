using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Application.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
        [ApiController]
        [Route("api/laboratories/me")]
        [Authorize(Roles = "Laboratory")]
        public class LaboratoryProfileController : ControllerBase
        {
                private readonly ILaboratoryProfileService _laboratoryProfileService;
                private readonly ILabOrderService _labOrderService;
                private readonly ILogger<LaboratoryProfileController> _logger;

                public LaboratoryProfileController(
                    ILaboratoryProfileService laboratoryProfileService,
                    ILabOrderService labOrderService,
                    ILogger<LaboratoryProfileController> logger)
                {
                        _laboratoryProfileService = laboratoryProfileService;
                        _labOrderService = labOrderService;
                        _logger = logger;
                }

                #region Basic Info Operations
                [HttpGet("profile/basic")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryBasicInfoResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryBasicInfoResponse>>> GetBasicInfo()
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to access laboratory profile");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        _logger.LogInformation("Get basic info request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var basicInfo = await _laboratoryProfileService.GetBasicInfoAsync(currentLaboratoryId);
                                if (basicInfo == null)
                                {
                                        _logger.LogWarning("Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                        return NotFound(ApiResponse<object>.Failure(
                                            "تعذر العثور على معلومات المعمل",
                                            statusCode: 404
                                        ));
                                }

                                _logger.LogInformation("Basic info retrieved successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryBasicInfoResponse>.Success(
                                    basicInfo,
                                    "تم جلب المعلومات الأساسية بنجاح"
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving basic info for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء جلب المعلومات",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                [HttpPut("profile/basic")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryBasicInfoResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryBasicInfoResponse>>> UpdateBasicInfo(
                    [FromBody] UpdateLaboratoryBasicInfoRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to update laboratory basic info");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure(
                                    "بيانات غير صحيحة",
                                    errors,
                                    400
                                ));
                        }

                        _logger.LogInformation("Update basic info request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var updatedInfo = await _laboratoryProfileService.UpdateBasicInfoAsync(currentLaboratoryId, request);

                                _logger.LogInformation("Basic info updated successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryBasicInfoResponse>.Success(
                                    updatedInfo,
                                    "تم تحديث المعلومات الأساسية بنجاح"
                                ));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(
                                    "تعذر العثور على المعمل",
                                    statusCode: 404
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating basic info for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء تحديث المعلومات",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                [HttpPut("profile/image")]
                [Consumes("multipart/form-data")]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<object>>> UpdateProfileImage([FromForm] UpdateLaboratoryProfileImageRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to update laboratory profile image");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        if (request.ProfileImage == null || request.ProfileImage.Length == 0)
                        {
                                _logger.LogWarning("No image provided for laboratory: {LaboratoryId}", currentLaboratoryId);
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
                                _logger.LogWarning("Invalid image format for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return BadRequest(ApiResponse<object>.Failure(
                                    "صيغة الصورة غير مدعومة. الصيغ المدعومة: jpg, jpeg, png, gif",
                                    statusCode: 400
                                ));
                        }

                        // Validate image size (max 5MB)
                        if (request.ProfileImage.Length > 5 * 1024 * 1024)
                        {
                                _logger.LogWarning("Image size too large for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return BadRequest(ApiResponse<object>.Failure(
                                    "حجم الصورة يجب ألا يتجاوز 5 ميجابايت",
                                    statusCode: 400
                                ));
                        }

                        _logger.LogInformation("Update profile image request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var imageUrl = await _laboratoryProfileService.UpdateProfileImageAsync(currentLaboratoryId, request.ProfileImage);

                                _logger.LogInformation("Profile image updated successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<object>.Success(
                                    new { ImageUrl = imageUrl },
                                    "تم تحديث صورة البروفايل بنجاح"
                                ));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(
                                    "تعذر العثور على المعمل",
                                    statusCode: 404
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating profile image for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء تحديث الصورة",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                #endregion

                #region Address Operations
                [HttpGet("profile/address")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryAddressResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryAddressResponse>>> GetAddress()
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to access laboratory address");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        _logger.LogInformation("Get address request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var address = await _laboratoryProfileService.GetAddressAsync(currentLaboratoryId);
                                if (address == null)
                                {
                                        _logger.LogWarning("Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                        return NotFound(ApiResponse<object>.Failure(
                                            "تعذر العثور على عنوان المعمل",
                                            statusCode: 404
                                        ));
                                }

                                _logger.LogInformation("Address retrieved successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryAddressResponse>.Success(
                                    address,
                                    "تم جلب العنوان بنجاح"
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving address for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء جلب العنوان",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                /// <summary>
                /// تحديث عنوان المعمل (Partial Update)
                /// PUT /api/laboratories/me/profile/address
                /// </summary>
                [HttpPut("profile/address")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryAddressResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryAddressResponse>>> UpdateAddress(
                    [FromBody] UpdateLaboratoryAddressRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to update laboratory address");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure(
                                    "بيانات غير صحيحة",
                                    errors,
                                    400
                                ));
                        }

                        _logger.LogInformation("Update address request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var updatedAddress = await _laboratoryProfileService.UpdateAddressAsync(currentLaboratoryId, request);

                                _logger.LogInformation("Address updated successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryAddressResponse>.Success(
                                    updatedAddress,
                                    "تم تحديث العنوان بنجاح"
                                ));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(
                                    "تعذر العثور على المعمل",
                                    statusCode: 404
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating address for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء تحديث العنوان",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                #endregion

                #region Working Hours Operations
                [HttpGet("profile/workinghours")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryWorkingHoursResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryWorkingHoursResponse>>> GetWorkingHours()
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to access laboratory working hours");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        _logger.LogInformation("Get working hours request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var workingHours = await _laboratoryProfileService.GetWorkingHoursAsync(currentLaboratoryId);

                                _logger.LogInformation("Working hours retrieved successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryWorkingHoursResponse>.Success(
                                    workingHours,
                                    "تم جلب ساعات العمل بنجاح"
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving working hours for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء جلب ساعات العمل",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                [HttpPut("profile/workinghours")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryWorkingHoursResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryWorkingHoursResponse>>> UpdateWorkingHours(
                    [FromBody] UpdateLaboratoryWorkingHoursRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to update laboratory working hours");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure(
                                    "بيانات غير صحيحة",
                                    errors,
                                    400
                                ));
                        }

                        _logger.LogInformation("Update working hours request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var updatedWorkingHours = await _laboratoryProfileService.UpdateWorkingHoursAsync(currentLaboratoryId, request);

                                _logger.LogInformation("Working hours updated successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryWorkingHoursResponse>.Success(
                                    updatedWorkingHours,
                                    "تم تحديث ساعات العمل بنجاح"
                                ));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(
                                    "تعذر العثور على المعمل",
                                    statusCode: 404
                                ));
                        }
                        catch (InvalidOperationException ex)
                        {
                                _logger.LogWarning(ex, "Invalid operation for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return BadRequest(ApiResponse<object>.Failure(
                                    ex.Message,
                                    statusCode: 400
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating working hours for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء تحديث ساعات العمل",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                #endregion

                #region Home Sample Collection Operations
                [HttpGet("home-collection")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryHomeSampleCollectionResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryHomeSampleCollectionResponse>>> GetHomeSampleCollectionSettings()
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to access laboratory home sample collection settings");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        _logger.LogInformation("Get home sample collection settings request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var settings = await _laboratoryProfileService.GetHomeSampleCollectionSettingsAsync(currentLaboratoryId);

                                _logger.LogInformation("Home sample collection settings retrieved successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryHomeSampleCollectionResponse>.Success(
                                    settings,
                                    "تم جلب إعدادات خدمة سحب العينة من البيت بنجاح"
                                ));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(
                                    "تعذر العثور على المعمل",
                                    statusCode: 404
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving home sample collection settings for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء جلب إعدادات الخدمة",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                [HttpPut("home-collection")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryHomeSampleCollectionResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LaboratoryHomeSampleCollectionResponse>>> UpdateHomeSampleCollectionSettings(
                    [FromBody] UpdateLaboratoryHomeSampleCollectionRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                _logger.LogWarning("Unauthorized attempt to update laboratory home sample collection settings");
                                return Unauthorized(ApiResponse<object>.Failure(
                                    "غير مصرح لك بالوصول",
                                    statusCode: 401
                                ));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure(
                                    "بيانات غير صحيحة",
                                    errors,
                                    400
                                ));
                        }

                        _logger.LogInformation("Update home sample collection settings request for laboratory: {LaboratoryId}", currentLaboratoryId);

                        try
                        {
                                var updatedSettings = await _laboratoryProfileService.UpdateHomeSampleCollectionSettingsAsync(currentLaboratoryId, request);

                                _logger.LogInformation("Home sample collection settings updated successfully for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return Ok(ApiResponse<LaboratoryHomeSampleCollectionResponse>.Success(
                                    updatedSettings,
                                    "تم تحديث إعدادات خدمة سحب العينة من البيت بنجاح"
                                ));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory not found: {LaboratoryId}", currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(
                                    "تعذر العثور على المعمل",
                                    statusCode: 404
                                ));
                        }
                        catch (InvalidOperationException ex)
                        {
                                _logger.LogWarning(ex, "Invalid operation for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return BadRequest(ApiResponse<object>.Failure(
                                    ex.Message,
                                    statusCode: 400
                                ));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating home sample collection settings for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                    "حدث خطأ أثناء تحديث إعدادات الخدمة",
                                    new[] { ex.Message },
                                    500
                                ));
                        }
                }

                #endregion

                #region Lab Services Operations
                [HttpGet("services")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabServiceDetailResponse>>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<IEnumerable<LabServiceDetailResponse>>>> GetLabServices()
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                var services = await _laboratoryProfileService.GetLabServicesAsync(currentLaboratoryId);
                                return Ok(ApiResponse<IEnumerable<LabServiceDetailResponse>>.Success(services, "تم جلب الخدمات بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving lab services for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب الخدمات", new[] { ex.Message }, 500));
                        }
                }

                [HttpPost("services")]
                [ProducesResponseType(typeof(ApiResponse<LabServiceDetailResponse>), StatusCodes.Status201Created)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabServiceDetailResponse>>> AddLabService([FromBody] AddLabServiceRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var service = await _laboratoryProfileService.AddLabServiceAsync(currentLaboratoryId, request);
                                return StatusCode(201, ApiResponse<LabServiceDetailResponse>.Success(service, "تم إضافة الخدمة بنجاح"));
                        }
                        catch (KeyNotFoundException ex)
                        {
                                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
                        }
                        catch (InvalidOperationException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error adding lab service for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء إضافة الخدمة", new[] { ex.Message }, 500));
                        }
                }

                [HttpPut("services/{serviceId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<LabServiceDetailResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabServiceDetailResponse>>> UpdateLabService(
                        Guid serviceId,
                        [FromBody] UpdateLabServiceRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var service = await _laboratoryProfileService.UpdateLabServiceAsync(currentLaboratoryId, serviceId, request);
                                return Ok(ApiResponse<LabServiceDetailResponse>.Success(service, "تم تحديث الخدمة بنجاح"));
                        }
                        catch (KeyNotFoundException)
                        {
                                return NotFound(ApiResponse<object>.Failure("الخدمة غير موجودة", statusCode: 404));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating lab service {ServiceId} for laboratory: {LaboratoryId}", serviceId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء تحديث الخدمة", new[] { ex.Message }, 500));
                        }
                }

                [HttpDelete("services/{serviceId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<object>>> DeleteLabService(Guid serviceId)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                var deleted = await _laboratoryProfileService.DeleteLabServiceAsync(currentLaboratoryId, serviceId);
                                if (!deleted)
                                {
                                        return NotFound(ApiResponse<object>.Failure("الخدمة غير موجودة", statusCode: 404));
                                }

                                return Ok(ApiResponse<object>.Success(null!, "تم حذف الخدمة بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error deleting lab service {ServiceId} for laboratory: {LaboratoryId}", serviceId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء حذف الخدمة", new[] { ex.Message }, 500));
                        }
                }

                [HttpPut("services/{serviceId:guid}/availability")]
                [ProducesResponseType(typeof(ApiResponse<LabServiceDetailResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabServiceDetailResponse>>> UpdateLabServiceAvailability(
                        Guid serviceId,
                        [FromBody] UpdateLabServiceAvailabilityRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var service = await _laboratoryProfileService.UpdateLabServiceAvailabilityAsync(currentLaboratoryId, serviceId, request);
                                var message = request.IsAvailable ? "تم تفعيل الخدمة بنجاح" : "تم تعطيل الخدمة بنجاح";
                                return Ok(ApiResponse<LabServiceDetailResponse>.Success(service, message));
                        }
                        catch (KeyNotFoundException)
                        {
                                return NotFound(ApiResponse<object>.Failure("الخدمة غير موجودة", statusCode: 404));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error updating availability for lab service {ServiceId} for laboratory: {LaboratoryId}", serviceId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء تحديث حالة الخدمة", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Available Lab Tests
                [HttpGet("available-tests")]
                [AllowAnonymous] // إلغاء authorization من الـ Controller
                [Authorize(Roles = "Doctor,Laboratory")] // تحديد الـ roles المسموحة
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabTestListResponse>>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<IEnumerable<LabTestListResponse>>>> GetAvailableLabTests(
                        [FromQuery] string? search = null,
                        [FromQuery] string? category = null)
                {
                        try
                        {
                                var tests = await _laboratoryProfileService.GetAvailableLabTestsAsync(search, category);
                                return Ok(ApiResponse<IEnumerable<LabTestListResponse>>.Success(tests, "تم جلب التحاليل المتاحة بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving available lab tests");
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب التحاليل", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Lab Orders Operations

                /// <summary>
                /// جلب كل الطلبات للمعمل (مع فلترة)
                /// GET /api/laboratories/me/orders
                /// </summary>
                [HttpGet("orders")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabOrderResponse>>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<IEnumerable<LabOrderResponse>>>> GetLabOrders(
                        [FromQuery] Core.Enums.Laboratory.LabOrderStatus? status = null)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                var orders = await _labOrderService.GetAllLabOrdersAsync(
                                        laboratoryId: currentLaboratoryId,
                                        status: status);
                                return Ok(ApiResponse<IEnumerable<LabOrderResponse>>.Success(orders, "تم جلب الطلبات بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving lab orders for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب الطلبات", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// جلب تفاصيل طلب معين
                /// GET /api/laboratories/me/orders/{orderId}
                /// </summary>
                [HttpGet("orders/{orderId:guid}")]
                [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabOrderResponse>>> GetLabOrderById(Guid orderId)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                var order = await _labOrderService.GetLabOrderByIdAsync(orderId);
                                if (order == null || order.LaboratoryId != currentLaboratoryId)
                                {
                                        return NotFound(ApiResponse<object>.Failure("الطلب غير موجود", statusCode: 404));
                                }

                                return Ok(ApiResponse<LabOrderResponse>.Success(order, "تم جلب تفاصيل الطلب بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving lab order {OrderId} for laboratory: {LaboratoryId}", orderId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب تفاصيل الطلب", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// رد المعمل على الطلب (قبول/رفض مع السعر)
                /// PUT /api/laboratories/me/orders/{orderId}/respond
                /// </summary>
                [HttpPut("orders/{orderId:guid}/respond")]
                [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabOrderResponse>>> RespondToLabOrder(Guid orderId, [FromBody] RespondToLabOrderRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var order = await _labOrderService.GetLabOrderByIdAsync(orderId);
                                if (order == null || order.LaboratoryId != currentLaboratoryId)
                                {
                                        return NotFound(ApiResponse<object>.Failure("الطلب غير موجود", statusCode: 404));
                                }

                                LabOrderResponse updatedOrder;
                                if (request.Accept)
                                {
                                        updatedOrder = await _labOrderService.ConfirmLabOrderAsync(orderId);
                                        return Ok(ApiResponse<LabOrderResponse>.Success(updatedOrder, "تم قبول الطلب بنجاح"));
                                }
                                else
                                {
                                        updatedOrder = await _labOrderService.RejectLabOrderAsync(orderId, request.RejectionReason ?? "تم الرفض من المعمل");
                                        return Ok(ApiResponse<LabOrderResponse>.Success(updatedOrder, "تم رفض الطلب"));
                                }
                        }
                        catch (InvalidOperationException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error responding to lab order {OrderId} for laboratory: {LaboratoryId}", orderId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء الرد على الطلب", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// بدء العمل على الطلب - نقل الحالة من "في انتظار العينات" إلى "قيد التنفيذ في المعمل"
                /// POST /api/laboratories/me/lab-orders/{orderId}/start-work
                /// </summary>
                [HttpPost("lab-orders/{orderId}/start-work")]
                [ProducesResponseType(typeof(ApiResponse<LabOrderResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabOrderResponse>>> StartLabWork(
                        Guid orderId,
                        CancellationToken cancellationToken)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                var updatedOrder = await _labOrderService.StartLabWorkAsync(
                                        orderId, 
                                        currentLaboratoryId, 
                                        cancellationToken);

                                return Ok(ApiResponse<LabOrderResponse>.Success(
                                        updatedOrder, 
                                        "تم بدء العمل على الطلب بنجاح - الحالة: قيد التنفيذ في المعمل"));
                        }
                        catch (ArgumentException ex)
                        {
                                _logger.LogWarning(ex, "Lab order {OrderId} not found for laboratory {LaboratoryId}", 
                                        orderId, currentLaboratoryId);
                                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                                _logger.LogWarning(ex, "Laboratory {LaboratoryId} unauthorized to access order {OrderId}", 
                                        currentLaboratoryId, orderId);
                                return StatusCode(403, ApiResponse<object>.Failure(ex.Message, statusCode: 403));
                        }
                        catch (InvalidOperationException ex)
                        {
                                _logger.LogWarning(ex, "Invalid operation for lab order {OrderId} by laboratory {LaboratoryId}", 
                                        orderId, currentLaboratoryId);
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error starting lab work for order {OrderId} by laboratory {LaboratoryId}", 
                                        orderId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure(
                                        "حدث خطأ أثناء بدء العمل على الطلب", 
                                        new[] { ex.Message }, 
                                        500));
                        }
                }

                /// <summary>
                /// رفع نتيجة التحليل
                /// POST /api/laboratories/me/orders/{orderId}/results
                /// </summary>
                [HttpPost("orders/{orderId:guid}/results")]
                [ProducesResponseType(typeof(ApiResponse<LabResultResponse>), StatusCodes.Status201Created)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<LabResultResponse>>> UploadLabResult(
                        Guid orderId,
                        [FromForm] UploadLabResultRequest request)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        if (!ModelState.IsValid)
                        {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                return BadRequest(ApiResponse<object>.Failure("بيانات غير صحيحة", errors, 400));
                        }

                        try
                        {
                                var order = await _labOrderService.GetLabOrderByIdAsync(orderId);
                                if (order == null || order.LaboratoryId != currentLaboratoryId)
                                {
                                        return NotFound(ApiResponse<object>.Failure("الطلب غير موجود", statusCode: 404));
                                }

                                var createRequest = new CreateLabResultRequest
                                {
                                        LabTestId = request.LabTestId,
                                        ResultValue = request.ResultValue,
                                        ReferenceRange = request.ReferenceRange,
                                        Unit = request.Unit,
                                        Notes = request.Notes
                                };

                                var result = await _labOrderService.AddLabOrderResultAsync(orderId, createRequest);
                                return StatusCode(201, ApiResponse<LabResultResponse>.Success(result, "تم رفع النتيجة بنجاح"));
                        }
                        catch (ArgumentException ex)
                        {
                                return BadRequest(ApiResponse<object>.Failure(ex.Message, statusCode: 400));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error uploading result for lab order {OrderId} for laboratory: {LaboratoryId}", orderId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء رفع النتيجة", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// جلب نتائج طلب معين
                /// GET /api/laboratories/me/orders/{orderId}/results
                /// </summary>
                [HttpGet("orders/{orderId:guid}/results")]
                [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabResultResponse>>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
                public async Task<ActionResult<ApiResponse<IEnumerable<LabResultResponse>>>> GetLabOrderResults(Guid orderId)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                var order = await _labOrderService.GetLabOrderByIdAsync(orderId);
                                if (order == null || order.LaboratoryId != currentLaboratoryId)
                                {
                                        return NotFound(ApiResponse<object>.Failure("الطلب غير موجود", statusCode: 404));
                                }

                                var results = await _labOrderService.GetLabOrderResultsAsync(orderId);
                                return Ok(ApiResponse<IEnumerable<LabResultResponse>>.Success(results, "تم جلب النتائج بنجاح"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error retrieving results for lab order {OrderId} for laboratory: {LaboratoryId}", orderId, currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ أثناء جلب النتائج", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Dashboard & Statistics

                /// <summary>
                /// إحصائيات الداشبورد
                /// GET /api/laboratories/me/dashboard
                /// </summary>
                [HttpGet("dashboard")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryDashboardResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                public async Task<ActionResult<ApiResponse<LaboratoryDashboardResponse>>> GetDashboard()
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                // Get today's date range
                                var today = DateTime.UtcNow.Date;
                                var tomorrow = today.AddDays(1);

                                // Get all orders for this laboratory
                                var allOrders = await _labOrderService.GetLaboratoryLabOrdersAsync(currentLaboratoryId);
                                var todayOrders = allOrders.Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow).ToList();

                                // Get statistics
                                var stats = await _labOrderService.GetLabOrderStatisticsAsync(currentLaboratoryId);

                                var dashboard = new LaboratoryDashboardResponse
                                {
                                        // Today's Stats
                                        TodayOrdersCount = todayOrders.Count,
                                        TodayPendingOrders = todayOrders.Count(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.NewRequest ||
                                                                                    o.Status == Core.Enums.Laboratory.LabOrderStatus.AwaitingLabReview ||
                                                                                    o.Status == Core.Enums.Laboratory.LabOrderStatus.AwaitingPayment),
                                        TodayCompletedOrders = todayOrders.Count(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.Completed),
                                        TodayRevenue = todayOrders.Where(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.Completed)
                                                                  .Sum(o => o.TestsTotalCost + o.SampleCollectionDeliveryCost),

                                        // Overall Stats
                                        TotalOrders = stats.TotalOrders,
                                        PendingOrders = stats.PendingPaymentOrders + stats.ConfirmedOrders,
                                        InProgressOrders = stats.InProgressOrders,
                                        CompletedOrders = stats.CompletedOrders,
                                        CancelledOrders = stats.CancelledOrders,
                                        TotalRevenue = stats.TotalRevenue,

                                        // Recent Orders
                                        RecentOrders = allOrders.OrderByDescending(o => o.CreatedAt).Take(5).ToList()
                                };

                                return Ok(ApiResponse<LaboratoryDashboardResponse>.Success(dashboard, "تم جلب بيانات الداشبورد"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting dashboard for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                /// <summary>
                /// إحصائيات تفصيلية
                /// GET /api/laboratories/me/statistics
                /// </summary>
                [HttpGet("statistics")]
                [ProducesResponseType(typeof(ApiResponse<LaboratoryStatisticsResponse>), StatusCodes.Status200OK)]
                [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
                public async Task<ActionResult<ApiResponse<LaboratoryStatisticsResponse>>> GetStatistics(
                        [FromQuery] DateTime? startDate = null,
                        [FromQuery] DateTime? endDate = null)
                {
                        var currentLaboratoryId = GetCurrentLaboratoryId();

                        if (currentLaboratoryId == Guid.Empty)
                        {
                                return Unauthorized(ApiResponse<object>.Failure("غير مصرح لك بالوصول", statusCode: 401));
                        }

                        try
                        {
                                // Default to last 30 days if no dates provided
                                var end = endDate ?? DateTime.UtcNow;
                                var start = startDate ?? end.AddDays(-30);

                                var stats = await _labOrderService.GetLabOrderStatisticsAsync(currentLaboratoryId, start, end);
                                var allOrders = await _labOrderService.GetAllLabOrdersAsync(
                                        laboratoryId: currentLaboratoryId,
                                        startDate: start,
                                        endDate: end);

                                // Calculate daily stats
                                var dailyStats = allOrders
                                        .GroupBy(o => o.CreatedAt.Date)
                                        .Select(g => new DailyStatistics
                                        {
                                                Date = g.Key,
                                                OrdersCount = g.Count(),
                                                CompletedCount = g.Count(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.Completed),
                                                Revenue = g.Where(o => o.Status == Core.Enums.Laboratory.LabOrderStatus.Completed)
                                                           .Sum(o => o.TestsTotalCost + o.SampleCollectionDeliveryCost)
                                        })
                                        .OrderBy(d => d.Date)
                                        .ToList();

                                // Calculate status breakdown
                                var statusBreakdown = allOrders
                                        .GroupBy(o => o.Status)
                                        .Select(g => new StatusBreakdown
                                        {
                                                Status = g.Key.ToString(),
                                                Count = g.Count(),
                                                Percentage = allOrders.Any() ? Math.Round((double)g.Count() / allOrders.Count() * 100, 1) : 0
                                        })
                                        .ToList();

                                var response = new LaboratoryStatisticsResponse
                                {
                                        StartDate = start,
                                        EndDate = end,
                                        TotalOrders = stats.TotalOrders,
                                        CompletedOrders = stats.CompletedOrders,
                                        CancelledOrders = stats.CancelledOrders,
                                        TotalRevenue = stats.TotalRevenue,
                                        AverageOrderValue = stats.AverageOrderValue,
                                        CompletionRate = stats.TotalOrders > 0
                                                ? Math.Round((double)stats.CompletedOrders / stats.TotalOrders * 100, 1)
                                                : 0,
                                        DailyStatistics = dailyStats,
                                        StatusBreakdown = statusBreakdown
                                };

                                return Ok(ApiResponse<LaboratoryStatisticsResponse>.Success(response, "تم جلب الإحصائيات"));
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Error getting statistics for laboratory: {LaboratoryId}", currentLaboratoryId);
                                return StatusCode(500, ApiResponse<object>.Failure("حدث خطأ", new[] { ex.Message }, 500));
                        }
                }

                #endregion

                #region Helper Methods

                private Guid GetCurrentLaboratoryId()
                {
                        var userIdClaim = User.Claims.FirstOrDefault(c =>
                            c.Type == "sub" ||
                            c.Type == "uid" ||
                            c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                        {
                                return userId;
                        }

                        _logger.LogWarning("Could not find laboratory ID in JWT claims. Available claims: {Claims}",
                            string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

                        return Guid.Empty;
                }

                #endregion
        }
}
