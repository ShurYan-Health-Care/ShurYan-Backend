using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class CreatePharmacyWorkingHoursRequestValidator : AbstractValidator<CreatePharmacyWorkingHoursRequest>
    {
        public CreatePharmacyWorkingHoursRequestValidator()
        {
            RuleFor(x => x.DayOfWeek)
                .IsInEnum()
                .WithMessage("Invalid day of week");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Start time is required");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time");
        }
    }
}
