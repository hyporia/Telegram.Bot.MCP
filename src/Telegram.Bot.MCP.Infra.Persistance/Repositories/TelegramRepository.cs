using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Infra.Persistance.Repositories;

public class TelegramRepository : ITelegramRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TelegramRepository> _logger;

    public TelegramRepository(ApplicationDbContext context, ILogger<TelegramRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Domain.User?> GetUserByIdAsync(long userId)
        => await _context.Users.FindAsync([userId]);

    public async Task<Domain.User> CreateUserAsync(Domain.User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var existingUser = await GetUserByIdAsync(user.Id);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} already exists.");
        }

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} created successfully.", user.Id);
        return user;
    }

    public async Task<Domain.User> UpdateUserAsync(Domain.User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var existingUser = await GetUserByIdAsync(user.Id)
            ?? throw new InvalidOperationException($"User with ID {user.Id} does not exist.");

        if (existingUser.Equals(user))
        {
            return existingUser; // No changes made
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} updated successfully.", user.Id);
        return user;
    }

    public async Task<Domain.User> CreateOrUpdateUserAsync(Domain.User user)
    {
        var existingUser = await GetUserByIdAsync(user.Id);

        if (existingUser == null)
        {
            return await CreateUserAsync(user);
        }

        return await UpdateUserAsync(user);
    }

    public async Task<List<Domain.User>> GetAllUsersAsync() => await _context.Users.ToListAsync(); public async Task<List<Domain.User>> GetAdminUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsAdmin)
            .ToListAsync();
    }

    public Task<Domain.User?> GetMeAsync() => _context.Users.FirstOrDefaultAsync(u => u.IsMe);


    public async Task<bool> SetUserAdminStatusAsync(long userId, bool isAdmin)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsAdmin = isAdmin;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetMeAsync(long userId)
    {
        // First, reset IsMe flag for all users
        var users = await _context.Users.Where(u => u.IsMe).ToListAsync();
        foreach (var user in users)
        {
            user.IsMe = false;
        }

        // Now set the specified user as "Me"
        var meUser = await _context.Users.FindAsync(userId);
        if (meUser == null)
        {
            return false;
        }

        meUser.IsMe = true;
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} set as 'Me'", userId);
        return true;
    }

    public async Task<Domain.Message> SaveMessageAsync(Domain.Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<Domain.Message>> GetUserMessagesAsync(long userId, int limit = 50)
    {
        return await _context.Messages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    // Group management methods
    public async Task<Domain.Group?> GetGroupByIdAsync(int groupId)
        => await _context.Groups.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == groupId);

    public async Task<Domain.Group?> GetGroupByNameAsync(string name)
        => await _context.Groups.Include(x => x.Users).FirstOrDefaultAsync(x => x.Name == name);

    public async Task<List<Domain.Group>> GetAllGroupsAsync()
        => await _context.Groups.ToListAsync();

    public async Task<Domain.Group> CreateGroupAsync(Domain.Group group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var existingGroup = await GetGroupByNameAsync(group.Name);
        if (existingGroup != null)
        {
            throw new InvalidOperationException($"Group with name '{group.Name}' already exists.");
        }

        await _context.Groups.AddAsync(group);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Group '{groupName}' created successfully.", group.Name);
        return group;
    }

    public async Task<Domain.Group> UpdateGroupAsync(Domain.Group group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var existingGroup = await GetGroupByIdAsync(group.Id)
            ?? throw new InvalidOperationException($"Group with ID {group.Id} does not exist.");

        _context.Entry(existingGroup).CurrentValues.SetValues(group);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Group {groupId} updated successfully.", group.Id);
        return group;
    }

    public async Task<bool> DeleteGroupAsync(int groupId)
    {
        var group = await GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return false;
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Group {groupId} deleted successfully.", groupId);
        return true;
    }

    public async Task<bool> AddUserToGroupAsync(long userId, int groupId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Failed to add user to group: User {userId} not found.", userId);
            return false;
        }

        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            _logger.LogWarning("Failed to add user to group: Group {groupId} not found.", groupId);
            return false;
        }

        if (group.Users.Any(u => u.Id == userId))
        {
            // User is already in the group
            return true;
        }

        group.AddUser(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} added to group {groupId}.", userId, groupId);
        return true;
    }

    public async Task<bool> RemoveUserFromGroupAsync(long userId, int groupId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return false;
        }

        if (!group.Users.Any(u => u.Id == userId))
        {
            // User is not in the group
            return true;
        }

        group.RemoveUser(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} removed from group {groupId}.", userId, groupId);
        return true;
    }

    public async Task<List<Domain.User>> GetUsersInGroupAsync(int groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        return group?.Users.ToList() ?? [];
    }

    public async Task<List<Domain.Group>> GetUserGroupsAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Groups.ToList() ?? [];
    }
}