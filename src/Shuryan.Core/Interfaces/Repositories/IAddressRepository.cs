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
	public interface IAddressRepository : IGenericRepository<Address>
	{
		Task<IEnumerable<Address>> GetByGovernorateAsync(Governorate governorate);
		Task<IEnumerable<Address>> GetByCityAsync(string city);
	}
}
