using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.External;
using Shuryan.Core.Entities.Identity;

namespace Shuryan.Core.Entities.External.Laboratories
{
    /// <summary>
    /// جدول بيربط بين ال Laboratory وال LabTest -> (Many-to-Many)
    /// عشان نحدد كل معمل بيقدم أي تحليل وبكام
    /// </summary>
    public class LabService
    {
        public Guid Id { get; set; }

        [ForeignKey("Laboratory")]
        public Guid LaboratoryId { get; set; } // المعمل ال بيقدم التحليل

        [ForeignKey("LabTest")]
        public Guid LabTestId { get; set; } // التحليل نفسه

        public decimal Price { get; set; } // سعر التحليل في المعمل ده
        public bool IsAvailable { get; set; } = true; // هل التحليل متاح حالياً - عشان ف حال ان ف مشاكل ف نوع تحليل معين
        public string? LabSpecificNotes { get; set; } // ملاحظات خاصة بالتحليل ده في المعمل (مثلاً: "النتيجة بعد يومين" أو "لازم صيام 12 ساعة")

        // Navigation Properties
        public virtual Laboratory Laboratory { get; set; } = null!; // علاقة Many-to-One: الخدمة دي تبع معمل واحد بس - لاكن المعمل ممكن يقدم خدمات كتير
        public virtual LabTest LabTest { get; set; } = null!; // علاقة Many-to-One: الخدمة دي بتشير لتحليل واحد من الـ Master Data - لاكن الماستر داتا فيه تحاليل كتيره
    }
}
