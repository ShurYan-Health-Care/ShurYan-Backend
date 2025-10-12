using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabServiceRequestValidator : AbstractValidator<CreateLabServiceRequest>
    {
        public CreateLabServiceRequestValidator()
        {
            RuleFor(x => x.LabTestId)
                .NotEmpty()
                .WithMessage("Lab test ID is required");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .LessThanOrEqualTo(100000)
                .WithMessage("Price must be between 0.01 and 100000");

            RuleFor(x => x.LabSpecificNotes)
                .Length(0, 500)
                .When(x => !string.IsNullOrEmpty(x.LabSpecificNotes))
                .WithMessage("Lab specific notes cannot exceed 500 characters");
        }
    }
}
