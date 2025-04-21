using System.ComponentModel.DataAnnotations;

namespace TelegramBotMCP.Models;

public class User : IEquatable<User>
{
    [Key]
    public long Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsAdmin { get; set; }

    public User(Telegram.Bot.Types.User user)
    {
        Id = user.Id;
        Username = user.Username ?? string.Empty;
        FirstName = user.FirstName ?? string.Empty;
        LastName = user.LastName ?? string.Empty;
        IsAdmin = false;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private User() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public bool Equals(User? other)
    {
        if (other == null)
        {
            return false;
        }

        return Id == other.Id && Username == other.Username && FirstName == other.FirstName && LastName == other.LastName;
    }

    public override int GetHashCode() => HashCode.Combine(Id, Username, FirstName, LastName);

    public override string ToString() => $"{Id}: {Username} ({FirstName} {LastName})";
}