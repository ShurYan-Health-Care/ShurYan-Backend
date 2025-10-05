using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External.Pharmacies
{
    /// <summary>
    /// بيمثل دواء واحد (سطر واحد) داخل روشتة الأدوية
    /// </summary>
    public class PrescribedMedication
    {
        public string Dosage { get; set; } // الجرعة: "قرص واحد"
        public string Frequency { get; set; } // التكرار: "3 مرات يوميًا"
        public int DurationDays { get; set; } // المدة بالأيام
        public string? SpecialInstructions { get; set; } // تعليمات خاصة

        [ForeignKey("MedicationPrescription")]
        public Guid MedicationPrescriptionId { get; set; }

        [ForeignKey("Medication")]
        public Guid MedicationId { get; set; }

        public virtual Prescription MedicationPrescription { get; set; } = null!;
        public virtual Medication Medication { get; set; } = null!;
    }
}
