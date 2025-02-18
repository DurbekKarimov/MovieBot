using MovieBot.Domain.Enums;

namespace MovieBot.Domain.DTOs.BotAdmins;

public class BotAdminForDto
{
    public long TelegramUserId { get; set; }
    public string Username { get; set; }
    public AdminRole Role { get; set; }
    public AddAdminState CurrentStep { get; set; } = AddAdminState.None;

}
