using Microsoft.Extensions.Configuration;
using MovieBot.Domain.DTOs.BotAdmins;
using MovieBot.Domain.DTOs.SebscriptionChannels;
using MovieBot.Service.Interfaces;
using MovieBot.Service.Services.BotConfigurations.ServiceHelpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MovieBot.Service.Services.BotConfigurations.Handlers;

public partial class UpdateHandler
{
    private readonly ITelegramBotClient botClient;
    private readonly IConfiguration configuration;
    private readonly IBotAdminService botAdminService;
    private readonly ISubscriptionChannelService subscriptionChannelService;
    private readonly IStateService<BotAdminForDto> addAdminState;
    private readonly IStateService<BotAdminForDeleteDto> deleteAdminState;
    private readonly IStateService<SubscriptionChannelForDto> subscriptionChannelState;

    public UpdateHandler(
        ITelegramBotClient botClient,
        IBotAdminService botAdminService,
        IStateService<BotAdminForDto> addAdminState,
        IConfiguration configuration,
        IStateService<BotAdminForDeleteDto> deleteAdminState,
        IStateService<SubscriptionChannelForDto> subscriptionChannelState,
        ISubscriptionChannelService subscriptionChannelService)
    {
        this.botClient = botClient;
        this.addAdminState = addAdminState;
        this.botAdminService = botAdminService;
        this.configuration = configuration;
        this.deleteAdminState = deleteAdminState;
        this.subscriptionChannelService = subscriptionChannelService;
        this.subscriptionChannelState = subscriptionChannelState;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => HandleCommandAsync(update.Message),
            UpdateType.ChannelPost => HandleChannelPostAsync(update.ChannelPost),
            UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery),
            _ => HandleNotAvailableCommandAsync(update.Message)
        };

        await handler;
    }



    private async Task HandleNotAvailableCommandAsync(Message message)
    {
        if(int.TryParse(message.Text,out int id) || message is null)
            return;

        await this.botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "❌ Noto‘g‘ri buyruq"
        );
    }
}
