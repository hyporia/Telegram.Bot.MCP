using Mediator;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Commands;
using Telegram.Bot.MCP.Application.Queries;

namespace Telegram.Bot.MCP.Infra.Host;

[McpServerToolType]
public sealed class TelegramBotTools(IMediator mediator)
{
    [McpServerTool, Description("Send message to a specific Telegram user")]
    public async Task<string> SendMessage(
        [Description("Recipient user ID")] long userId,
        [Description("The message text")] string messageText)
        => await mediator.Send(new SendMessageCommand(userId, messageText));

    [McpServerTool, Description("Send message to all admin users")]
    public async Task<string> SendMessageToAdmin([Description("The message text")] string messageText)
        => await mediator.Send(new SendMessageToAdminCommand(messageText));

    [McpServerTool, Description("Set admin status for a user")]
    public async Task<string> SetAdminStatus(
        [Description("User ID to update")] long userId,
        [Description("Admin status (true/false)")] bool isAdmin) => await mediator.Send(new SetAdminStatusCommand(userId, isAdmin));

    [McpServerTool, Description("Read new messages.")]
    public async Task<string> ReadNewMessages() => await mediator.Send(new ReadNewMessagesCommand(100));

    [McpServerTool, Description("Get all users")]
    public async Task<string> GetAllUsers() => await mediator.Send(new GetAllUsersQuery());

    [McpServerTool, Description("Get conversation history with a specific user")]
    public async Task<string> GetUserConversation([Description("User ID")] long userId, [Description("Number of messages")] int limit = 50)
        => await mediator.Send(new GetUserConversationQuery(userId, limit));
}