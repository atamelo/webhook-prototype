using AutoMapper;
using WebHook.SubscriptionSotre.Client.Models;
using WebHook.SubscriptionStore.Client.Postgres.Entities;

namespace WebHook.SubscriptionStore.Client.Postgres.Extensions;

public class SubscriptionProfile : Profile
{
    public SubscriptionProfile()
    {
        CreateMap<SubscriptionEntity, SubscriptionDto>()
            .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.event_id))
            .ForMember(dest => dest.SubscriberId, opt => opt.MapFrom(src => src.subscriber_id))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.url))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.active))
            .ReverseMap();
    }
}
