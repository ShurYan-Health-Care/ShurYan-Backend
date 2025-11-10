using AutoMapper;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Entities.Medical.Consultations;

namespace Shuryan.Application.Mappers
{
    public class AppointmentMappingProfile : Profile
    {
        public AppointmentMappingProfile()
        {
            #region Appointment Mappings
            CreateMap<Appointment, AppointmentResponse>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => 
                    src.Patient != null ? $"{src.Patient.FirstName} {src.Patient.LastName}" : null))
                .ForMember(dest => dest.PatientAge, opt => opt.MapFrom(src => 
                    src.Patient != null && src.Patient.BirthDate.HasValue 
                        ? CalculateAge(src.Patient.BirthDate.Value) 
                        : (int?)null))
                .ForMember(dest => dest.PatientProfileImageUrl, opt => opt.MapFrom(src => 
                    src.Patient != null ? src.Patient.ProfileImageUrl : null))
                .ForMember(dest => dest.PrescriptionId, opt => opt.MapFrom(src => 
                    src.Prescription != null ? src.Prescription.Id : (Guid?)null));
            
            CreateMap<CreateAppointmentRequest, Appointment>();
            CreateMap<UpdateAppointmentRequest, Appointment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Consultation Record Mappings
            CreateMap<ConsultationRecord, ConsultationRecordResponse>();
            CreateMap<CreateConsultationRecordRequest, ConsultationRecord>();
            CreateMap<UpdateConsultationRecordRequest, ConsultationRecord>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }

        /// <summary>
        /// حساب العمر من تاريخ الميلاد
        /// </summary>
        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            // لو لسه ما جاش عيد ميلاده السنة دي، نطرح سنة
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            
            return age;
        }
    }
}
