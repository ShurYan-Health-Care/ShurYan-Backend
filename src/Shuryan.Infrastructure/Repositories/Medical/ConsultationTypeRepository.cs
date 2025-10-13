using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories.Medical
{
    public class ConsultationTypeRepository : GenericRepository<ConsultationType>, IConsultationTypeRepository
    {
        public ConsultationTypeRepository(ShuryanDbContext context) : base(context) { }

        public async Task<ConsultationType?> GetByEnumAsync(ConsultationTypeEnum consultationType)
        {
            return await _dbSet
                .Include(ct => ct.Consultations)
                    .ThenInclude(dc => dc.Doctor)
                .FirstOrDefaultAsync(ct => ct.ConsultationTypeEnum == consultationType);
        }
    }
}

