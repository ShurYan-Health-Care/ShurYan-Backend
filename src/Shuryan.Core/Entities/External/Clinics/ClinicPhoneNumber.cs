using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Clinic;

namespace Shuryan.Core.Entities.External.Clinic
{
    public class ClinicPhoneNumber : AuditableEntity
    {
        public string Number { get; set; } = string.Empty;
        public ClinicPhoneNumberType Type { get; set; }

        [ForeignKey("Clinic")]
        public Guid ClinicId { get; set; }

        // Navigation Properties
        public virtual Clinic Clinic { get; set; } = null!;
    }
}