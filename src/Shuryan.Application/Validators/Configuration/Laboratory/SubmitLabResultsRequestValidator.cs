using FluentValidation;
using Shuryan.Application.DTOs.Requests.Laboratory;

namespace Shuryan.Application.Validators.Configuration.Laboratory
{
    public class SubmitLabResultsRequestValidator : AbstractValidator<SubmitLabResultsRequest>
    {
        public SubmitLabResultsRequestValidator()
        {
            RuleFor(x => x.Results)
                .NotNull().WithMessage("Results are required")
                .NotEmpty().WithMessage("At least one result is required");

            RuleForEach(x => x.Results).SetValidator(new LabResultSubmissionDtoValidator());
        }
    }

    public class LabResultSubmissionDtoValidator : AbstractValidator<LabResultSubmissionDto>
    {
        public LabResultSubmissionDtoValidator()
        {
            RuleFor(x => x.LabTestId)
                .NotEmpty().WithMessage("Lab test ID is required");

            RuleFor(x => x.ResultValue)
                .NotEmpty().WithMessage("Result value is required")
                .MaximumLength(500).WithMessage("Result value cannot exceed 500 characters");

            RuleFor(x => x.ReferenceRange)
                .MaximumLength(200).WithMessage("Reference range cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.ReferenceRange));

            RuleFor(x => x.Unit)
                .MaximumLength(50).WithMessage("Unit cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Unit));

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.AttachmentUrl)
                .MaximumLength(500).WithMessage("Attachment URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.AttachmentUrl));
        }
    }
}
