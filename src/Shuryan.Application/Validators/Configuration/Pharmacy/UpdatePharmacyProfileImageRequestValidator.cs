using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class UpdatePharmacyProfileImageRequestValidator : AbstractValidator<UpdatePharmacyProfileImageRequest>
    {
        public UpdatePharmacyProfileImageRequestValidator()
        {
            RuleFor(x => x.ProfileImage)
                .NotNull()
                .WithMessage("يجب تحميل صورة");
        }
    }
}
