using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IPatientRepository : IGenericRepository<Patient>
	{
		Task<Patient?> GetByIdWithDetailsAsync(Guid id);
		Task<Patient?> GetByEmailAsync(string email);
		Task<IEnumerable<Patient>> GetPatientsWithMedicalHistoryAsync();
	}
}
