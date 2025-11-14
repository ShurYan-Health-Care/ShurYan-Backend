using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories.Doctors
{
    public class DoctorConsultationRepository : GenericRepository<DoctorConsultation>, IDoctorConsultationRepository
    {
        public DoctorConsultationRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<DoctorConsultation>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(dc => dc.Doctor)
                .Include(dc => dc.ConsultationType)
                .Where(dc => dc.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<DoctorConsultation?> GetByDoctorIdAndConsultationTypeIdAsync(Guid doctorId, Guid consultationTypeId)
        {
            return await _dbSet
                .Include(dc => dc.ConsultationType)
                .FirstOrDefaultAsync(dc => dc.DoctorId == doctorId && dc.ConsultationTypeId == consultationTypeId);
        }
    }
}

