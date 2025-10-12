using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Appointment;

namespace Shuryan.Application.Validators.Configuration.Appointment
{
    public class UpdateConsultationRecordRequestValidator : AbstractValidator<UpdateConsultationRecordRequest>
    {
        public UpdateConsultationRecordRequestValidator()
        {
            RuleFor(x => x.ChiefComplaint)
                .Length(5, 1000)
                .When(x => !string.IsNullOrEmpty(x.ChiefComplaint))
                .WithMessage("Chief complaint must be between 5 and 1000 characters");

            RuleFor(x => x.HistoryOfPresentIllness)
                .Length(5, 2000)
                .When(x => !string.IsNullOrEmpty(x.HistoryOfPresentIllness))
                .WithMessage("History of present illness must be between 5 and 2000 characters");

            RuleFor(x => x.PhysicalExamination)
                .Length(5, 2000)
                .When(x => !string.IsNullOrEmpty(x.PhysicalExamination))
                .WithMessage("Physical examination must be between 5 and 2000 characters");

            RuleFor(x => x.Diagnosis)
                .Length(3, 1000)
                .When(x => !string.IsNullOrEmpty(x.Diagnosis))
                .WithMessage("Diagnosis must be between 3 and 1000 characters");

            RuleFor(x => x.ManagementPlan)
                .Length(5, 2000)
                .When(x => !string.IsNullOrEmpty(x.ManagementPlan))
                .WithMessage("Management plan must be between 5 and 2000 characters");
        }
    }
}