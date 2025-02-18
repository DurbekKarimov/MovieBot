using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MovieBot.Data.IRepositories;
using MovieBot.Domain.DTOs.SebscriptionChannels;
using MovieBot.Domain.Entities;
using MovieBot.Domain.Exceptions;
using MovieBot.Service.Interfaces;

namespace MovieBot.Service.Services.SubscriptionChannels;

public class SubscriptionChannelService : ISubscriptionChannelService
{
    private readonly IMapper mapper;
    private readonly IRepository<SubscriptionChannel> channelRepository;

    public SubscriptionChannelService(IMapper mapper,
        IRepository<SubscriptionChannel> channelRepository)
    {
        this.mapper = mapper;
        this.channelRepository = channelRepository;
    }
    public async Task<SubscriptionChannelForResultDto> CreateSubscriptionChannelAsync(SubscriptionChannelForDto dto)
    {
        var channel = await this.channelRepository.GetAllAsync()
                                                  .Where(ch => ch.ChannelLink == dto.ChannelLink)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();
        if (channel is not null)
            throw new MovieBotException(409, "Channel is already exists");

        var mappedChannel = this.mapper.Map<SubscriptionChannel>(dto);
        mappedChannel.CreatedAt = DateTime.UtcNow;

        var result = await this.channelRepository.AddAsync(mappedChannel);

        return this.mapper.Map<SubscriptionChannelForResultDto>(result);
    }

    public async Task<bool> DeleteSubscriptionChannelAsync(long id)
    {
        var channel = await this.channelRepository.GetAllAsync()
                                                  .Where(ch => ch.Id == id)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();
        if (channel is null)
            throw new MovieBotException(404, "Channel is not found");

        return await this.channelRepository.RemoveAsync(id);
    }

    public async Task<IEnumerable<SubscriptionChannelForResultDto>> GetAllSubscriptionChannelsAsync()
    {
        var channels = await this.channelRepository.GetAllAsync()
                                                   .AsNoTracking()
                                                   .ToListAsync();

        return this.mapper.Map<IEnumerable<SubscriptionChannelForResultDto>>(channels);
    }

    public async Task<SubscriptionChannelForResultDto> UpdateSubscriptionChannelAsync(long id, SubscriptionChannelForDto dto)
    {
        var channel = await this.channelRepository.GetAllAsync()
                                                  .Where(ch => ch.Id == id)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

        if (channel is null)
            throw new MovieBotException(404, "Channel is not found");

        var mappedChannel = this.mapper.Map(dto, channel);
        mappedChannel.UpdatedAt = DateTime.UtcNow;

        var result = await this.channelRepository.UpdateAsync(mappedChannel);

        return this.mapper.Map<SubscriptionChannelForResultDto>(result);
    }
}
