using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class GetUserGroupsTool(ITelegramRepository repository, ILogger<GetUserGroupsTool> logger)
{
    [McpServerTool, Description("Get all groups a user belongs to")]
    public async ValueTask<string> GetUserGroups(
        [Description("User ID")] long userId)
    {
        try
        {
            var user = await repository.GetUserByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Failed to get user groups: User {userId} not found", userId);
                return $"User {userId} not found";
            }

            var groups = await repository.GetUserGroupsAsync(userId);

            var formattedGroups = groups.Select(g => new
            {
                g.Id,
                g.Name
            }).ToList();

            return JsonSerializer.Serialize(formattedGroups);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get groups for user {userId}", userId);
            return $"An error occurred: {ex.Message}";
        }
    }
}
