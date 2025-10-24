using AutoMapper;

namespace Shuryan.Application.Mappers
{
    /// <summary>
    /// Master AutoMapper Profile - All mappings are organized into separate profile files
    /// AutoMapper automatically discovers all Profile classes in the assembly
    /// 
    /// Mapping Profiles Organization:
    /// - CommonMappingProfile: Address and common DTOs
    /// - PatientMappingProfile: Patient and Medical History mappings
    /// - DoctorMappingProfile: Doctor, Availability, Consultation, and Document mappings
    /// - PharmacyMappingProfile: Pharmacy, Orders, Working Hours, and Document mappings
    /// - LaboratoryMappingProfile: Laboratory, Lab Orders, Tests, Results, and Services
    /// - ClinicMappingProfile: Clinic, Services, Phone Numbers, and Photos
    /// - AppointmentMappingProfile: Appointments and Consultation Records
    /// - PrescriptionMappingProfile: Prescriptions, Medications, and Prescribed Medications
    /// - ReviewMappingProfile: Doctor, Pharmacy, and Laboratory Reviews
    /// - NotificationMappingProfile: Notification mappings
    /// - ConsultationMappingProfile: Consultation Type mappings
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // This profile is kept for backward compatibility and documentation
            // All actual mappings are in their respective domain-specific profile files
            // AutoMapper will automatically discover and register all Profile classes
        }
    }
}
