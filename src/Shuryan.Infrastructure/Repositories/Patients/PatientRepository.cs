using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Infrastructure.Repositories.Patients
{
    public class PatientRepository : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Patient?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Address)
                .Include(p => p.MedicalHistory)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .Include(p => p.LabOrders)
                    .ThenInclude(lo => lo.Laboratory)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                .Include(p => p.PharmacyOrders)
                    .ThenInclude(po => po.Pharmacy)
                .Include(p => p.DoctorReviews)
                .Include(p => p.LaboratoryReviews)
                .Include(p => p.PharmacyReviews)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Patient?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithMedicalHistoryAsync()
        {
            return await _dbSet
                .Include(p => p.MedicalHistory)
                .Where(p => p.MedicalHistory.Any() && !p.IsDeleted)
                .ToListAsync();
        }
    }
}

