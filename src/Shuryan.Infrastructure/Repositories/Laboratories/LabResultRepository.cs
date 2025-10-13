using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LabResultRepository : GenericRepository<LabResult>, ILabResultRepository
    {
        public LabResultRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<LabResult>> GetResultsByLabOrderAsync(Guid labOrderId)
        {
            return await _dbSet
                .Include(r => r.LabTest)
                .Where(r => r.LabOrderId == labOrderId)
                .OrderBy(r => r.LabTest.Name)
                .ToListAsync();
        }

        public async Task<LabResult?> GetResultByOrderAndTestAsync(Guid labOrderId, Guid labTestId)
        {
            return await _dbSet
                .Include(r => r.LabTest)
                .FirstOrDefaultAsync(r => r.LabOrderId == labOrderId 
                    && r.LabTestId == labTestId);
        }

        public async Task<IEnumerable<LabResult>> GetResultsByPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(r => r.LabTest)
                .Include(r => r.LabOrder)
                    .ThenInclude(o => o.Laboratory)
                .Where(r => r.LabOrder.PatientId == patientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AreAllResultsCompletedAsync(Guid labOrderId)
        {
            var order = await _context.Set<LabOrder>()
                .Include(o => o.LabPrescription)
                    .ThenInclude(p => p.Items)
                .Include(o => o.LabResults)
                .FirstOrDefaultAsync(o => o.Id == labOrderId);

            if (order == null) return false;

            var prescribedTestsCount = order.LabPrescription.Items.Count;
            var completedResultsCount = order.LabResults.Count;

            return prescribedTestsCount == completedResultsCount;
        }
    }
}

