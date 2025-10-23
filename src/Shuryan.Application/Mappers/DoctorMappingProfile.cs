using AutoMapper;
using Shuryan.Application.DTOs.Requests.Doctor;
using Shuryan.Application.DTOs.Responses.Doctor;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Entities.Medical.Schedules;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Application.Mappers
{
    public class DoctorMappingProfile : Profile
    {
        public DoctorMappingProfile()
        {
            #region Doctor Mappings
            CreateMap<Doctor, DoctorResponse>();
            CreateMap<Doctor, DoctorBasicResponse>();
            CreateMap<CreateDoctorRequest, Doctor>();
            CreateMap<UpdateDoctorRequest, Doctor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Doctor Availability Mappings
            CreateMap<DoctorAvailability, DoctorAvailabilityResponse>();
            CreateMap<CreateDoctorAvailabilityRequest, DoctorAvailability>();
            CreateMap<UpdateDoctorAvailabilityRequest, DoctorAvailability>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Doctor Override Mappings
            CreateMap<DoctorOverride, DoctorOverrideResponse>();
            CreateMap<CreateDoctorOverrideRequest, DoctorOverride>();
            #endregion

            #region Doctor Consultation Mappings
            CreateMap<DoctorConsultation, DoctorConsultationResponse>();
            CreateMap<CreateDoctorConsultationRequest, DoctorConsultation>();
            CreateMap<UpdateDoctorConsultationRequest, DoctorConsultation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Doctor Document Mappings
            CreateMap<DoctorDocument, DoctorDocumentResponse>();
            CreateMap<CreateDoctorDocumentRequest, DoctorDocument>();
            CreateMap<UpdateDoctorDocumentRequest, DoctorDocument>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
