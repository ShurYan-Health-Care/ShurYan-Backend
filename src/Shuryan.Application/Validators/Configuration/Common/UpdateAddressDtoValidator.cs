using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Common.Address;

namespace Shuryan.Application.Validators.Configuration.Common
{
    public class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressRequest>
    {
        public UpdateAddressDtoValidator()
        {
            RuleFor(x => x.Street)
                .Length(2, 200)
                .When(x => !string.IsNullOrEmpty(x.Street))
                .WithMessage("Street must be between 2 and 200 characters");

            RuleFor(x => x.City)
                .Length(2, 100)
                .When(x => !string.IsNullOrEmpty(x.City))
                .WithMessage("City must be between 2 and 100 characters");


            RuleFor(x => x.BuildingNumber)
                .Length(0, 20)
                .When(x => !string.IsNullOrEmpty(x.BuildingNumber))
                .WithMessage("Building number cannot exceed 20 characters");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("Longitude must be between -180 and 180");
        }
    }
}
