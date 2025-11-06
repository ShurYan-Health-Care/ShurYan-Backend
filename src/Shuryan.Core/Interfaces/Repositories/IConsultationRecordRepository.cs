using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Consultations;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IConsultationRecordRepository : IGenericRepository<ConsultationRecord>
	{
		Task<ConsultationRecord?> GetByAppointmentIdAsync(Guid appointmentId);
	}
}
