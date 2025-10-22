using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Prescription
{
    public class RenewPrescriptionRequestValidator : AbstractValidator<RenewPrescriptionRequest>
    {
        public RenewPrescriptionRequestValidator()
        {
            RuleFor(x => x.DurationInDays)
                .GreaterThan(0).WithMessage("Duration must be greater than 0")
                .LessThanOrEqualTo(365).WithMessage("Duration cannot exceed 365 days");

            RuleFor(x => x.GeneralInstructions)
                .MaximumLength(1000).WithMessage("Instructions cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.GeneralInstructions));

            RuleFor(x => x.RenewalReason)
                .MaximumLength(500).WithMessage("Renewal reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.RenewalReason));
        }
    }
}
