using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Review;
using Shuryan.Application.Validators.Configuration.Common;

namespace Shuryan.Application.Validators.Configuration.Review
{
    public class CreatePharmacyReviewRequestValidator : AbstractValidator<CreatePharmacyReviewRequest>
    {
        public CreatePharmacyReviewRequestValidator()
        {
            RuleFor(x => x.PharmacyOrderId)
                .NotEmpty()
                .WithMessage("Pharmacy order ID is required");

            RuleFor(x => x.OverallSatisfaction)
                .InclusiveBetween(1, 5)
                .WithMessage("Overall satisfaction must be between 1 and 5");

            RuleFor(x => x.MedicationAvailability)
                .InclusiveBetween(1, 5)
                .WithMessage("Medication Availabil rating must be between 1 and 5");

            RuleFor(x => x.DeliverySpeed)
                .InclusiveBetween(1, 5)
                .WithMessage("Delivery speed rating must be between 1 and 5");

            RuleFor(x => x.ServiceQuality)
                .InclusiveBetween(1, 5)
                .WithMessage("Service quality rating must be between 1 and 5");

            RuleFor(x => x.ValueForMoney)
                .InclusiveBetween(1, 5)
                .WithMessage("Value for money rating must be between 1 and 5");
        }
    }
}