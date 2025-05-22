using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class ReadNewMessagesTool(ITelegramBot telegramBot, ITelegramRepository repository, ILogger<ReadNewMessagesTool> logger)
{
    [McpServerTool, Description("Read new messages.")]
    public async ValueTask<string> ReadNewMessages([Description("Maximum number of messages")] int limit)
    {
        try
        {
            var messages = await telegramBot.ReadNewMessages(limit);

            var result = new List<NewMessageDto>();

            foreach (var messageDto in messages)
            {
                // For now, we only care about text messages
                if (string.IsNullOrWhiteSpace(messageDto.Text) || messageDto.From.Username == null)
                {
                    logger.LogWarning("Received an update from {userId} without a message or username.", messageDto.From.Id);
                    continue;
                }

                // Save or update the user in the database
                var tgUser = messageDto.From;
                var user = await repository.CreateOrUpdateUserAsync(
                    new(tgUser.Id, tgUser.Username, tgUser.FirstName, tgUser.LastName, false));

                var message = new Domain.Message(user, messageDto.Text, messageDto.Timestamp, true);
                await repository.SaveMessageAsync(message);

                result.Add(new(messageDto.Text, messageDto.From.Username, messageDto.From.Id));
            }

            if (result.Count == 0)
            {
                return "[]";
            }

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get unread messages");
            return $"Failed to get unread messages: {ex.Message}";
        }
    }

    public record NewMessageDto(string Message, string From, long UserId);
}