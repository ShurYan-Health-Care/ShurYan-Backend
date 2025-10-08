using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IDoctorAvailabilityRepository : IGenericRepository<DoctorAvailability>
	{
		Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId);
		Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAndDayAsync(Guid doctorId, SysDayOfWeek day);
		Task<bool> HasOverlappingAvailabilityAsync(
			Guid doctorId,
			SysDayOfWeek day,
			TimeOnly startTime,
			TimeOnly endTime,
			Guid? excludeId = null);
	}
}
