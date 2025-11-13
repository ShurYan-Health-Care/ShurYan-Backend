using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class UpdatePharmacyAddressRequestValidator : AbstractValidator<UpdatePharmacyAddressRequest>
    {
        public UpdatePharmacyAddressRequestValidator()
        {
            RuleFor(x => x.City)
                .Length(2, 100)
                .When(x => !string.IsNullOrWhiteSpace(x.City))
                .WithMessage("اسم المدينة يجب أن يكون بين 2-100 حرف");

            RuleFor(x => x.Street)
                .Length(2, 200)
                .When(x => !string.IsNullOrWhiteSpace(x.Street))
                .WithMessage("اسم الشارع يجب أن يكون بين 2-200 حرف");

            RuleFor(x => x.BuildingNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.BuildingNumber))
                .WithMessage("رقم المبنى يجب ألا يتجاوز 20 حرف");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("خط العرض يجب أن يكون بين -90 و 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("خط الطول يجب أن يكون بين -180 و 180");
        }
    }
}
