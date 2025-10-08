using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IDoctorDocumentRepository : IGenericRepository<DoctorDocument>
	{
		Task<IEnumerable<DoctorDocument>> GetByDoctorIdAsync(Guid doctorId);
		Task<IEnumerable<DoctorDocument>> GetPendingDocumentsAsync();
		Task<IEnumerable<DoctorDocument>> GetByStatusAsync(VerificationDocumentStatus status);
	}
}
