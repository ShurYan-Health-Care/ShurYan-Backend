using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IConsultationTypeRepository : IGenericRepository<ConsultationType>
	{
		Task<ConsultationType?> GetByEnumAsync(ConsultationTypeEnum consultationType);
	}
}
