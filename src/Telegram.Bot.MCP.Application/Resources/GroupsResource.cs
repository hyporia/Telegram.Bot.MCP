using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;

namespace Telegram.Bot.MCP.Application.Resources;

[McpServerResourceType]
public class GroupsResource(ITelegramRepository repository)
{
    [McpServerResource(UriTemplate = "telegram://group", Name = "All groups")]
    [Description("Get all available groups")]
    public async ValueTask<ResourceContents> AllGroups(RequestContext<ReadResourceRequestParams> requestContext)
    {
        var groups = await repository.GetAllGroupsAsync();

        var formattedGroups = groups.Select(g => new
        {
            g.Id,
            g.Name
        }).ToList();

        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(formattedGroups),
            MimeType = "application/json",
            Uri = "telegram://groups",
        };
    }

    [McpServerResource(UriTemplate = "telegram://group/{groupId}/users", Name = "Group users")]
    [Description("Get all users in a specific group")]
    public async ValueTask<ResourceContents> GroupUsers(RequestContext<ReadResourceRequestParams> requestContext, int groupId)
    {
        var group = await repository.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return new TextResourceContents
            {
                Text = JsonSerializer.Serialize(new { error = $"Group {groupId} not found" }),
                MimeType = "application/json",
                Uri = $"telegram://group/{groupId}/users",
            };
        }

        var formattedUsers = group.Users.Select(u => new
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
            Uri = $"telegram://group/{groupId}/users",
        };
    }
}
