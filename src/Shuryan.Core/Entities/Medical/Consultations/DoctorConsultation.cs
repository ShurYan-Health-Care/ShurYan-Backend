using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.Medical.Consultations
{
    public class DoctorConsultation : AuditableEntity
	{
        public decimal ConsultationFee { get; set; }
        public int SessionDurationMinutes { get; set; }

        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        [ForeignKey("ConsultationType")]
        public Guid ConsultationTypeId { get; set; }

        public virtual Doctor Doctor { get; set; } = null!;
        public virtual ConsultationType ConsultationType { get; set; } = null!;
    }
}
