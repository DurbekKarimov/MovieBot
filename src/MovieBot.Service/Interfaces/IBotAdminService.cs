using MovieBot.Domain.DTOs.BotAdmins;

namespace MovieBot.Service.Interfaces;

public interface IBotAdminService
{
    Task<BotAdminForResultDto> CreateBotAdminAsync(BotAdminForDto dto);
    Task<IEnumerable<BotAdminForResultDto>> GetAllBotAdminsAsync();
    Task<bool> DeleteBotAdminAsync(long telegramId);
    Task<BotAdminForResultDto> GetByTelegramIdAsync(long telegramId);
}
