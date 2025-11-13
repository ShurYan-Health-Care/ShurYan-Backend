using Shuryan.Core.Entities.External.Laboratories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabResultRepository : IGenericRepository<LabResult>
    {
        Task<IEnumerable<LabResult>> GetResultsByLabOrderAsync(Guid labOrderId);
        Task<LabResult?> GetResultByOrderAndTestAsync(Guid labOrderId, Guid labTestId);
        Task<IEnumerable<LabResult>> GetResultsByPatientAsync(Guid patientId);
        Task<bool> AreAllResultsCompletedAsync(Guid labOrderId);
    }
}

