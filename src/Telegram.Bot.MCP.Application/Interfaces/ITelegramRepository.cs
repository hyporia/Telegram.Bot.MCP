namespace Telegram.Bot.MCP.Application.Interfaces;

public interface ITelegramRepository
{
    Task<Domain.User?> GetUserByIdAsync(long userId);
    Task<Domain.User> CreateUserAsync(Domain.User user);
    Task<Domain.User> UpdateUserAsync(Domain.User user);
    Task<Domain.User> CreateOrUpdateUserAsync(Domain.User user);
    Task<List<Domain.User>> GetAllUsersAsync();
    Task<Domain.User?> GetMeAsync();
    Task<Domain.Message> SaveMessageAsync(Domain.Message message);
    Task<List<Domain.Message>> GetUserMessagesAsync(long userId, int limit = 50);

    // Group management methods
    Task<Domain.Group?> GetGroupByIdAsync(int groupId);
    Task<Domain.Group?> GetGroupByNameAsync(string name);
    Task<List<Domain.Group>> GetAllGroupsAsync();
    Task<Domain.Group> CreateGroupAsync(Domain.Group group);
    Task<Domain.Group> UpdateGroupAsync(Domain.Group group);
    Task<bool> DeleteGroupAsync(int groupId);
    Task<bool> AddUserToGroupAsync(long userId, int groupId);
    Task<bool> RemoveUserFromGroupAsync(long userId, int groupId);
    Task<List<Domain.User>> GetUsersInGroupAsync(int groupId);
    Task<List<Domain.Group>> GetUserGroupsAsync(long userId);
    Task<bool> SetMeAsync(long userId);
}