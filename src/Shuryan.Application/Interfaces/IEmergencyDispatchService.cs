using System;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Requests.Emergency;

namespace Shuryan.Application.Interfaces
{
    public interface IEmergencyDispatchService
    {
        Task<bool> DispatchSosAsync(Guid patientId, SosDispatchRequest request);
    }
}
