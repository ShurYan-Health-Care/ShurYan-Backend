using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabPrescriptionRequestValidator : AbstractValidator<CreateLabPrescriptionRequest>
    {
        public CreateLabPrescriptionRequestValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty()
                .WithMessage("Appointment ID is required");

            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .WithMessage("Doctor ID is required");

            RuleFor(x => x.PatientId)
                .NotEmpty()
                .WithMessage("Patient ID is required");

            RuleFor(x => x.GeneralNotes)
                .Length(0, 1000)
                .When(x => !string.IsNullOrEmpty(x.GeneralNotes))
                .WithMessage("General notes cannot exceed 1000 characters");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("At least one lab test item is required");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateLabPrescriptionItemRequestValidator());
        }
    }
}
