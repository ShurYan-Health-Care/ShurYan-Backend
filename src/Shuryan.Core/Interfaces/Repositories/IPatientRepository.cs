using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.DTOs;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IPatientRepository : IGenericRepository<Patient>
	{
		Task<Patient?> GetByIdWithDetailsAsync(Guid id);
		Task<Patient?> GetByEmailAsync(string email);
		Task<IEnumerable<Patient>> GetPatientsWithMedicalHistoryAsync();
        Task RemoveAsync(Patient patient, bool softDelete = true);
		Task<(IEnumerable<DoctorPatientDto> Patients, int TotalCount)> GetDoctorPatientsOptimizedAsync(
			Guid doctorId, 
			int pageNumber, 
			int pageSize);
		
		/// <summary>
		/// جلب المريض مع العنوان فقط (للبحث عن معامل/صيدليات قريبة)
		/// </summary>
		Task<Patient?> GetPatientWithAddressAsync(Guid patientId);
    }
}
