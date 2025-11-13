using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class UpdatePharmacyBasicInfoRequestValidator : AbstractValidator<UpdatePharmacyBasicInfoRequest>
    {
        public UpdatePharmacyBasicInfoRequestValidator()
        {
            RuleFor(x => x.Name)
                .Length(2, 100)
                .When(x => !string.IsNullOrWhiteSpace(x.Name))
                .WithMessage("اسم الصيدلية يجب أن يكون بين 2-100 حرف");

            RuleFor(x => x.PhoneNumber)
                .Length(10, 20)
                .Matches(@"^[\d\s\+\-\(\)]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("رقم الهاتف يجب أن يكون بين 10-20 رقم وبصيغة صحيحة");
        }
    }
}
