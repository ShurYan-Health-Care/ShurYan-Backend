using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IPrescriptionShareRepository : IGenericRepository<PrescriptionShare>
    {
        Task<IEnumerable<PrescriptionShare>> GetByPharmacyIdAsync(Guid pharmacyId);
        Task<PrescriptionShare?> GetByShareCodeAsync(string shareCode);
    }
}
