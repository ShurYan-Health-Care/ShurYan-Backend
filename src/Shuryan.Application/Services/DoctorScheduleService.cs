using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DoctorScheduleService> _logger;

        public DoctorScheduleService(
            IUnitOfWork unitOfWork,
            ILogger<DoctorScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Weekly Schedule

        public async Task<WeeklyScheduleResponse> GetWeeklyScheduleAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting weekly schedule for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allAvailabilities = await _unitOfWork.DoctorAvailabilities.GetAllAsync();
                var doctorAvailabilities = allAvailabilities
                    .Where(a => a.DoctorId == doctorId)
                    .ToList();

                var response = new WeeklyScheduleResponse
                {
                    WeeklySchedule = new WeeklyScheduleDataDto
                    {
                        Saturday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Saturday),
                        Sunday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Sunday),
                        Monday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Monday),
                        Tuesday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Tuesday),
                        Wednesday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Wednesday),
                        Thursday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Thursday),
                        Friday = GetDaySchedule(doctorAvailabilities, SysDayOfWeek.Friday)
                    }
                };

                _logger.LogInformation("Successfully retrieved weekly schedule for doctor {DoctorId}", doctorId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedule for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<WeeklyScheduleResponse> UpdateWeeklyScheduleAsync(Guid doctorId, UpdateWeeklyScheduleRequest request)
        {
            try
            {
                _logger.LogInformation("Updating weekly schedule for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get existing availabilities
                var allAvailabilities = await _unitOfWork.DoctorAvailabilities.GetAllAsync();
                var existingAvailabilities = allAvailabilities
                    .Where(a => a.DoctorId == doctorId)
                    .ToList();

                // Delete all existing availabilities
                foreach (var availability in existingAvailabilities)
                {
                    _unitOfWork.DoctorAvailabilities.Delete(availability);
                }

                // Add new availabilities
                var schedule = request.WeeklySchedule;
                await AddDayAvailability(doctorId, SysDayOfWeek.Saturday, schedule.Saturday);
                await AddDayAvailability(doctorId, SysDayOfWeek.Sunday, schedule.Sunday);
                await AddDayAvailability(doctorId, SysDayOfWeek.Monday, schedule.Monday);
                await AddDayAvailability(doctorId, SysDayOfWeek.Tuesday, schedule.Tuesday);
                await AddDayAvailability(doctorId, SysDayOfWeek.Wednesday, schedule.Wednesday);
                await AddDayAvailability(doctorId, SysDayOfWeek.Thursday, schedule.Thursday);
                await AddDayAvailability(doctorId, SysDayOfWeek.Friday, schedule.Friday);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated weekly schedule for doctor {DoctorId}", doctorId);
                return await GetWeeklyScheduleAsync(doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating weekly schedule for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Exceptional Dates

        public async Task<ExceptionalDatesListResponse> GetExceptionalDatesAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting exceptional dates for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allOverrides = await _unitOfWork.DoctorOverrides.GetAllAsync();
                var doctorOverrides = allOverrides
                    .Where(o => o.DoctorId == doctorId)
                    .OrderBy(o => o.StartTime)
                    .ToList();

                var exceptionalDates = doctorOverrides.Select(o => new ExceptionalDateResponse
                {
                    Id = o.Id,
                    Date = o.StartTime.ToString("yyyy-MM-dd"),
                    FromTime = o.Type == OverrideType.Unavailable ? "" : o.StartTime.ToString("hh:mm"),
                    ToTime = o.Type == OverrideType.Unavailable ? "" : o.EndTime.ToString("hh:mm"),
                    FromPeriod = o.Type == OverrideType.Unavailable ? "AM" : (o.StartTime.Hour >= 12 ? "PM" : "AM"),
                    ToPeriod = o.Type == OverrideType.Unavailable ? "PM" : (o.EndTime.Hour >= 12 ? "PM" : "AM"),
                    IsClosed = o.Type == OverrideType.Unavailable
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} exceptional dates for doctor {DoctorId}",
                    exceptionalDates.Count, doctorId);

                return new ExceptionalDatesListResponse { ExceptionalDates = exceptionalDates };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exceptional dates for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<ExceptionalDateResponse> AddExceptionalDateAsync(Guid doctorId, AddExceptionalDateRequest request)
        {
            try
            {
                _logger.LogInformation("Adding exceptional date for doctor {DoctorId}. Date: {Date}, IsClosed: {IsClosed}",
                    doctorId, request.Date, request.IsClosed);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Validate date is in the future
                if (request.Date.Date < DateTime.UtcNow.Date)
                    throw new InvalidOperationException("Cannot add exceptional date in the past");

                // Check if date already exists
                var allOverrides = await _unitOfWork.DoctorOverrides.GetAllAsync();
                var existingOverride = allOverrides.FirstOrDefault(o =>
                    o.DoctorId == doctorId &&
                    o.StartTime.Date == request.Date.Date);

                if (existingOverride != null)
                    throw new InvalidOperationException($"Exceptional date already exists for {request.Date:yyyy-MM-dd}");

                DateTime startTime, endTime;

                if (request.IsClosed)
                {
                    // Closed day - use full day
                    startTime = request.Date.Date;
                    endTime = request.Date.Date.AddDays(1).AddSeconds(-1);
                }
                else
                {
                    // Parse times
                    if (string.IsNullOrWhiteSpace(request.FromTime) || string.IsNullOrWhiteSpace(request.ToTime))
                        throw new InvalidOperationException("FromTime and ToTime are required when IsClosed is false");

                    startTime = ParseTime(request.Date, request.FromTime, request.FromPeriod);
                    endTime = ParseTime(request.Date, request.ToTime, request.ToPeriod);

                    if (endTime <= startTime)
                        throw new InvalidOperationException("End time must be after start time");
                }

                var doctorOverride = new DoctorOverride
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Type = request.IsClosed ? OverrideType.Unavailable : OverrideType.Available,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DoctorOverrides.AddAsync(doctorOverride);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully added exceptional date {OverrideId} for doctor {DoctorId}",
                    doctorOverride.Id, doctorId);

                return new ExceptionalDateResponse
                {
                    Id = doctorOverride.Id,
                    Date = doctorOverride.StartTime.ToString("yyyy-MM-dd"),
                    FromTime = request.IsClosed ? "" : request.FromTime,
                    ToTime = request.IsClosed ? "" : request.ToTime,
                    FromPeriod = request.FromPeriod,
                    ToPeriod = request.ToPeriod,
                    IsClosed = request.IsClosed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exceptional date for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> RemoveExceptionalDateAsync(Guid doctorId, Guid exceptionId)
        {
            try
            {
                _logger.LogInformation("Removing exceptional date {ExceptionId} for doctor {DoctorId}",
                    exceptionId, doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var doctorOverride = await _unitOfWork.DoctorOverrides.GetByIdAsync(exceptionId);
                if (doctorOverride == null)
                    throw new ArgumentException($"Exceptional date with ID {exceptionId} not found");

                // Verify ownership
                if (doctorOverride.DoctorId != doctorId)
                    throw new InvalidOperationException("You don't have permission to delete this exceptional date");

                _unitOfWork.DoctorOverrides.Delete(doctorOverride);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully removed exceptional date {ExceptionId} for doctor {DoctorId}",
                    exceptionId, doctorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing exceptional date {ExceptionId} for doctor {DoctorId}",
                    exceptionId, doctorId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private DayScheduleResponseDto GetDaySchedule(List<DoctorAvailability> availabilities, SysDayOfWeek dayOfWeek)
        {
            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);

            if (availability == null)
            {
                return new DayScheduleResponseDto
                {
                    Enabled = false,
                    FromTime = "",
                    ToTime = "",
                    FromPeriod = "AM",
                    ToPeriod = "PM"
                };
            }

            return new DayScheduleResponseDto
            {
                Enabled = true,
                FromTime = availability.StartTime.ToString("hh:mm"),
                ToTime = availability.EndTime.ToString("hh:mm"),
                FromPeriod = availability.StartTime.Hour >= 12 ? "PM" : "AM",
                ToPeriod = availability.EndTime.Hour >= 12 ? "PM" : "AM"
            };
        }

        private async Task AddDayAvailability(Guid doctorId, SysDayOfWeek dayOfWeek, DayScheduleDto daySchedule)
        {
            if (!daySchedule.Enabled)
                return;

            if (string.IsNullOrWhiteSpace(daySchedule.FromTime) || string.IsNullOrWhiteSpace(daySchedule.ToTime))
            {
                _logger.LogWarning("Skipping {Day} - times are required when enabled", dayOfWeek);
                return;
            }

            var startTime = ParseTimeOnly(daySchedule.FromTime, daySchedule.FromPeriod);
            var endTime = ParseTimeOnly(daySchedule.ToTime, daySchedule.ToPeriod);

            if (endTime <= startTime)
            {
                _logger.LogWarning("Skipping {Day} - end time must be after start time", dayOfWeek);
                return;
            }

            var availability = new DoctorAvailability
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DoctorAvailabilities.AddAsync(availability);
        }

        private TimeOnly ParseTimeOnly(string time, string period)
        {
            var timeParts = time.Split(':');
            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);

            // Convert to 24-hour format
            if (period.ToUpper() == "PM" && hour != 12)
                hour += 12;
            else if (period.ToUpper() == "AM" && hour == 12)
                hour = 0;

            return new TimeOnly(hour, minute);
        }

        private DateTime ParseTime(DateTime date, string time, string period)
        {
            var timeParts = time.Split(':');
            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);

            // Convert to 24-hour format
            if (period.ToUpper() == "PM" && hour != 12)
                hour += 12;
            else if (period.ToUpper() == "AM" && hour == 12)
                hour = 0;

            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }

        #endregion

        #region Frontend Integration - New Format

        /// <summary>
        /// جلب الجدول الأسبوعي بصيغة الفرونت (array of 7 days with dayOfWeek 0-6)
        /// 0=Sunday, 1=Monday, ..., 6=Saturday
        /// </summary>
        public async Task<List<DayScheduleSlotResponse>> GetWeeklyScheduleForFrontendAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting weekly schedule for frontend for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allAvailabilities = await _unitOfWork.DoctorAvailabilities.GetAllAsync();
                var doctorAvailabilities = allAvailabilities
                    .Where(a => a.DoctorId == doctorId)
                    .ToList();

                // Map from our system enum (1=Saturday, 7=Friday) to frontend format (0=Sunday, 6=Saturday)
                var schedule = new List<DayScheduleSlotResponse>();

                // Frontend expects: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
                // Our system: 1=Saturday, 2=Sunday, 3=Monday, 4=Tuesday, 5=Wednesday, 6=Thursday, 7=Friday
                
                // Sunday (0 in frontend, 2 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Sunday, 0));
                
                // Monday (1 in frontend, 3 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Monday, 1));
                
                // Tuesday (2 in frontend, 4 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Tuesday, 2));
                
                // Wednesday (3 in frontend, 5 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Wednesday, 3));
                
                // Thursday (4 in frontend, 6 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Thursday, 4));
                
                // Friday (5 in frontend, 7 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Friday, 5));
                
                // Saturday (6 in frontend, 1 in our system)
                schedule.Add(GetDayScheduleSlot(doctorAvailabilities, SysDayOfWeek.Saturday, 6));

                _logger.LogInformation("Successfully retrieved weekly schedule for frontend for doctor {DoctorId}", doctorId);
                return schedule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly schedule for frontend for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// جلب المواعيد الاستثنائية بصيغة الفرونت (مع isClosed flag)
        /// </summary>
        public async Task<List<ExceptionalDateResponse>> GetExceptionalDatesForFrontendAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting exceptional dates for frontend for doctor {DoctorId}", doctorId);

                var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                var allOverrides = await _unitOfWork.DoctorOverrides.GetAllAsync();
                var doctorOverrides = allOverrides
                    .Where(o => o.DoctorId == doctorId)
                    .OrderBy(o => o.StartTime)
                    .ToList();

                var exceptionalDates = doctorOverrides.Select(o => new ExceptionalDateResponse
                {
                    Id = o.Id,
                    Date = o.StartTime.ToString("yyyy-MM-dd"),
                    IsClosed = o.Type == OverrideType.Unavailable,
                    FromTime = o.Type == OverrideType.Unavailable ? null : o.StartTime.ToString("HH:mm"),
                    ToTime = o.Type == OverrideType.Unavailable ? null : o.EndTime.ToString("HH:mm")
                }).ToList();

                _logger.LogInformation("Successfully retrieved {Count} exceptional dates for frontend for doctor {DoctorId}",
                    exceptionalDates.Count, doctorId);

                return exceptionalDates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exceptional dates for frontend for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// Helper method للحصول على معلومات يوم واحد بصيغة الفرونت
        /// </summary>
        private DayScheduleSlotResponse GetDayScheduleSlot(List<DoctorAvailability> availabilities, SysDayOfWeek dayOfWeek, int frontendDayNumber)
        {
            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);

            if (availability == null)
            {
                return new DayScheduleSlotResponse
                {
                    DayOfWeek = frontendDayNumber,
                    IsEnabled = false,
                    FromTime = null,
                    ToTime = null
                };
            }

            return new DayScheduleSlotResponse
            {
                DayOfWeek = frontendDayNumber,
                IsEnabled = true,
                FromTime = availability.StartTime.ToString("HH:mm"),
                ToTime = availability.EndTime.ToString("HH:mm")
            };
        }

        #endregion
    }
}
