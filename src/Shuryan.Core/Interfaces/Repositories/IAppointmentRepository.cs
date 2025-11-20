using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IAppointmentRepository : IGenericRepository<Appointment>
	{
		Task<Appointment?> GetByIdWithDetailsAsync(Guid id);
		Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId);
		Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId);
		Task<IEnumerable<Appointment>> GetByDoctorIdAndDateAsync(Guid doctorId, DateTime date);
		Task<IEnumerable<Appointment>> GetByDoctorIdAndDateRangeAsync(Guid doctorId, DateTime startDate, DateTime endDate, List<AppointmentStatus>? statuses = null);
		Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid userId, bool isDoctor);
		Task<IEnumerable<Appointment>> GetPastAppointmentsAsync(Guid userId, bool isDoctor);
		Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status);
		Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null);
		Task<int> GetCompletedAppointmentsCountAsync(Guid doctorId);

		// Dashboard Statistics Methods
		Task<int> GetUniquePatientsCountAsync(Guid doctorId);
		Task<int> GetTodayAppointmentsCountAsync(Guid doctorId);
		Task<decimal> GetTotalRevenueAsync(Guid doctorId);
		Task<decimal> GetMonthlyRevenueAsync(Guid doctorId, int year, int month);
		Task<int> GetPendingAppointmentsCountAsync(Guid doctorId);
		Task<int> GetCancelledAppointmentsCountAsync(Guid doctorId);

		// Session Management Methods
		Task<Appointment?> GetDoctorActiveAppointmentAsync(Guid doctorId, Guid? excludeAppointmentId = null);

		// Booking System Methods
		Task<IEnumerable<Appointment>> GetBookedAppointmentsForDateAsync(Guid doctorId, DateTime startOfDay, DateTime endOfDay);

		// Paginated Query Methods
		Task<(IEnumerable<Appointment> Appointments, int TotalCount)> GetByDoctorIdWithFiltersAsync(
			Guid doctorId,
			DateTime? startDate,
			DateTime? endDate,
			AppointmentStatus? status,
			int pageNumber,
			int pageSize,
			string sortBy,
			string sortOrder);
		
		// Statistics Methods
		Task<Dictionary<AppointmentStatus, int>> GetAppointmentStatisticsByDoctorIdAsync(
			Guid doctorId,
			DateTime? startDate,
			DateTime? endDate);
	}
}
