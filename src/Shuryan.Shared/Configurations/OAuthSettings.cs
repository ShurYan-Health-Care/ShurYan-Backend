using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
    public class OAuthSettings
    {
        public GoogleOAuthSettings Google { get; set; } = new();
        // Future: Facebook, Twitter, etc.
    }

    public class GoogleOAuthSettings
    {

        public bool Enabled { get; set; } = false;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = "/api/auth/google-callback";
        public string[] Scopes { get; set; } = new[]
        {
            "openid",
            "profile",
            "email"
        };
    }
}