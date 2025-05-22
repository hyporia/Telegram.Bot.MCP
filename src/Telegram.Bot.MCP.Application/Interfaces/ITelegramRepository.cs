namespace Telegram.Bot.MCP.Application.Interfaces;

public interface ITelegramRepository
{
    Task<Domain.User?> GetUserByIdAsync(long userId);
    Task<Domain.User> CreateUserAsync(Domain.User user);
    Task<Domain.User> UpdateUserAsync(Domain.User user);
    Task<Domain.User> CreateOrUpdateUserAsync(Domain.User user);
    Task<List<Domain.User>> GetAllUsersAsync();
    Task<List<Domain.User>> GetAdminUsersAsync();
    Task<Domain.User?> GetMeAsync();
    Task<bool> SetUserAdminStatusAsync(long userId, bool isAdmin);
    Task<bool> SetMeAsync(long userId);
    Task<Domain.Message> SaveMessageAsync(Domain.Message message);
    Task<List<Domain.Message>> GetUserMessagesAsync(long userId, int limit = 50);
}