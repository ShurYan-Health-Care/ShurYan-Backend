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
	public class ClinicPhoneNumber
	{
		public Guid Id { get; set; }
		public string Number { get; set; } = string.Empty;
		public ClinicPhoneNumberType Type { get; set; }

		[ForeignKey("Clinic")]
		public Guid ClinicId { get; set; }

		// Navigation Properties
		public virtual Clinic Clinic { get; set; } = null!;
	}
}