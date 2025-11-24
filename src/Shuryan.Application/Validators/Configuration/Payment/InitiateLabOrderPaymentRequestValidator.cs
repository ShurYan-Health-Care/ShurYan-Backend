using FluentValidation;
using Shuryan.Application.DTOs.Requests.Payment;
using Shuryan.Core.Enums.Payment;

namespace Shuryan.Application.Validators.Configuration.Payment
{
    public class InitiateLabOrderPaymentRequestValidator : AbstractValidator<InitiateLabOrderPaymentRequest>
    {
        public InitiateLabOrderPaymentRequestValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithMessage("طريقة الدفع غير صحيحة");

            RuleFor(x => x.PaymentType)
                .IsInEnum()
                .WithMessage("نوع الدفع غير صحيح");

            When(x => x.PaymentMethod != PaymentMethod.CashOnDelivery, () =>
            {
                RuleFor(x => x.PaymentType)
                    .IsInEnum()
                    .WithMessage("نوع الدفع مطلوب للدفع الإلكتروني");
            });
        }
    }
}
