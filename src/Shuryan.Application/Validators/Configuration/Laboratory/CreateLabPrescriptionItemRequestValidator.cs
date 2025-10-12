using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabPrescriptionItemRequestValidator : AbstractValidator<CreateLabPrescriptionItemRequest>
    {
        public CreateLabPrescriptionItemRequestValidator()
        {
            RuleFor(x => x.LabTestId)
                .NotEmpty()
                .WithMessage("Lab test ID is required");

            RuleFor(x => x.SpecialInstructions)
                .Length(0, 500)
                .When(x => !string.IsNullOrEmpty(x.SpecialInstructions))
                .WithMessage("Special instructions cannot exceed 500 characters");
        }
    }
}
