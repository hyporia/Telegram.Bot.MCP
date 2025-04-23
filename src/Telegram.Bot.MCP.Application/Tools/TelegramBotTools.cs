using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Tools;

[McpServerToolType]
public sealed class TelegramBotTools
{
    private readonly ITelegramBot _telegramBot;
    private readonly ITelegramRepository _repository;
    private readonly ILogger<TelegramBotTools> _logger;

    public TelegramBotTools(ITelegramBot telegramBot, ITelegramRepository repository, ILogger<TelegramBotTools> logger)
    {
        _telegramBot = telegramBot;
        _repository = repository;
        _logger = logger;
    }

    [McpServerTool, Description("Send message to a specific Telegram user")]
    public async Task<string> SendMessage(
        [Description("Recipient user ID")] long userId,
        [Description("The message text")] string messageText)
    {
        try
        {
            // Find the user in the database - without creating if not exists
            var user = await _repository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User with ID {userId} not found in database.", userId);

                return $"Cannot send message: User with ID {userId} not found in database. " +
                       $"Users are only created when they send a message to the bot first.";
            }

            var message = new Domain.Message(user, messageText, DateTime.UtcNow, false);
            await _repository.SaveMessageAsync(message);

            // Send the message via Telegram API with more options
            await _telegramBot.SendMessage(
                userId: userId,
                text: messageText);

            return $"Message sent to user {userId}.";
        }
        catch (Exception ex)
        {
            return $"Failed to send message: {ex.Message}";
        }
    }

    [McpServerTool, Description("Send message to all admin users")]
    public async Task<string> SendMessageToAdmin([Description("The message text")] string messageText)
    {
        try
        {
            var adminUsers = await _repository.GetAdminUsersAsync();

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
                    await _repository.SaveMessageAsync(message); // false = message is from bot

                    // Send the message via Telegram API
                    await _telegramBot.SendMessage(
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

    [McpServerTool, Description("Set admin status for a user")]
    public async Task<string> SetAdminStatus(
        [Description("User ID to update")] long userId,
        [Description("Admin status (true/false)")] bool isAdmin)
    {
        var result = await _repository.SetUserAdminStatusAsync(userId, isAdmin);
        if (result)
        {
            return $"Successfully {(isAdmin ? "set" : "removed")} admin status for user {userId}";
        }
        else
        {
            return $"Failed to update admin status: User {userId} not found";
        }
    }

    public record NewMessageDto(string Message, string From, long UserId);

    [McpServerTool, Description("Read new messages.")]
    public async Task<string> ReadNewMessages()
    {
        try
        {
            var messages = await _telegramBot.ReadNewMessages(100);

            var result = new List<NewMessageDto>();

            foreach (var messageDto in messages)
            {
                // For now, we only care about text messages
                if (string.IsNullOrWhiteSpace(messageDto.Text) || messageDto.From.Username == null)
                {
                    _logger.LogWarning("Received an update from {userId} without a message or username.", messageDto.From.Id);
                    continue;
                }

                // Save or update the user in the database
                var tgUser = messageDto.From;
                var user = await _repository.CreateOrUpdateUserAsync(
                    new(tgUser.Id, tgUser.Username, tgUser.FirstName, tgUser.LastName, false));

                var message = new Domain.Message(user, messageDto.Text, messageDto.Timestamp, true);
                await _repository.SaveMessageAsync(message);

                result.Add(new(messageDto.Text, messageDto.From.Username, messageDto.From.Id));
            }

            if (result.Count == 0)
            {
                return "[]";
            }

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread messages");
            return $"Failed to get unread messages: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get all users")]
    public async Task<string> GetAllUsers()
    {
        var users = await _repository.GetAllUsersAsync();

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

    [McpServerTool, Description("Get conversation history with a specific user")]
    public async Task<string> GetUserConversation([Description("User ID")] long userId, [Description("Number of messages")] int limit = 50)
    {
        var messages = await _repository.GetUserMessagesAsync(userId, limit);

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