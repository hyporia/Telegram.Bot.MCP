using AutoFixture.Xunit3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TelegramBotMCP.Data;
using TelegramBotMCP.Services.Abstract;
using TelegramBotMCP.Tools;

namespace TelegramBotMCP.Tests;

public class TelegramBotToolsTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TelegramRepository _repository;
    private readonly Mock<ITelegramBot> _botMock = new();
    private readonly TelegramBotTools _tools;
    private readonly ILogger<TelegramBotTools> _logger;

    public TelegramBotToolsTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<TelegramBotTools>();
        var repoLogger = loggerFactory.CreateLogger<TelegramRepository>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _repository = new TelegramRepository(_dbContext, repoLogger);
        _tools = new TelegramBotTools(_botMock.Object, _repository, _logger);
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContext.Database.OpenConnectionAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

    [Theory, AutoData]
    public async Task ReadNewMessages_WithMessages_ReturnsFormattedJson(UpdateDTO[] updates)
    {
        // Arrange
        _botMock.Setup(m => m.GetUpdates(It.IsAny<int>())).ReturnsAsync(updates);

        // Act
        var result = await _tools.ReadNewMessages();

        // Assert
        var messagesSentAsResponse = JsonSerializer.Deserialize<List<TelegramBotTools.NewMessageDto>>(result);
        Assert.NotNull(messagesSentAsResponse);
        Assert.Equal(updates.Length, messagesSentAsResponse.Count);
        Assert.All(messagesSentAsResponse, message =>
        {
            var incomingMessage = updates.FirstOrDefault(u => u.Message!.Text == message.Message);
            Assert.NotNull(incomingMessage);
            Assert.Equal(incomingMessage.Message!.From!.Id, message.UserId);
            Assert.Equal(incomingMessage.Message.From.Username, message.From);
        });

        // Verify persistence
        var savedMessages = await _dbContext.Messages.Include(m => m.User).ToListAsync(CancellationToken.None);
        Assert.Equal(updates.Length, savedMessages.Count);
        Assert.All(savedMessages, savedMessage =>
        {
            var incomingMessage = updates.FirstOrDefault(u => u.Message!.Text == savedMessage.Text);
            Assert.NotNull(incomingMessage);
            Assert.Equal(incomingMessage.Message!.Date.ToUniversalTime(), savedMessage.Timestamp);
            Assert.Equal(incomingMessage.Message.Text, savedMessage.Text);
            Assert.Equal(incomingMessage.Message.From!.Username, savedMessage.User.Username);
            Assert.Equal(incomingMessage.Message.From.FirstName, savedMessage.User.FirstName);
            Assert.Equal(incomingMessage.Message.From.LastName, savedMessage.User.LastName);
        });
    }

    [Fact]
    public async Task ReadNewMessages_NoMessages_ReturnsEmptyArray()
    {
        // Arrange
        _botMock.Setup(m => m.GetUpdates(It.IsAny<int>())).ReturnsAsync([]);

        // Act
        var result = await _tools.ReadNewMessages();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task ReadNewMessages_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        _botMock.Setup(m => m.GetUpdates(It.IsAny<int>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _tools.ReadNewMessages();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Failed to get unread messages", result);
        Assert.Contains("Test exception", result);
    }
}
