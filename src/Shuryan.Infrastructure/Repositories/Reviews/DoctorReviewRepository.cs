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

        public async Task<(IEnumerable<DoctorReview> Reviews, int TotalCount)> GetPaginatedReviewsByDoctorAsync(
            Guid doctorId, 
            int pageNumber, 
            int pageSize)
        {
            var query = _dbSet
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid doctorId)
        {
            var reviews = await _dbSet
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();

            var distribution = new Dictionary<int, int>
            {
                { 5, 0 },
                { 4, 0 },
                { 3, 0 },
                { 2, 0 },
                { 1, 0 }
            };

            foreach (var review in reviews)
            {
                var roundedRating = (int)Math.Round(review.AverageRating);
                if (roundedRating >= 1 && roundedRating <= 5)
                {
                    distribution[roundedRating]++;
                }
            }

            return distribution;
        }

        public async Task<DoctorReview?> GetReviewByIdWithPatientAsync(Guid reviewId, Guid doctorId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.DoctorId == doctorId);
        }

        public async Task<Dictionary<Guid, double?>> GetAverageRatingsForDoctorsAsync(IEnumerable<Guid> doctorIds)
        {
            var doctorIdsList = doctorIds.ToList();
            
            // جلب كل الـ reviews للدكاترة المطلوبين في query واحد
            var reviews = await _dbSet
                .Where(r => doctorIdsList.Contains(r.DoctorId))
                .AsNoTracking()
                .ToListAsync();

            // حساب المتوسط لكل دكتور
            var averageRatings = reviews
                .GroupBy(r => r.DoctorId)
                .ToDictionary(
                    g => g.Key,
                    g => (double?)g.Average(r => r.AverageRating)
                );

            // إضافة الدكاترة اللي ملهمش reviews
            foreach (var doctorId in doctorIdsList)
            {
                if (!averageRatings.ContainsKey(doctorId))
                {
                    averageRatings[doctorId] = null;
                }
            }

            return averageRatings;
        }
    }
}

