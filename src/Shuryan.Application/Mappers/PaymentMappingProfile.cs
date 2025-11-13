using AutoMapper;
using Shuryan.Application.DTOs.Responses.Payment;
using Shuryan.Core.Entities.External.Payments;
using System.ComponentModel;
using System.Linq;

namespace Shuryan.Application.Mappers
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Payment, PaymentResponse>()
                .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => GetEnumDescription(src.PaymentMethod)))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetEnumDescription(src.Status)))
                .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.Provider.HasValue ? GetEnumDescription(src.Provider.Value) : null))
                .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions));

            CreateMap<PaymentTransaction, PaymentTransactionResponse>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetEnumDescription(src.Status)));
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }
    }
}
