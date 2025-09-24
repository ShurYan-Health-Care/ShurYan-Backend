using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.System;

namespace Shuryan.Core.Entities.Common
{
	public class ClinicPhotos
	{
		public Guid Id { get; set; }
		public string PhotoUrl { get; set; } = string.Empty;

		// Foreign Keys
		[ForeignKey("Clinic")]
		public Guid ClinicId { get; set; }

		// Navigation Properties
		public virtual Clinic Clinic { get; set; } = null!;
	}
}
