using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MovieBot.Service.Services.BotConfigurations.Handlers;

public partial class UpdateHandler
{
    public async Task HandleChannelPostAsync(Message? channelPost)
    {
        var chatId = channelPost.Chat.Id;
        var channelId = long.Parse(this.configuration["ChannelIds:ChannelId"]);

        if (chatId == channelId)
        {
            await HandleAddMovieCodeAsync(channelPost);
        }
    }

    private async Task HandleAddMovieCodeAsync(Message message)
    {
        if (message is not null && message.Video is not null)
        {
            int messageId = message.MessageId;
            await botClient.EditMessageCaption(
                chatId: message.Chat.Id,
                messageId: messageId,
                caption: $"🎬 Kino kodi: {messageId}"
            );
            return;

        }
    }
}
