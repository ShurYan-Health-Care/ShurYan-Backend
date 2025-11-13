using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILaboratoryDocumentRepository : IGenericRepository<LaboratoryDocument>
    {
        Task<IEnumerable<LaboratoryDocument>> GetByLaboratoryIdAsync(Guid laboratoryId);
        Task<IEnumerable<LaboratoryDocument>> GetPendingDocumentsAsync();
        Task<IEnumerable<LaboratoryDocument>> GetByStatusAsync(VerificationDocumentStatus status);
    }
}

