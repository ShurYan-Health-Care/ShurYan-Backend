using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories.Medical
{
    public class TelemedicineSessionRepository : GenericRepository<TelemedicineSession>, ITelemedicineSessionRepository
    {
        public TelemedicineSessionRepository(ShuryanDbContext context) : base(context) { }

        public async Task<TelemedicineSession?> GetByRoomIdAsync(string roomId)
        {
            return await _dbSet
                .Include(ts => ts.Doctor)
                .Include(ts => ts.Patient)
                .Include(ts => ts.Appointment)
                .FirstOrDefaultAsync(ts => ts.RoomId == roomId);
        }

        public async Task<TelemedicineSession?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _dbSet
                .Include(ts => ts.Doctor)
                .Include(ts => ts.Patient)
                .FirstOrDefaultAsync(ts => ts.AppointmentId == appointmentId);
        }

        public async Task<TelemedicineSession?> GetActiveSessionByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(ts => ts.Patient)
                .Include(ts => ts.Appointment)
                .FirstOrDefaultAsync(ts =>
                    ts.DoctorId == doctorId &&
                    (ts.Status == TelemedicineSessionStatus.Waiting || ts.Status == TelemedicineSessionStatus.Active));
        }

        public async Task<TelemedicineSession?> GetActiveSessionByPatientIdAsync(Guid patientId)
        {
            return await _dbSet
                .Include(ts => ts.Doctor)
                .Include(ts => ts.Appointment)
                .FirstOrDefaultAsync(ts =>
                    ts.PatientId == patientId &&
                    (ts.Status == TelemedicineSessionStatus.Waiting || ts.Status == TelemedicineSessionStatus.Active));
        }
    }
}
