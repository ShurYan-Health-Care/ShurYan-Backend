using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Responses.Emergency;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.Interfaces
{
    public interface IEmergencyAuditService
    {
        Task LogActionAsync(Guid doctorId, Guid patientId, EmergencyActionType action);
        Task<IEnumerable<EmergencyAuditLogResponse>> GetAuditLogsAsync(Guid? doctorId = null, Guid? patientId = null);
    }
}
