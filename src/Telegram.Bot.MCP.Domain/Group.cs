namespace Telegram.Bot.MCP.Domain;

public class Group
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    // Navigation property for many-to-many relationship
    public virtual ICollection<User> Users { get; private set; } = [];

    public Group(string name) => Name = name;

    private Group() { }

    public void AddUser(User user)
    {
        if (!Users.Contains(user))
        {
            Users.Add(user);
        }
    }

    public void RemoveUser(User user)
    {
        if (Users.Contains(user))
        {
            Users.Remove(user);
        }
    }

    public override string ToString() => $"{Id}: {Name}";
}
