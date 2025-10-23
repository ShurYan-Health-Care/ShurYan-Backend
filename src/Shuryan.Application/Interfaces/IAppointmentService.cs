using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;

namespace Shuryan.Application.Interfaces
{
    public interface IAppointmentService
    {
        #region Basic CRUD Operations 
        Task<AppointmentResponse?> GetAppointmentByIdAsync(Guid id);
        Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request);
        Task<AppointmentResponse> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request);
        Task<bool> DeleteAppointmentAsync(Guid id);
        #endregion
        
        #region Appointment Management 
        Task<AppointmentResponse> CancelAppointmentAsync(Guid id, CancelAppointmentRequest request);
        Task<AppointmentResponse> RescheduleAppointmentAsync(Guid id, RescheduleAppointmentRequest request);
        Task<AppointmentResponse> ConfirmAppointmentAsync(Guid id);
        Task<AppointmentResponse> CompleteAppointmentAsync(Guid id);
        #endregion

        #region Query Operations 
        Task<IEnumerable<AppointmentResponse>> GetAppointmentsByPatientIdAsync(Guid patientId);
        Task<IEnumerable<AppointmentResponse>> GetAppointmentsByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<AppointmentResponse>> GetUpcomingAppointmentsAsync(Guid userId, string userRole);
        Task<IEnumerable<AppointmentResponse>> GetPastAppointmentsAsync(Guid userId, string userRole);
        Task<IEnumerable<AppointmentResponse>> GetAppointmentsByStatusAsync(Core.Enums.Appointments.AppointmentStatus status);
        Task<IEnumerable<AppointmentResponse>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> IsTimeSlotAvailableAsync(Guid doctorId, DateTime startTime, DateTime endTime);
        Task<int> GetAppointmentsCountAsync(Guid userId, string userRole);
        #endregion
    }
}
