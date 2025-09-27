using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.System;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Common
{
	public class ClinicService
	{
		public Guid Id { get; set; }
		public ClinicServiceType ServiceType { get; set; }

		[ForeignKey("Clinic")]
		public Guid ClinicId { get; set; }

		// Navigation property to the Clinic
		public virtual Clinic Clinic { get; set; } = null!;
	} 
}