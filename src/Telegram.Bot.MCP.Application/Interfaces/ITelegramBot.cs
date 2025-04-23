namespace Telegram.Bot.MCP.Application.Interfaces;

/// <summary>
/// Interface for Telegram bot operations, abstracting ITelegramBotClient
/// </summary>
public interface ITelegramBot
{
    /// <summary>
    /// Send a text message to a chat
    /// </summary>
    /// <param name="userId">Unique identifier for the target chat</param>
    /// <param name="text">Text of the message to be sent</param>
    /// <returns>The sent message</returns>
    Task SendMessage(long userId, string text);

    /// <summary>
    /// Get updates from Telegram
    /// </summary>
    /// <param name="limit">Limit the number of updates to be retrieved</param>
    /// <returns>A collection of updates</returns>
    Task<IEnumerable<Message>> ReadNewMessages(int limit);
}

/// <summary>
/// Represents a message from Telegram
/// </summary>
public record Message(User From, string Text, DateTime Timestamp);

/// <summary>
/// Represents a Telegram user
/// </summary>
public record User(long Id, string Username, string? FirstName, string? LastName);
