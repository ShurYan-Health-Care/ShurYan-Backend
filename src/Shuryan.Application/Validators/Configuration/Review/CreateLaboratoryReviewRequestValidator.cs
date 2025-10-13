using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Review;

namespace Shuryan.Application.Validators.Configuration.Review
{
    public class CreateLaboratoryReviewRequestValidator : AbstractValidator<CreateLaboratoryReviewRequest>
    {
        public CreateLaboratoryReviewRequestValidator()
        {
            RuleFor(x => x.LabOrderId)
                .NotEmpty()
                .WithMessage("Lab order ID is required");

            RuleFor(x => x.OverallSatisfaction)
                .InclusiveBetween(1, 5)
                .WithMessage("Overall satisfaction must be between 1 and 5");

            RuleFor(x => x.ResultAccuracy)
                .InclusiveBetween(1, 5)
                .WithMessage("Result accuracy rating must be between 1 and 5");

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
