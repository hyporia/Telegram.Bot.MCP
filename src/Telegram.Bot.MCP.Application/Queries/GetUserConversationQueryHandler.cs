using Mediator;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Queries;

public class GetUserConversationQueryHandler(ITelegramRepository repository) : IRequestHandler<GetUserConversationQuery, string>
{
    public async ValueTask<string> Handle(GetUserConversationQuery request, CancellationToken cancellationToken)
    {
        var messages = await repository.GetUserMessagesAsync(request.UserId, request.Limit);

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

public sealed class GetUserConversationQuery : IRequest<string>
{
    public long UserId { get; }
    public int Limit { get; }

    public GetUserConversationQuery(long userId, int limit)
    {
        UserId = userId;
        Limit = limit;
    }
}