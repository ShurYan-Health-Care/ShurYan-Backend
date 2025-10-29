using Shuryan.Core.Entities.Medical.Partners;
using System;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IDoctorPartnerSuggestionRepository : IGenericRepository<DoctorPartnerSuggestion>
    {
        Task<DoctorPartnerSuggestion?> GetByDoctorIdAsync(Guid doctorId);
        Task<bool> DeleteByDoctorIdAsync(Guid doctorId);
    }
}
