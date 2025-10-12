using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class CreateMedicationRequestValidator : AbstractValidator<CreateMedicationRequest>
    {
        public CreateMedicationRequestValidator()
        {
            RuleFor(x => x.BrandName)
                .NotEmpty()
                .Length(2, 200)
                .WithMessage("Brand name must be between 2 and 200 characters");

            RuleFor(x => x.GenericName)
                .Length(2, 200)
                .When(x => !string.IsNullOrEmpty(x.GenericName))
                .WithMessage("Generic name must be between 2 and 200 characters");

            RuleFor(x => x.Strength)
                .Length(0, 100)
                .When(x => !string.IsNullOrEmpty(x.Strength))
                .WithMessage("Strength cannot exceed 100 characters");

            RuleFor(x => x.DosageForm)
                .IsInEnum()
                .WithMessage("Invalid dosage form");
        }
    }
}
