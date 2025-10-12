using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Appointment;

namespace Shuryan.Application.Validators.Configuration.Appointment
{
    public class CreateConsultationRecordRequestValidator : AbstractValidator<CreateConsultationRecordRequest>
    {
        public CreateConsultationRecordRequestValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty()
                .WithMessage("Appointment ID is required");

            RuleFor(x => x.ChiefComplaint)
                .NotEmpty()
                .Length(5, 1000)
                .WithMessage("Chief complaint must be between 5 and 1000 characters");

            RuleFor(x => x.HistoryOfPresentIllness)
                .NotEmpty()
                .Length(5, 2000)
                .WithMessage("History of present illness must be between 5 and 2000 characters");

            RuleFor(x => x.PhysicalExamination)
                .NotEmpty()
                .Length(5, 2000)
                .WithMessage("Physical examination must be between 5 and 2000 characters");

            RuleFor(x => x.Diagnosis)
                .NotEmpty()
                .Length(3, 1000)
                .WithMessage("Diagnosis must be between 3 and 1000 characters");

            RuleFor(x => x.ManagementPlan)
                .NotEmpty()
                .Length(5, 2000)
                .WithMessage("Management plan must be between 5 and 2000 characters");
        }
    }
}
