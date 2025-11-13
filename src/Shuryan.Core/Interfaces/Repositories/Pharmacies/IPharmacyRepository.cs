using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Interfaces.Repositories.Pharmacies
{
    public interface IPharmacyRepository : IGenericRepository<Pharmacy>
    {
        Task<Pharmacy?> GetByEmailAsync(string email);
        Task<Pharmacy?> GetPharmacyWithDetailsAsync(Guid pharmacyId);
        Task<IEnumerable<Pharmacy>> SearchPharmaciesAsync(string? searchTerm, Governorate? governorate,
                                        bool? offersDelivery,
                                        int pageNumber,
                                        int pageSize);
        Task<IEnumerable<Pharmacy>> GetPendingVerificationPharmaciesAsync();
        Task<IEnumerable<Pharmacy>> GetPharmaciesByMedicationsAsync(IEnumerable<Guid> medicationIds);
        Task<IEnumerable<Pharmacy>> GetAllActivePharmaciesWithDetailsAsync();
    }
}
