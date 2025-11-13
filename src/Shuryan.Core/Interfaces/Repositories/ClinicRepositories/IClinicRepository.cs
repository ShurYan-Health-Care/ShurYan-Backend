using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Clinic;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.ClinicRepositories
{
    public interface IClinicRepository : IGenericRepository<Clinic>
    {
        Task<Clinic?> GetClinicByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<Clinic>> GetClinicsNearLocationAsync(double latitude, double longitude, double radiusInKm);
    }
}