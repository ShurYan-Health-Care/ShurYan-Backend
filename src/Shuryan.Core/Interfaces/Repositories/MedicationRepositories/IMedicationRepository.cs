using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.MedicationRepositories
{
    public interface IMedicationRepository : IGenericRepository<Medication>
    {
        Task<Medication?> GetByCodeAsync(string code);
        Task<IEnumerable<Medication>> SearchMedicationsAsync(string searchTerm);
        Task<IEnumerable<Medication>> GetByManufacturerAsync(string manufacturer);
        Task<IEnumerable<Medication>> GetPrescriptionRequiredMedicationsAsync();
        Task<IEnumerable<Medication>> GetMostPrescribedMedicationsAsync(int count);
    }
}

