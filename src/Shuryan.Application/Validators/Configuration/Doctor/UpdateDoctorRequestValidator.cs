using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
    {
        public UpdateDoctorRequestValidator()
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

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(70)
                .When(x => x.YearsOfExperience.HasValue)
                .WithMessage("Years of experience must be between 0 and 70");

            RuleFor(x => x.Biography)
                .Length(0, 5000)
                .When(x => !string.IsNullOrEmpty(x.Biography))
                .WithMessage("Biography cannot exceed 5000 characters");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Today.AddYears(-18))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Doctor must be at least 18 years old");

            RuleFor(x => x.MedicalSpecialty)
                .IsInEnum()
                .When(x => x.MedicalSpecialty.HasValue)
                .WithMessage("Invalid medical specialty");
        }

    }
}
