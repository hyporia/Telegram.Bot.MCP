using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Resources;

[McpServerResourceType]
public class ConversationResource(ITelegramRepository repository)
{
    [McpServerResource(UriTemplate = "telegram://user/{userId}/conversation?limit={limit}", Name = "Telegram conversation")]
    [Description("Conversation history with a user")]
    public async ValueTask<ResourceContents> Conversation(RequestContext<ReadResourceRequestParams> requestContext, long userId, int limit)
    {
        var messages = await repository.GetUserMessagesAsync(userId, limit);

        var conversation = messages.Select(m => new
        {
            m.Id,
            m.Text,
            m.IsFromUser,
            m.Timestamp
        }).ToList();

        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(conversation),
            MimeType = "application/json",
            Uri = $"telegram://user/{userId}/conversation",
        };
    }
}