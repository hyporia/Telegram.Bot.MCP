using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegram.Bot.MCP.Domain;

public class Message
{
    [Key]
    public long Id { get; private set; }

    [ForeignKey("User")]
    public long UserId { get; private set; }
    public User User { get; private set; }

    public string Text { get; private set; }
    public DateTime Timestamp { get; private set; }
    public bool IsFromUser { get; private set; }

    public Message(User user, string text, DateTime timestamp, bool isFromUser)
    {
        UserId = user.Id;
        User = user;
        Text = text;
        Timestamp = timestamp.ToUniversalTime();
        IsFromUser = isFromUser;
    }

    private Message() { }
}