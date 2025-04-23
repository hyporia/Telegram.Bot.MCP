namespace Telegram.Bot.MCP.Domain;

public class User : IEquatable<User>
{
    public long Id { get; private set; }
    public string Username { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public bool IsAdmin { get; set; }

    public User(long id, string username, string? firstName, string? lastName, bool isAdmin)
    {
        Id = id;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        IsAdmin = isAdmin;
    }

    private User() { }

    public bool Equals(User? other)
    {
        if (other == null)
        {
            return false;
        }

        return Id == other.Id && Username == other.Username && FirstName == other.FirstName && LastName == other.LastName;
    }

    public override bool Equals(object? obj) => obj is User other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Id, Username, FirstName, LastName);
    public override string ToString() => $"{Id}: {Username} ({FirstName} {LastName})";
}