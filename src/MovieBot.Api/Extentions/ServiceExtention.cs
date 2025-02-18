using MovieBot.Data.IRepositories;
using MovieBot.Data.Repositories;
using MovieBot.Domain.DTOs.BotAdmins;
using MovieBot.Service.Interfaces;
using MovieBot.Service.Mappings;
using MovieBot.Service.Services.BotAdmins;
using MovieBot.Service.Services.BotConfigurations.ServiceHelpers;
using MovieBot.Service.Services.SubscriptionChannels;

namespace MovieBot.Api.Extentions;

public static class ServiceExtention
{
    public static void AddCustomServices(this IServiceCollection services)
    {
        // repository
        services.AddScoped(typeof(IRepository<>),typeof(Repository<>));

        // entity
        services.AddScoped<IBotAdminService, BotAdminService>();
        services.AddScoped<ISubscriptionChannelService,SubscriptionChannelService>();

        // memotycache and state service
        services.AddMemoryCache();
        services.AddScoped(typeof(IStateService<>),typeof(StateService<>));

        // mapping profile
        services.AddAutoMapper(typeof(MappingProfile));
    }
}
