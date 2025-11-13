using FluentValidation;
using Shuryan.Application.DTOs.Requests.Pharmacy;

namespace Shuryan.Application.Validators.Configuration.Pharmacy
{
    public class UpdatePharmacyWorkingHoursRequestValidator : AbstractValidator<UpdatePharmacyWorkingHoursRequest>
    {
        public UpdatePharmacyWorkingHoursRequestValidator()
        {
            RuleFor(x => x.WeeklySchedule)
                .NotNull()
                .WithMessage("جدول العمل الأسبوعي مطلوب");

            RuleFor(x => x.WeeklySchedule.Saturday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Saturday != null);

            RuleFor(x => x.WeeklySchedule.Sunday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Sunday != null);

            RuleFor(x => x.WeeklySchedule.Monday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Monday != null);

            RuleFor(x => x.WeeklySchedule.Tuesday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Tuesday != null);

            RuleFor(x => x.WeeklySchedule.Wednesday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Wednesday != null);

            RuleFor(x => x.WeeklySchedule.Thursday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Thursday != null);

            RuleFor(x => x.WeeklySchedule.Friday)
                .SetValidator(new DayScheduleDtoValidator()!)
                .When(x => x.WeeklySchedule?.Friday != null);
        }
    }

    public class DayScheduleDtoValidator : AbstractValidator<DayScheduleDto>
    {
        public DayScheduleDtoValidator()
        {
            When(x => x.Enabled, () =>
            {
                RuleFor(x => x.FromTime)
                    .NotEmpty()
                    .WithMessage("وقت البداية مطلوب عندما يكون اليوم مفعل")
                    .Matches(@"^(0?[1-9]|1[0-2])$")
                    .WithMessage("وقت البداية يجب أن يكون بين 1-12");

                RuleFor(x => x.ToTime)
                    .NotEmpty()
                    .WithMessage("وقت النهاية مطلوب عندما يكون اليوم مفعل")
                    .Matches(@"^(0?[1-9]|1[0-2])$")
                    .WithMessage("وقت النهاية يجب أن يكون بين 1-12");

                RuleFor(x => x.FromPeriod)
                    .NotEmpty()
                    .WithMessage("فترة البداية مطلوبة عندما يكون اليوم مفعل")
                    .Must(x => x == "AM" || x == "PM")
                    .WithMessage("فترة البداية يجب أن تكون AM أو PM");

                RuleFor(x => x.ToPeriod)
                    .NotEmpty()
                    .WithMessage("فترة النهاية مطلوبة عندما يكون اليوم مفعل")
                    .Must(x => x == "AM" || x == "PM")
                    .WithMessage("فترة النهاية يجب أن تكون AM أو PM");
            });
        }
    }
}
