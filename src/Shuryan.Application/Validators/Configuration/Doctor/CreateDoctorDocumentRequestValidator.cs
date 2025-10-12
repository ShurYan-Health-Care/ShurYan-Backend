using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Doctor;

namespace Shuryan.Application.Validators.Configuration.Doctor
{
    public class CreateDoctorDocumentRequestValidator : AbstractValidator<CreateDoctorDocumentRequest>
    {
        public CreateDoctorDocumentRequestValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid document type");
        }
    }
}
