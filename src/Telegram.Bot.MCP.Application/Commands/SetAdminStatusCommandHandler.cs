using Mediator;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Commands;

public class SetAdminStatusCommandHandler(ITelegramRepository repository) : IRequestHandler<SetAdminStatusCommand, string>
{
    public async ValueTask<string> Handle(SetAdminStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await repository.SetUserAdminStatusAsync(request.UserId, request.IsAdmin);
        if (result)
        {
            return $"Successfully {(request.IsAdmin ? "set" : "removed")} admin status for user {request.UserId}";
        }
        else
        {
            return $"Failed to update admin status: User {request.UserId} not found";
        }
    }
}

public sealed class SetAdminStatusCommand : IRequest<string>
{
    public long UserId { get; }
    public bool IsAdmin { get; }

    public SetAdminStatusCommand(long userId, bool isAdmin)
    {
        UserId = userId;
        IsAdmin = isAdmin;
    }
}