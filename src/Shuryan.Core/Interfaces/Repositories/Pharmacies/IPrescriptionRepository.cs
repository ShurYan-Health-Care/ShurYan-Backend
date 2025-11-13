using Shuryan.Core.Entities.External.Pharmacies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Interfaces.Repositories
{
    public interface IPrescriptionRepository : IGenericRepository<Prescription>
    {
        Task<Prescription?> GetPrescriptionWithDetailsAsync(Guid prescriptionId);
        Task<IEnumerable<Prescription>> GetPagedPrescriptionsForPatientAsync(Guid patientId, int pageNumber, int pageSize);
        Task<IEnumerable<Prescription>> GetAllPrescriptionsForPatientWithDetailsAsync(Guid patientId);
        Task<IEnumerable<Prescription>> GetActivePrescriptionsForPatientAsync(Guid patientId);
        Task<Prescription?> FindByPrescriptionNumberAsync(string prescriptionNumber);
        Task<IEnumerable<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Prescription>> GetPrescriptionsContainingMedicationAsync(Guid medicationId);
    }
}

