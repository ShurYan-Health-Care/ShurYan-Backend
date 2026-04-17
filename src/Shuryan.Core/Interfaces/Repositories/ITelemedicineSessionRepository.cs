using System;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface ITelemedicineSessionRepository : IGenericRepository<TelemedicineSession>
    {
        Task<TelemedicineSession?> GetByRoomIdAsync(string roomId);
        Task<TelemedicineSession?> GetByAppointmentIdAsync(Guid appointmentId);
        Task<TelemedicineSession?> GetActiveSessionByDoctorIdAsync(Guid doctorId);
        Task<TelemedicineSession?> GetActiveSessionByPatientIdAsync(Guid patientId);
    }
}
