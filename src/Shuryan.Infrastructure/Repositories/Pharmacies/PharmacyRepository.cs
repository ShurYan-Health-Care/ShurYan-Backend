using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Pharmacies
{
    public class PharmacyRepository : GenericRepository<Pharmacy>, IPharmacyRepository
    {
        public PharmacyRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Pharmacy?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(p => p.Address)
                .FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        }

        public async Task<Pharmacy?> GetPharmacyWithDetailsAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Include(p => p.Address)
                .Include(p => p.WorkingHours)
                .Include(p => p.VerificationDocuments)
                .FirstOrDefaultAsync(p => p.Id == pharmacyId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Pharmacy>> SearchPharmaciesAsync(
            string? searchTerm,
            Governorate? governorate,
            bool? offersDelivery,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Pharmacy> query = _dbSet
                .Include(p => p.Address)
                .Include(p => p.WorkingHours)
                .Where(p => !p.IsDeleted);

            // Search Term Filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(lowerSearch) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
            }

            // Governorate Filter
            if (governorate.HasValue)
            {
                query = query.Where(p => p.Address != null && p.Address.Governorate == governorate.Value);
            }

            // Delivery Filter
            if (offersDelivery.HasValue)
            {
                query = query.Where(p => p.OffersDelivery == offersDelivery.Value);
            }

            // Pagination
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pharmacy>> GetPendingVerificationPharmaciesAsync()
        {
            return await _dbSet
                .Include(p => p.Address)
                .Include(p => p.VerificationDocuments)
                .Where(p => p.VerificationStatus == Shuryan.Core.Enums.Identity.VerificationStatus.UnderReview 
                    && !p.IsDeleted)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pharmacy>> GetPharmaciesByMedicationsAsync(IEnumerable<Guid> medicationIds)
        {
            if (!medicationIds.Any())
                return new List<Pharmacy>();

            var pharmacies = await _dbSet
                .Include(p => p.Address)
                .Include(p => p.WorkingHours)
                .Where(p => !p.IsDeleted 
                    && p.VerificationStatus == Shuryan.Core.Enums.Identity.VerificationStatus.Verified)
                .ToListAsync();

            var filteredPharmacies = pharmacies;

            return filteredPharmacies;
        }

        public async Task<IEnumerable<Pharmacy>> GetAllActivePharmaciesWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.Address)
                .Include(p => p.WorkingHours)
                .Include(p => p.PharmacyReviews)
                .Where(p => !p.IsDeleted 
                    && p.VerificationStatus == Shuryan.Core.Enums.Identity.VerificationStatus.Verified
                    && p.Address != null
                    && p.Address.Latitude.HasValue
                    && p.Address.Longitude.HasValue)
                .ToListAsync();
        }
    }
}

