using FluentValidation;
using Shuryan.Application.DTOs.Requests.Review;

namespace Shuryan.Application.Validators.Review
{
    public class CreateDoctorReviewValidator : AbstractValidator<CreateDoctorReviewRequest>
    {
        public CreateDoctorReviewValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty()
                .WithMessage("Appointment ID is required");

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
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Comment));
        }
    }
}
