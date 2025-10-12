using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabWorkingHoursRequestValidator : AbstractValidator<CreateLabWorkingHoursRequest>
    {
        public CreateLabWorkingHoursRequestValidator()
        {
            RuleFor(x => x.Day)
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
