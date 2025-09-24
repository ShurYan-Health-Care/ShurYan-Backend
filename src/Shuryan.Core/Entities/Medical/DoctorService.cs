using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Medical
{
	// الكلاس د ال هتبقى مسؤولة عن الخدمات اللي بيقدمها الدكتور
	// بما انه الدكتور ممكن يقدم اكتر من خدمة وكل خدمة ممكن يقدمها اكتر من دكتور
	// ف احنا محتاجين Junction Table ما بين الدكتور والخدمة
	// Doctor Entity (M) <-> DoctorService <-> (M) ConsultationType Enum
	public class DoctorService
	{
		public Guid Id { get; set; }
		public ConsultationType ConsultationType { get; set; }
		public decimal ConsultationFee { get; set; }
		public int SessionDurationMinutes { get; set; }

		[ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }


		// Navigation Properties
		public virtual Doctor Doctor { get; set; } = null!;
	}
}
