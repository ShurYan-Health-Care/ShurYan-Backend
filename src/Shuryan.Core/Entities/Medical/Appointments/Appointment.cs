using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.System.Review;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Core.Entities.Medical.Appointments
{
    public class Appointment : AuditableEntity
	{
        [ForeignKey("Patient")]
        public Guid PatientId { get; set; }
        [ForeignKey("Doctor")]
        public Guid DoctorId { get; set; }

        [ForeignKey("PreviousAppointment")]
        public Guid? PreviousAppointmentId { get; set; }

        //[Range(0, 5)] // 0 means not rated yet
        //public int Rating { get; set; } // المريض بيقيم تجربته فالحجز واللي بردو بتشمل الكشف والدكتور هنا وال من خلالها هنحسب التقييم بتاع الدكتور
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public ConsultationTypeEnum ConsultationType { get; set; }
        public decimal ConsultationFee { get; set; }
        public int SessionDurationMinutes { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Prescription? Prescription { get; set; }
        public virtual ConsultationRecord ConsultationRecord { get; set; } = null!;
        public virtual Appointment? PreviousAppointment { get; set; }
        public virtual ICollection<Appointment> FollowUpAppointments { get; set; } = new HashSet<Appointment>();
        public virtual ICollection<LabPrescription> LabPrescription { get; set; } = new HashSet<LabPrescription>();
		public virtual DoctorReview? DoctorReview { get; set; }

	}
}
