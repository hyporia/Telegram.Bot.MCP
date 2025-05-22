using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class GetAllUsersTool(ITelegramRepository repository)
{
    [McpServerTool, Description("Get all users")]
    public async ValueTask<string> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await repository.GetAllUsersAsync();

        var formattedUsers = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.FirstName,
            u.LastName,
            u.IsAdmin
        }).ToList();

        return JsonSerializer.Serialize(formattedUsers);
    }
}