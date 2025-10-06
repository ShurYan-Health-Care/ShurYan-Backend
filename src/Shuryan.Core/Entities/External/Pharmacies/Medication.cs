using Shuryan.Core.Entities.Base;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    /// <summary>
    /// بيمثل الدواء نفسه في قاعدة البيانات المركزية (كتالوج الأدوية)
    /// </summary>
    public class Medication : AuditableEntity
	{
        public string BrandName { get; set; }
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public DosageForm DosageForm { get; set; }

        public virtual ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new HashSet<PrescribedMedication>();
    }
}
