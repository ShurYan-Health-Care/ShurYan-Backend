using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Interfaces.Repositories.ReviewRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Reviews
{
    public class PharmacyReviewRepository : GenericRepository<PharmacyReview>, IPharmacyReviewRepository
    {
        public PharmacyReviewRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<PharmacyReview>> GetByPharmacyIdAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Where(r => r.PharmacyId == pharmacyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PharmacyReview>> GetReviewsByPharmacyAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Where(r => r.PharmacyId == pharmacyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PharmacyReview>> GetReviewsByPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(r => r.Pharmacy)
                .Include(r => r.PharmacyOrder)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<PharmacyReview?> GetReviewByPharmacyOrderAsync(Guid pharmacyOrderId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Pharmacy)
                .FirstOrDefaultAsync(r => r.PharmacyOrderId == pharmacyOrderId);
        }

        public async Task<double> GetAverageRatingForPharmacyAsync(Guid pharmacyId)
        {
            var reviews = await _dbSet
                .Where(r => r.PharmacyId == pharmacyId)
                .ToListAsync();

            if (!reviews.Any()) return 0;

            return reviews.Average(r => r.AverageRating);
        }

        public async Task<int> GetReviewCountForPharmacyAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Where(r => r.PharmacyId == pharmacyId)
                .CountAsync();
        }
    }
}

