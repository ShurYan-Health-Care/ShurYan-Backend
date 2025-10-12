using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class UpdateDoctorAvailabilityRequestValidator : AbstractValidator<UpdateDoctorAvailabilityRequest>
    {
        public UpdateDoctorAvailabilityRequestValidator()
        {
            RuleFor(x => x.StartTime)
                .NotEmpty()
                .When(x => x.StartTime.HasValue)
                .WithMessage("Start time is required");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .When(x => x.EndTime.HasValue && x.StartTime.HasValue)
                .WithMessage("End time must be after start time");
        }
    }
}
