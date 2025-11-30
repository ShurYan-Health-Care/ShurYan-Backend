using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Verifier;

namespace Shuryan.Application.Validators.Configuration.Verifier
{
    public class CreateVerifierRequestValidator : AbstractValidator<CreateVerifierRequest>
    {
        public CreateVerifierRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .Length(2, 50)
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("First name must contain only letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .Length(2, 50)
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("Last name must contain only letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Length(5, 255)
                .WithMessage("Email must be valid and between 5 and 255 characters");

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

            //RuleFor(x => x.CreatedByAdminId)
            //    .NotEmpty()
            //    .WithMessage("Created by admin ID is required");
        }
    }
}
