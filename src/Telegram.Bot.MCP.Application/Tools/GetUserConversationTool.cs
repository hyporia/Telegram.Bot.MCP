using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class GetUserConversationTool(ITelegramRepository repository)
{
    [McpServerTool, Description("Get conversation history with a specific user")]
    public async ValueTask<string> GetUserConversation(
        [Description("User ID")] long userId,
        [Description("Number of messages")] int limit = 50)
    {
        var messages = await repository.GetUserMessagesAsync(userId, limit);

        var conversation = messages.Select(m => new
        {
            m.Id,
            m.Text,
            m.IsFromUser,
            m.Timestamp
        }).ToList();

        return JsonSerializer.Serialize(conversation);
    }
}