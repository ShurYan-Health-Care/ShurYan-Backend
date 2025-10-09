using Shuryan.Core.Entities.External.Clinic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ClinicRepositories
{
    public interface IClinicPhoneNumberRepository : IGenericRepository<ClinicPhoneNumber>
    {
        Task<IEnumerable<ClinicPhoneNumber>> GetClinicPhoneNumbersAsync(Guid clinicId);
    }
}
