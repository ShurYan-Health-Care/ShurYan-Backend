namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// روشتة تحاليل للمريض
        /// </summary>
        public class PatientLabPrescriptionResponse
        {
                public Guid Id { get; set; }
                public Guid AppointmentId { get; set; }

                // Doctor Info
                public Guid DoctorId { get; set; }
                public string DoctorName { get; set; } = string.Empty;
                public string? DoctorSpecialty { get; set; }
                public string? DoctorProfileImage { get; set; }

                public string? GeneralNotes { get; set; }
                public DateTime CreatedAt { get; set; }

                // Tests
                public List<PatientLabPrescriptionItemResponse> Tests { get; set; } = new();

                // Order Status
                public bool HasOrder { get; set; }
                public Guid? LabOrderId { get; set; }
                public string? OrderStatus { get; set; }
        }

        public class PatientLabPrescriptionItemResponse
        {
                public Guid Id { get; set; }
                public Guid LabTestId { get; set; }
                public string TestName { get; set; } = string.Empty;
                public string TestCode { get; set; } = string.Empty;
                public string Category { get; set; } = string.Empty;
                public string? SpecialInstructions { get; set; }
                public string? DoctorNotes { get; set; }
        }
}
