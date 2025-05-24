using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class IAmTool(ITelegramRepository repository)
{
    [McpServerTool, Description("Set yourself as a specific Telegram user")]
    public async ValueTask<string> IAm(
        [Description("Telegram User ID")] long userId)
    {
        var result = await repository.SetMeAsync(userId);
        if (result)
        {
            var user = await repository.GetUserByIdAsync(userId);
            return $"You've been successfully identified as {user}";
        }
        else
        {
            return $"Failed to set you as user {userId}: User not found";
        }
    }
}
