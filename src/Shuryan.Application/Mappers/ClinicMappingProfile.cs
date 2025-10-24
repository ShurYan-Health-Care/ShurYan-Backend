using AutoMapper;
using Shuryan.Application.DTOs.Requests.Clinic;
using Shuryan.Application.DTOs.Responses.Clinic;
using Shuryan.Core.Entities.External.Clinic;

namespace Shuryan.Application.Mappers
{
    public class ClinicMappingProfile : Profile
    {
        public ClinicMappingProfile()
        {
            #region Clinic Mappings
            CreateMap<Clinic, ClinicResponse>();
            CreateMap<CreateClinicRequest, Clinic>();
            CreateMap<UpdateClinicRequest, Clinic>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Clinic Service Mappings
            CreateMap<ClinicService, ClinicServiceResponse>();
            CreateMap<CreateClinicServiceRequest, ClinicService>();
            #endregion

            #region Clinic Phone Number Mappings
            CreateMap<ClinicPhoneNumber, ClinicPhoneNumberResponse>();
            CreateMap<CreateClinicPhoneNumberRequest, ClinicPhoneNumber>();
            #endregion

            #region Clinic Photo Mappings
            CreateMap<ClinicPhoto, ClinicPhotoResponse>();
            CreateMap<CreateClinicPhotoRequest, ClinicPhoto>();
            #endregion
        }
    }
}
