using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.System;

namespace Shuryan.Core.Interfaces.Repositories
{
   public interface IEmailServiceRepository
   {
       Task AddTokenAsync(EmailVerification token);
       Task<EmailVerification?> GetTokenByTokenAsync(string token);
       Task RemoveTokenAsync(EmailVerification token);
       Task SaveChangesAsync();
   }
}
