namespace Shuryan.Application.Interfaces
{
    public interface IAgoraTokenService
    {
        string GenerateRtcToken(string channelName, string uid, bool isPublisher);
    }
}
