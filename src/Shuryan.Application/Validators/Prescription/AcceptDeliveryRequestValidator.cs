using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Prescription
{
    public class AcceptDeliveryRequestValidator : AbstractValidator<AcceptDeliveryRequest>
    {
        public AcceptDeliveryRequestValidator()
        {
            RuleFor(x => x.PharmacyId)
                .NotEmpty().WithMessage("Pharmacy ID is required");

            RuleFor(x => x.EstimatedDeliveryMinutes)
                .GreaterThanOrEqualTo(15).WithMessage("Delivery time must be at least 15 minutes")
                .LessThanOrEqualTo(1440).WithMessage("Delivery time cannot exceed 24 hours");

            RuleFor(x => x.DeliveryAddress)
                .NotEmpty().WithMessage("Delivery address is required")
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

            RuleFor(x => x.DeliveryFee)
                .GreaterThanOrEqualTo(0).WithMessage("Delivery fee cannot be negative");

            RuleFor(x => x.DeliveryNotes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.DeliveryNotes));
        }
    }
}
