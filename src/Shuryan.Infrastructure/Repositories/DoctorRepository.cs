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

namespace Shuryan.Infrastructure.Repositories
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        private readonly ShuryanDbContext _context;

        public DoctorRepository(ShuryanDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Doctor?> GetByEmailAsync(string email)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(MedicalSpecialty specialty)
        {
            return await _context.Doctors
                .Where(d => d.MedicalSpecialty == specialty)
                .ToListAsync();
        }

        public async Task<Doctor?> GetByIdWithAvailabilitiesAsync(Guid id)
        {
            return await _context.Doctors
                .Include(d => d.Availabilities)
                .Include(d => d.Overrides)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Doctor?> GetByIdWithClinicAsync(Guid id)
        {
            return await _context.Doctors
                .Include(d => d.Clinic)
                    .ThenInclude(c => c.Address)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Doctor?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Doctors
                .Include(d => d.Clinic).ThenInclude(c => c != null ? c.Address : null)
                .Include(d => d.Availabilities)
                .Include(d => d.Overrides)
                .Include(d => d.Consultations)
                .Include(d => d.VerificationDocuments)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsByGovernorateAsync(Governorate governorate)
        {
            return await _context.Doctors
                .Where(d => d.Clinic != null && d.Clinic.Address.Governorate == governorate)
                .Include(d => d.Clinic.Address)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetVerifiedDoctorsAsync()
        {
            return await _context.Doctors
                .Where(d => d.VerificationStatus == VerificationStatus.Verified)
                .ToListAsync();
        }

        public async Task<bool> IsAvailableAtAsync(Guid doctorId, DateTime dateTime)
        {
            var dayOfWeek = (SysDayOfWeek)dateTime.DayOfWeek;
            var time = TimeOnly.FromDateTime(dateTime);

            var isUnavailableOverride = await _context.DoctorOverride
                .AnyAsync(o => o.DoctorId == doctorId &&
                               o.Type == OverrideType.Unavailable &&
                               dateTime >= o.StartTime && dateTime < o.EndTime);
            if (isUnavailableOverride)
            {
                return false;
            }

            var isAvailableOverride = await _context.DoctorOverride
                .AnyAsync(o => o.DoctorId == doctorId &&
                               o.Type == OverrideType.Available &&
                               dateTime >= o.StartTime && dateTime < o.EndTime);
            if (isAvailableOverride)
            {
                return true; 
            }

            var isInRegularSchedule = await _context.DoctorAvailability
                .AnyAsync(a => a.DoctorId == doctorId &&
                               a.DayOfWeek == dayOfWeek &&
                               time >= a.StartTime && time < a.EndTime);

            return isInRegularSchedule;
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(string? searchTerm = null,
                                    MedicalSpecialty? specialty = null,
                                    Governorate? governorate = null,
                                    int? minYearsOfExperience = null,
                                    decimal? maxConsultationFee = null,
                                    double? minRating = null)
        {
            var query = _context.Doctors.Include(d => d.Clinic).ThenInclude(c => c.Address).Include(d => d.DoctorReviews).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.Trim().ToLower();
                query = query.Where(d =>
                    d.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    d.LastName.ToLower().Contains(lowerSearchTerm) ||
                    (d.Clinic != null && d.Clinic.Name.ToLower().Contains(lowerSearchTerm))
                );
            }

            if (specialty.HasValue)
            {
                query = query.Where(d => d.MedicalSpecialty == specialty.Value);
            }

            if (governorate.HasValue)
            {
                query = query.Where(d => d.Clinic != null && d.Clinic.Address.Governorate == governorate.Value);
            }

            if (minYearsOfExperience.HasValue)
            {
                query = query.Where(d => d.YearsOfExperience >= minYearsOfExperience.Value);
            }

            if (maxConsultationFee.HasValue)
            {
                query = query.Where(d => d.Consultations.Any(c => c.ConsultationFee <= maxConsultationFee.Value));
            }
            if (minRating.HasValue)
            {
                query = query.Where(d =>
                    d.DoctorReviews.Any() && 
                    d.DoctorReviews.Average(r =>
                        (r.OverallSatisfaction +
                         r.WaitingTime +
                         r.CommunicationQuality +
                         r.ClinicCleanliness +
                         r.ValueForMoney) / 5.0
                    ) >= minRating.Value
                );
            }
            return await query.ToListAsync();
        }
    }
}