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
	// الكلاس د ال هتبقى مسؤولة عن اضافة او حذف مواعيد استثنائية ف ايام معينة
	public class DoctorOverride
	{
		public Guid Id { get; set; }

		[ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public OverrideType Type { get; set; } // Available or Unavailable


		// Navigation Properties
		public virtual Doctor Doctor { get; set; } = null!;
	}
}