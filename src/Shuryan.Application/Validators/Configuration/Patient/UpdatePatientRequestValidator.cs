using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Patient;

namespace Shuryan.Application.Validators.Configuration.Patient
{
    public class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequest>
    {
        public UpdatePatientRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .Length(2, 50)
                .Matches(@"^[a-zA-Z\s'-]+$")
                .When(x => !string.IsNullOrEmpty(x.FirstName))
                .WithMessage("First name must contain only letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.LastName)
                .Length(2, 50)
                .Matches(@"^[a-zA-Z\s'-]+$")
                .When(x => !string.IsNullOrEmpty(x.LastName))
                .WithMessage("Last name must contain only letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^01[0125][0-9]{8}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must be a valid Egyptian number (11 digits, starts with 010, 011, 012, or 015)");

            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(DateTime.Today.AddYears(-15))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Patient must be at least 15 years old");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .When(x => x.Gender.HasValue)
                .WithMessage("Invalid gender");
        }
    }
}
