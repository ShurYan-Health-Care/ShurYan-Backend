using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shuryan.Core.Entities.Base;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical;

namespace Shuryan.Core.Entities.System.Review
{
    public class DoctorReview : AuditableEntity
	{
		[ForeignKey("Appointment")]
		public Guid AppointmentId { get; set; }

		[ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		[ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }

		[Range(1, 5)]
		public int OverallSatisfaction { get; set; }

		[Range(1, 5)]
		public int WaitingTime { get; set; }

		[Range(1, 5)]
		public int CommunicationQuality { get; set; }

		[Range(1, 5)]
		public int ClinicCleanliness { get; set; }

		[Range(1, 5)]
		public int ValueForMoney { get; set; }

		[MaxLength(500)]
		public string? Comment { get; set; }
		public bool IsAnonymous { get; set; } = false;
		public bool IsEdited { get; set; } = false;

		[MaxLength(300)]
		public string? DoctorReply { get; set; }
		public DateTime? DoctorRepliedAt { get; set; }

		// Navigation Properties
		public virtual Appointment Appointment { get; set; } = null!;
		public virtual Patient Patient { get; set; } = null!;
		public virtual Doctor Doctor { get; set; } = null!;

		// Computed Property
		[NotMapped]
		public double AverageRating => (OverallSatisfaction + WaitingTime + CommunicationQuality + ClinicCleanliness + ValueForMoney) / 5.0;
	}
}