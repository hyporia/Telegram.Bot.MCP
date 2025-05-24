namespace Telegram.Bot.MCP.Domain;

public class Group
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public virtual ICollection<User> Users { get; private set; } = [];

    public Group(string name) => Name = name;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Group() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void AddUser(User user)
    {
        if (!Users.Contains(user))
        {
            Users.Add(user);
        }
    }

    public void RemoveUser(User user)
    {
        Users.Remove(user);
    }

    public override string ToString() => $"{Id}: {Name}";
}
