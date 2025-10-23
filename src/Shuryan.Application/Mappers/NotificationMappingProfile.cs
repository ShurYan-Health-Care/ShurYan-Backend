using AutoMapper;
using Shuryan.Application.DTOs.Requests.Notification;
using Shuryan.Application.DTOs.Responses.Notification;
using Shuryan.Core.Entities.System;

namespace Shuryan.Application.Mappers
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            #region Notification Mappings
            CreateMap<Notification, NotificationResponse>();
            CreateMap<CreateNotificationRequest, Notification>();
            #endregion
        }
    }
}
