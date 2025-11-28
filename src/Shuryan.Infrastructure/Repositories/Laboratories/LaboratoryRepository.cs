using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Identity;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LaboratoryRepository : GenericRepository<Laboratory>, ILaboratoryRepository
    {
        public LaboratoryRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Laboratory?> GetLaboratoryWithDetailsAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Include(l => l.Address)
                .Include(l => l.WorkingHours)
                .Include(l => l.LabServices)
                    .ThenInclude(ls => ls.LabTest)
                .Include(l => l.VerificationDocuments)
                .Include(l => l.LaboratoryReviews)
                .FirstOrDefaultAsync(l => l.Id == laboratoryId && !l.IsDeleted);
        }

        public async Task<IEnumerable<Laboratory>> SearchLaboratoriesAsync(
            string? searchTerm,
            Governorate? governorate,
            bool? offersHomeSampleCollection,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Laboratory> query = _dbSet
                .Include(l => l.Address)
                .Include(l => l.WorkingHours)
                .Where(l => !l.IsDeleted 
                    && l.VerificationStatus == VerificationStatus.Verified 
                    && l.LaboratoryStatus == Status.Active);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(l =>
                    l.Name.ToLower().Contains(lowerSearch) ||
                    (l.Description != null && l.Description.ToLower().Contains(lowerSearch)));
            }

            if (governorate.HasValue)
            {
                query = query.Where(l => l.Address != null && l.Address.Governorate == governorate.Value);
            }

            if (offersHomeSampleCollection.HasValue)
            {
                query = query.Where(l => l.OffersHomeSampleCollection == offersHomeSampleCollection.Value);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Laboratory>> GetPendingVerificationLaboratoriesAsync()
        {
            return await _dbSet
                .Include(l => l.Address)
                .Include(l => l.VerificationDocuments)
                .Where(l => l.VerificationStatus == VerificationStatus.UnderReview && !l.IsDeleted)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Laboratory>> GetLaboratoriesWithHomeSampleCollectionAsync(Governorate? governorate = null)
        {
            IQueryable<Laboratory> query = _dbSet
                .Include(l => l.Address)
                .Include(l => l.WorkingHours)
                .Where(l => l.OffersHomeSampleCollection 
                    && !l.IsDeleted 
                    && l.VerificationStatus == VerificationStatus.Verified
                    && l.LaboratoryStatus == Status.Active);

            if (governorate.HasValue)
            {
                query = query.Where(l => l.Address != null && l.Address.Governorate == governorate.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Laboratory>> GetLaboratoriesByTestsAsync(IEnumerable<Guid> testIds)
        {
            if (!testIds.Any())
                return new List<Laboratory>();

            var laboratories = await _dbSet
                .Include(l => l.Address)
                .Include(l => l.WorkingHours)
                .Include(l => l.LabServices)
                    .ThenInclude(ls => ls.LabTest)
                .Where(l => !l.IsDeleted 
                    && l.VerificationStatus == VerificationStatus.Verified
                    && l.LaboratoryStatus == Status.Active)
                .ToListAsync();

            var filteredLabs = laboratories.Where(l =>
                testIds.All(testId =>
                    l.LabServices.Any(ls => ls.LabTestId == testId && ls.IsAvailable)));

            return filteredLabs;
        }

        public async Task<Laboratory?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(l => l.Email == email && !l.IsDeleted);
        }

        public async Task<IEnumerable<Laboratory>> GetAllActiveLaboratoriesWithDetailsAsync()
        {
            return await _dbSet
                .Include(l => l.Address)
                .Include(l => l.WorkingHours)
                .Include(l => l.LaboratoryReviews)
                .Where(l => !l.IsDeleted 
                    && l.VerificationStatus == VerificationStatus.Verified
                    && l.LaboratoryStatus == Status.Active
                    && l.Address != null
                    && l.Address.Latitude.HasValue
                    && l.Address.Longitude.HasValue)
                .ToListAsync();
        }
    }
}

