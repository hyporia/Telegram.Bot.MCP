using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class GetGroupUsersTool(ITelegramRepository repository, ILogger<GetGroupUsersTool> logger)
{
    [McpServerTool, Description("Get all users in a specific group")]
    public async ValueTask<string> GetGroupUsers(
        [Description("Group ID")] int groupId)
    {
        try
        {
            var group = await repository.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                logger.LogWarning("Failed to get group users: Group {groupId} not found", groupId);
                return $"Group {groupId} not found";
            }

            var formattedUsers = group.Users.Select(u => new
            {
                u.Id,
                u.Username,
                u.FirstName,
                u.LastName
            }).ToList();

            return JsonSerializer.Serialize(formattedUsers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get users for group {groupId}", groupId);
            return $"An error occurred: {ex.Message}";
        }
    }
}
