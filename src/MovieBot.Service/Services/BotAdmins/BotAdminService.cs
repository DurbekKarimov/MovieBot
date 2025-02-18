using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MovieBot.Data.IRepositories;
using MovieBot.Domain.DTOs.BotAdmins;
using MovieBot.Domain.Entities;
using MovieBot.Domain.Exceptions;
using MovieBot.Service.Interfaces;

namespace MovieBot.Service.Services.BotAdmins;

public class BotAdminService : IBotAdminService
{
    private readonly IMapper mapper;
    private readonly IRepository<BotAdmin> repository;

    public BotAdminService(IMapper mapper, IRepository<BotAdmin> repository)
    {
        this.mapper = mapper;
        this.repository = repository;
    }
    public async Task<BotAdminForResultDto> CreateBotAdminAsync(BotAdminForDto dto)
    {
        var admin = await this.repository.GetAllAsync()
                                         .Where(a => a.TelegramUserId == dto.TelegramUserId)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();

        if (admin is not null)
            throw new MovieBotException(409, "Bot Admin is already exists");

        var mappedAdmin = this.mapper.Map<BotAdmin> (dto);
        mappedAdmin.CreatedAt = DateTime.UtcNow;

        var result = await this.repository.AddAsync(mappedAdmin);

        return this.mapper.Map<BotAdminForResultDto>(result);
    }

    public async Task<bool> DeleteBotAdminAsync(long telegramId)
    {
        var admin = await this.repository.GetAllAsync()
                                         .Where(a => a.TelegramUserId == telegramId)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();

        if (admin is null)
            throw new MovieBotException(404, "Admin is not found");
        
        return await this.repository.RemoveAsync(admin.Id);
    }

    public async Task<IEnumerable<BotAdminForResultDto>> GetAllBotAdminsAsync()
    {
        var admins = await this.repository.GetAllAsync()
                                          .AsNoTracking()
                                          .ToListAsync();
        
        return this.mapper.Map<IEnumerable<BotAdminForResultDto>>(admins);
    }

    public async Task<BotAdminForResultDto> GetByTelegramIdAsync(long telegramId)
    {
        var admin = await this.repository.GetAllAsync()
                                         .Where(a => a.TelegramUserId == telegramId)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();

        return this.mapper.Map<BotAdminForResultDto>(admin);
    }
}
