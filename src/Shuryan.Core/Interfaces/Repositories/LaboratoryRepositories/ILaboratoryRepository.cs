using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILaboratoryRepository : IGenericRepository<Laboratory>
    {
        Task<Laboratory?> GetLaboratoryWithDetailsAsync(Guid laboratoryId);
        Task<IEnumerable<Laboratory>> SearchLaboratoriesAsync(
            string? searchTerm,
            Governorate? governorate,
            bool? offersHomeSampleCollection,
            int pageNumber,
            int pageSize);
        Task<IEnumerable<Laboratory>> GetPendingVerificationLaboratoriesAsync();
        Task<IEnumerable<Laboratory>> GetLaboratoriesWithHomeSampleCollectionAsync(Governorate? governorate = null);
        Task<IEnumerable<Laboratory>> GetLaboratoriesByTestsAsync(IEnumerable<Guid> testIds);
        Task<Laboratory?> GetByEmailAsync(string email);
        
        /// <summary>
        /// جلب كل المعامل المفعلة مع التفاصيل (للبحث عن أقرب 3 معامل)
        /// </summary>
        Task<IEnumerable<Laboratory>> GetAllActiveLaboratoriesWithDetailsAsync();
    }
}

