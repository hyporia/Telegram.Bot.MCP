using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class SendMessageToMeTool(ITelegramBot telegramBot, ITelegramRepository repository, ILogger<SendMessageToMeTool> logger)
{
    [McpServerTool, Description("Send message to me (the user marked as 'Me')")]
    public async ValueTask<string> SendMessageToMe(
        [Description("The message text to send")] string messageText)
    {
        try
        {
            // Find the user marked as "Me" in the database
            var meUser = await repository.GetMeAsync();

            if (meUser == null)
            {
                logger.LogWarning("No user is marked as 'Me'. Use the IAm tool first.");
                return $"Cannot send message: No user is marked as 'Me'. Please use the {nameof(IAmTool)} tool to identify yourself first.";
            }

            var message = new Domain.Message(meUser, messageText, DateTime.UtcNow, false);
            await repository.SaveMessageAsync(message);

            await telegramBot.SendMessage(
                userId: meUser.Id,
                text: messageText);

            return $"Message sent to {meUser}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to Me user");
            return $"Failed to send message: {ex.Message}";
        }
    }
}
