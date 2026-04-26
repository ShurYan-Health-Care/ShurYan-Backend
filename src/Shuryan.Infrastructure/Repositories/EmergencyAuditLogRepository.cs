using Shuryan.Core.Entities.System;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories
{
    public class EmergencyAuditLogRepository : GenericRepository<EmergencyAuditLog>, IEmergencyAuditLogRepository
    {
        public EmergencyAuditLogRepository(ShuryanDbContext context) : base(context)
        {
        }
    }
}
