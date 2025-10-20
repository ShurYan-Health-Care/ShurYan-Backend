using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
    /// <summary>
    /// OAuth Provider Settings (Google, Facebook, etc.)
    /// </summary>
    public class OAuthSettings
    {
        public GoogleOAuthSettings Google { get; set; } = new();
        // Future: Facebook, Twitter, etc.
    }

    public class GoogleOAuthSettings
    {
        /// <summary>
        /// Enable/Disable Google OAuth
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Google OAuth Client ID
        /// Get from: https://console.cloud.google.com/
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Google OAuth Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Callback URL after Google authentication
        /// </summary>
        public string CallbackPath { get; set; } = "/api/auth/google-callback";

        /// <summary>
        /// Scopes to request from Google
        /// </summary>
        public string[] Scopes { get; set; } = new[]
        {
            "openid",
            "profile",
            "email"
        };
    }
}