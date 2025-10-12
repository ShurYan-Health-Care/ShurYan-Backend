using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Patient;

namespace Shuryan.Application.Validators.Configuration.Patient
{
    public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
    {
        public CreatePatientRequestValidator()
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
                .LessThan(DateTime.Today)
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Birth date must be in the past");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .When(x => x.Gender.HasValue)
                .WithMessage("Invalid gender");
        }
    }
}
