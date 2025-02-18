using MovieBot.Domain.Enums;

namespace MovieBot.Domain.DTOs.BotAdmins;

public class BotAdminForResultDto
{
    public long Id { get; set; }
    public long TelegramUserId { get; set; }
    public string Username { get; set; }
    public AdminRole Role { get; set; }
}
