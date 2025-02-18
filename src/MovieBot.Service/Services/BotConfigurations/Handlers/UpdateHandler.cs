using MovieBot.Domain.DTOs.BotAdmins;
using MovieBot.Domain.Enums;
using MovieBot.Service.Interfaces;
using MovieBot.Service.Services.BotConfigurations.ServiceHelpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MovieBot.Service.Services.BotConfigurations.Handlers;

public partial class UpdateHandler
{
    private readonly ITelegramBotClient botClient;
    private readonly IBotAdminService botAdminService;
    private readonly ISubscriptionChannelService subscriptionChannelService;
    private readonly IStateService<BotAdminForDto> addAdminState;
    private readonly IStateService<BotAdminForDeleteDto> deleteAdminState;

    public UpdateHandler(
        ITelegramBotClient botClient,
        IBotAdminService botAdminService,
        IStateService<BotAdminForDto> addAdminState,
        IStateService<BotAdminForDeleteDto> deleteAdminState,
        ISubscriptionChannelService subscriptionChannelService)
    {
        this.botClient = botClient;
        this.addAdminState = addAdminState;
        this.botAdminService = botAdminService;
        this.deleteAdminState = deleteAdminState;
        this.subscriptionChannelService = subscriptionChannelService;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => HandleCommandAsync(update.Message),
            UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery),
            _ => HandleNotAvailableCommandAsync(update.Message)
        };

        await handler;
    }
    private async Task HandleNotAvailableCommandAsync(Message message)
    {
        if(message is null)
        {
            return;
        }
        await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "Mavjud bo'lmagan komanda kiritildi. " +
                "Tekshirib ko'ring.");
    }
}
