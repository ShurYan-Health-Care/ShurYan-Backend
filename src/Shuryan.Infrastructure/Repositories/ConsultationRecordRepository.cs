using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories
{
    public class ConsultationRecordRepository : GenericRepository<ConsultationRecord>, IConsultationRecordRepository
    {
        private readonly ShuryanDbContext _context;

        public ConsultationRecordRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }
        //هنا هعرض سجل الكشف مع بيانات المريض والطبيب
        public async Task<ConsultationRecord?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _context.ConsultationRecords
                .Include(c => c.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Include(c => c.Appointment)
                    .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);
        }
    }
}