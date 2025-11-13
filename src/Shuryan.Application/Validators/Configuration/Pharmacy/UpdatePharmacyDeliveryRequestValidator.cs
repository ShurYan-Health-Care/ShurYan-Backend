using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class UpdatePharmacyDeliveryRequestValidator : AbstractValidator<UpdatePharmacyDeliveryRequest>
    {
        public UpdatePharmacyDeliveryRequestValidator()
        {
            RuleFor(x => x.DeliveryFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("سعر التوصيل يجب أن يكون 0 أو أكثر")
                .LessThanOrEqualTo(1000)
                .WithMessage("سعر التوصيل يجب ألا يتجاوز 1000");
        }
    }
}
