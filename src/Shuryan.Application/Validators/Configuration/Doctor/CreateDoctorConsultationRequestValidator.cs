using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class CreateDoctorConsultationRequestValidator : AbstractValidator<CreateDoctorConsultationRequest>
    {
        public CreateDoctorConsultationRequestValidator()
        {
            RuleFor(x => x.ConsultationType)
                .IsInEnum()
                .WithMessage("Invalid consultation type");

            RuleFor(x => x.ConsultationFee)
                .GreaterThan(0)
                .LessThanOrEqualTo(10000)
                .WithMessage("Consultation fee must be between 0.01 and 10000");

            RuleFor(x => x.SessionDurationMinutes)
                .GreaterThanOrEqualTo(15)
                .LessThanOrEqualTo(480)
                .Must(duration => duration % 5 == 0)
                .WithMessage("Session duration must be between 15-480 minutes and a multiple of 5");
        }
    }
}