using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLabResultRequestValidator : AbstractValidator<CreateLabResultRequest>
    {
        public CreateLabResultRequestValidator()
        {
            RuleFor(x => x.LabOrderId)
                .NotEmpty()
                .WithMessage("Lab order ID is required");

            RuleFor(x => x.LabTestId)
                .NotEmpty()
                .WithMessage("Lab test ID is required");

            RuleFor(x => x.ResultValue)
                .NotEmpty()
                .Length(1, 500)
                .WithMessage("Result value must be between 1 and 500 characters");

            RuleFor(x => x.ReferenceRange)
                .Length(0, 200)
                .When(x => !string.IsNullOrEmpty(x.ReferenceRange))
                .WithMessage("Reference range cannot exceed 200 characters");

            RuleFor(x => x.Unit)
                .Length(0, 50)
                .When(x => !string.IsNullOrEmpty(x.Unit))
                .WithMessage("Unit cannot exceed 50 characters");

            RuleFor(x => x.Notes)
                .Length(0, 1000)
                .When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Notes cannot exceed 1000 characters");
        }
    }
}
