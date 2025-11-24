namespace Shuryan.Core.Settings
{
    public class FrontendSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string PaymentSuccessUrl { get; set; } = "/payment/success";
        public string PaymentFailedUrl { get; set; } = "/payment/failed";
    }
}
