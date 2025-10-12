using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class CreateLaboratoryDocumentRequestValidator : AbstractValidator<CreateLaboratoryDocumentRequest>
    {
        public CreateLaboratoryDocumentRequestValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid document type");
        }
    }
}
