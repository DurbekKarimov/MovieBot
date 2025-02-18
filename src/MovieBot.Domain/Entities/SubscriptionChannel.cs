using MovieBot.Domain.Commons;
using MovieBot.Domain.Enums;

namespace MovieBot.Domain.Entities;

public class SubscriptionChannel : Auditable
{
    public string ChannelLink { get; set; }
}
