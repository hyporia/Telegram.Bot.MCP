
using Microsoft.EntityFrameworkCore;
using Moq;
using Telegram.Bot.MCP.Application.Interfaces;
using Telegram.Bot.MCP.Application.Tools;
using Telegram.Bot.MCP.Infra.Persistance;
using Telegram.Bot.MCP.Infra.Persistance.Repositories;

namespace TelegramBotMCP.Tests._seedWork;
internal class TestFixture : IAsyncLifetime
{
    public readonly ReadNewMessagesTool SUT;
    public readonly ApplicationDbContext DbContext;
    public readonly Mock<ITelegramBot> BotMock = new();

    public TestFixture(ITestOutputHelper output)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Filename=:memory:").Options;
        DbContext = new ApplicationDbContext(options);
        var repo = new TelegramRepository(DbContext, output.BuildLoggerFor<TelegramRepository>());
        SUT = new ReadNewMessagesTool(BotMock.Object, repo, output.BuildLoggerFor<ReadNewMessagesTool>());
    }

    public async ValueTask InitializeAsync()
    {
        await DbContext.Database.OpenConnectionAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public ValueTask DisposeAsync() => DbContext.DisposeAsync();
}
