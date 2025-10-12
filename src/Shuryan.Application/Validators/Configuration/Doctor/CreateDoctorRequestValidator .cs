using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
    {
        public CreateDoctorRequestValidator()
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

            RuleFor(x => x.MedicalSpecialty)
                .IsInEnum()
                .WithMessage("Invalid medical specialty");

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(70)
                .WithMessage("Years of experience must be between 0 and 70");

            RuleFor(x => x.Biography)
                .Length(0, 5000)
                .When(x => !string.IsNullOrEmpty(x.Biography))
                .WithMessage("Biography cannot exceed 5000 characters");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Today.AddYears(-18))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Doctor must be at least 18 years old");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .When(x => x.Gender.HasValue)
                .WithMessage("Invalid gender");
        }
    }
}