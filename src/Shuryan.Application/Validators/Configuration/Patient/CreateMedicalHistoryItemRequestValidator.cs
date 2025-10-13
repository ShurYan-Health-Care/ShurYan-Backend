using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Patient;

namespace Shuryan.Application.Validators.Configuration.Patient
{
    public class CreateMedicalHistoryItemRequestValidator : AbstractValidator<CreateMedicalHistoryItemRequest>
    {
        public CreateMedicalHistoryItemRequestValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid medical history type");

            RuleFor(x => x.Text)
                .NotEmpty()
                .Length(5, 2000)
                .WithMessage("Medical history text must be between 5 and 2000 characters");
        }
    }
}
