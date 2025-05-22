namespace Telegram.Bot.MCP.Domain;

public class Message
{
    public long Id { get; private set; }
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