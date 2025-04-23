using Telegram.Bot.MCP.Application.Interfaces;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.MCP.Services;

internal class TelegramBot(ITelegramBotClient botClient) : ITelegramBot
{
    /// <inheritdoc/>
    public async Task SendMessage(long chatId, string text)
        => await botClient.SendMessage(
               chatId: chatId,
               text: text,
               parseMode: ParseMode.MarkdownV2);

    /// <inheritdoc/>
    public async Task<IEnumerable<Message>> ReadNewMessages(int limit)
    {
        var updates = await botClient.GetUpdates(limit: limit, allowedUpdates: [UpdateType.Message]);
        return updates
            .Where(x => x.Message!.Text != null)
            .Where(x => x.Message!.From?.Username != null)
            .Select(MapToMessage);
    }

    static Message MapToMessage(Types.Update update)
    {
        var message = update.Message;
        var user = message!.From!;

        return new Message(
            From: new User(
                Id: user.Id,
                Username: user.Username!,
                FirstName: user.FirstName,
                LastName: user.LastName),
            Text: message.Text!,
            Timestamp: message.Date.ToUniversalTime());
    }
}
