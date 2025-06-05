using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Resources;

[McpServerResourceType]
public class UsersResource(ITelegramRepository repository)
{
    [McpServerResource(UriTemplate = "telegram://user", Name = "All users")]
    [Description("Get all users")]
    public async ValueTask<ResourceContents> AllUsers(RequestContext<ReadResourceRequestParams> requestContext)
    {
        var users = await repository.GetAllUsersAsync();

        var formattedUsers = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.FirstName,
            u.LastName
        }).ToList();

        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(formattedUsers),
            MimeType = "application/json",
            Uri = "telegram://users",
        };
    }

    [McpServerResource(UriTemplate = "telegram://user/{userId}/groups", Name = "User groups")]
    [Description("Get all groups a user belongs to")]
    public async ValueTask<ResourceContents> UserGroups(RequestContext<ReadResourceRequestParams> requestContext, long userId)
    {
        var user = await repository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return new TextResourceContents
            {
                Text = JsonSerializer.Serialize(new { error = $"User {userId} not found" }),
                MimeType = "application/json",
                Uri = $"telegram://user/{userId}/groups",
            };
        }

        var groups = await repository.GetUserGroupsAsync(userId);

        var formattedGroups = groups.Select(g => new
        {
            g.Id,
            g.Name
        }).ToList();

        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(formattedGroups),
            MimeType = "application/json",
            Uri = $"telegram://user/{userId}/groups",
        };
    }
}
