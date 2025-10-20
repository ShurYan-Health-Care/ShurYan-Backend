using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.Repositories.Patients;

namespace Shuryan.Infrastructure.Repositories.Patients
{
    public class PatientRepository : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Patient?> GetByIdWithDetailsAsync(Guid id)
        {
            // Use AsSplitQuery to avoid cartesian explosion and improve performance
            // Note: This will track the entity for updates
            return await _dbSet
                .AsSplitQuery() // This splits the query into multiple SQL queries
                .Include(p => p.Address)
                .Include(p => p.MedicalHistory)
                .Include(p => p.Appointments)
                .Include(p => p.LabOrders)
                .Include(p => p.Prescriptions)
                .Include(p => p.PharmacyOrders)
                .Include(p => p.DoctorReviews)
                .Include(p => p.LaboratoryReviews)
                .Include(p => p.PharmacyReviews)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Patient?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .AsNoTracking() // Don't track for read operations
                .FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithMedicalHistoryAsync()
        {
            return await _dbSet
                .Include(p => p.MedicalHistory)
                .Where(p => p.MedicalHistory.Any() && !p.IsDeleted)
                .ToListAsync();
        }
        public async Task RemoveAsync(Patient patient, bool softDelete = true)
        {
            if (softDelete)
            {
                patient.IsDeleted = true;
                patient.DeletedAt = DateTime.UtcNow;
                _dbSet.Update(patient);
            }
            else
            {
                _dbSet.Remove(patient);
            }

            await _context.SaveChangesAsync();
        }
    }
}