using MovieBot.Domain.Enums;

namespace MovieBot.Domain.DTOs.SebscriptionChannels;

public class SubscriptionChannelForDto
{
    public string ChannelLink { get; set; }
    public ChannelState CurrentStep { get; set; } = ChannelState.None;
}
