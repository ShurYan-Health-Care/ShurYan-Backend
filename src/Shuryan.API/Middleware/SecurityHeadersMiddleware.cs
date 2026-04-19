namespace Shuryan.API.Middleware
{
    /// <summary>
    /// Adds security headers to all HTTP responses to protect against common web vulnerabilities:
    /// - Clickjacking (X-Frame-Options)
    /// - MIME sniffing (X-Content-Type-Options)
    /// - Referrer leakage (Referrer-Policy)
    /// - Feature abuse (Permissions-Policy)
    /// - HTTPS downgrade (Strict-Transport-Security, production only)
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;

        public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent clickjacking
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Prevent MIME-type sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Modern recommendation: disable browser XSS filter (it can cause vulnerabilities)
            context.Response.Headers["X-XSS-Protection"] = "0";

            // Control referrer information
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Restrict browser features
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            // HSTS — only in production (browsers remember this setting)
            if (!_environment.IsDevelopment())
            {
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            await _next(context);
        }
    }
}
