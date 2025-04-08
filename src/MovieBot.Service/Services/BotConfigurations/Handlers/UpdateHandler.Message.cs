using MovieBot.Domain.DTOs.BotAdmins;
using MovieBot.Domain.Enums;
using MovieBot.Domain.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MovieBot.Service.Services.BotConfigurations.Handlers;

public partial class UpdateHandler
{
    public async Task HandleCommandAsync(Message message)
    {
        var userId = message.From.Id;
        var addAdminState = this.addAdminState.GetState($"{userId}_addadmin");
        var deleteAdminState = this.deleteAdminState.GetState($"{userId}_deleteadmin");
        var addChannelState = this.subscriptionChannelState.GetState($"{userId}_addchannel");
        var deleteChannelState = this.subscriptionChannelState.GetState($"{userId}_removechannel");

        if (addChannelState is not null && addChannelState.CurrentStep != ChannelState.None)
        {
            if (addChannelState.CurrentStep == ChannelState.ChannelLink)
                await HandleAddChannelLinkAsync(message);
            return;
        }
        if(deleteChannelState is not null && deleteChannelState.CurrentStep != ChannelState.None)
        {
            if (deleteChannelState.CurrentStep == ChannelState.ChannelLink)
                await HandleRemoveChannelLinkAsync(message);
            return;
        }
        if (addAdminState is not null && addAdminState.CurrentStep != AddAdminState.None)
        {
            if (addAdminState.CurrentStep == AddAdminState.Username)
                await HandleAddAdminUsernameAsync(message);
            else if (addAdminState.CurrentStep == AddAdminState.TelegramId)
                await HandleAddAdminTelegramIdAsync(message);
            return;
        }
        if (deleteAdminState is not null && deleteAdminState.CurrentStep != RemoveAdminState.None)
        {
            if (deleteAdminState.CurrentStep == RemoveAdminState.TelegramId)
                await HandleDeleteAdminTelegramIdAsync(message);
            return;
        }
        if (int.TryParse(message.Text, out int messageId))
        {
            await HandleSendMovieAsync(message, messageId);
        }
        var command = message.Text.Split(' ').First();
        var handler = command switch
        {
            "/start" => HandleStartCommandAsync(message),
            "/admins" => HandleAdminsCommandAsync(message),
            "/addadmin" => HandleAddAdminCommandAsync(message),
            "/removeadmin" => HandleRemoveAdminCommandAsync(message),
            "/channels" => HandleChannelsCommandAsync(message),
            "/addchannel" => HandleAddChannelCommandAsync(message),
            "/removechannel" => HandleRemoveChannelCommandAsync(message),
            _ => HandleNotAvailableCommandAsync(message)
        };
        await handler;
    }

    private async Task HandleSendMovieAsync(Message message, int messageId)
    {
        var channelId = long.Parse(this.configuration["ChannelIds:ChannelId"]);

        try
        {

            
            await this.botClient.CopyMessage(
                chatId: message.From.Id,
                fromChatId: channelId,
                messageId: messageId
            );
            
        }
        catch (ApiRequestException ex)
        {
            if (ex.ErrorCode == 400 || ex.ErrorCode == 404)
            {
                await this.botClient.SendMessage(
                    chatId: message.From.Id,
                    text: "⚠️ Ushbu kodda kino mavjud emas!"
                );
            }
            else
            {
                await this.botClient.SendMessage(
                    chatId: message.From.Id,
                    text: "❌ Xatolik yuz berdi. Keyinroq qayta urinib ko'ring."
                );
            }
        }
    }




