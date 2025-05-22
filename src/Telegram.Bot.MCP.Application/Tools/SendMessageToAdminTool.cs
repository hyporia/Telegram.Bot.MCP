using ModelContextProtocol.Server;
using System.ComponentModel;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public class SendMessageToAdminTool(ITelegramBot telegramBot, ITelegramRepository repository)
{
    [McpServerTool, Description("Send message to all admin users")]
    public async ValueTask<string> SendMessageToAdmin(
        [Description("The message text to send to all admins")] string messageText)
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
                    var message = new Domain.Message(admin, messageText, DateTime.UtcNow, false);
                    // Save the outgoing message to the database
                    await repository.SaveMessageAsync(message); // false = message is from bot

                    // Send the message via Telegram API
                    await telegramBot.SendMessage(
                        userId: admin.Id,
                        text: messageText);

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