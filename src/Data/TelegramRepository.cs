using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramBotMCP.Models;

namespace TelegramBotMCP.Data;

public class TelegramRepository(ApplicationDbContext context, ILogger<TelegramRepository> logger)
{
    // User operations
    public async Task<User?> GetUserByIdAsync(long userId)
        => await context.Users.FindAsync([userId]);

    public async Task<User> CreateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var existingUser = await GetUserByIdAsync(user.Id);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} already exists.");
        }

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        logger.LogInformation("User {userId} created successfully.", user.Id);
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var existingUser = await GetUserByIdAsync(user.Id)
            ?? throw new InvalidOperationException($"User with ID {user.Id} does not exist.");

        if (existingUser.Equals(user))
        {
            return existingUser; // No changes made
        }

        context.Users.Update(user);
        await context.SaveChangesAsync();
        logger.LogInformation("User {userId} updated successfully.", user.Id);
        return user;
    }

    public async Task<User> CreateOrUpdateUserAsync(User user)
    {
        var existingUser = await GetUserByIdAsync(user.Id);

        if (existingUser == null)
        {
            return await CreateUserAsync(user);
        }

        return await UpdateUserAsync(user);
    }

    public async Task<List<User>> GetAllUsersAsync() => await context.Users.ToListAsync();

    public async Task<List<User>> GetAdminUsersAsync()
    {
        return await context.Users
            .Where(u => u.IsAdmin)
            .ToListAsync();
    }

    public async Task<bool> SetUserAdminStatusAsync(long userId, bool isAdmin)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsAdmin = isAdmin;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Message> SaveMessageAsync(Message message)
    {
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
        return message;
    }

    public async Task<List<Message>> GetUserMessagesAsync(long userId, int limit = 50)
    {
        return await context.Messages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Message>> GetRecentMessagesAsync(int limit = 100)
    {
        return await context.Messages
            .Include(m => m.User)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}