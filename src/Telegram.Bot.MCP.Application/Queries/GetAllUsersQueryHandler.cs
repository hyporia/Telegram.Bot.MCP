using Mediator;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Queries;
public class GetAllUsersQueryHandler(ITelegramRepository repository) : IRequestHandler<GetAllUsersQuery, string>
{
    public async ValueTask<string> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
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

public class GetAllUsersQuery : IRequest<string>
{
    public GetAllUsersQuery() { }
}
