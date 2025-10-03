using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Core.Entities.Identity
{
    public class Patient : ProfileUser
    {
        [ForeignKey("Address")]
        public Guid? AddressId { get; set; }

        // Navigation Properties
        public virtual Address? Address { get; set; }
        public virtual ICollection<MedicalHistoryItem> MedicalHistory { get; set; } = new HashSet<MedicalHistoryItem>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
        public virtual ICollection<LabOrder> LabOrders { get; set; } = new HashSet<LabOrder>();
    }
}
