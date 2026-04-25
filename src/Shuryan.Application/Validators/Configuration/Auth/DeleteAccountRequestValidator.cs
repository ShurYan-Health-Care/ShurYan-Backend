using FluentValidation;
using Shuryan.Application.DTOs.Requests.Auth;

namespace Shuryan.Application.Validators.Configuration.Auth
{
    public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
    {
        public DeleteAccountRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress()
                .WithMessage("صيغة البريد الإلكتروني غير صحيحة");

            RuleFor(x => x.ConfirmationText)
                .NotEmpty()
                .WithMessage("نص التأكيد مطلوب")
                .Must(x => x?.Trim().ToUpper() == "DELETE")
                .WithMessage("يجب كتابة 'DELETE' لتأكيد حذف الحساب");
        }
    }
}
