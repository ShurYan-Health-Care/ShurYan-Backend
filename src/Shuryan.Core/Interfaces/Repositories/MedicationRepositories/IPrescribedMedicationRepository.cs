using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.MedicationRepositories
{
    public interface IPrescribedMedicationRepository : IGenericRepository<PrescribedMedication>
    {
        Task<IEnumerable<PrescribedMedication>> GetByPrescriptionAsync(Guid prescriptionId);
        Task<PrescribedMedication?> GetPrescribedMedicationAsync(Guid prescriptionId, Guid medicationId);
        Task<IEnumerable<PrescribedMedication>> GetPrescriptionsContainingMedicationAsync(Guid medicationId);
    }
}

