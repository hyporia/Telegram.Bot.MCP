using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramBotMCP.Models;

namespace TelegramBotMCP.Data;

public class TelegramRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TelegramRepository> _logger;

    public TelegramRepository(ApplicationDbContext context, ILogger<TelegramRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // User operations
    public async Task<User?> GetUserByIdAsync(long userId)
        => await _context.Users.FindAsync([userId]);

    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

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

    public async Task<User> UpdateUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var existingUser = await GetUserByIdAsync(user.Id);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} does not exist.");
        }

        if (existingUser.Equals(user))
        {
            return existingUser; // No changes made
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {userId} updated successfully.", user.Id);
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

    public async Task<List<User>> GetAllUsersAsync() => await _context.Users.ToListAsync();

    public async Task<List<User>> GetAdminUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsAdmin)
            .ToListAsync();
    }

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

    public async Task<Message> SaveMessageAsync(Message message)
    {
        var user = await _context.Users.FindAsync(message.UserId);
        if (user == null)
        {
            throw new Exception($"User with ID {message.UserId} not found");
        }

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<Message>> GetUserMessagesAsync(long userId, int limit = 50)
    {
        return await _context.Messages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Message>> GetRecentMessagesAsync(int limit = 100)
    {
        return await _context.Messages
            .Include(m => m.User)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}