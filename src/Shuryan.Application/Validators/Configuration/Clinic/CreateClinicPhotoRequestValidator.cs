using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Clinic;

namespace Shuryan.Application.Validators.Configuration.Clinic
{
    public class CreateClinicPhotoRequestValidator : AbstractValidator<CreateClinicPhotoRequest>
    {
        public CreateClinicPhotoRequestValidator()
        {
            RuleFor(x => x.Caption)
                .Length(0, 500)
                .When(x => !string.IsNullOrEmpty(x.Caption))
                .WithMessage("Caption cannot exceed 500 characters");
        }

    }
}