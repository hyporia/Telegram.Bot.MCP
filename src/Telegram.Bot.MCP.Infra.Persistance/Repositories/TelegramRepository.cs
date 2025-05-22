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

    // User operations
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
}