using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Configuration.Prescription
{
    public class CreatePrescriptionRequestValidator : AbstractValidator<CreatePrescriptionRequest>
    {
        public CreatePrescriptionRequestValidator()
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

            RuleFor(x => x.PrescriptionNumber)
                .NotEmpty()
                .Length(3, 100)
                .Matches(@"^[A-Z0-9\-]+$")
                .WithMessage("Prescription number must be uppercase letters, numbers, or hyphens (3-100 characters)");

            RuleFor(x => x.DigitalSignature)
                .NotEmpty()
                .Length(10, 1000)
                .WithMessage("Digital signature must be between 10 and 1000 characters");

            RuleFor(x => x.GeneralInstructions)
                .Length(0, 1000)
                .When(x => !string.IsNullOrEmpty(x.GeneralInstructions))
                .WithMessage("General instructions cannot exceed 1000 characters");

            RuleFor(x => x.PrescribedMedications)
                .NotEmpty()
                .WithMessage("At least one prescribed medication is required");

            RuleForEach(x => x.PrescribedMedications)
                .SetValidator(new CreatePrescribedMedicationRequestValidator());
        }
    }
}