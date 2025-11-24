using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;
using Shuryan.Core.Enums.Doctor;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using System;
using Shuryan.Core.Enums.Appointments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shuryan.Core.Enums.Identity;

namespace Shuryan.Infrastructure.Repositories.Doctors
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ShuryanDbContext context) : base(context) { }

        public async Task<Doctor?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.Email == email && !d.IsDeleted);
        }

        public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(MedicalSpecialty specialty)
        {
            return await _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Consultations)
                    .ThenInclude(dc => dc.ConsultationType)
                .Where(d => d.MedicalSpecialty == specialty
                    && d.VerificationStatus == VerificationStatus.Verified
                    && !d.IsDeleted)
                .ToListAsync();
        }

        public async Task<Doctor?> GetByIdWithAvailabilitiesAsync(Guid id)
        {
            return await _dbSet
                .Include(d => d.Availabilities.Where(a => !a.IsDeleted))
                .Include(d => d.Overrides)
                .Include(d => d.Consultations)
                    .ThenInclude(dc => dc.ConsultationType)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<Doctor?> GetByIdWithClinicAsync(Guid id)
        {
            return await _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Photos)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.PhoneNumbers)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<Doctor?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Photos)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.PhoneNumbers)
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.OfferedServices)
                .Include(d => d.Consultations)
                    .ThenInclude(dc => dc.ConsultationType)
                .Include(d => d.VerificationDocuments)
                .Include(d => d.Availabilities.Where(a => !a.IsDeleted))
                .Include(d => d.DoctorReviews)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsByGovernorateAsync(Governorate governorate)
        {
            return await _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Consultations)
                    .ThenInclude(dc => dc.ConsultationType)
                .Where(d => d.Clinic != null
                    && d.Clinic.Address.Governorate == governorate
                    && d.VerificationStatus == VerificationStatus.Verified
                    && !d.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetVerifiedDoctorsAsync()
        {
            return await _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Consultations)
                .Where(d => d.VerificationStatus == VerificationStatus.Verified && !d.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsAvailableAtAsync(Guid doctorId, DateTime dateTime)
        {
            var doctor = await _dbSet
                .Include(d => d.Availabilities.Where(a => !a.IsDeleted))
                .Include(d => d.Overrides)
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

            if (doctor == null) return false;

            var dayOfWeek = (SysDayOfWeek)((int)dateTime.DayOfWeek + 1);
            var timeOnly = TimeOnly.FromDateTime(dateTime);

            // Check Overrides first
            var overrideAtTime = doctor.Overrides
                .FirstOrDefault(o => dateTime >= o.StartTime && dateTime <= o.EndTime);

            if (overrideAtTime != null)
                return overrideAtTime.Type == Shuryan.Core.Enums.Appointments.OverrideType.Available;

            // Check Regular Availability
            var availability = doctor.Availabilities
                .FirstOrDefault(a => a.DayOfWeek == dayOfWeek
                    && timeOnly >= a.StartTime
                    && timeOnly <= a.EndTime);

            return availability != null;
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(
            string? searchTerm = null,
            MedicalSpecialty? specialty = null,
            Governorate? governorate = null,
            int? minYearsOfExperience = null,
            decimal? maxConsultationFee = null,
            double? minRating = null)
        {
            IQueryable<Doctor> query = _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Consultations)
                    .ThenInclude(dc => dc.ConsultationType)
                .Include(d => d.DoctorReviews)
                .Where(d => d.VerificationStatus == VerificationStatus.Verified && !d.IsDeleted);

            // Search Term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(d =>
                    d.FirstName.ToLower().Contains(lowerSearch)
                    || d.LastName.ToLower().Contains(lowerSearch)
                    || (d.Biography != null && d.Biography.ToLower().Contains(lowerSearch)));
            }

            // Specialty Filter
            if (specialty.HasValue)
                query = query.Where(d => d.MedicalSpecialty == specialty.Value);

            // Governorate Filter
            if (governorate.HasValue)
                query = query.Where(d => d.Clinic != null && d.Clinic.Address.Governorate == governorate.Value);

            // Years of Experience Filter
            if (minYearsOfExperience.HasValue)
                query = query.Where(d => d.YearsOfExperience >= minYearsOfExperience.Value);

            // Consultation Fee Filter
            if (maxConsultationFee.HasValue)
                query = query.Where(d => d.Consultations.Any(c => c.ConsultationFee <= maxConsultationFee.Value));

            var doctors = await query.ToListAsync();

            // Rating Filter (in memory because it's computed)
            if (minRating.HasValue)
            {
                doctors = doctors.Where(d =>
                {
                    if (!d.DoctorReviews.Any()) return false;
                    var avgRating = d.DoctorReviews.Average(r => r.AverageRating);
                    return avgRating >= minRating.Value;
                }).ToList();
            }

            return doctors;
        }

        public async Task<IEnumerable<Doctor>> GetVerifiedDoctorsWithDetailsForListAsync()
        {
            return await _dbSet
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .Include(d => d.Consultations)
                    .ThenInclude(dc => dc.ConsultationType)
                .Include(d => d.Availabilities.Where(a => !a.IsDeleted))
                .Include(d => d.Overrides)
                .Where(d => d.VerificationStatus == VerificationStatus.Verified && !d.IsDeleted)
                .AsSplitQuery() // Avoid cartesian explosion - use multiple queries instead of one giant JOIN
                .AsNoTracking() // Performance optimization - read-only
                .ToListAsync();
        }
    }
}

