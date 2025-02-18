using MovieBot.Domain.Enums;

namespace MovieBot.Domain.DTOs.BotAdmins;

public class BotAdminForDeleteDto
{
    public long TelegramId { get; set; }
    public RemoveAdminState CurrentStep { get; set; } = RemoveAdminState.None;
}
