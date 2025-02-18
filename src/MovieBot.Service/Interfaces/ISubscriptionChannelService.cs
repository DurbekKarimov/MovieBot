using MovieBot.Domain.DTOs.SebscriptionChannels;

namespace MovieBot.Service.Interfaces;

public interface ISubscriptionChannelService
{
    Task<SubscriptionChannelForResultDto> CreateSubscriptionChannelAsync(SubscriptionChannelForDto dto);
    Task<SubscriptionChannelForResultDto> UpdateSubscriptionChannelAsync(long id,SubscriptionChannelForDto dto);
    Task<bool> DeleteSubscriptionChannelAsync(long id);
    Task<IEnumerable<SubscriptionChannelForResultDto>> GetAllSubscriptionChannelsAsync();
}
