using AutoMapper;
using Shuryan.Application.DTOs.Requests.Consultation;
using Shuryan.Application.DTOs.Responses.Consultation;
using Shuryan.Core.Entities.Medical.Consultations;

namespace Shuryan.Application.Mappers
{
    public class ConsultationMappingProfile : Profile
    {
        public ConsultationMappingProfile()
        {
            #region Consultation Type Mappings
            CreateMap<ConsultationType, ConsultationTypeResponse>();
            CreateMap<CreateConsultationTypeRequest, ConsultationType>();
            #endregion
        }
    }
}