    #region RemoveChannel
    private async Task HandleRemoveChannelLinkAsync(Message message)
    {
        var userId = message.From.Id;
        var state = this.subscriptionChannelState.GetState($"{userId}_removechannel");

        state.ChannelLink = message.Text;
        state.CurrentStep = ChannelState.None;
        var delete = await this.subscriptionChannelService.DeleteSubscriptionChannelAsync(state.ChannelLink);
        if (delete)
        {
            this.subscriptionChannelState.ClearState($"{userId}_removechannel");
            await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "✅Channel muvaffaqiyatli o'chirildi"
                );
        }
        else
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Ushbu linkda kanal mavjud emas!"
                );
            this.subscriptionChannelState.ClearState($"{userId}_removechannel");
        }
    }
    private async Task HandleRemoveChannelCommandAsync(Message message)
    {
        var userId = message.From.Id;
        var tgUser = await this.botAdminService.GetByTelegramIdAsync(userId);

        if (tgUser is not null && tgUser.Role == AdminRole.SuperAdmin || tgUser.Role == AdminRole.Admin)
        {
            this.subscriptionChannelState.ClearState($"{userId}_removechannel");
            var state = this.subscriptionChannelState.GetState($"{userId}_removechannel");

            state.CurrentStep = ChannelState.ChannelLink;
            this.subscriptionChannelState.SetState($"{userId}_removechannel", state);

            await this.botClient.SendMessage(
            chatId: userId,
            text: "⚠️ Diqqat! \n🛡 O‘chirmoqchi bo‘lgan kanal linkini kiriting: ");

        }
        else
        {
            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "❌ Siz ushbu komandani ishlata olmaysiz!!!");
        }
    }
    #endregion

    #region AddChannels
    private async Task HandleAddChannelLinkAsync(Message message)
    {
        var userId = message.From.Id;
        var stateChannel = this.subscriptionChannelState.GetState($"{userId}_addchannel");

        stateChannel.ChannelLink = message.Text;
        stateChannel.CurrentStep = ChannelState.None;
        string channelUsername = message.Text.StartsWith("@") ? message.Text : "@" + message.Text;

        try
        {
            var chatMember = await this.botClient.GetChatMember(channelUsername, botClient.BotId);

            if (chatMember.Status != ChatMemberStatus.Administrator && chatMember.Status != ChatMemberStatus.Creator)
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ Bot ushbu kanalda admin emas \nAdmin qilib qayta urining!!!"
                );
                return;
            }

            await this.subscriptionChannelService.CreateSubscriptionChannelAsync(stateChannel);
            this.subscriptionChannelState.ClearState($"{userId}_addchannel");

            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "✅ Kanal muvaffaqiyatli qo'shildi"
            );
        }
        catch (MovieBotException ex) when (ex.StatusCode == 409)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Ushbu kanal allaqachon qo'shilgan!!!"
            );
        }
        catch (ApiRequestException ex)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Xatolik: Kanal topilmadi yoki bot kanalga qo‘shilmagan. \nIltimos, tekshirib qayta urining!"
            );
        }
    }


    private async Task HandleAddChannelCommandAsync(Message message)
    {
        var userId = message.From.Id;
        var tgUser = await this.botAdminService.GetByTelegramIdAsync(userId);

        if (tgUser is not null && tgUser.Role == AdminRole.SuperAdmin || tgUser.Role == AdminRole.Admin)
        {
            this.subscriptionChannelState.ClearState($"{userId}_addchannel");
            var stateChannel = this.subscriptionChannelState.GetState($"{userId}_addchannel");
            if (stateChannel is not null && stateChannel.CurrentStep == ChannelState.None)
            {
                stateChannel.CurrentStep = ChannelState.ChannelLink;
                this.subscriptionChannelState.SetState($"{userId}_addchannel", stateChannel);
                await this.botClient.SendMessage(
                    chatId: userId,
                    text: "🔗 Kanal linkini yuboring:(@ belgisiz) ");
            }
            else
            {
                await this.botClient.SendMessage(
                    chatId: userId,
                    text: "Uzr qandaydir xatolik bo'ldi. Qayta urining");
            }
        }
        else
        {
            await this.botClient.SendMessage(
            chatId: userId,
            text: "❌ Siz ushbu komandani ishlata olmaysiz!!!"
            );
        }
    }
    #endregion  

    #region Channels
    private async Task HandleChannelsCommandAsync(Message message)
    {
        var tgUser = await this.botAdminService.GetByTelegramIdAsync(message.From.Id);

        if (tgUser is not null && (tgUser.Role == AdminRole.SuperAdmin || tgUser.Role == AdminRole.Admin))
        {
            var channels = await this.subscriptionChannelService.GetAllSubscriptionChannelsAsync();
            var channelList = channels.ToArray();
            var channelLinks = new List<string>();

            string result = string.Join("\n", channelList.Select((ch, i) => $"{i + 1}. {ch.ChannelLink}"));

            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: $"📋 Homiy kanallaringiz:\n{result}");
        }
        else
        {
            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "❌ Siz ushbu komandani ishlata olmaysiz!!!"
            );
        }
    }
    #endregion 

    #region RemoveAdmin
    private async Task HandleDeleteAdminTelegramIdAsync(Message message)
    {
        var userId = message.From.Id;
        var state = this.deleteAdminState.GetState($"{userId}_deleteadmin");

        if (long.TryParse(message.Text, out long telegramId))
        {
            state.TelegramId = telegramId;
            state.CurrentStep = RemoveAdminState.None;
            var delete = await this.botAdminService.DeleteBotAdminAsync(telegramId);
            if (delete)
            {
                this.deleteAdminState.ClearState($"{userId}_deleteadmin");

                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "✅Admin muvaffaqiyatli o'chirildi"
                );
            }
            else
            {
                await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Ushbu id da admin mavjud emas!"
                );
            }

        }
        else
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Iltimos, faqat raqam kiriting!"
            );
            return;
        }
    }
    private async Task HandleRemoveAdminCommandAsync(Message message)
    {
        var userId = message.From.Id;
        var tgUser = await this.botAdminService.GetByTelegramIdAsync(userId);

        if (tgUser is not null && tgUser.Role == AdminRole.SuperAdmin)
        {
            this.deleteAdminState.ClearState($"{userId}_deleteadmin");
            var state = this.deleteAdminState.GetState($"{userId}_deleteadmin");
            if (state is null || state.CurrentStep == RemoveAdminState.None)
            {
                state = new BotAdminForDeleteDto
                {
                    CurrentStep = RemoveAdminState.TelegramId
                };
                this.deleteAdminState.SetState($"{userId}_deleteadmin", state);
            }

            this.deleteAdminState.SetState($"{userId}_deleteadmin", state);
            await this.botClient.SendMessage(
            chatId: userId,
            text: "⚠️ Diqqat! \n🛡 O‘chirmoqchi bo‘lgan adminning Telegram ID sini kiriting: ");

        }
        else
        {
            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "❌ Siz ushbu komandani ishlata olmaysiz!!!");
        }
    }

    #endregion

    #region AddAdmin
    private async Task HandleAddAdminTelegramIdAsync(Message message)
    {
        var userId = message.From.Id;
        var adminState = this.addAdminState.GetState($"{userId}_addadmin");

        if (long.TryParse(message.Text, out long telegramId))
        {
            adminState.TelegramUserId = telegramId;
            adminState.CurrentStep = AddAdminState.None;

            try
            {
                await this.botAdminService.CreateBotAdminAsync(adminState);
                this.addAdminState.ClearState($"{userId}_addadmin");

                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "✅ Admin muvaffaqiyatli qo'shildi"
                );
            }
            catch (MovieBotException ex) when (ex.StatusCode == 409)  // ❗ 409 xatolikni ushlash
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ Bu foydalanuvchi allaqachon admin!"
                );
            }
            catch (Exception ex) // ❗ Boshqa xatoliklar uchun
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ Xatolik yuz berdi: {ex.Message}"
                );
            }
        }
        else
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Iltimos, faqat raqam kiriting!"
            );
        }
    }


    private async Task HandleAddAdminUsernameAsync(Message message)
    {
        var userId = message.From.Id;
        var adminState = this.addAdminState.GetState($"{userId}_addadmin");

        adminState.CurrentStep = AddAdminState.TelegramId;
        adminState.Username = message.Text;

        this.addAdminState.SetState($"{userId}_addadmin", adminState);

        await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "🆔 UserId yuboring: ");
    }

    private async Task HandleAddAdminCommandAsync(Message message)
    {
        var userId = message.From.Id;
        var tgUser = await this.botAdminService.GetByTelegramIdAsync(userId);
        if (tgUser is not null && tgUser.Role == AdminRole.SuperAdmin)
        {
            this.addAdminState.ClearState($"{userId}_addadmin");
            var addState = this.addAdminState.GetState($"{userId}_addadmin");
            if (addState is not null && addState.CurrentStep == AddAdminState.None)
            {
                addState.CurrentStep = AddAdminState.Username;
                this.addAdminState.SetState($"{userId}_addadmin", addState);
                await this.botClient.SendMessage(
                    chatId: userId,
                    text: "👤 Username yuboring (@ belgisiz): ");
            }
            else
            {
                // Handle the case where addState is null
                await this.botClient.SendMessage(
                    chatId: userId,
                    text: "Uzr qandaydir xatolik bo'ldi. Qayta urining");
            }
        }
        else
        {
            await this.botClient.SendMessage(
                chatId: userId,
                text: "❌ Siz ushbu komandani ishlata olmaysiz!!!");
        }
    }

    #endregion 

    #region Admins
    private async Task HandleAdminsCommandAsync(Message message)
    {
        var tgUser = await this.botAdminService.GetByTelegramIdAsync(message.From.Id);
        if (tgUser is not null && tgUser.Role == AdminRole.SuperAdmin)
        {
            var users = await this.botAdminService.GetAllBotAdminsAsync();
            var admins = new List<string>();
            foreach (var user in users)
            {
                if (user.Role == AdminRole.Admin)
                    admins.Add($"{user.Username} - {user.TelegramUserId}");

            }
            var adminList = string.Join("\n", admins);

            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: $"📋 Sizning adminlaringiz:\n{adminList}");
        }
        else
            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "❌ Siz ushbu komandani ishlata olmaysiz!!!");

    }
    #endregion

    #region Start
    private async Task HandleStartCommandAsync(Message message)
    {
        var user = await this.botAdminService.GetByTelegramIdAsync(message.From.Id);

        if (user is not null && user.Role == AdminRole.SuperAdmin)
        {
            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "Siz super admin sifatida quyidagi buyruqlardan foydalanishingiz mumkin:\n\n" +
                        "/admins - Adminlar ro'yxati\n" +
                        "/addadmin – Yangi admin qo‘shish\n" +
                        "/removeadmin – Adminni olib tashlash\n" +
                        "/channels – Kanallar ro‘yxatini ko‘rish\n" +
                        "/addchannel – Yangi kanal qo‘shish\n" +
                        "/removechannel – Kanalni olib tashlash\n");

        }
        else if (user is not null && user.Role == AdminRole.Admin)
        {
            await this.botClient.SendMessage(
                chatId: message.From.Id,
                text: "Siz admin sifatida quyidagi buyruqlardan foydalanishingiz mumkin:\n\n" +
                        "/channels – Kanallar ro‘yxatini ko‘rish\n" +
                        "/addchannel – Yangi kanal qo‘shish\n" +
                        "/removechannel – Kanalni olib tashlash\n");
        }

        else
        {

            var notSubscribedChannels = new List<string>();
            var channels = await this.subscriptionChannelService.GetAllSubscriptionChannelsAsync();

            if (channels is null)
            {
                await botClient.SendMessage(
                message.From.Id, "🎬 Kino kodini yuboring: ");
            }
            else
            {
                foreach (var channel in channels)
                {
                    var chatMember = await this.botClient.GetChatMember($"@{channel.ChannelLink}", message.From.Id);
                    if (chatMember.Status == ChatMemberStatus.Left || chatMember.Status == ChatMemberStatus.Kicked)
                    {
                        notSubscribedChannels.Add($"https://t.me/{channel.ChannelLink}");
                    }
                }

                if (!notSubscribedChannels.Any())
                {
                    await this.botClient.SendMessage(
                        message.From.Id, "✅ Tabriklaymiz! Siz barcha kanallarga obuna bo‘lgansiz.\n " +
                        "🎬 Kino kodini yuboring: ");
                }

                else
                {
                    var messageText = "Quyidagi kannalarga obuna bo'ling";

                    var buttons = notSubscribedChannels.Select((notSubscribedChannel, index) =>
                    InlineKeyboardButton.WithUrl($"{index + 1}-kanal", $"{notSubscribedChannel}")).ToArray();
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                    buttons,
                    new[] { InlineKeyboardButton.WithCallbackData("✅ Obuna bo‘ldim", "check_subscription") }
                });

                    await botClient.SendMessage(message.From.Id, messageText, replyMarkup: inlineKeyboard);
                }

            }

        }
    }
    #endregion
}
