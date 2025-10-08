using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IAppointmentRepository : IGenericRepository<Appointment>
	{
		Task<Appointment?> GetByIdWithDetailsAsync(Guid id);
		Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId);
		Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId);
		Task<IEnumerable<Appointment>> GetByDoctorIdAndDateAsync(Guid doctorId, DateTime date);
		Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid userId, bool isDoctor);
		Task<IEnumerable<Appointment>> GetPastAppointmentsAsync(Guid userId, bool isDoctor);
		Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status);
		Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null);
		Task<int> GetCompletedAppointmentsCountAsync(Guid doctorId);
	}
}
