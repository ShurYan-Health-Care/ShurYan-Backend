using System;

namespace Shuryan.Application.DTOs.Responses.Session
{
    /// <summary>
    /// Response لإنهاء الجلسة
    /// </summary>
    public class EndSessionResponse
    {
        public Guid SessionId { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = null!;
    }
}
