using Shuryan.Core.Entities.External.Laboratories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabServiceRepository : IGenericRepository<LabService>
    {
        Task<IEnumerable<LabService>> GetLabServicesAsync(Guid laboratoryId);
        Task<IEnumerable<LabService>> GetAvailableLabServicesAsync(Guid laboratoryId);
        Task<LabService?> GetLabServiceByTestAsync(Guid laboratoryId, Guid labTestId);
        Task<IEnumerable<LabService>> GetLaboratoriesOfferingTestAsync(Guid labTestId);
    }
}

