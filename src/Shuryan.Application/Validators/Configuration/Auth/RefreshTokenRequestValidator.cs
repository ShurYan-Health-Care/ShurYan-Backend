using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shuryan.Application.DTOs.Requests.Auth;

namespace Shuryan.Application.Validators.Configuration.Auth
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public  RefreshTokenRequestValidator()
        {

        }
    }
}
