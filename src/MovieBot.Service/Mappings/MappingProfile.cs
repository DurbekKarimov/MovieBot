using AutoMapper;
using MovieBot.Domain.DTOs.BotAdmins;
using MovieBot.Domain.DTOs.SebscriptionChannels;
using MovieBot.Domain.Entities;

namespace MovieBot.Service.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // BotAdmin mappings
        CreateMap<BotAdmin, BotAdminForDto>().ReverseMap();
        CreateMap<BotAdmin, BotAdminForResultDto>().ReverseMap();
        // SubscriptionChannel mappings
        CreateMap<SubscriptionChannel, SubscriptionChannelForDto>().ReverseMap();
        CreateMap<SubscriptionChannel, SubscriptionChannelForResultDto>().ReverseMap();
    }
}
