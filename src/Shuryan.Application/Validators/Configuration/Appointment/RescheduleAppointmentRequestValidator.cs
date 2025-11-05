using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Appointment;

namespace Shuryan.Application.Validators.Configuration.Appointment
{
    public class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
    {
        public RescheduleAppointmentRequestValidator()
        {
            RuleFor(x => x.NewScheduledStartTime)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("New appointment start time must be in the future");

            RuleFor(x => x.NewScheduledEndTime)
                .NotEmpty()
                .GreaterThan(x => x.NewScheduledStartTime)
                .WithMessage("New appointment end time must be after start time");
        }
    }
}