using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Telemedicine
{
    public class IceConfigResponse
    {
        public IEnumerable<IceServerConfig> IceServers { get; set; } = new List<IceServerConfig>();
    }

    public class IceServerConfig
    {
        public string[] Urls { get; set; } = System.Array.Empty<string>();
        public string? Username { get; set; }
        public string? Credential { get; set; }
    }
}
