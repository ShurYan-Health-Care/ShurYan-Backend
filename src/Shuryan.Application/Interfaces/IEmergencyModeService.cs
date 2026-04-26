using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Responses.Emergency;

namespace Shuryan.Application.Interfaces
{
    public interface IEmergencyModeService
    {
        Task<bool> ActivateEmergencyModeAsync(Guid doctorId, Guid appointmentId);
        Task<bool> DeactivateEmergencyModeAsync(Guid doctorId, Guid patientId);
        Task<IEnumerable<EmergencyPatientResponse>> GetDoctorEmergencyPatientsAsync(Guid doctorId);
        Task<IEnumerable<EmergencyEventResponse>> GetDoctorSosEventsAsync(Guid doctorId);
        Task<bool> ResolveEmergencyEventAsync(Guid doctorId, Guid eventId);
    }
}
