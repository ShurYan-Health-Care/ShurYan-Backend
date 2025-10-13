using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.Validators.Configuration.Common;

namespace Shuryan.Application.Validators.Configuration.Clinic
{
    public class CreateClinicRequestValidator : AbstractValidator<CreateClinicRequest>
    {
        public CreateClinicRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(3, 200)
                .WithMessage("Clinic name must be between 3 and 200 characters");

            RuleFor(x => x.Address)
                .NotNull()
                .WithMessage("Address is required")
                .SetValidator(new CreateAddressDtoValidator());

            RuleFor(x => x.PhoneNumbers)
                .Must(x => x != null && x.Any())
                .WithMessage("At least one phone number is required");

            RuleForEach(x => x.PhoneNumbers)
                .NotEmpty()
                .Length(10, 20)
                .WithMessage("Each phone number must be between 10 and 20 characters");

            RuleFor(x => x.Services)
                .Must(x => x == null || !x.Any() || x.Count() <= 50)
                .WithMessage("Cannot have more than 50 services");
        }
    }
}