using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MovieBot.Service.Services.BotConfigurations.Handlers;

public partial class UpdateHandler
{
    public async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        if(callbackQuery.Data == "check_subscription")
            await CheckSubscription(callbackQuery);
    }

    private async Task CheckSubscription(CallbackQuery callbackQuery)
    {
        var notSubscribedChannels = new List<string>();
        var channels = await this.subscriptionChannelService.GetAllSubscriptionChannelsAsync();

        await this.botClient.DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

        foreach (var channel in channels)
        {
            var chatMember = await this.botClient.GetChatMember($"@{channel.ChannelLink}", callbackQuery.From.Id);
            if (chatMember.Status == ChatMemberStatus.Left || chatMember.Status == ChatMemberStatus.Kicked)
            {
                notSubscribedChannels.Add($"https://t.me/{channel.ChannelLink}");
            }
        }
        if (!notSubscribedChannels.Any())
        {
            await this.botClient.SendMessage(
                callbackQuery.From.Id, "✅ Tabriklaymiz! Siz barcha kanallarga obuna bo‘lgansiz.\n " +
                "🎬 Kino kodini yuboring: ");
        }
        else
        {
            string messageText = "❌ Kechirasiz, siz hali quyidagi kanallarga obuna bo‘lmadingiz:\n";
            
            var retryKeyboard = notSubscribedChannels.Select((notSubscribedChannel, index) =>
            InlineKeyboardButton.WithUrl($"{index + 1}-kanal", $"{notSubscribedChannel}")).ToArray();
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                retryKeyboard,
                new[] { InlineKeyboardButton.WithCallbackData("✅ Obuna bo‘ldim", "check_subscription") }
            });

            await this.botClient.SendMessage(callbackQuery.From.Id, messageText, replyMarkup: inlineKeyboard);
        }
    }
}
