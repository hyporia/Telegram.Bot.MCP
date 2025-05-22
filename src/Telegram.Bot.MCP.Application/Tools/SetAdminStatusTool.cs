using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class SetAdminStatusTool(ITelegramRepository repository)
{
    [McpServerTool, Description("Set or remove admin status for a user")]
    public async ValueTask<string> SetAdminStatus(
        [Description("User ID")] long userId,
        [Description("Whether the user should be an admin")] bool isAdmin)
    {
        var result = await repository.SetUserAdminStatusAsync(userId, isAdmin);
        if (result)
        {
            return $"Successfully {(isAdmin ? "set" : "removed")} admin status for user {userId}";
        }
        else
        {
            return $"Failed to update admin status: User {userId} not found";
        }
    }
}