using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabTestRepository : IGenericRepository<LabTest>
    {
        Task<LabTest?> GetByCodeAsync(string code);
        Task<IEnumerable<LabTest>> GetTestsByCategoryAsync(LabTestCategory category);
        Task<IEnumerable<LabTest>> SearchTestsAsync(string searchTerm);
        Task<IEnumerable<LabTest>> GetMostRequestedTestsAsync(int count);
    }
}

