namespace Shuryan.Core.Settings
{
    public class TelemedicineSettings
    {
        public string[] StunServers { get; set; } = new[] { "stun:stun.l.google.com:19302" };
        public string TurnServer { get; set; } = string.Empty;
        public string TurnUsername { get; set; } = string.Empty;
        public string TurnCredential { get; set; } = string.Empty;
        public int SessionTimeoutMinutes { get; set; } = 60;
    }
}
