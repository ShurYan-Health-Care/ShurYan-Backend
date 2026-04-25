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
                .Matches(@"^01[0125][0-9]{8}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح يتكون من 11 رقم ويبدأ بـ 010 أو 011 أو 012 أو 015");
        }
    }
}
