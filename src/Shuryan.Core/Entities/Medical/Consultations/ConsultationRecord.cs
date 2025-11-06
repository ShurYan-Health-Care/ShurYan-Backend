using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;

namespace Shuryan.Core.Entities.Medical.Consultations
{
    public class ConsultationRecord : AuditableEntity
	{

        [ForeignKey("Appointment")]
        public Guid AppointmentId { get; set; }

        public string ChiefComplaint { get; set; } = string.Empty; // الشكوي الاساسية: زي ارتفاع في درجة الحرارة او برد

        public string HistoryOfPresentIllness { get; set; } = string.Empty; // تاريخ المرض الحالي: بدأت الأعراض منذ يومين، مع فقدان للشهية

        public string PhysicalExamination { get; set; } = string.Empty; // الفحص الجسدي: احتقان في الحلق، صوت صفير خفيف في الصدر

        public string Diagnosis { get; set; } = string.Empty; // التشخيص: نزلة شعبية حادة.

        public string ManagementPlan { get; set; } = string.Empty; // الخطة العلاجية: راحة تامة، سوائل دافئة، علاج موسع للشعب الهوائية، متابعة بعد 3 أيام

        // Navigation Properties
        public virtual Appointment Appointment { get; set; } = null!;
    }
}
