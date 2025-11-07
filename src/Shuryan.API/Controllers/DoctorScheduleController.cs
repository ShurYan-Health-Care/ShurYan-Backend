using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shuryan.API.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IDoctorScheduleService _scheduleService;
        private readonly IDoctorServicePricingService _servicePricingService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<DoctorScheduleController> _logger;

        public DoctorScheduleController(
            IDoctorScheduleService scheduleService,
            IDoctorServicePricingService servicePricingService,
            IAppointmentService appointmentService,
            ILogger<DoctorScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _servicePricingService = servicePricingService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        #region Public Schedule Endpoints (For Booking)

        [HttpGet("{doctorId:guid}/appointments/schedule")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<DayScheduleSlotResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<DayScheduleSlotResponse>>>> GetDoctorWeeklySchedule(Guid doctorId)
        {
            _logger.LogInformation("Getting weekly schedule for doctor {DoctorId}", doctorId);

            try
            {
                var schedule = await _scheduleService.GetWeeklyScheduleForFrontendAsync(doctorId);
                return Ok(ApiResponse<List<DayScheduleSlotResponse>>.Success(
                    schedule,
                    "تم جلب الجدول الأسبوعي بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedule for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الجدول الأسبوعي",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{doctorId:guid}/appointments/exceptions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<ExceptionalDateResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ExceptionalDateResponse>>>> GetDoctorExceptionalDates(Guid doctorId)
        {
            _logger.LogInformation("Getting exceptional dates for doctor {DoctorId}", doctorId);

            try
            {
                var exceptions = await _scheduleService.GetExceptionalDatesForFrontendAsync(doctorId);
                return Ok(ApiResponse<List<ExceptionalDateResponse>>.Success(
                    exceptions,
                    "تم جلب المواعيد الاستثنائية بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exceptional dates for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المواعيد الاستثنائية",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{doctorId:guid}/services")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<DoctorServicesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DoctorServicesResponse>>> GetDoctorServices(Guid doctorId)
        {
            _logger.LogInformation("Getting services for doctor {DoctorId}", doctorId);

            try
            {
                var services = await _servicePricingService.GetAllServicesAsync(doctorId);
                return Ok(ApiResponse<DoctorServicesResponse>.Success(
                    services,
                    "تم جلب الخدمات بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب الخدمات",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{doctorId:guid}/appointments/booked")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookedAppointmentSlotResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookedAppointmentSlotResponse>>>> GetBookedAppointments(
            Guid doctorId,
            [FromQuery] string date)
        {
            _logger.LogInformation("Getting booked appointments for doctor {DoctorId} on date {Date}", doctorId, date);

            try
            {
                if (string.IsNullOrWhiteSpace(date))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "التاريخ مطلوب",
                        new[] { "Date parameter is required" },
                        400
                    ));
                }

                if (!DateTime.TryParse(date, out var appointmentDate))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "صيغة التاريخ غير صحيحة. الصيغة المطلوبة: YYYY-MM-DD",
                        new[] { "Invalid date format. Expected YYYY-MM-DD" },
                        400
                    ));
                }

                var bookedAppointments = await _appointmentService.GetBookedAppointmentsForDateAsync(doctorId, appointmentDate);
                return Ok(ApiResponse<IEnumerable<BookedAppointmentSlotResponse>>.Success(
                    bookedAppointments,
                    "تم جلب المواعيد المحجوزة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booked appointments for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء جلب المواعيد المحجوزة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        [HttpGet("{doctorId:guid}/appointments/available-slots")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AvailableTimeSlotResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AvailableTimeSlotResponse>>>> GetAvailableTimeSlots(
            Guid doctorId,
            [FromQuery] string date,
            [FromQuery] int consultationType)
        {
            _logger.LogInformation("Getting available time slots for doctor {DoctorId} on date {Date} for consultationType {ConsultationType}",
                doctorId, date, consultationType);

            try
            {
                if (string.IsNullOrWhiteSpace(date))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "التاريخ مطلوب",
                        new[] { "Date parameter is required" },
                        400
                    ));
                }

                if (!DateTime.TryParse(date, out var appointmentDate))
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "صيغة التاريخ غير صحيحة. الصيغة المطلوبة: YYYY-MM-DD",
                        new[] { "Invalid date format. Expected YYYY-MM-DD" },
                        400
                    ));
                }

                if (consultationType != 0 && consultationType != 1)
                {
                    return BadRequest(ApiResponse<object>.Failure(
                        "نوع الاستشارة غير صحيح. يجب أن يكون 0 أو 1",
                        new[] { "Invalid consultationType. Must be 0 or 1" },
                        400
                    ));
                }

                var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, appointmentDate, consultationType);
                return Ok(ApiResponse<IEnumerable<AvailableTimeSlotResponse>>.Success(
                    availableSlots,
                    "تم حساب الفترات المتاحة بنجاح"
                ));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Doctor {DoctorId} not found", doctorId);
                return NotFound(ApiResponse<object>.Failure(ex.Message, statusCode: 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available time slots for doctor {DoctorId}", doctorId);
                return StatusCode(500, ApiResponse<object>.Failure(
                    "حدث خطأ أثناء حساب الفترات المتاحة",
                    new[] { ex.Message },
                    500
                ));
            }
        }

        #endregion

        #region Helper Methods

        private Guid GetCurrentDoctorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
