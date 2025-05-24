using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class AddUserToGroupTool(ITelegramRepository repository, ILogger<AddUserToGroupTool> logger)
{
    [McpServerTool, Description("Add a user to a group")]
    public async ValueTask<string> AddUserToGroup(
        [Description("User ID")] long userId,
        [Description("Group ID")] int groupId)
    {
        try
        {
            var user = await repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Failed to add user to group: User {userId} not found", userId);
                return $"User {userId} not found";
            }

            var group = await repository.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                logger.LogWarning("Failed to add user to group: Group {groupId} not found", groupId);
                return $"Group {groupId} not found";
            }

            var success = await repository.AddUserToGroupAsync(userId, groupId);
            if (success)
            {
                return $"Successfully added user {user.Username} to group {group.Name}";
            }
            else
            {
                return $"Failed to add user to group";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add user {userId} to group {groupId}", userId, groupId);
            return $"An error occurred: {ex.Message}";
        }
    }
}
