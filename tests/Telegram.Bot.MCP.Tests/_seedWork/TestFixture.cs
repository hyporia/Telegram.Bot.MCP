
using Microsoft.EntityFrameworkCore;
using Moq;
using Telegram.Bot.MCP.Application.Interfaces;
using Telegram.Bot.MCP.Application.Tools;
using Telegram.Bot.MCP.Infra.Persistance;
using Telegram.Bot.MCP.Infra.Persistance.Repositories;

namespace TelegramBotMCP.Tests._seedWork;
internal class TestFixture : IAsyncLifetime
{
    public readonly TelegramBotTools TelegramBotTools;
    public readonly ApplicationDbContext DbContext;
    public readonly Mock<ITelegramBot> BotMock = new();

    public TestFixture(ITestOutputHelper output)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Filename=:memory:").Options;
        DbContext = new ApplicationDbContext(options);
        var repo = new TelegramRepository(DbContext, output.BuildLoggerFor<TelegramRepository>());
        TelegramBotTools = new TelegramBotTools(BotMock.Object, repo, output.BuildLoggerFor<TelegramBotTools>());
    }

    public async ValueTask InitializeAsync()
    {
        await DbContext.Database.OpenConnectionAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public ValueTask DisposeAsync() => DbContext.DisposeAsync();
}
