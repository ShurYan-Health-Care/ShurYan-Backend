using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Appointment;

namespace Shuryan.Application.Validators.Configuration.Appointment
{
    public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
    {
        public CancelAppointmentRequestValidator()
        {
            RuleFor(x => x.CancellationReason)
                .Length(5, 500)
                .When(x => !string.IsNullOrWhiteSpace(x.CancellationReason))
                .WithMessage("Cancellation reason must be between 5 and 500 characters when provided");
        }
    }
}
