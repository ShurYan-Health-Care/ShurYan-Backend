using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IDispensingRecordRepository : IGenericRepository<DispensingRecord>
    {
        Task<IEnumerable<DispensingRecord>> GetByPrescriptionIdAsync(Guid prescriptionId);
        Task<IEnumerable<DispensingRecord>> GetByPharmacyIdAsync(Guid pharmacyId);
    }
}
