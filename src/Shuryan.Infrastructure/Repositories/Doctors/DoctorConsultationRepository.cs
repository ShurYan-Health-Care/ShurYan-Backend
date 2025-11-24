using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Infrastructure.Data;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Infrastructure.Repositories.Doctors
{
    public class DoctorConsultationRepository : GenericRepository<DoctorConsultation>, IDoctorConsultationRepository
    {
        public DoctorConsultationRepository(ShuryanDbContext context) : base(context) { }

        public async Task<IEnumerable<DoctorConsultation>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _dbSet
                .Include(dc => dc.Doctor)
                .Include(dc => dc.ConsultationType)
                .Where(dc => dc.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<DoctorConsultation?> GetByDoctorIdAndConsultationTypeIdAsync(Guid doctorId, Guid consultationTypeId)
        {
            return await _dbSet
                .Include(dc => dc.ConsultationType)
                .FirstOrDefaultAsync(dc => dc.DoctorId == doctorId && dc.ConsultationTypeId == consultationTypeId);
        }

        public async Task<Dictionary<Guid, decimal>> GetRegularConsultationFeesForDoctorsAsync(IEnumerable<Guid> doctorIds)
        {
            var doctorIdsList = doctorIds.ToList();
            
            // جلب كل الـ consultations للدكاترة المطلوبين في query واحد
            var consultations = await _dbSet
                .Include(dc => dc.ConsultationType)
                .Where(dc => doctorIdsList.Contains(dc.DoctorId) 
                    && dc.ConsultationType.ConsultationTypeEnum == ConsultationTypeEnum.Regular)
                .AsNoTracking()
                .ToListAsync();

            // تحويل لـ Dictionary
            var fees = consultations
                .GroupBy(dc => dc.DoctorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().ConsultationFee
                );

            // إضافة الدكاترة اللي ملهمش regular consultation بقيمة 0
            foreach (var doctorId in doctorIdsList)
            {
                if (!fees.ContainsKey(doctorId))
                {
                    fees[doctorId] = 0;
                }
            }

            return fees;
        }
    }
}

