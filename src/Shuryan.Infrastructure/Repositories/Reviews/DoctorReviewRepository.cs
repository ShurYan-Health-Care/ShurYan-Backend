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
    public class DoctorReviewRepository : GenericRepository<DoctorReview>, IDoctorReviewRepository
    {
        public DoctorReviewRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<DoctorReview>> GetReviewsByDoctorAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorReview>> GetReviewsByPatientAsync(Guid patientId)
        {
            return await _dbSet
                .Include(r => r.Doctor)
                .Include(r => r.Appointment)
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<DoctorReview?> GetReviewByAppointmentAsync(Guid appointmentId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task<double> GetAverageRatingForDoctorAsync(Guid doctorId)
        {
            var reviews = await _dbSet
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();

            if (!reviews.Any()) return 0;

            return reviews.Average(r => r.AverageRating);
        }

        public async Task<int> GetReviewCountForDoctorAsync(Guid doctorId)
        {
            return await _dbSet
                .Where(r => r.DoctorId == doctorId)
                .CountAsync();
        }
    }
}

