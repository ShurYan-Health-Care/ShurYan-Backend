using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class UpdatePharmacyRequestValidator : AbstractValidator<UpdatePharmacyRequest>
    {
        public UpdatePharmacyRequestValidator()
        {
            RuleFor(x => x.Name)
                .Length(3, 200)
                .When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Pharmacy name must be between 3 and 200 characters");

            RuleFor(x => x.Description)
                .Length(0, 2000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.WhatsAppNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrEmpty(x.WhatsAppNumber))
                .WithMessage("WhatsApp number must be in valid E.164 format");
        }
    }
}
