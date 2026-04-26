using Shuryan.Core.Entities.System;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IEmergencyAuditLogRepository : IGenericRepository<EmergencyAuditLog>
    {
        // Explicitly overriding to hide/disable Delete and Update at the repository interface level,
        // though typically they still exist on IGenericRepository. The service layer handles enforcement.
    }
}
