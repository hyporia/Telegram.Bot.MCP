using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class CreateGroupTool(ITelegramRepository repository, ILogger<CreateGroupTool> logger)
{
    [McpServerTool, Description("Create a new user group")]
    public async ValueTask<string> CreateGroup(
        [Description("Group name")] string groupName)
    {
        try
        {
            var existingGroup = await repository.GetGroupByNameAsync(groupName);
            if (existingGroup != null)
            {
                logger.LogWarning("Failed to create group: Group with name '{groupName}' already exists", groupName);
                return $"Failed to create group: Group with name '{groupName}' already exists";
            }

            var group = new Domain.Group(groupName);
            var createdGroup = await repository.CreateGroupAsync(group);

            return JsonSerializer.Serialize(new
            {
                createdGroup.Id,
                createdGroup.Name,
                Message = $"Group '{groupName}' created successfully with ID {createdGroup.Id}"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create group with name '{groupName}'", groupName);
            return $"An error occurred: {ex.Message}";
        }
    }
}
