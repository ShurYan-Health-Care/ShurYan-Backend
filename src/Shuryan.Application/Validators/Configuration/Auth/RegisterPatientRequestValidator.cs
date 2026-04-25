using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Auth;

namespace Shuryan.Application.Validators.Configuration.Auth
{
    public class RegisterPatientRequestValidator : AbstractValidator<RegisterPatientRequest>
    {
        public RegisterPatientRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("الاسم الأول مطلوب")
                .Length(2, 50).WithMessage("الاسم الأول يجب أن يكون بين 2 و 50 حرفاً")
                .Matches(@"^[a-zA-Z\s؀-ۿ]+$").WithMessage("الاسم الأول يجب أن يحتوي على حروف فقط");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("الاسم الأخير مطلوب")
                .Length(2, 50).WithMessage("الاسم الأخير يجب أن يكون بين 2 و 50 حرفاً")
                .Matches(@"^[a-zA-Z\s؀-ۿ]+$").WithMessage("الاسم الأخير يجب أن يحتوي على حروف فقط");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
                .MaximumLength(255).WithMessage("البريد الإلكتروني لا يمكن أن يتجاوز 255 حرفاً");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل")
                .Matches(@"[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل")
                .Matches(@"[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل")
                .Matches(@"[0-9]").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل")
                .Matches(@"[\W_]").WithMessage("كلمة المرور يجب أن تحتوي على رمز خاص واحد على الأقل");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تأكيد كلمة المرور مطلوب")
                .Equal(x => x.Password).WithMessage("كلمتا المرور غير متطابقتين");
        }
    }
}
