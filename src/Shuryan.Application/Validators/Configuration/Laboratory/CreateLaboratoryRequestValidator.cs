using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLaboratoryRequestValidator : AbstractValidator<CreateLaboratoryRequest>
    {
        public CreateLaboratoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(3, 200)
                .WithMessage("Laboratory name must be between 3 and 200 characters");

            RuleFor(x => x.Description)
                .Length(0, 2000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.WhatsAppNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrEmpty(x.WhatsAppNumber))
                .WithMessage("WhatsApp number must be in valid E.164 format");

            RuleFor(x => x.HomeSampleCollectionFee)
                .GreaterThan(0)
                .LessThanOrEqualTo(10000)
                .When(x => x.HomeSampleCollectionFee.HasValue && x.OffersHomeSampleCollection)
                .WithMessage("Home sample collection fee must be between 0 and 10000");
        }
    }
}
