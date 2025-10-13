using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Verifier;

namespace Shuryan.Application.Validators.Configuration.Verifier
{
    public class UpdateVerifierRequestValidator : AbstractValidator<UpdateVerifierRequest>
    {
        public UpdateVerifierRequestValidator()
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
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must be in valid E.164 format");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Today.AddYears(-18))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Verifier must be at least 18 years old");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .When(x => x.Gender.HasValue)
                .WithMessage("Invalid gender");
        }
    }
}