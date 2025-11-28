﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.DTOs;
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

        public async Task<(IEnumerable<DoctorPatientDto> Patients, int TotalCount)> GetDoctorPatientsOptimizedAsync(
            Guid doctorId, 
            int pageNumber, 
            int pageSize)
        {
            var query = _dbSet
                .Where(p => !p.IsDeleted && 
                            p.Appointments.Any(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Completed))
                .Select(p => new DoctorPatientDto
                {
                    PatientId = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    PhoneNumber = p.PhoneNumber,
                    ProfileImageUrl = p.ProfileImageUrl,
                    
                    City = p.Address != null ? p.Address.City : null,
                    Governorate = p.Address != null ? p.Address.Governorate.ToString() : null,
                    
                    TotalSessions = p.Appointments
                        .Count(a => a.DoctorId == doctorId && 
                                   a.Status == AppointmentStatus.Completed),
                    
                    // Last visit - using Max directly in projection
                    LastVisitDate = p.Appointments
                        .Where(a => a.DoctorId == doctorId && 
                                   a.Status == AppointmentStatus.Completed)
                        .Max(a => (DateTime?)a.ScheduledStartTime) // Nullable in case no appointments
                });

            var pagedQuery = query
                .OrderByDescending(p => p.LastVisitDate ?? DateTime.MinValue) // Handle nulls
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var result = await pagedQuery
                .Select(p => new 
                { 
                    Patient = p,
                    TotalCount = query.Count()
                })
                .AsNoTracking()
                .ToListAsync();

            var patients = result.Select(r => r.Patient).ToList();
            var totalCount = result.FirstOrDefault()?.TotalCount ?? 0;

            return (patients, totalCount);
        }

        public async Task<Patient?> GetPatientWithAddressAsync(Guid patientId)
        {
            return await _dbSet
                .Include(p => p.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);
        }
    }
}