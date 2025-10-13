using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class CreateDoctorOverrideRequestValidator : AbstractValidator<CreateDoctorOverrideRequest>
    {
        public CreateDoctorOverrideRequestValidator()
        {
            RuleFor(x => x.StartTime)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Start time must be in the future");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid override type");
        }
    }
}
