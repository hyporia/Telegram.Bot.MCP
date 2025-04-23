using Telegram.Bot.Types;

namespace TelegramBotMCP.Services.Abstract;

/// <summary>
/// Interface for Telegram bot operations, abstracting ITelegramBotClient
/// </summary>
public interface ITelegramBot
{
    /// <summary>
    /// Send a text message to a chat
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="text">Text of the message to be sent</param>
    /// <returns>The sent message</returns>
    Task SendMessage(long chatId, string text);

    /// <summary>
    /// Get updates from Telegram
    /// </summary>
    /// <param name="limit">Limit the number of updates to be retrieved</param>
    /// <returns>A collection of updates</returns>
    Task<IEnumerable<UpdateDTO>> GetUpdates(int limit);
}

/// <summary>
/// Represents a message update from Telegram
/// </summary>
public class UpdateDTO
{
    public MessageDTO? Message { get; set; }

    public UpdateDTO(Update update) => Message = new MessageDTO(update.Message!);

    public UpdateDTO() { }
}

/// <summary>
/// Represents a message from Telegram
/// </summary>
public class MessageDTO
{
    public UserDTO? From { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }

    public MessageDTO(Message message)
    {
        From = message.From != null ? new UserDTO(message.From) : null;
        Text = message.Text ?? string.Empty;
        Date = message.Date;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MessageDTO() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

/// <summary>
/// Represents a Telegram user
/// </summary>
public class UserDTO
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public UserDTO(User user)
    {
        Id = user.Id;
        Username = user.Username ?? string.Empty;
        FirstName = user.FirstName ?? string.Empty;
        LastName = user.LastName ?? string.Empty;
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public UserDTO() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

