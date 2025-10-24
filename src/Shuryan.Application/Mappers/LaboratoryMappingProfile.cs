using AutoMapper;
using Shuryan.Application.DTOs.Requests.Laboratory;
using Shuryan.Application.DTOs.Responses.Laboratory;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.External.Laboratories;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Application.Mappers
{
    public class LaboratoryMappingProfile : Profile
    {
        public LaboratoryMappingProfile()
        {
            #region Laboratory Mappings
            CreateMap<Laboratory, LaboratoryResponse>();
            CreateMap<Laboratory, LaboratoryBasicResponse>();
            CreateMap<CreateLaboratoryRequest, Laboratory>();
            CreateMap<UpdateLaboratoryRequest, Laboratory>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Laboratory Document Mappings
            CreateMap<LaboratoryDocument, LaboratoryDocumentResponse>();
            CreateMap<CreateLaboratoryDocumentRequest, LaboratoryDocument>();
            #endregion

            #region Lab Order Mappings
            CreateMap<LabOrder, LabOrderResponse>();
            CreateMap<CreateLabOrderRequest, LabOrder>();
            #endregion

            #region Lab Test Mappings
            CreateMap<LabTest, LabTestResponse>();
            CreateMap<CreateLabTestRequest, LabTest>();
            #endregion

            #region Lab Result Mappings
            CreateMap<LabResult, LabResultResponse>();
            CreateMap<CreateLabResultRequest, LabResult>();
            CreateMap<UpdateLabResultRequest, LabResult>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Lab Service Mappings
            CreateMap<LabService, LabServiceResponse>();
            CreateMap<CreateLabServiceRequest, LabService>();
            #endregion

            #region Lab Working Hours Mappings
            CreateMap<LabWorkingHours, LabWorkingHoursResponse>();
            CreateMap<CreateLabWorkingHoursRequest, LabWorkingHours>();
            #endregion

            #region Lab Prescription Mappings
            CreateMap<LabPrescription, LabPrescriptionResponse>();
            CreateMap<LabPrescriptionItem, LabPrescriptionItemResponse>();
            #endregion
        }
    }
}
