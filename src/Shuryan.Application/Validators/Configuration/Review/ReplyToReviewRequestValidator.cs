using FluentValidation;
using Shuryan.Application.DTOs.Requests.Review;

namespace Shuryan.Application.Validators.Configuration.Review
{
    public class ReplyToReviewRequestValidator : AbstractValidator<ReplyToReviewRequest>
    {
        public ReplyToReviewRequestValidator()
        {
            RuleFor(x => x.Reply)
                .NotEmpty().WithMessage("الرد مطلوب")
                .MinimumLength(3).WithMessage("الرد يجب أن يكون على الأقل 3 أحرف")
                .MaximumLength(300).WithMessage("الرد يجب ألا يتجاوز 300 حرف");
        }
    }
}

