using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabResult
    {
        public Guid Id { get; set; }

        [ForeignKey("LabOrder")]
        public Guid LabOrderId { get; set; } // النتيجة دي خاصة بأي Order ؟ (الطلب اللي المعمل استلمه)

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; } // التحليل نفسه من جدول التحاليل اللي النتيجة ده تخصه

        // أو لو عاوز تخزن PDF
        public string? ResultFileUrl { get; set; } // رابط ملف PDF بالنتيجة

        public string? LabNotes { get; set; } // ملاحظات من المعمل

        public DateTime RecordedAt { get; set; }

        // Navigation Properties
        public virtual LabOrder LabOrder { get; set; } = null!;
        public virtual LabTest LabTest { get; set; } = null!;
    }
}
