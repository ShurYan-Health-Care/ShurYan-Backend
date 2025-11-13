using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Clinic;
using Shuryan.Core.Interfaces.Repositories.ClinicRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Clinics
{
    public class ClinicRepository : GenericRepository<Clinic>, IClinicRepository
    {
        public ClinicRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Clinic?> GetClinicByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(c => c.Address)
                .Include(c => c.Photos)
                .Include(c => c.PhoneNumbers)
                .Include(c => c.OfferedServices)
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.DoctorId == doctorId);
        }

        public async Task<IEnumerable<Clinic>> GetClinicsNearLocationAsync(double latitude, double longitude, double radiusInKm)
        {
            // Using Haversine formula for distance calculation
            var clinics = await _dbSet
                .Include(c => c.Address)
                .Include(c => c.Doctor)
                .Include(c => c.Photos)
                .Include(c => c.PhoneNumbers)
                .Where(c => c.Address != null)
                .ToListAsync();


            var nearbyClinics = clinics.Where(c =>
            {
                if (c.Address == null) return false;
                
                var distance = CalculateDistance(
                    latitude, 
                    longitude, 
                    c.Address.Latitude ?? 0, 
                    c.Address.Longitude ?? 0
                );
                
                return distance <= radiusInKm;
            }).ToList();

            return nearbyClinics;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}

