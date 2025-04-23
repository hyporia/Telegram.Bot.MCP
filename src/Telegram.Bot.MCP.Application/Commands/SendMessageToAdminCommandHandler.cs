using Mediator;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Commands;

public class SendMessageToAdminCommandHandler(ITelegramBot telegramBot, ITelegramRepository repository)
    : IRequestHandler<SendMessageToAdminCommand, string>
{
    public async ValueTask<string> Handle(SendMessageToAdminCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var adminUsers = await repository.GetAdminUsersAsync();

            if (adminUsers.Count == 0)
            {
                return "No admin users found. Users must first send a message to the bot and then be marked as admin.";
            }

            var successCount = 0;
            var errorMessages = new List<string>();

            foreach (var admin in adminUsers)
            {
                try
                {
                    var message = new Domain.Message(admin, request.MessageText, DateTime.UtcNow, false);
                    // Save the outgoing message to the database
                    await repository.SaveMessageAsync(message); // false = message is from bot

                    // Send the message via Telegram API
                    await telegramBot.SendMessage(
                        userId: admin.Id,
                        text: request.MessageText);

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Failed to send to admin {admin.Id}. Reason: {ex.Message}");
                }
            }

            var resultMessage = $"Message sent to {successCount} of {adminUsers.Count} admin users.";
            if (errorMessages.Count != 0)
            {
                resultMessage += $"\nErrors: {string.Join(", ", errorMessages)}";
            }

            return resultMessage;
        }
        catch (Exception ex)
        {
            return $"Failed to send message to admins: {ex.Message}";
        }
    }
}

public sealed class SendMessageToAdminCommand : IRequest<string>
{
    public string MessageText { get; }

    public SendMessageToAdminCommand(string messageText) => MessageText = messageText;
}