using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Entities.System.Review;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Entities.Identity
{
    public class Patient : ProfileUser
    {
        [ForeignKey("Address")]
        public Guid? AddressId { get; set; }

        // Emergency SOS & Medical Profile Fields
        public Enums.Medical.BloodType? BloodType { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public bool? IsPregnant { get; set; }
        public string? PhysicalDisabilities { get; set; }
        public bool EmergencyModeActive { get; set; } = false;

        [ForeignKey("EmergencyModeActivatedBy")]
        public Guid? EmergencyModeActivatedById { get; set; }
        public virtual Doctor? EmergencyModeActivatedBy { get; set; }

        // Navigation Properties
        public virtual Address? Address { get; set; }
        public virtual ICollection<MedicalHistoryItem> MedicalHistory { get; set; } = new HashSet<MedicalHistoryItem>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();
        public virtual ICollection<LabOrder> LabOrders { get; set; } = new HashSet<LabOrder>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new HashSet<Prescription>();
        public virtual ICollection<PharmacyOrder> PharmacyOrders { get; set; } = new HashSet<PharmacyOrder>();
		public virtual ICollection<DoctorReview> DoctorReviews { get; set; } = new HashSet<DoctorReview>();
		public virtual ICollection<LaboratoryReview> LaboratoryReviews { get; set; } = new HashSet<LaboratoryReview>();
		public virtual ICollection<PharmacyReview> PharmacyReviews { get; set; } = new HashSet<PharmacyReview>();


	}
}
