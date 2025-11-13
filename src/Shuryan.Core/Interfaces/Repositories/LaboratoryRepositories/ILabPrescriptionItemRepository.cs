using Shuryan.Core.Entities.External.Laboratories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabPrescriptionItemRepository : IGenericRepository<LabPrescriptionItem>
    {
        Task<IEnumerable<LabPrescriptionItem>> GetItemsByPrescriptionAsync(Guid labPrescriptionId);
        Task<LabPrescriptionItem?> GetItemAsync(Guid labPrescriptionId, Guid labTestId);
    }
}

