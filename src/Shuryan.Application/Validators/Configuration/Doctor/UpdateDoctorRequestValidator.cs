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
                .Matches(@"^01[0125][0-9]{8}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must be a valid Egyptian number (11 digits, starts with 010, 011, 012, or 015)");

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
                .LessThanOrEqualTo(DateTime.Today.AddYears(-15))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Doctor must be at least 15 years old");

            RuleFor(x => x.MedicalSpecialty)
                .IsInEnum()
                .When(x => x.MedicalSpecialty.HasValue)
                .WithMessage("Invalid medical specialty");
        }

    }
}
