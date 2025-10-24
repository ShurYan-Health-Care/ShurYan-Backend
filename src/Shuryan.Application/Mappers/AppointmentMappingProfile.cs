using AutoMapper;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Core.Entities.Medical.Appointments;

namespace Shuryan.Application.Mappers
{
    public class AppointmentMappingProfile : Profile
    {
        public AppointmentMappingProfile()
        {
            #region Appointment Mappings
            CreateMap<Appointment, AppointmentResponse>();
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
    }
}
