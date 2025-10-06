using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Entities.Medical.Consultations
{
    public class ConsultationType
	{
        public Guid Id { get; set; }
        public ConsultationTypeEnum ConsultationTypeEnum { get; set; }
        public virtual ICollection<DoctorConsultation> Consultations { get; set; } = new HashSet<DoctorConsultation>();
    }
}
