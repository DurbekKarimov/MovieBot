using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MovieBot.Service.Services.BotConfigurations.Configurations;

public class ConfigurationWebhook : IHostedService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<ConfigurationWebhook> logger;
    private readonly IServiceProvider serviceProvider;

    public ConfigurationWebhook(IConfiguration configuration, ILogger<ConfigurationWebhook> logger, IServiceProvider serviceProvider)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();

        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        await ConfigureBotCommandsAsync();


        var webhook = $@"{this.configuration["BotConfiguration:HostAddress"]}api/bot/post";
        this.logger.LogInformation("Configuring Webhook");

        await botClient.SendMessage(
            chatId: 1812987067,
            text: "webhook o'rnatilmoqda",
            cancellationToken: cancellationToken
        );


        await botClient.SetWebhook(webhook, cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();

        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        this.logger.LogInformation("Removing Webhook");

        await botClient.SendMessage(
            chatId: 1812987067,
            text: "Bot uxlamoqda",
            cancellationToken: cancellationToken
            );
    }

    public async Task ConfigureBotCommandsAsync()
    {

        using var scope = this.serviceProvider.CreateScope();

        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        var commands = new[]
        {
            new BotCommand { Command = "start", Description = "Botni ishga tushirish" }
        };

        await botClient.SetMyCommands(commands);
    }
}
