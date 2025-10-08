using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical.Schedules;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IDoctorOverrideRepository : IGenericRepository<DoctorOverride>
	{
		Task<IEnumerable<DoctorOverride>> GetByDoctorIdAsync(Guid doctorId);
		Task<IEnumerable<DoctorOverride>> GetByDoctorIdAndDateRangeAsync(
			Guid doctorId,
			DateTime startDate,
			DateTime endDate);
		Task<DoctorOverride?> GetOverrideAtTimeAsync(Guid doctorId, DateTime dateTime);
	}
}
