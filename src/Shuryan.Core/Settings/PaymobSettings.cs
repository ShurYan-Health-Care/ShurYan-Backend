namespace Shuryan.Core.Settings
{
    public class PaymobSettings
    {
        public string APIKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string HMAC { get; set; } = string.Empty;
        public string CardIntegrationId { get; set; } = string.Empty;
        public string MobileIntegrationId { get; set; } = string.Empty;
        public string CardIFrameId { get; set; } = string.Empty;
        public string MobileIFrameId { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://accept.paymob.com";
        public string IFrameUrl { get; set; } = "https://accept.paymob.com/api/acceptance/iframes";
        public int TimeoutSeconds { get; set; } = 30;
    }
}
