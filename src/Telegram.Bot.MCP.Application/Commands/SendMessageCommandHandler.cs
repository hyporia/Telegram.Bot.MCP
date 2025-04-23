using Mediator;
using Microsoft.Extensions.Logging;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Commands;

public class SendMessageCommandHandler(ITelegramBot telegramBot, ITelegramRepository repository, ILogger<SendMessageCommandHandler> logger) 
    : IRequestHandler<SendMessageCommand, string>
{
    public async ValueTask<string> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the user in the database - without creating if not exists
            var user = await repository.GetUserByIdAsync(request.UserId);

            if (user == null)
            {
                logger.LogWarning("User with ID {userId} not found in database.", request.UserId);

                return $"Cannot send message: User with ID {request.UserId} not found in database. " +
                       $"Users are only created when they send a message to the bot first.";
            }

            var message = new Domain.Message(user, request.MessageText, DateTime.UtcNow, false);
            await repository.SaveMessageAsync(message);

            // Send the message via Telegram API with more options
            await telegramBot.SendMessage(
                userId: request.UserId,
                text: request.MessageText);

            return $"Message sent to user {request.UserId}.";
        }
        catch (Exception ex)
        {
            return $"Failed to send message: {ex.Message}";
        }
    }
}

public sealed class SendMessageCommand : IRequest<string>
{
    public long UserId { get; }
    public string MessageText { get; }

    public SendMessageCommand(long userId, string messageText)
    {
        UserId = userId;
        MessageText = messageText;
    }
}