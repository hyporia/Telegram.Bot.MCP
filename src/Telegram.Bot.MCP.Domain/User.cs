namespace Telegram.Bot.MCP.Domain;

public class User : IEquatable<User>
{
    public long Id { get; private set; }
    public string Username { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public bool IsMe { get; set; }

    public virtual ICollection<Group> Groups { get; private set; } = [];

    public User(long id, string username, string? firstName, string? lastName, bool isMe = false)
    {
        Id = id;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        IsMe = isMe;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private User() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    // Helper methods to manage group memberships
    public void AddToGroup(Group group)
    {
        if (!Groups.Contains(group))
        {
            Groups.Add(group);
        }
    }

    public void RemoveFromGroup(Group group)
    {
        if (Groups.Contains(group))
        {
            Groups.Remove(group);
        }
    }

    public bool IsMemberOf(int groupId) => Groups.Any(g => g.Id == groupId);

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
    public override string ToString() => $"{Id}: {Username} ({FirstName} {LastName}){(IsMe ? " (Me)" : "")}";
}