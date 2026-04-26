using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shuryan.Application.DTOs.Responses.Emergency;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class EmergencyAuditService : IEmergencyAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmergencyAuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogActionAsync(Guid doctorId, Guid patientId, EmergencyActionType action)
        {
            var auditLog = new EmergencyAuditLog
            {
                DoctorId = doctorId,
                PatientId = patientId,
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.EmergencyAuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmergencyAuditLogResponse>> GetAuditLogsAsync(Guid? doctorId = null, Guid? patientId = null)
        {
            var query = _unitOfWork.EmergencyAuditLogs.GetQueryable();

            if (doctorId.HasValue)
                query = query.Where(l => l.DoctorId == doctorId.Value);

            if (patientId.HasValue)
                query = query.Where(l => l.PatientId == patientId.Value);

            var logs = await query
                .Include(l => l.Doctor)
                .Include(l => l.Patient)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return logs.Select(l => new EmergencyAuditLogResponse
            {
                Id = l.Id,
                DoctorId = l.DoctorId,
                DoctorName = l.Doctor != null ? $"{l.Doctor.FirstName} {l.Doctor.LastName}" : string.Empty,
                PatientId = l.PatientId,
                PatientName = l.Patient != null ? $"{l.Patient.FirstName} {l.Patient.LastName}" : string.Empty,
                Timestamp = l.Timestamp,
                Action = l.Action
            });
        }
    }
}
