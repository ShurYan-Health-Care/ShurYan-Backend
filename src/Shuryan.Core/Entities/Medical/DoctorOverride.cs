using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.Medical
{
	// الكلاس د ال هتبقى مسؤولة عن اضافة مواعيد استثنائية ف ايام معينة
	public class DoctorOverride
	{
		public Guid Id { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		[ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }


		// Navigation Properties
		public virtual Doctor Doctor { get; set; } = null!;
	}
}