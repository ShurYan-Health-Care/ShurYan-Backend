using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shuryan.Core.Enums;

namespace Shuryan.Core.Entities.Identity
{
    public class Verifier : User
    {
        // I need to make it required after Admin module is ready
        public Guid? CreatedByAdminId { get; set; }

        // Navigation Properties
        public virtual ICollection<Doctor> VerifiedDoctors { get; set; } = new HashSet<Doctor>();
        public virtual ICollection<Laboratory> VerifiedLabors { get; set; } = new HashSet<Laboratory>();
        public virtual ICollection<Pharmacy> VerifiedPharmacies { get; set; } = new HashSet<Pharmacy>();
    }
}
