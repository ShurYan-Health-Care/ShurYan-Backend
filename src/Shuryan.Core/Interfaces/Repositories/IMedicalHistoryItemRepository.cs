using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IMedicalHistoryItemRepository : IGenericRepository<MedicalHistoryItem>
	{
		Task<IEnumerable<MedicalHistoryItem>> GetByPatientIdAsync(Guid patientId);
		Task<IEnumerable<MedicalHistoryItem>> GetByTypeAsync(Guid patientId, MedicalHistoryType type);
	}
}
