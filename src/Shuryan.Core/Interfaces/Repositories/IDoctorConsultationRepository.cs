using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Medical.Consultations;

namespace Shuryan.Core.Interfaces.Repositories
{
	public interface IDoctorConsultationRepository : IGenericRepository<DoctorConsultation>
	{
		Task<IEnumerable<DoctorConsultation>> GetByDoctorIdAsync(Guid doctorId);
		Task<DoctorConsultation?> GetByDoctorIdAndConsultationTypeIdAsync(Guid doctorId, Guid consultationTypeId);
		
		/// <summary>
		/// جلب أسعار الكشف العادي لمجموعة دكاترة دفعة واحدة (batch operation)
		/// </summary>
		Task<Dictionary<Guid, decimal>> GetRegularConsultationFeesForDoctorsAsync(IEnumerable<Guid> doctorIds);
	}
}
