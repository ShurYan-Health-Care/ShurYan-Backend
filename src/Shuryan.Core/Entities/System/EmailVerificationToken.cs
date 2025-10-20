using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.System
{
    public class EmailVerificationToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
