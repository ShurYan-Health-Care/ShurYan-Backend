namespace Shuryan.Core.Settings
{
    public class AgoraSettings
    {
        public string AppId { get; set; } = string.Empty;
        public string AppCertificate { get; set; } = string.Empty;
        public int TokenExpirySeconds { get; set; } = 3600;
    }
}
