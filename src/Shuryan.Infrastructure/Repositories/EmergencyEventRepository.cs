using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories
{
    public class EmergencyEventRepository : GenericRepository<EmergencyEvent>, IEmergencyEventRepository
    {
        public EmergencyEventRepository(ShuryanDbContext context) : base(context)
        {
        }
    }
}
