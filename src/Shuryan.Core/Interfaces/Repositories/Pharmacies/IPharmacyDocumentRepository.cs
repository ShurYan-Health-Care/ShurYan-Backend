using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.Pharmacies
{
    public interface IPharmacyDocumentRepository : IGenericRepository<PharmacyDocument>
    {
        Task<IEnumerable<PharmacyDocument>> GetByPharmacyIdAsync(Guid pharmacyId);
        Task<IEnumerable<PharmacyDocument>> GetPendingDocumentsAsync();
        Task<IEnumerable<PharmacyDocument>> GetByStatusAsync(VerificationDocumentStatus status);
    }
}

