using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Prescription
{
    public class CancelPrescriptionRequestValidator : AbstractValidator<CancelPrescriptionRequest>
    {
        public CancelPrescriptionRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Cancellation reason is required")
                .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
