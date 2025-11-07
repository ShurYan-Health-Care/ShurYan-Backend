using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Prescription;

namespace Shuryan.Application.Validators.Configuration.Prescription
{
    public class UpdatePrescriptionRequestValidator : AbstractValidator<UpdatePrescriptionRequest>
    {
        public UpdatePrescriptionRequestValidator()
        {
            RuleFor(x => x.GeneralInstructions)
                .Length(0, 1000)
                .When(x => !string.IsNullOrEmpty(x.GeneralInstructions))
                .WithMessage("General instructions cannot exceed 1000 characters");

        }
    }
}
