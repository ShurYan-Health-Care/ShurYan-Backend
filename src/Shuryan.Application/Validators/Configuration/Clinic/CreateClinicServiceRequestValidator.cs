using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Clinic;

namespace Shuryan.Application.Validators.Configuration.Clinic
{
    public class CreateClinicServiceRequestValidator : AbstractValidator<CreateClinicServiceRequest>
    {
        public CreateClinicServiceRequestValidator()
        {
            RuleFor(x => x.ServiceName)
                .NotEmpty()
                .Length(3, 200)
                .WithMessage("Service name must be between 3 and 200 characters");

            RuleFor(x => x.Description)
                .Length(0, 1000)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description cannot exceed 1000 characters");
        }
    }
}