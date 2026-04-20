using System;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.VideoCall;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.Interfaces
{
    public interface IVideoSessionService
    {
        Task<ApiResponse<VideoSessionResponse>> JoinSessionAsync(Guid appointmentId, Guid userId);
        Task<ApiResponse<VideoSessionResponse>> GetSessionAsync(Guid appointmentId, Guid userId);
        Task<ApiResponse<bool>> EndSessionAsync(Guid appointmentId, Guid userId, VideoSessionEndReason reason);
    }
}
