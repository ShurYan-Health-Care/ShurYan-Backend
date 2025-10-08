using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IVerifierRepository : IGenericRepository<Verifier>
	{
		Task<Verifier?> GetByIdWithVerifiedEntitiesAsync(Guid id);
		Task<Verifier?> GetByEmailAsync(string email);
		Task<int> GetVerifiedDoctorsCountAsync(Guid verifierId);
	}
}
