using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Review;

namespace Shuryan.Application.Validators.Configuration.Review
{
    public class UpdateDoctorReviewRequestValidator : AbstractValidator<UpdateDoctorReviewRequest>
    {
        public UpdateDoctorReviewRequestValidator()
        {
            RuleFor(x => x.OverallSatisfaction)
                .InclusiveBetween(1, 5)
                .WithMessage("Overall satisfaction must be between 1 and 5");

            RuleFor(x => x.WaitingTime)
                .InclusiveBetween(1, 5)
                .WithMessage("Waiting time rating must be between 1 and 5");

            RuleFor(x => x.CommunicationQuality)
                .InclusiveBetween(1, 5)
                .WithMessage("Communication quality rating must be between 1 and 5");

            RuleFor(x => x.ClinicCleanliness)
                .InclusiveBetween(1, 5)
                .WithMessage("Clinic cleanliness rating must be between 1 and 5");

            RuleFor(x => x.ValueForMoney)
                .InclusiveBetween(1, 5)
                .WithMessage("Value for money rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .Length(0, 2000)
                .When(x => !string.IsNullOrEmpty(x.Comment))
                .WithMessage("Comment cannot exceed 2000 characters");
        }
    }
}
