using AutoMapper;
using Shuryan.Application.DTOs.Requests.Pharmacy;
using Shuryan.Application.DTOs.Responses.Pharmacy;
using Shuryan.Application.DTOs.Responses.Review;
using Shuryan.Core.Entities.External.Pharmacies;
using Shuryan.Core.Entities.Identity;
using Shuryan.Core.Entities.Shared;
using Shuryan.Core.Entities.System.Review;

namespace Shuryan.Application.Mappers
{
    public class PharmacyMappingProfile : Profile
    {
        public PharmacyMappingProfile()
        {
            #region Pharmacy Mappings
            CreateMap<Pharmacy, PharmacyResponse>();
            CreateMap<Pharmacy, PharmacyBasicResponse>();
            CreateMap<CreatePharmacyRequest, Pharmacy>();
            CreateMap<UpdatePharmacyRequest, Pharmacy>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Pharmacy Working Hours Mappings
            CreateMap<PharmacyWorkingHours, PharmacyWorkingHoursResponse>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToTimeSpan()))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToTimeSpan()));
            CreateMap<CreatePharmacyWorkingHoursRequest, PharmacyWorkingHours>();
            CreateMap<UpdatePharmacyWorkingHoursRequest, PharmacyWorkingHours>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Pharmacy Document Mappings
            CreateMap<PharmacyDocument, PharmacyDocumentResponse>();
            CreateMap<CreatePharmacyDocumentRequest, PharmacyDocument>();
            #endregion

            #region Pharmacy Order Mappings
            CreateMap<PharmacyOrder, PharmacyOrderResponse>();
            CreateMap<CreatePharmacyOrderRequest, PharmacyOrder>();
            CreateMap<UpdateOrderStatusRequest, PharmacyOrder>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region Pharmacy Review Mappings
            CreateMap<PharmacyReview, PharmacyReviewResponse>();
            #endregion
        }
    }
}
