using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Prescription
{
    public class DispensePrescriptionRequestValidator : AbstractValidator<DispensePrescriptionRequest>
    {
        public DispensePrescriptionRequestValidator()
        {
            RuleFor(x => x.PharmacyId)
                .NotEmpty().WithMessage("Pharmacy ID is required");

            RuleFor(x => x.PharmacistId)
                .NotEmpty().WithMessage("Pharmacist ID is required");

            RuleFor(x => x.DispensedMedications)
                .NotEmpty().WithMessage("At least one medication must be dispensed")
                .Must(x => x != null && x.Count > 0).WithMessage("Dispensed medications list cannot be empty");

            RuleForEach(x => x.DispensedMedications)
                .ChildRules(medication =>
                {
                    medication.RuleFor(m => m.MedicationId)
                        .NotEmpty().WithMessage("Medication ID is required");

                    medication.RuleFor(m => m.QuantityDispensed)
                        .GreaterThan(0).WithMessage("Quantity must be greater than 0");

                    medication.RuleFor(m => m.UnitPrice)
                        .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
                });

            RuleFor(x => x.TotalCost)
                .GreaterThanOrEqualTo(0).WithMessage("Total cost cannot be negative");

            RuleFor(x => x.PharmacistNotes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.PharmacistNotes));
        }
    }
}
