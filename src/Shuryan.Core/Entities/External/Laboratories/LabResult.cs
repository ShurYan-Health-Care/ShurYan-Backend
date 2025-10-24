using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.External.Laboratories
{
    public class LabResult : AuditableEntity
	{

        [ForeignKey("LabOrder")]
        public Guid LabOrderId { get; set; } // النتيجة دي خاصة بأي Order ؟ (الطلب اللي المعمل استلمه)

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; } // التحليل نفسه من جدول التحاليل اللي النتيجة ده تخصه

        public string ResultValue { get; set; } = string.Empty; // قيمة النتيجة (نص أو رقم)
        
        public string? ReferenceRange { get; set; } // المدى الطبيعي للنتيجة
        
        public string? Unit { get; set; } // وحدة القياس (mg/dL, g/dL, etc.)
        
        public string? Notes { get; set; } // ملاحظات من المعمل
        
        public string? AttachmentUrl { get; set; } // رابط ملف PDF بالنتيجة

        // Navigation Properties
        public virtual LabOrder LabOrder { get; set; } = null!;
        public virtual LabTest LabTest { get; set; } = null!;
    }
}
