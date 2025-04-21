using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBotMCP.Services.Abstract;

namespace TelegramBotMCP.Services;

internal class TelegramBot(ITelegramBotClient botClient) : ITelegramBot
{

    /// <inheritdoc/>
    public async Task SendMessage(long chatId, string text)
        => await botClient.SendMessage(
               chatId: chatId,
               text: text,
               parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);


    /// <inheritdoc/>
    public async Task<IEnumerable<UpdateDTO>> GetUpdates(int limit)
    {
        var updates = await botClient.GetUpdates(limit: limit, allowedUpdates: [UpdateType.Message]);
        return updates.Select(x => new UpdateDTO(x));
    }
}
