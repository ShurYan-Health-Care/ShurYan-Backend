using FluentValidation;
using Shuryan.Application.DTOs.Requests.Appointment;
using System;
using System.Text.RegularExpressions;

namespace Shuryan.Application.Validators.Configuration.Appointment
{
    public class BookAppointmentRequestValidator : AbstractValidator<BookAppointmentRequest>
    {
        public BookAppointmentRequestValidator()
        {
            // Validate DoctorId
            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .WithMessage("معرف الدكتور مطلوب")
                .WithErrorCode("DOCTOR_ID_REQUIRED");

            // Validate AppointmentDate
            RuleFor(x => x.AppointmentDate)
                .NotEmpty()
                .WithMessage("تاريخ الموعد مطلوب")
                .WithErrorCode("DATE_REQUIRED")
                .Must(BeValidDateFormat)
                .WithMessage("صيغة التاريخ غير صحيحة. الصيغة المطلوبة: YYYY-MM-DD")
                .WithErrorCode("DATE_INVALID_FORMAT")
                .Must(BeInFuture)
                .WithMessage("لا يمكن حجز موعد في الماضي")
                .WithErrorCode("DATE_IN_PAST");

            // Validate AppointmentTime
            RuleFor(x => x.AppointmentTime)
                .NotEmpty()
                .WithMessage("وقت الموعد مطلوب")
                .WithErrorCode("TIME_REQUIRED")
                .Must(BeValidTimeFormat)
                .WithMessage("صيغة الوقت غير صحيحة. الصيغة المطلوبة: HH:mm (24-hour format)")
                .WithErrorCode("TIME_INVALID_FORMAT");

            // Validate ConsultationType
            RuleFor(x => x.ConsultationType)
                .Must(x => x == 1 || x == 2)
                .WithMessage("نوع الاستشارة غير صحيح. يجب أن يكون 0 (كشف عادي) أو 1 (إعادة كشف)")
                .WithErrorCode("CONSULTATION_TYPE_INVALID");
        }

        private bool BeValidDateFormat(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return false;

            // Pattern: YYYY-MM-DD
            var datePattern = @"^\d{4}-\d{2}-\d{2}$";
            if (!Regex.IsMatch(date, datePattern))
                return false;

            // Try to parse as DateTime
            return DateTime.TryParse(date, out _);
        }

        private bool BeInFuture(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return false;

            if (!DateTime.TryParse(date, out var parsedDate))
                return false;

            // Compare date only (ignore time)
            return parsedDate.Date >= DateTime.Now.Date;
        }

        private bool BeValidTimeFormat(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return false;

            // Pattern: HH:mm (00:00 to 23:59)
            var timePattern = @"^([01]\d|2[0-3]):([0-5]\d)$";
            return Regex.IsMatch(time, timePattern);
        }
    }
}
