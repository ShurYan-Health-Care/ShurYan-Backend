using AutoMapper;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Entities.Common;
using Shuryan.Core.Entities.Shared;

namespace Shuryan.Application.Mappers
{
    public class CommonMappingProfile : Profile
    {
        public CommonMappingProfile()
        {
            #region Address Mappings
            CreateMap<Address, AddressResponse>();
            CreateMap<CreateAddressRequest, Address>();
            CreateMap<UpdateAddressRequest, Address>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
