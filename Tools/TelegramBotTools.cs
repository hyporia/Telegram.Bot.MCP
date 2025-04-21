using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramBotMCP.Data;
using TelegramBotMCP.Models;

namespace TelegramBotMCP.Tools;

[McpServerToolType]
public sealed class TelegramBotTools
{
    private readonly ITelegramBotClient _telegramBot;
    private readonly TelegramRepository _repository;
    private readonly ILogger<TelegramBotTools> _logger;

    public TelegramBotTools(ITelegramBotClient telegramBot, TelegramRepository repository, ILogger<TelegramBotTools> logger)
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
            var users = await _repository.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found in database. " +
                    $"Users are only created when they send a message to the bot first.");

                return $"Cannot send message: User with ID {userId} not found in database. " +
                       $"Users are only created when they send a message to the bot first.";
            }

            var message = new Message(messageText, DateTime.UtcNow, user, false);
            await _repository.SaveMessageAsync(message);

            // Send the message via Telegram API with more options
            var sentMessage = await _telegramBot.SendMessage(
                chatId: userId,
                text: messageText,
                parseMode: ParseMode.Markdown,
                disableNotification: false);

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
                    var message = new Message(messageText, DateTime.UtcNow, admin, false);
                    // Save the outgoing message to the database
                    await _repository.SaveMessageAsync(message); // false = message is from bot

                    // Send the message via Telegram API
                    await _telegramBot.SendMessage(
                        chatId: admin.Id,
                        text: messageText,
                        parseMode: ParseMode.Markdown,
                        disableNotification: false);

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Failed to send to admin {admin.Id}. Reason: {ex.Message}");
                }
            }

            var resultMessage = $"Message sent to {successCount} of {adminUsers.Count} admin users.";
            if (errorMessages.Any())
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

    [McpServerTool, Description("Get unread messages. Pass default limit to 100")]
    public async Task<string> GetUnreadMessages()
    {
        try
        {
            var updates = await _telegramBot.GetUpdates(100);

            var messages = new List<object>();

            foreach (var update in updates)
            {
                if (update.Message?.Text == null || update.Message?.From == null)
                {
                    continue;
                }

                // Save or update the user in the database
                var user = await _repository.CreateOrUpdateUserAsync(new(update.Message.From));

                var message = new Message(update.Message, user);
                await _repository.SaveMessageAsync(message);

                messages.Add(new
                {
                    Message = update.Message.Text,
                    From = update.Message.From.Username ?? "Unknown",
                    UserId = update.Message.From.Id
                });
            }

            if (messages.Count == 0)
            {
                return "[]";
            }

            return JsonSerializer.Serialize(messages);
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
            u.LastName
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