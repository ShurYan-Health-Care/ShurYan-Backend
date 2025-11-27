using FluentValidation;
using Shuryan.Application.DTOs.Requests.Auth;

namespace Shuryan.Application.Validators.Configuration.Auth
{
    /// <summary>
    /// Validator for Google OAuth Login/Registration requests
    /// </summary>
    public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
    {
        public GoogleLoginRequestValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty()
                .WithMessage("Google ID token is required")
                .MinimumLength(100)
                .WithMessage("Invalid Google ID token format");

            RuleFor(x => x.UserType)
                .Must(BeValidUserType)
                .When(x => !string.IsNullOrEmpty(x.UserType))
                .WithMessage("User type must be one of: patient, doctor, pharmacy, laboratory");
        }

        private bool BeValidUserType(string? userType)
        {
            if (string.IsNullOrEmpty(userType))
                return true;

            var validTypes = new[] { "patient", "doctor", "pharmacy", "laboratory" };
            return validTypes.Contains(userType.ToLower());
        }
    }
}
