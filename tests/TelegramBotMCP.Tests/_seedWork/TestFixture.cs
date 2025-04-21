
using Microsoft.EntityFrameworkCore;
using Moq;
using TelegramBotMCP.Data;
using TelegramBotMCP.Services.Abstract;
using TelegramBotMCP.Tools;

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
