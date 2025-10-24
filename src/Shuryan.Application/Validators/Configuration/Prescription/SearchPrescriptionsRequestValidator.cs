using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;
using System;

namespace Shuryan.Application.Validators.Configuration.Prescription
{
    public class SearchPrescriptionsRequestValidator : AbstractValidator<SearchPrescriptionsRequest>
    {
        public SearchPrescriptionsRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.PrescriptionNumber)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.PrescriptionNumber))
                .WithMessage("Prescription number cannot exceed 100 characters");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.SearchTerm))
                .WithMessage("Search term cannot exceed 200 characters");

            RuleFor(x => x)
                .Must(x => x.StartDate == null || x.EndDate == null || x.StartDate <= x.EndDate)
                .WithMessage("Start date must be less than or equal to end date");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .When(x => x.StartDate.HasValue)
                .WithMessage("Start date cannot be in the future");

            RuleFor(x => x.EndDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .When(x => x.EndDate.HasValue)
                .WithMessage("End date cannot be in the future");
        }
    }
}
