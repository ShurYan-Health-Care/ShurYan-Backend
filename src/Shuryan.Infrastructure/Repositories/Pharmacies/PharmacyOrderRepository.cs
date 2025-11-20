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

        public async Task<int> CountNewOrdersByDateAsync(Guid pharmacyId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _dbSet
                .Where(o => o.PharmacyId == pharmacyId &&
                           o.CreatedAt >= startOfDay &&
                           o.CreatedAt < endOfDay)
                .CountAsync();
        }

        public async Task<int> CountPendingOrdersAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Where(o => o.PharmacyId == pharmacyId &&
                           o.Status == PharmacyOrderStatus.PendingPharmacyResponse)
                .CountAsync();
        }

        public async Task<int> CountCompletedOrdersAsync(Guid pharmacyId)
        {
            return await _dbSet
                .Where(o => o.PharmacyId == pharmacyId &&
                           o.Status == PharmacyOrderStatus.Confirmed)
                .CountAsync();
        }

        public async Task<decimal> CalculateRevenueByDateAsync(Guid pharmacyId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _dbSet
                .Where(o => o.PharmacyId == pharmacyId &&
                           o.Status == PharmacyOrderStatus.Confirmed &&
                           o.CreatedAt >= startOfDay &&
                           o.CreatedAt < endOfDay)
                .SumAsync(o => o.TotalCost);
        }

        public async Task<int> CountOrdersByMonthAsync(Guid pharmacyId, int year, int month)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return await _dbSet
                .Where(o => o.PharmacyId == pharmacyId &&
                           o.CreatedAt >= startOfMonth &&
                           o.CreatedAt < endOfMonth)
                .CountAsync();
        }

        public async Task<decimal> CalculateRevenueByMonthAsync(Guid pharmacyId, int year, int month)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return await _dbSet
                .Where(o => o.PharmacyId == pharmacyId &&
                           o.Status == PharmacyOrderStatus.Confirmed &&
                           o.CreatedAt >= startOfMonth &&
                           o.CreatedAt < endOfMonth)
                .SumAsync(o => o.TotalCost);
        }

        public async Task<PharmacyOrder?> GetOrderForPatientAsync(Guid orderId, Guid patientId)
        {
            return await _dbSet
                .Where(o => o.Id == orderId && o.PatientId == patientId)
                .FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<PharmacyOrder> Orders, int TotalCount)> GetOptimizedOrdersForPharmacyAsync(
            Guid pharmacyId,
            int pageNumber,
            int pageSize)
        {
            var query = _dbSet
                .Include(o => o.Patient)
                .Include(o => o.Prescription)
                .Where(o => o.PharmacyId == pharmacyId);

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<PharmacyOrder?> GetOrderDetailForPharmacyAsync(Guid orderId, Guid pharmacyId)
        {
            return await _dbSet
                .Include(o => o.Patient)
                    .ThenInclude(p => p.Address)
                .Include(o => o.Prescription)
                    .ThenInclude(pr => pr.PrescribedMedications)
                        .ThenInclude(pm => pm.Medication)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.RequestedMedication)
                .Where(o => o.Id == orderId && o.PharmacyId == pharmacyId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

    }
}

