using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;

namespace Shuryan.Core.Entities.Identity
{
	public class Patient : ProfileUser
	{
		[ForeignKey("Address")]
		public Guid? AddressId { get; set; }
		public virtual Address? Address { get; set; }
		public virtual ICollection<MedicalHistoryItem> MedicalHistory { get; set; } = new HashSet<MedicalHistoryItem>();
	}
}
