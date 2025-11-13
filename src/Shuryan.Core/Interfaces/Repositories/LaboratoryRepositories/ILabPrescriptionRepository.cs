using Shuryan.Core.Entities.External.Laboratories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories
{
    public interface ILabPrescriptionRepository : IGenericRepository<LabPrescription>
    {
        Task<LabPrescription?> GetPrescriptionWithDetailsAsync(Guid prescriptionId);
        Task<LabPrescription?> GetByAppointmentIdAsync(Guid appointmentId);
        Task<IEnumerable<LabPrescription>> GetPagedPrescriptionsForPatientAsync(Guid patientId, int pageNumber, int pageSize);
        Task<IEnumerable<LabPrescription>> GetActivePrescriptionsForPatientAsync(Guid patientId);
        Task<IEnumerable<LabPrescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<LabPrescription>> GetPrescriptionsContainingTestAsync(Guid testId);
    }
}

