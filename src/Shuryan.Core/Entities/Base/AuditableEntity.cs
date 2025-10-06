using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.Base
{
	public abstract class AuditableEntity
	{
		[Key]
		public Guid Id { get; set; }

		public DateTime CreatedAt { get; set; }
		public Guid? CreatedBy { get; set; } // User ID who created this records

		public DateTime? UpdatedAt { get; set; }
		public Guid? UpdatedBy { get; set; } // User ID who last updated this record
	}
}