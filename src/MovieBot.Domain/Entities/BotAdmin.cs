using MovieBot.Domain.Commons;
using MovieBot.Domain.Enums;

namespace MovieBot.Domain.Entities;

public class BotAdmin : Auditable
{
    public long TelegramUserId { get; set; }
    public string Username { get; set; }
    public AdminRole Role { get; set; }

}
