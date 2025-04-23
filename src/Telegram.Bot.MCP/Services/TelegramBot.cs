using Telegram.Bot.MCP.Services.Abstract;
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
    public async Task<IEnumerable<UpdateDTO>> GetUpdates(int limit)
    {
        var updates = await botClient.GetUpdates(limit: limit, allowedUpdates: [UpdateType.Message]);
        return updates.Select(x => new UpdateDTO(x));
    }
}
