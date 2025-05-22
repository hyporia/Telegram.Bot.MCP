using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class GetAllGroupsTool(ITelegramRepository repository)
{
    [McpServerTool, Description("Get all available groups")]
    public async ValueTask<string> GetAllGroups(CancellationToken cancellationToken)
    {
        var groups = await repository.GetAllGroupsAsync();

        var formattedGroups = groups.Select(g => new
        {
            g.Id,
            g.Name
        }).ToList();

        return JsonSerializer.Serialize(formattedGroups);
    }
}
