using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class CreatePharmacyOrderRequestValidator : AbstractValidator<CreatePharmacyOrderRequest>
    {
        public CreatePharmacyOrderRequestValidator()
        {
            RuleFor(x => x.PharmacyId)
                .NotEmpty()
                .WithMessage("Pharmacy ID is required");

            RuleFor(x => x.DeliveryType)
                .IsInEnum()
                .WithMessage("Invalid delivery type");

            RuleFor(x => x.DeliveryPersonPhone)
                .NotEmpty()
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Delivery person phone must be in valid E.164 format");

            RuleFor(x => x.DeliveryPersonName)
                .Length(2, 100)
                .When(x => !string.IsNullOrEmpty(x.DeliveryPersonName))
                .WithMessage("Delivery person name must be between 2 and 100 characters");

            RuleFor(x => x.DeliveryNotes)
                .Length(0, 500)
                .When(x => !string.IsNullOrEmpty(x.DeliveryNotes))
                .WithMessage("Delivery notes cannot exceed 500 characters");
        }
    }
}
