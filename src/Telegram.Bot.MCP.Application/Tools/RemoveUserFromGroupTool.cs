using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class RemoveUserFromGroupTool(ITelegramRepository repository, ILogger<RemoveUserFromGroupTool> logger)
{
    [McpServerTool, Description("Remove a user from a group")]
    public async ValueTask<string> RemoveUserFromGroup(
        [Description("User ID")] long userId,
        [Description("Group ID")] int groupId)
    {
        try
        {
            var user = await repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Failed to remove user from group: User {userId} not found", userId);
                return $"User {userId} not found";
            }

            var group = await repository.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                logger.LogWarning("Failed to remove user from group: Group {groupId} not found", groupId);
                return $"Group {groupId} not found";
            }

            var success = await repository.RemoveUserFromGroupAsync(userId, groupId);
            if (success)
            {
                return $"Successfully removed user {user.Username} from group {group.Name}";
            }
            else
            {
                return $"Failed to remove user from group";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove user {userId} from group {groupId}", userId, groupId);
            return $"An error occurred: {ex.Message}";
        }
    }
}
