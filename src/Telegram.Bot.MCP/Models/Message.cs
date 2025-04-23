using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TelegramBotMCP.Services.Abstract;

namespace TelegramBotMCP.Models;

public class Message
{
    [Key]
    public long Id { get; set; }

    [ForeignKey("User")]
    public long UserId { get; set; }
    public User User { get; set; }

    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsFromUser { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Message() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Message(MessageDTO message, User user)
    {
        Text = message.Text ?? string.Empty;
        Timestamp = message.Date.ToUniversalTime();
        UserId = user.Id;
        User = user;
        IsFromUser = true;
    }

    public Message(string text, DateTime timestamp, User user, bool isFromUser = false)
    {
        Text = text;
        Timestamp = timestamp;
        IsFromUser = isFromUser;
        UserId = user.Id;
        User = user;
    }
}