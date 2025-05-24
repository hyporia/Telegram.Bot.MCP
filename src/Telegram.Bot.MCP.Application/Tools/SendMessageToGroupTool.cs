using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class SendMessageToGroupTool(
    ITelegramBot telegramBot,
    ITelegramRepository repository,
    ILogger<SendMessageToGroupTool> logger)
{
    [McpServerTool, Description("Send a message to all users in a group")]
    public async ValueTask<string> SendMessageToGroup(
        [Description("Group ID")] int groupId,
        [Description("The message text to send")] string messageText)
    {
        try
        {
            var group = await repository.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                logger.LogWarning("Failed to send message to group: Group {groupId} not found", groupId);
                return $"Group {groupId} not found";
            }

            var successCount = 0;
            var failedUsers = new List<string>();

            foreach (var user in group.Users)
            {
                try
                {
                    var message = new Domain.Message(user, messageText, DateTime.UtcNow, false);
                    await repository.SaveMessageAsync(message);

                    await telegramBot.SendMessage(
                        userId: user.Id,
                        text: messageText);

                    successCount++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send message to user {userId} in group {groupId}", user.Id, groupId);
                    failedUsers.Add(user.Username);
                }
            }

            if (failedUsers.Count > 0)
            {
                return $"Message sent to {successCount} out of {group.Users.Count} users in group {group.Name}. " +
                       $"Failed to send to: {string.Join(", ", failedUsers)}";
            }

            return $"Message sent to all {group.Users.Count} users in group {group.Name}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to group {groupId}", groupId);
            return $"An error occurred: {ex.Message}";
        }
    }
}
