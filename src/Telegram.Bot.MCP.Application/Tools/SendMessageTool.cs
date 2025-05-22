using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

public class SendMessageTool(ITelegramBot telegramBot, ITelegramRepository repository, ILogger<SendMessageTool> logger)
{
    [McpServerTool, Description("Send message to a specific Telegram user")]
    public async ValueTask<string> SendMessage(
        [Description("Recipient user ID")] long userId,
        [Description("The message text")] string messageText)
    {
        try
        {
            // Find the user in the database - without creating if not exists
            var user = await repository.GetUserByIdAsync(userId);

            if (user == null)
            {
                logger.LogWarning("User with ID {userId} not found in database.", userId);

                return $"Cannot send message: User with ID {userId} not found in database. " +
                       $"Users are only created when they send a message to the bot first.";
            }

            var message = new Domain.Message(user, messageText, DateTime.UtcNow, false);
            await repository.SaveMessageAsync(message);

            // Send the message via Telegram API with more options  
            await telegramBot.SendMessage(
                userId: userId,
                text: messageText);

            return $"Message sent to user {userId}.";
        }
        catch (Exception ex)
        {
            return $"Failed to send message: {ex.Message}";
        }
    }
}