using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabOrderRequestValidator : AbstractValidator<CreateLabOrderRequest>
    {
        public CreateLabOrderRequestValidator()
        {
            RuleFor(x => x.LabPrescriptionId)
                .NotEmpty()
                .WithMessage("Lab prescription ID is required");

            RuleFor(x => x.LaboratoryId)
                .NotEmpty()
                .WithMessage("Laboratory ID is required");

            RuleFor(x => x.SampleCollectionType)
                .IsInEnum()
                .WithMessage("Invalid sample collection type");
        }
    }
}
