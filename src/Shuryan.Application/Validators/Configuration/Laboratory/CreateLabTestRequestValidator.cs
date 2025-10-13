using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabTestRequestValidator : AbstractValidator<CreateLabTestRequest>
    {
        public CreateLabTestRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(3, 200)
                .WithMessage("Test name must be between 3 and 200 characters");

            RuleFor(x => x.Code)
                .NotEmpty()
                .Length(2, 50)
                .Matches(@"^[A-Z0-9\-]+$")
                .WithMessage("Test code must be uppercase letters, numbers, or hyphens (2-50 characters)");

            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Invalid test category");

            RuleFor(x => x.SpecialInstructions)
                .Length(0, 500)
                .When(x => !string.IsNullOrEmpty(x.SpecialInstructions))
                .WithMessage("Special instructions cannot exceed 500 characters");
        }
    }
}
