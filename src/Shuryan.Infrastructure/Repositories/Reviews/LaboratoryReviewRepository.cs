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
    public class LaboratoryReviewRepository : GenericRepository<LaboratoryReview>, ILaboratoryReviewRepository
    {
        public LaboratoryReviewRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<LaboratoryReview>> GetReviewsByLaboratoryAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Where(r => r.LaboratoryId == laboratoryId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LaboratoryReview>> GetReviewsByPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(r => r.Laboratory)
                .Include(r => r.LabOrder)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<LaboratoryReview?> GetReviewByLabOrderAsync(Guid labOrderId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Laboratory)
                .FirstOrDefaultAsync(r => r.LabOrderId == labOrderId);
        }

        public async Task<double> GetAverageRatingForLaboratoryAsync(Guid laboratoryId)
        {
            var reviews = await _dbSet
                .Where(r => r.LaboratoryId == laboratoryId)
                .ToListAsync();

            if (!reviews.Any()) return 0;

            return reviews.Average(r => r.AverageRating);
        }

        public async Task<int> GetReviewCountForLaboratoryAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Where(r => r.LaboratoryId == laboratoryId)
                .CountAsync();
        }
    }
}

