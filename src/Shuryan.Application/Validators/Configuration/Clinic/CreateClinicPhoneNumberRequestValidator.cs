using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Clinic;

namespace Shuryan.Application.Validators.Configuration.Clinic
{
    public class CreateClinicPhoneNumberRequestValidator : AbstractValidator<CreateClinicPhoneNumberRequest>
    {
        public CreateClinicPhoneNumberRequestValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                .Length(10, 20)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Phone number must be in valid E.164 format");

            RuleFor(x => x.Type)
                .NotEmpty()
                .Length(2, 50)
                .WithMessage("Phone type must be between 2 and 50 characters");
        }
    }
}