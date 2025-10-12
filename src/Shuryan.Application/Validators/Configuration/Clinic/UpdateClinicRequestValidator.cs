using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.Validators.Configuration.Common;

namespace Shuryan.Application.Validators.Configuration.Clinic
{
    public class UpdateClinicRequestValidator : AbstractValidator<UpdateClinicRequest>
    {
        public UpdateClinicRequestValidator()
        {
            RuleFor(x => x.Name)
                .Length(3, 200)
                .When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Clinic name must be between 3 and 200 characters");


            RuleFor(x => x.Address)
                .SetValidator(new UpdateAddressDtoValidator()!)
                .When(x => x.Address != null);
        }
    }
}
