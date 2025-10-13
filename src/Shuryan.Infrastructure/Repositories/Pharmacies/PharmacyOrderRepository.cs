using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Enums.Pharmacy;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Infrastructure.Repositories.Pharmacies
{
    public class PharmacyOrderRepository : GenericRepository<PharmacyOrder>, IPharmacyOrderRepository
    {
        public PharmacyOrderRepository(ShuryanDbContext context) : base(context) { }

        public async Task<PharmacyOrder?> GetOrderWithDetailsAsync(Guid orderId)
        {
            return await _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Pharmacy)
                    .ThenInclude(p => p.Address)
                .Include(o => o.Prescription)
                    .ThenInclude(pr => pr.PrescribedMedications)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<PharmacyOrder>> GetPagedOrdersForPatientAsync(Guid patientId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(o => o.Pharmacy)
                .Include(o => o.Prescription)
                .Where(o => o.PatientId == patientId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<PharmacyOrder>> GetPagedOrdersForPharmacyAsync(
            Guid pharmacyId,
            PharmacyOrderStatus? status,
            int pageNumber,
            int pageSize)
        {
            IQueryable<PharmacyOrder> query = _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Prescription)
                .Where(o => o.PharmacyId == pharmacyId);

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

        public async Task<PharmacyOrder?> FindByOrderNumberAsync(string orderNumber)
        {
            return await _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Pharmacy)
                .Include(o => o.Prescription)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<PharmacyOrder>> GetOrdersByStatusAsync(PharmacyOrderStatus status)
        {
            return await _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Pharmacy)
                .Include(o => o.Prescription)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}

