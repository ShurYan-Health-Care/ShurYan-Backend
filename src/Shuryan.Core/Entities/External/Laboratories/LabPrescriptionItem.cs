using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External.Laboratories
{
    // كل عنصر (تحليل) موجود فالروشته
    public class LabPrescriptionItem
    {
        public Guid Id { get; set; }

        [ForeignKey("LabPrescription")]
        public Guid LabPrescriptionId { get; set; } // التحليل ده يخص أي روشتة؟

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; } // التحليل نفسه من جدول التحاليل الموجوده فالسيستم

        public string? DoctorNotes { get; set; } // ملاحظات خاصة بهذا التحليل من الدكتور

        public DateTime CreatedAt { get; set; }  // التحليل ده اتضاف امته فالروشته ؟

        // Navigation Properties
        public virtual LabPrescription LabPrescription { get; set; } = null!;
        public virtual LabTest LabTest { get; set; } = null!;
    }
}
