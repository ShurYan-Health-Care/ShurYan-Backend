using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Enums.Laboratory;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Laboratories
{
    public class LabOrderRepository : GenericRepository<LabOrder>, ILabOrderRepository
    {
        public LabOrderRepository(ShuryanDbContext context) : base(context) { }

        public async Task<LabOrder?> GetOrderWithDetailsAsync(Guid orderId)
        {
            return await _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Laboratory)
                    .ThenInclude(l => l.Address)
                .Include(o => o.LabPrescription)
                    .ThenInclude(p => p.Items)
                        .ThenInclude(i => i.LabTest)
                .Include(o => o.LabResults)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<LabOrder>> GetPagedOrdersForPatientAsync(Guid patientId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(o => o.Laboratory)
                .Include(o => o.LabPrescription)
                .Include(o => o.LabResults)
                .Where(o => o.PatientId == patientId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> GetPagedOrdersForLaboratoryAsync(Guid laboratoryId, LabOrderStatus? status, int pageNumber, int pageSize)
        {
            IQueryable<LabOrder> query = _dbSet
                .Include(o => o.Patient)
                .Include(o => o.LabPrescription)
                .Include(o => o.LabResults)
                .Where(o => o.LaboratoryId == laboratoryId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> GetOrdersByStatusAsync(LabOrderStatus status)
        {
            return await _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Laboratory)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> GetPendingOrdersForLaboratoryAsync(Guid laboratoryId)
        {
            return await _dbSet
                .Include(o => o.Patient)
                .Include(o => o.LabPrescription)
                .Where(o => o.LaboratoryId == laboratoryId 
                    && o.Status == LabOrderStatus.AwaitingLabReview)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> GetActiveOrdersForPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(o => o.Laboratory)
                .Include(o => o.LabPrescription)
                .Where(o => o.PatientId == patientId 
                    && o.Status != LabOrderStatus.Completed
                    && o.Status != LabOrderStatus.CancelledByPatient
                    && o.Status != LabOrderStatus.RejectedByLab)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}

