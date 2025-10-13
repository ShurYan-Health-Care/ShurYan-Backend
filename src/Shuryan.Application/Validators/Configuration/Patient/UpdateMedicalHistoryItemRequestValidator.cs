using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Patient;

namespace Shuryan.Application.Validators.Configuration.Patient
{
    public class UpdateMedicalHistoryItemRequestValidator : AbstractValidator<UpdateMedicalHistoryItemRequest>
    {
        public UpdateMedicalHistoryItemRequestValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .When(x => x.Type.HasValue)
                .WithMessage("Invalid medical history type");

            RuleFor(x => x.Text)
                .Length(5, 2000)
                .When(x => !string.IsNullOrEmpty(x.Text))
                .WithMessage("Medical history text must be between 5 and 2000 characters");
        }
    }
}