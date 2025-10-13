using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Requests.Appointment;

namespace Shuryan.Application.Validators.Configuration.Appointment
{
    public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
    {
        public CreateAppointmentRequestValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEmpty()
                .WithMessage("Patient ID cannot be empty");

            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .WithMessage("Doctor ID cannot be empty");

            RuleFor(x => x.DoctorId)
                .NotEqual(x => x.PatientId)
                .WithMessage("Doctor and patient cannot be the same person");

            RuleFor(x => x.ScheduledStartTime)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Appointment start time must be in the future");

            RuleFor(x => x.ScheduledEndTime)
                .NotEmpty()
                .WithMessage("Appointment end time is required");

            RuleFor(x => x.ScheduledEndTime)
                .GreaterThan(x => x.ScheduledStartTime)
                .WithMessage("Appointment end time must be after start time");

            RuleFor(x => x.ConsultationType)
                .IsInEnum()
                .WithMessage("Invalid consultation type");
        }
    }
}