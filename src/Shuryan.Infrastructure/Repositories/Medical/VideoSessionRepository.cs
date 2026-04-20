using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories.Medical
{
    public class VideoSessionRepository : GenericRepository<VideoSession>, IVideoSessionRepository
    {
        public VideoSessionRepository(ShuryanDbContext context) : base(context) { }

        public async Task<VideoSession?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _dbSet
                .Include(vs => vs.Doctor)
                .Include(vs => vs.Patient)
                .FirstOrDefaultAsync(vs => vs.AppointmentId == appointmentId);
        }

        public async Task<VideoSession?> GetActiveSessionByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(vs => vs.Patient)
                .Include(vs => vs.Appointment)
                .FirstOrDefaultAsync(vs =>
                    vs.DoctorId == doctorId &&
                    (vs.Status == VideoSessionStatus.Waiting || vs.Status == VideoSessionStatus.Active));
        }

        public async Task<VideoSession?> GetActiveSessionByPatientIdAsync(Guid patientId)
        {
            return await _dbSet
                .Include(vs => vs.Doctor)
                .Include(vs => vs.Appointment)
                .FirstOrDefaultAsync(vs =>
                    vs.PatientId == patientId &&
                    (vs.Status == VideoSessionStatus.Waiting || vs.Status == VideoSessionStatus.Active));
        }
    }
}
