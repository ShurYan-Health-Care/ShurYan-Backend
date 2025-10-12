using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Configuration.Prescription
{
    public class CreatePrescribedMedicationRequestValidator : AbstractValidator<CreatePrescribedMedicationRequest>
    {
        public CreatePrescribedMedicationRequestValidator()
        {
            RuleFor(x => x.MedicationId)
                .NotEmpty()
                .WithMessage("Medication ID is required");

            RuleFor(x => x.Dosage)
                .NotEmpty()
                .Length(2, 200)
                .WithMessage("Dosage must be between 2 and 200 characters");

            RuleFor(x => x.Frequency)
                .NotEmpty()
                .Length(2, 200)
                .WithMessage("Frequency must be between 2 and 200 characters");

            RuleFor(x => x.DurationDays)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(365)
                .WithMessage("Duration must be between 1 and 365 days");

            RuleFor(x => x.SpecialInstructions)
                .Length(0, 500)
                .When(x => !string.IsNullOrEmpty(x.SpecialInstructions))
                .WithMessage("Special instructions cannot exceed 500 characters");
        }
    }
}