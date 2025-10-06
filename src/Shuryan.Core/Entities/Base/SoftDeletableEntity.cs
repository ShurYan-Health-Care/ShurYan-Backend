using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.Base
{
	public abstract class SoftDeletableEntity : AuditableEntity
	{
		public bool IsDeleted { get; set; } = false;
		public DateTime? DeletedAt { get; set; }
		public Guid? DeletedBy { get; set; } // User ID who deleted this record
	}
}