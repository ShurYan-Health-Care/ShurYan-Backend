using System;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IVideoSessionRepository : IGenericRepository<VideoSession>
    {
        Task<VideoSession?> GetByAppointmentIdAsync(Guid appointmentId);
        Task<VideoSession?> GetActiveSessionByDoctorIdAsync(Guid doctorId);
        Task<VideoSession?> GetActiveSessionByPatientIdAsync(Guid patientId);
    }
}
