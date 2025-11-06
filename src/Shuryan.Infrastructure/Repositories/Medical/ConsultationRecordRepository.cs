using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Medical
{
    public class ConsultationRecordRepository : GenericRepository<ConsultationRecord>, IConsultationRecordRepository
    {
        public ConsultationRecordRepository(ShuryanDbContext context) : base(context) { }
        public async Task<ConsultationRecord?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _dbSet
                .Include(cr => cr.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(cr => cr.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(cr => cr.AppointmentId == appointmentId);
        }
    }
}

