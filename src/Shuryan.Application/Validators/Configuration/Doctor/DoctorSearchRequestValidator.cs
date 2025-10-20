using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class DoctorSearchRequestValidator : AbstractValidator<DoctorSearchRequest>
    {
        public DoctorSearchRequestValidator()
        {
            RuleFor(x => x.SearchTerm)
                .Length(2, 100)
                .When(x => !string.IsNullOrEmpty(x.SearchTerm))
                .WithMessage("Search term must be between 2 and 100 characters");

            RuleFor(x => x.MinYearsOfExperience)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(70)
                .When(x => x.MinYearsOfExperience.HasValue)
                .WithMessage("Minimum years of experience must be between 0 and 70");

            RuleFor(x => x.MaxConsultationFee)
                .GreaterThan(0)
                .LessThanOrEqualTo(10000)
                .When(x => x.MaxConsultationFee.HasValue)
                .WithMessage("Maximum consultation fee must be between 0.01 and 10000");

            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5)
                .When(x => x.MinRating.HasValue)
                .WithMessage("Minimum rating must be between 0 and 5");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");
        }
    }
}